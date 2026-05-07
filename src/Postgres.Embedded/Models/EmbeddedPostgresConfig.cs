using Microsoft.Extensions.Logging;

namespace Postgres.Embedded.Models;

public class EmbeddedPostgresConfig
{
    public PostgresVersion Version { get; set; } = PostgresVersion.V18;
    public int Port { get; set; } = 5432;
    public string Username { get; set; } = "postgres";
    public string Password { get; set; } = "postgres";
    public string Database { get; set; } = "postgres";
    public string Locale { get; set; } = "C";
    public string Encoding { get; set; } = "UTF8";
    public TimeSpan StartTimeout { get; set; } = TimeSpan.FromSeconds(15);
    public Dictionary<string, string> StartParameters { get; set; } = new Dictionary<string, string>();
    public string BinaryRepositoryUrl { get; set; } = "https://repo1.maven.org/maven2";
    
    public string? RuntimePath { get; set; }
    public string? DataPath { get; set; }
    public string? BinariesPath { get; set; }
    public string? CachePath { get; set; }
    
    public ILogger? Logger { get; set; }
    
    public bool AutoCleanRuntimePath { get; set; } = false;
}