using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Npgsql;
using Postgres.Embedded.Models;
using Postgres.Embedded.Exceptions;
using Postgres.Embedded.Services;
using Postgres.Embedded.Detection;

namespace Postgres.Embedded;

public class EmbeddedPostgres : IDisposable, IAsyncDisposable
{
    private static readonly ConcurrentDictionary<int, WeakReference<EmbeddedPostgres>> PortRegistry 
        = new ConcurrentDictionary<int, WeakReference<EmbeddedPostgres>>();
    
    private static readonly object GlobalLock = new object();
    
    private readonly EmbeddedPostgresConfig _config;
    private volatile bool _started = false;
    private volatile bool _disposed = false;
    private Process? _postgresProcess;
    private string _dataPath = string.Empty;
    private string _runtimePath = string.Empty;
    private string _binariesPath = string.Empty;
    private string _cachePath = string.Empty;
    private readonly ILogger _logger;
    
    private readonly BinaryDownloader _binaryDownloader;
    private readonly ArchiveExtractor _archiveExtractor;
    private readonly DatabaseInitializer _databaseInitializer;
    private readonly ProcessManager _processManager;
    private readonly HealthChecker _healthChecker;
    
    public int Port => _config.Port;
    public bool IsRunning => _started && !_disposed;
    public int? ProcessId => _postgresProcess?.Id;
    public string ConnectionString => BuildConnectionString();
    
    internal EmbeddedPostgres(EmbeddedPostgresConfig config)
    {
        _config = ValidateConfig(config);
        _logger = config.Logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        
        _binaryDownloader = new BinaryDownloader(_logger);
        _archiveExtractor = new ArchiveExtractor(_logger);
        _databaseInitializer = new DatabaseInitializer(_logger);
        _processManager = new ProcessManager(_logger);
        _healthChecker = new HealthChecker(_logger);
    }
    
    public void Start()
    {
        ThrowIfDisposed();
        
        lock (GlobalLock)
        {
            if (_started)
            {
                _logger.LogInformation("Instance already started, auto-restarting on port {Port}", _config.Port);
                StopInternal();
            }
            
            if (PortRegistry.TryGetValue(_config.Port, out var weakRef))
            {
                if (weakRef.TryGetTarget(out var existingInstance) && existingInstance != null && existingInstance._started)
                {
                    throw new PortConflictException(_config.Port,
                        $"Port {_config.Port} is already in use by another EmbeddedPostgres instance (PID: {existingInstance.ProcessId}). " +
                        $"Please use a different port or stop the existing instance first.");
                }
                
                PortRegistry.TryRemove(_config.Port, out _);
            }
            
            EnsurePortAvailable(_config.Port);
            
            PortRegistry[_config.Port] = new WeakReference<EmbeddedPostgres>(this);
            
            try
            {
                InitializePaths();
                DownloadAndExtractBinary();
                InitializeDatabase();
                StartPostgresProcess();
                WaitForDatabaseReady();
                
                _started = true;
                _logger.LogInformation("Embedded PostgreSQL started successfully on port {Port}", _config.Port);
            }
            catch (Exception ex)
            {
                PortRegistry.TryRemove(_config.Port, out _);
                CleanupOnError();
                
                _logger.LogError(ex, "Failed to start Embedded PostgreSQL on port {Port}", _config.Port);
                throw new ProcessStartException($"Failed to start EmbeddedPostgres on port {_config.Port}", ex);
            }
        }
    }
    
    public void Stop()
    {
        ThrowIfDisposed();
        
        lock (GlobalLock)
        {
            if (!_started)
            {
                _logger.LogDebug("Instance not started, ignoring Stop() call");
                return;
            }
            
            StopInternal();
        }
    }
    
