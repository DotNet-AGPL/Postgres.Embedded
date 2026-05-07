# Configuration Guide

## Default Configuration

All configuration values have sensible defaults matching the Go version:

```csharp
var postgres = new EmbeddedPostgresBuilder().Build();
// Uses all defaults
```

| Configuration | Default Value | Description |
|---------------|---------------|-------------|
| **Version** | V18 (18.3.0) | PostgreSQL version |
| **Port** | 5432 | TCP port for connections |
| **Username** | postgres | Database username |
| **Password** | postgres | Database password |
| **Database** | postgres | Default database name |
| **Encoding** | UTF8 | Character encoding |
| **Locale** | C | Locale setting |
| **StartTimeout** | 15 seconds | Maximum startup time |
| **BinaryRepositoryUrl** | https://repo1.maven.org/maven2 | Maven repository URL |
| **RuntimePath** | ~/.embedded-postgres-dotnet/extracted | Runtime directory |
| **DataPath** | ~/.embedded-postgres-dotnet/extracted/data | Data directory |
| **BinariesPath** | ~/.embedded-postgres-dotnet/extracted | Binary location |
| **CachePath** | ~/.embedded-postgres-dotnet/cache | Download cache |
| **AutoCleanRuntimePath** | false | Cleanup on Dispose |

---

## Path Configuration

### Default Paths

```
User Home Directory (~/.embedded-postgres-dotnet/)
├── cache/                         ← Downloaded JAR files
│   └── postgres-windows-amd64.jar
│   └── postgres-linux-amd64.txz
└── extracted/                     ← Runtime directory
    ├── bin/                       ← PostgreSQL binaries
    │   ├── pg_ctl
    │   ├── initdb
    │   └── postgres
    └── data/                      ← Data directory
        ├── PG_VERSION
        ├── postgresql.conf
        └── postmaster.pid
```

### Custom Paths

```csharp
var postgres = new EmbeddedPostgresBuilder()
    .WithRuntimePath("/tmp/postgres-runtime")
    .WithDataPath("/var/lib/postgresql/data")     // Persistent data
    .WithCachePath("/opt/postgres-cache")
    .Build();
```

**Important Notes**:
- `RuntimePath` is erased and recreated on each `Start()` (not suitable for persistent data)
- `DataPath` can be set outside `RuntimePath` for persistence
- If `DataPath` is set and valid, database is reused (fast restart)
- `BinariesPath` defaults to `RuntimePath` but can be separate

---

## Database Configuration

### Custom Credentials

```csharp
var postgres = new EmbeddedPostgresBuilder()
    .WithUsername("myuser")
    .WithPassword("mypassword")
    .WithDatabase("mydb")
    .Build();
```

### Encoding and Locale

```csharp
var postgres = new EmbeddedPostgresBuilder()
    .WithEncoding("UTF8")
    .WithLocale("en_US.UTF-8")
    .Build();
```

### PostgreSQL Start Parameters

Pass runtime parameters to PostgreSQL (equivalent to `postgresql.conf` settings):

```csharp
var postgres = new EmbeddedPostgresBuilder()
    .WithStartParameters(new Dictionary<string, string>
    {
        ["max_connections"] = "200",
        ["shared_buffers"] = "256MB",
        ["work_mem"] = "64MB",
        ["log_statement"] = "all"
    })
    .Build();
```

See PostgreSQL documentation for all parameters: https://www.postgresql.org/docs/current/runtime-config.html

---

## Timeout Configuration

### Default Timeout (15 seconds)

Suitable for most cases where binaries are already cached.

```csharp
var postgres = new EmbeddedPostgresBuilder().Build();
postgres.Start();  // Timeout after 15s if not ready
```

### Extended Timeout

Recommended for slow networks or first-time downloads:

```csharp
var postgres = new EmbeddedPostgresBuilder()
    .WithStartTimeout(TimeSpan.FromSeconds(60))  // 1 minute
    .Build();
```

### Timeout Breakdown

```
Total Timeout includes:
- Binary download (first time): ~30-60s
- Binary extraction: ~10-20s
- Database initialization: ~2-5s
- Process startup: ~1-3s
- Health check: <1s (retries within timeout)
```

---

## Version Configuration

### Supported Versions

```csharp
// PostgreSQL 18 (latest)
var postgres = new EmbeddedPostgresBuilder()
    .WithVersion(PostgresVersion.V18)  // 18.3.0
    .Build();

// PostgreSQL 16 (popular)
var postgres = new EmbeddedPostgresBuilder()
    .WithVersion(PostgresVersion.V16)  // 16.9.0
    .Build();

// PostgreSQL 12 (legacy)
var postgres = new EmbeddedPostgresBuilder()
    .WithVersion(PostgresVersion.V12)  // 12.22.0
    .Build();
```

### Version Compatibility

- macOS ARM64 (M1/M2): PG 14.2+ has native binaries
- PG 9.x-14.1 on macOS ARM64: Requires Rosetta 2 (x86_64 binaries)

---

