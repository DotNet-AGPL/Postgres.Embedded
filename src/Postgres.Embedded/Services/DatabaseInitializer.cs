using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Npgsql;
using Postgres.Embedded.Exceptions;

namespace Postgres.Embedded.Services;

public class DatabaseInitializer
{
    private readonly ILogger _logger;
    
    public DatabaseInitializer(ILogger logger)
    {
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
    }
    
    public void Initialize(string binaryPath, string dataPath, string username, 
        string password, string locale, string encoding, string runtimePath)
    {
        _logger.LogInformation("Initializing PostgreSQL database: {DataPath}", dataPath);
        
        var passwordFile = CreatePasswordFile(runtimePath, password);
        
        var args = $"-A password -U {username} -D {dataPath} --pwfile={passwordFile}";
        
        if (!string.IsNullOrEmpty(locale))
            args += $" --locale={locale}";
        
        if (!string.IsNullOrEmpty(encoding))
            args += $" --encoding={encoding}";
        
        var initDbPath = Path.Combine(binaryPath, "bin", "initdb");
        ExecuteProcess(initDbPath, args, "initdb");
        
        File.Delete(passwordFile);
        
        _logger.LogInformation("Database initialization completed");
    }
    
    public void CreateDatabase(int port, string username, string password, string database)
    {
        if (database == "postgres")
            return;  // 默认数据库无需创建
        
        _logger.LogInformation("Creating database: {Database}", database);
        
        var connectionString = $"Host=localhost;Port={port};Username={username};Password={password};Database=postgres";
        
        try
        {
            using var conn = new NpgsqlConnection(connectionString);
            conn.Open();  // 同步连接
            
            using var cmd = new NpgsqlCommand($"CREATE DATABASE \"{database}\"", conn);
            cmd.ExecuteNonQuery();  // 同步执行
            
            _logger.LogInformation("Database created: {Database}", database);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create database: {Database}", database);
            throw new DatabaseInitException($"Failed to create database '{database}'", ex);
        }
    }
    
    private string CreatePasswordFile(string runtimePath, string password)
    {
        var passwordFile = Path.Combine(runtimePath, "pwfile");
        File.WriteAllText(passwordFile, password);
        return passwordFile;
    }
    
    private void ExecuteProcess(string fileName, string arguments, string processName)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        
        process.Start();
        process.WaitForExit();
        
        if (process.ExitCode != 0)
        {
            var error = process.StandardError.ReadToEnd();
            throw new DatabaseInitException($"{processName} failed with exit code {process.ExitCode}: {error}");
        }
    }
}