    public string GetConnectionString()
    {
        ThrowIfDisposed();
        return ConnectionString;
    }
    
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        await Task.Run(() => Start(), cancellationToken);
    }
    
    public async Task StopAsync()
    {
        ThrowIfDisposed();
        await Task.Run(() => Stop());
    }
    
    private void StopInternal()
    {
        _logger.LogInformation("Stopping Embedded PostgreSQL on port {Port}", _config.Port);
        
        try
        {
            if (_postgresProcess != null && !_postgresProcess.HasExited)
            {
                var stopProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = Path.Combine(_binariesPath, "bin", "pg_ctl"),
                        Arguments = $"stop -w -D {_dataPath}",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                
                stopProcess.Start();
                var timeoutMs = (int)_config.StartTimeout.TotalMilliseconds;
                stopProcess.WaitForExit(timeoutMs);
                
                if (!stopProcess.HasExited || stopProcess.ExitCode != 0)
                {
                    _logger.LogWarning("pg_ctl stop failed or timed out, killing process");
                    _postgresProcess.Kill();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during graceful shutdown, forcing kill");
            try { _postgresProcess?.Kill(); } catch { }
        }
        finally
        {
            PortRegistry.TryRemove(_config.Port, out _);
            
            _started = false;
            _postgresProcess = null;
            
            _logger.LogInformation("Embedded PostgreSQL stopped on port {Port}", _config.Port);
        }
    }
    
    private void EnsurePortAvailable(int port)
    {
        try
        {
            using var listener = new TcpListener(IPAddress.Loopback, port);
            listener.Start();
            listener.Stop();
        }
        catch (SocketException ex)
        {
            throw new PortConflictException(port,
                $"Port {port} is already in use by an external process. " +
                $"Socket error: {ex.SocketErrorCode}. Use 'netstat -ano | findstr :{port}' (Windows) or 'lsof -i :{port}' (Unix) to identify the process.",
                ex);
        }
    }
    
    private void InitializePaths()
    {
        var homePath = GetHomePath();
        var defaultBasePath = Path.Combine(homePath, ".embedded-postgres-dotnet");
        
        _cachePath = _config.CachePath ?? Path.Combine(defaultBasePath, "cache");
        _runtimePath = _config.RuntimePath ?? Path.Combine(defaultBasePath, "extracted");
        _dataPath = _config.DataPath ?? Path.Combine(_runtimePath, "data");
        _binariesPath = _config.BinariesPath ?? _runtimePath;
        
        _logger.LogDebug("Initialized paths - Cache: {Cache}, Runtime: {Runtime}, Data: {Data}, Binaries: {Binaries}",
            _cachePath, _runtimePath, _dataPath, _binariesPath);
    }
    
    private void DownloadAndExtractBinary()
    {
        lock (GlobalLock)
        {
            var pgCtlPath = Path.Combine(_binariesPath, "bin", "pg_ctl");
            
            if (!File.Exists(pgCtlPath))
            {
                _logger.LogInformation("PostgreSQL binaries not found, downloading...");
                
                var platformInfo = PlatformDetector.Detect();
                var downloadUrl = _binaryDownloader.BuildDownloadUrl(_config.Version, platformInfo);
                var jarPath = Path.Combine(_cachePath, $"postgres-{platformInfo}.jar");
                var txzPath = Path.Combine(_cachePath, $"postgres-{platformInfo}.txz");
                
                Directory.CreateDirectory(_cachePath);
                
                if (!File.Exists(jarPath))
                {
                    _logger.LogInformation("Downloading from: {Url}", downloadUrl);
                    _binaryDownloader.Download(downloadUrl, jarPath);
                    
                    var checksumUrl = $"{downloadUrl}.sha256";
                    _binaryDownloader.DownloadChecksumAndVerify(jarPath, checksumUrl);
                }
                
                _logger.LogInformation("Extracting JAR archive...");
                _archiveExtractor.ExtractJar(jarPath, txzPath);
                
                _logger.LogInformation("Extracting tar.xz archive...");
                _archiveExtractor.ExtractTarXz(txzPath, _binariesPath);
                
                _logger.LogInformation("PostgreSQL binaries extracted successfully");
            }
            else
            {
                _logger.LogInformation("PostgreSQL binaries already exist at: {Path}", _binariesPath);
            }
        }
    }
    
    private void InitializeDatabase()
    {
        var pgVersionFile = Path.Combine(_dataPath, "PG_VERSION");
        var reuseData = File.Exists(pgVersionFile);
        
        if (reuseData)
        {
            var versionContent = File.ReadAllText(pgVersionFile).Trim();
            var expectedVersion = GetVersionPrefix(_config.Version);
            
            if (!versionContent.StartsWith(expectedVersion))
            {
                _logger.LogWarning("Data directory version mismatch, reinitializing");
                reuseData = false;
            }
        }
        
        if (!reuseData)
        {
            _logger.LogInformation("Initializing PostgreSQL data directory");
            
            if (Directory.Exists(_runtimePath))
            {
                Directory.Delete(_runtimePath, recursive: true);
            }
            
            Directory.CreateDirectory(_runtimePath);
            
            _databaseInitializer.Initialize(_binariesPath, _dataPath, _config.Username, 
                _config.Password, _config.Locale, _config.Encoding, _runtimePath);
            
            _logger.LogInformation("Data directory initialized successfully");
        }
        else
        {
            _logger.LogInformation("Reusing existing data directory");
        }
    }
    
    private void StartPostgresProcess()
    {
        _logger.LogInformation("Starting PostgreSQL process");
        
        _postgresProcess = _processManager.StartPostgres(_binariesPath, _dataPath, 
            _config.Port, _config.StartParameters);
        
        _logger.LogInformation("PostgreSQL process started (PID: {Pid})", _postgresProcess?.Id);
    }
    
    private void WaitForDatabaseReady()
    {
        _logger.LogInformation("Waiting for database to be ready");
        _healthChecker.WaitForReady(_config.Port, _config.Database, _config.Username, 
            _config.Password, _config.StartTimeout);
    }
    
    private string BuildConnectionString()
    {
        return $"Host=localhost;Port={_config.Port};Username={_config.Username};Password={_config.Password};Database={_config.Database}";
    }
    
    private void CleanupOnError()
    {
        try
        {
            if (_postgresProcess != null && !_postgresProcess.HasExited)
            {
                _postgresProcess.Kill();
            }
        }
        catch { }
        
        _postgresProcess = null;
        _started = false;
    }
    
    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(EmbeddedPostgres));
    }
    
    private static string GetHomePath()
    {
        return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    }
    
    private static string GetVersionPrefix(PostgresVersion version)
    {
        return version switch
        {
            PostgresVersion.V18 => "18",
            PostgresVersion.V17 => "17",
            PostgresVersion.V16 => "16",
            PostgresVersion.V15 => "15",
            PostgresVersion.V14 => "14",
            PostgresVersion.V13 => "13",
            PostgresVersion.V12 => "12",
            PostgresVersion.V11 => "11",
            PostgresVersion.V10 => "10",
            PostgresVersion.V9 => "9",
            _ => throw new ArgumentOutOfRangeException(nameof(version))
        };
    }
    
    private static EmbeddedPostgresConfig ValidateConfig(EmbeddedPostgresConfig config)
    {
        if (config.Port < 1 || config.Port > 65535)
            throw new ArgumentException($"Invalid port: {config.Port}");
        
        if (string.IsNullOrWhiteSpace(config.Username))
            throw new ArgumentException("Username cannot be empty");
        
        if (string.IsNullOrWhiteSpace(config.Password))
            throw new ArgumentException("Password cannot be empty");
        
        if (string.IsNullOrWhiteSpace(config.Database))
            throw new ArgumentException("Database cannot be empty");
        
        return config;
    }
    
    public void Dispose()
    {
        lock (GlobalLock)
        {
            if (_disposed)
                return;
            
            if (_started)
                StopInternal();
            
            try
            {
                if (_config.AutoCleanRuntimePath && Directory.Exists(_runtimePath))
                {
                    Directory.Delete(_runtimePath, recursive: true);
                    _logger.LogDebug("Cleaned up runtime path: {Path}", _runtimePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to clean up runtime path: {Path}", _runtimePath);
            }
            
            PortRegistry.TryRemove(_config.Port, out _);
            
            _disposed = true;
        }
    }
    
    public async ValueTask DisposeAsync()
    {
        Dispose();
        await ValueTask.CompletedTask;
    }
    
    public static bool IsPortInUse(int port)
    {
        lock (GlobalLock)
        {
            if (PortRegistry.TryGetValue(port, out var weakRef))
            {
                if (weakRef.TryGetTarget(out var instance) && instance != null && instance._started)
                {
                    return true;
                }
            }
            return false;
        }
    }
    
    public static IReadOnlyList<EmbeddedPostgres> GetActiveInstances()
    {
        lock (GlobalLock)
        {
            return PortRegistry.Values
                .Where(wr => wr.TryGetTarget(out var instance) && instance != null && instance._started)
                .Select(wr => wr.TryGetTarget(out var instance) ? instance! : null!)
                .Where(i => i != null)
                .ToList();
        }
    }
    
    public static void CleanupDeadReferences()
    {
        lock (GlobalLock)
        {
            var deadPorts = PortRegistry
                .Where(kvp => !kvp.Value.TryGetTarget(out _) || kvp.Value.TryGetTarget(out var instance) && instance == null)
                .Select(kvp => kvp.Key)
                .ToList();
            
            foreach (var port in deadPorts)
                PortRegistry.TryRemove(port, out _);
        }
    }
}