## Maven Repository Configuration

### Default Repository

```csharp
var postgres = new EmbeddedPostgresBuilder().Build();
// Uses: https://repo1.maven.org/maven2
```

### Custom Repository (Mirror)

```csharp
var postgres = new EmbeddedPostgresBuilder()
    .WithBinaryRepositoryUrl("https://repo.local/central.proxy")
    .Build();
```

Useful for:
- Corporate networks with Maven proxy
- Faster download from regional mirror
- Offline environments with local cache

---

## Logging Configuration

### Null Logger (Default)

```csharp
var postgres = new EmbeddedPostgresBuilder().Build();
// No logging output
```

### Console Logger

```csharp
using var loggerFactory = LoggerFactory.Create(builder => 
{
    builder.AddConsole().SetMinimumLevel(LogLevel.Debug);
});

var logger = loggerFactory.CreateLogger<EmbeddedPostgres>();

var postgres = new EmbeddedPostgresBuilder()
    .WithLogger(logger)
    .Build();
```

### Custom Logger

```csharp
var postgres = new EmbeddedPostgresBuilder()
    .WithLogger(myCustomLogger)
    .Build();
```

---

## Cleanup Configuration

### Auto-Clean Disabled (Default)

```csharp
var postgres = new EmbeddedPostgresBuilder().Build();
postgres.Start();
postgres.Dispose();  // Runtime directory preserved (faster next start)
```

### Auto-Clean Enabled

```csharp
var postgres = new EmbeddedPostgresBuilder()
    .WithAutoCleanRuntimePath(true)
    .Build();

postgres.Start();
postgres.Dispose();  // Runtime directory deleted (clean slate)
```

**Note**: Cache directory is never auto-cleaned (binaries persist).

---

## Advanced Configuration

### Persistent Data Directory

```csharp
var postgres = new EmbeddedPostgresBuilder()
    .WithDataPath("/var/lib/postgres-data")  // Outside RuntimePath
    .Build();

// First start: initializes data directory
postgres.Start();

// Stop (data persists)
postgres.Stop();

// Second start: reuses data (fast!)
postgres.Start();
```

### Multiple Instances with Isolated Paths

```csharp
var db1 = new EmbeddedPostgresBuilder()
    .WithPort(5432)
    .WithRuntimePath("/tmp/pg1")
    .WithDataPath("/tmp/pg1/data")
    .Build();

var db2 = new EmbeddedPostgresBuilder()
    .WithPort(5433)
    .WithRuntimePath("/tmp/pg2")
    .WithDataPath("/tmp/pg2/data")
    .Build();
```

### Reuse Binaries Across Instances

```csharp
// Shared binaries directory
var sharedBinaries = "/opt/postgres-binaries";

var db1 = new EmbeddedPostgresBuilder()
    .WithPort(5432)
    .WithBinariesPath(sharedBinaries)
    .WithRuntimePath("/tmp/db1-runtime")
    .Build();

var db2 = new EmbeddedPostgresBuilder()
    .WithPort(5433)
    .WithBinariesPath(sharedBinaries)  // Reuse binaries
    .WithRuntimePath("/tmp/db2-runtime")
    .Build();
```

---

## Configuration Validation

All configuration is validated at build time:

```csharp
// Invalid port
var postgres = new EmbeddedPostgresBuilder()
    .WithPort(0)  // Throws ArgumentException
    .Build();

// Invalid timeout
var postgres = new EmbeddedPostgresBuilder()
    .WithStartTimeout(TimeSpan.Zero)  // Throws ArgumentException
    .Build();

// Null username
var postgres = new EmbeddedPostgresBuilder()
    .WithUsername(null)  // Throws ArgumentNullException
    .Build();
```

---

## Environment Variables

The library does NOT use environment variables. All configuration must be explicitly set via builder methods.

If you need environment variable support, implement it in your application:

```csharp
var port = int.Parse(Environment.GetEnvironmentVariable("PG_PORT") ?? "5432");
var version = Enum.Parse<PostgresVersion>(Environment.GetEnvironmentVariable("PG_VERSION") ?? "V16");

var postgres = new EmbeddedPostgresBuilder()
    .WithPort(port)
    .WithVersion(version)
    .Build();
```

---

## Configuration Best Practices

1. **Test Environments**:
   - Use default paths (temp directories)
   - Enable auto-cleanup for isolation
   - Short timeout (15-30s)

2. **Production Environments**:
   - Use persistent `DataPath`
   - Disable auto-cleanup
   - Long timeout (60-120s for first download)
   - Custom Maven mirror if needed

3. **Multiple Instances**:
   - Isolate `RuntimePath` and `DataPath`
   - Share `BinariesPath` to save disk space
   - Use different ports

4. **CI/CD Pipelines**:
   - Use default configuration
   - Cache binaries directory across builds
   - Enable auto-cleanup after tests

5. **Debugging Issues**:
   - Enable debug-level logging
   - Check `RuntimePath` contents
   - Verify binary downloads in `CachePath`