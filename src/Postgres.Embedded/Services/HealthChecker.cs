using Microsoft.Extensions.Logging;
using Npgsql;
using Postgres.Embedded.Exceptions;

namespace Postgres.Embedded.Services;

public class HealthChecker
{
    private readonly ILogger _logger;
    
    public HealthChecker(ILogger logger)
    {
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
    }
    
    public void WaitForReady(int port, string database, string username, string password, TimeSpan timeout)
    {
        _logger.LogInformation("Waiting for database to be ready (timeout: {Timeout}s)", timeout.TotalSeconds);
        
        var deadline = DateTime.Now.Add(timeout);
        
        while (DateTime.Now < deadline)  // 同步轮询
        {
            try
            {
                var connectionString = $"Host=localhost;Port={port};Username={username};Password={password};Database={database}";
                
                using var conn = new NpgsqlConnection(connectionString);
                conn.Open();  // 同步连接
                
                using var cmd = new NpgsqlCommand("SELECT 1", conn);
                cmd.ExecuteScalar();  // 同步执行
                
                _logger.LogInformation("Database is ready");
                return;  // 成功
            }
            catch (Exception ex)
            {
                _logger.LogDebug("Database not ready yet: {Error}", ex.Message);
                Thread.Sleep(100);  // 同步等待100ms
            }
        }
        
        _logger.LogError("Database not ready within timeout");
        throw new ProcessStartException($"Database not ready within {timeout.TotalSeconds} seconds");
    }
}