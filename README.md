# Postgres.Embedded

Run a real PostgreSQL database locally on Linux, macOS, or Windows as part of another .NET application or test.

This library provides a higher level of confidence than using any in-memory alternative when testing. It requires no other external dependencies outside of the .NET build ecosystem.

Heavily inspired by Go project [fergusstrange/embedded-postgres](https://github.com/fergusstrange/embedded-postgres) and Java projects [zonkyio/embedded-postgres](https://github.com/zonkyio/embedded-postgres) and [opentable/otj-pg-embedded](https://github.com/opentable/otj-pg-embedded).

## Features

- ✅ **Fluent Builder API** - Modern, chainable configuration
- ✅ **Synchronous-first design** - Blocking methods aligned with Go version
- ✅ **Auto-restart on second start** - User-friendly lifecycle management
- ✅ **Global port registry** - Prevents concurrent conflicts within process
- ✅ **WeakReference cleanup** - Automatic garbage collection of failed instances
- ✅ **Cross-platform support** - Windows, Linux, macOS (x64 & ARM64)
- ✅ **Multiple PostgreSQL versions** - From 9.6 to 18.3
- ✅ **Npgsql integration** - Seamless connection with Npgsql driver

## Installation

```bash
dotnet add package Postgres.Embedded
```

## Quick Start

```csharp
using Postgres.Embedded;

// Basic usage with defaults
using var postgres = new EmbeddedPostgresBuilder()
    .WithPort(5432)
    .Build();

postgres.Start();

var connectionString = postgres.GetConnectionString();

// Use Npgsql to connect
using var conn = new NpgsqlConnection(connectionString);
conn.Open();

using var cmd = new NpgsqlCommand("SELECT version()", conn);
var version = cmd.ExecuteScalar();

Console.WriteLine($"PostgreSQL version: {version}");

postgres.Stop();
```

## Configuration

| Configuration         | Default Value                                   |
|-----------------------|-------------------------------------------------|
| Username              | postgres                                        |
| Password              | postgres                                        |
| Database              | postgres                                        |
| Version               | 18.3.0                                          |
| Port                  | 5432                                            |
| Encoding              | UTF8                                            |
| Locale                | C                                               |
| StartTimeout          | 15 seconds                                      |
| CachePath             | ~/.embedded-postgres-dotnet/                    |
| RuntimePath           | ~/.embedded-postgres-dotnet/extracted           |
| DataPath              | ~/.embedded-postgres-dotnet/extracted/data      |
| BinariesPath          | ~/.embedded-postgres-dotnet/extracted           |
| BinaryRepositoryUrl   | https://repo1.maven.org/maven2                  |

### Custom Configuration Example

```csharp
using var loggerFactory = LoggerFactory.Create(builder => 
{
    builder.AddConsole().SetMinimumLevel(LogLevel.Debug);
});

var logger = loggerFactory.CreateLogger<EmbeddedPostgres>();

using var postgres = new EmbeddedPostgresBuilder()
    .WithVersion(PostgresVersion.V16)
    .WithPort(5433)
    .WithUsername("myuser")
    .WithPassword("mypassword")
    .WithDatabase("mydb")
    .WithLocale("en_US.UTF-8")
    .WithEncoding("UTF8")
    .WithStartTimeout(TimeSpan.FromSeconds(30))
    .WithStartParameters(new Dictionary<string, string>
    {
        ["max_connections"] = "200",
        ["shared_buffers"] = "256MB"
    })
    .WithLogger(logger)
    .Build();

postgres.Start();
```

## Supported PostgreSQL Versions

| Version | Version String |
|---------|---------------|
| V18     | 18.3.0        |
| V17     | 17.5.0        |
| V16     | 16.9.0        |
| V15     | 15.13.0       |
| V14     | 14.18.0       |
| V13     | 13.21.0       |
| V12     | 12.22.0       |
| V11     | 11.22.0       |
| V10     | 10.23.0       |
| V9      | 9.6.24        |

## Platform Support

| Platform      | Architecture | Supported |
|---------------|--------------|-----------|
| Windows       | x64          | ✅        |
| Windows       | ARM64        | ✅        |
| Linux         | x64          | ✅        |
| Linux         | ARM64        | ✅        |
| Linux Alpine  | x64          | ✅        |
| Linux Alpine  | ARM64        | ✅        |
| macOS         | x64          | ✅        |
| macOS         | ARM64 (M1)   | ✅        |

**Note**: PostgreSQL versions before 14.2 on macOS ARM64 require Rosetta 2 due to upstream binaries being x86_64-only. Version 14.2+ includes universal binaries that work natively on Apple Silicon.

## Concurrent Control

The library uses a **global port registry** to prevent concurrent conflicts:

- ✅ Prevents multiple instances on the same port within a process
- ✅ Auto-restarts when calling `Start()` twice on the same instance
- ✅ Throws `PortConflictException` if port is occupied by another instance
- ✅ Automatic cleanup of failed instances using `WeakReference`

### Exception Handling

```csharp
try
{
    postgres.Start();
}
catch (PortConflictException ex)
{
    Console.WriteLine($"Port {ex.Port} is already in use");
    // Choose a different port or stop the conflicting instance
}
catch (ProcessStartException ex)
{
    Console.WriteLine($"Failed to start PostgreSQL: {ex.Message}");
}
catch (EmbeddedPostgresException ex)
{
    Console.WriteLine($"General error: {ex.Message}");
}
```

## Usage in Tests

### xUnit Example

```csharp
public class DatabaseTests : IDisposable
{
    private readonly EmbeddedPostgres _postgres;
    
    public DatabaseTests()
    {
        _postgres = new EmbeddedPostgresBuilder()
            .WithPort(5433)
            .WithDatabase("testdb")
            .Build();
        
        _postgres.Start();
    }
    
    [Fact]
    public void Test_InsertAndQuery()
    {
        var connectionString = _postgres.GetConnectionString();
        
        using var conn = new NpgsqlConnection(connectionString);
        conn.Open();
        
        conn.Execute("CREATE TABLE users (id SERIAL, name TEXT)");
        conn.Execute("INSERT INTO users (name) VALUES ('Alice')");
        
        var users = conn.Query<User>("SELECT * FROM users").ToList();
        
        Assert.Single(users);
        Assert.Equal("Alice", users[0].Name);
    }
    
    public void Dispose()
    {
        _postgres?.Stop();
        _postgres?.Dispose();
    }
}
```

## Multiple Instances

```csharp
var instance1 = new EmbeddedPostgresBuilder()
    .WithPort(5432)
    .WithDatabase("db1")
    .Build();

var instance2 = new EmbeddedPostgresBuilder()
    .WithPort(5433)
    .WithDatabase("db2")
    .Build();

instance1.Start();
instance2.Start();

// Run two databases simultaneously

instance1.Stop();
instance2.Stop();
```

## Credits

- [zonkyio/embedded-postgres-binaries](https://github.com/zonkyio/embedded-postgres-binaries) - Precompiled PostgreSQL binaries
- [fergusstrange/embedded-postgres](https://github.com/fergusstrange/embedded-postgres) - Original Go implementation

## License

GNU Affero General Public License v3.0 (AGPL-3.0) - See [LICENSE](LICENSE) file for details.

This is a strong copyleft license that ensures:
- ✅ Free to use, modify, and distribute
- ✅ Modifications must be shared under the same license
- ✅ Network use (SaaS) requires providing source code to users
- ✅ Commercial use allowed but requires source code availability

For more information, see: https://www.gnu.org/licenses/agpl-3.0.html

## Contributing

Contributions are welcome! Please feel free to submit pull requests or open issues at:
https://github.com/DotNet-AGPL/Postgres.Embedded/issues

## Roadmap

- [ ] Docker container support
- [ ] Entity Framework Core integration
- [ ] Test framework extensions (xUnit, NUnit)
- [ ] Connection pool management
- [ ] Async API improvements

## Project Status

**Repository**: https://github.com/DotNet-AGPL/Postgres.Embedded  
**License**: AGPL-3.0  
**Status**: Active Development