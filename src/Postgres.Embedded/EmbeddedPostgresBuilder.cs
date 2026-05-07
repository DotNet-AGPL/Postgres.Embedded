using Microsoft.Extensions.Logging;
using Postgres.Embedded.Models;

namespace Postgres.Embedded;

public class EmbeddedPostgresBuilder
{
    private readonly EmbeddedPostgresConfig _config = new EmbeddedPostgresConfig();
    
    public EmbeddedPostgresBuilder WithVersion(PostgresVersion version)
    {
        _config.Version = version;
        return this;
    }
    
    public EmbeddedPostgresBuilder WithPort(int port)
    {
        if (port < 1 || port > 65535)
            throw new ArgumentException("Port must be between 1 and 65535", nameof(port));
        _config.Port = port;
        return this;
    }
    
    public EmbeddedPostgresBuilder WithUsername(string username)
    {
        _config.Username = username ?? throw new ArgumentNullException(nameof(username));
        return this;
    }
    
    public EmbeddedPostgresBuilder WithPassword(string password)
    {
        _config.Password = password ?? throw new ArgumentNullException(nameof(password));
        return this;
    }
    
    public EmbeddedPostgresBuilder WithDatabase(string database)
    {
        _config.Database = database ?? throw new ArgumentNullException(nameof(database));
        return this;
    }
    
    public EmbeddedPostgresBuilder WithRuntimePath(string path)
    {
        _config.RuntimePath = path ?? throw new ArgumentNullException(nameof(path));
        return this;
    }
    
    public EmbeddedPostgresBuilder WithDataPath(string path)
    {
        _config.DataPath = path ?? throw new ArgumentNullException(nameof(path));
        return this;
    }
    
    public EmbeddedPostgresBuilder WithBinariesPath(string path)
    {
        _config.BinariesPath = path ?? throw new ArgumentNullException(nameof(path));
        return this;
    }
    
    public EmbeddedPostgresBuilder WithCachePath(string path)
    {
        _config.CachePath = path ?? throw new ArgumentNullException(nameof(path));
        return this;
    }
    
    public EmbeddedPostgresBuilder WithLocale(string locale)
    {
        _config.Locale = locale ?? throw new ArgumentNullException(nameof(locale));
        return this;
    }
    
    public EmbeddedPostgresBuilder WithEncoding(string encoding)
    {
        _config.Encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
        return this;
    }
    
    public EmbeddedPostgresBuilder WithStartTimeout(TimeSpan timeout)
    {
        if (timeout <= TimeSpan.Zero)
            throw new ArgumentException("Timeout must be positive", nameof(timeout));
        _config.StartTimeout = timeout;
        return this;
    }
    
    public EmbeddedPostgresBuilder WithLogger(ILogger logger)
    {
        _config.Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        return this;
    }
    
    public EmbeddedPostgresBuilder WithStartParameters(Dictionary<string, string> parameters)
    {
        _config.StartParameters = parameters ?? new Dictionary<string, string>();
        return this;
    }
    
    public EmbeddedPostgresBuilder WithBinaryRepositoryUrl(string url)
    {
        _config.BinaryRepositoryUrl = url ?? throw new ArgumentNullException(nameof(url));
        return this;
    }
    
    public EmbeddedPostgresBuilder WithAutoCleanRuntimePath(bool autoClean)
    {
        _config.AutoCleanRuntimePath = autoClean;
        return this;
    }
    
    public EmbeddedPostgres Build()
    {
        return new EmbeddedPostgres(_config);
    }
}