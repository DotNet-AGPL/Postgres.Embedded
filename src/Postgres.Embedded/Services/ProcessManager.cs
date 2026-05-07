using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Postgres.Embedded.Exceptions;

namespace Postgres.Embedded.Services;

public class ProcessManager
{
    private readonly ILogger _logger;
    
    public ProcessManager(ILogger logger)
    {
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
    }
    
    public Process StartPostgres(string binaryPath, string dataPath, int port, 
        Dictionary<string, string> startParameters)
    {
        _logger.LogInformation("Starting PostgreSQL process on port {Port}", port);
        
        var options = EncodeOptions(port, startParameters);
        
        var pgCtlPath = Path.Combine(binaryPath, "bin", "pg_ctl");
        var args = $"start -w -D {dataPath} -o \"{options}\"";
        
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = pgCtlPath,
                Arguments = args,
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
            var output = process.StandardOutput.ReadToEnd();
            var fullError = $"{error}\n{output}";
            
            _logger.LogError("pg_ctl start failed with exit code {Code}: {Error}", process.ExitCode, fullError);
            throw new ProcessStartException($"pg_ctl start failed with exit code {process.ExitCode}: {fullError}");
        }
        
        _logger.LogInformation("PostgreSQL process started successfully");
        
        // Return the PostgreSQL process itself (not pg_ctl)
        return FindPostgresProcess(port);
    }
    
    private Process FindPostgresProcess(int port)
    {
        // Simple implementation: return a dummy process for now
        // In production, we would query the system for the actual postgres process
        return new Process();
    }
    
    private string EncodeOptions(int port, Dictionary<string, string> parameters)
    {
        var options = $"-p {port}";
        
        foreach (var param in parameters)
        {
            options += $" -c {param.Key}=\"{param.Value}\"";
        }
        
        return options;
    }
}