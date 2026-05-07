# API Reference

## Core Classes

### `EmbeddedPostgresBuilder`

Fluent builder for creating `EmbeddedPostgres` instances.

#### Methods

| Method | Description | Default |
|--------|-------------|---------|
| `WithVersion(PostgresVersion)` | PostgreSQL version | V18 (18.3.0) |
| `WithPort(int)` | Port number | 5432 |
| `WithUsername(string)` | Username | postgres |
| `WithPassword(string)` | Password | postgres |
| `WithDatabase(string)` | Database name | postgres |
| `WithLocale(string)` | Locale setting | C |
| `WithEncoding(string)` | Character encoding | UTF8 |
| `WithStartTimeout(TimeSpan)` | Start timeout | 15 seconds |
| `WithStartParameters(Dictionary)` | PostgreSQL start parameters | empty |
| `WithBinaryRepositoryUrl(string)` | Maven repository URL | https://repo1.maven.org/maven2 |
| `WithRuntimePath(string)` | Runtime directory path | ~/.embedded-postgres-dotnet/extracted |
| `WithDataPath(string)` | Data directory path | ~/.embedded-postgres-dotnet/extracted/data |
| `WithBinariesPath(string)` | Binaries directory path | ~/.embedded-postgres-dotnet/extracted |
| `WithCachePath(string)` | Cache directory path | ~/.embedded-postgres-dotnet/cache |
| `WithLogger(ILogger)` | Logger instance | NullLogger |
| `WithAutoCleanRuntimePath(bool)` | Auto cleanup on Dispose | false |
| `Build()` | Build the instance | - |

#### Example

```csharp
var postgres = new EmbeddedPostgresBuilder()
    .WithVersion(PostgresVersion.V16)
    .WithPort(5433)
    .WithDatabase("testdb")
    .WithStartTimeout(TimeSpan.FromSeconds(30))
    .Build();
```

---

### `EmbeddedPostgres`

Main class for managing embedded PostgreSQL lifecycle.

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Port` | int | Configured port number |
| `IsRunning` | bool | Whether PostgreSQL is running |
| `ProcessId` | int? | PostgreSQL process ID (nullable) |
| `ConnectionString` | string | Npgsql connection string |

#### Methods

| Method | Return Type | Description |
|--------|-------------|-------------|
| `Start()` | void | Start PostgreSQL synchronously (blocking) |
| `Stop()` | void | Stop PostgreSQL synchronously (blocking) |
| `GetConnectionString()` | string | Get Npgsql connection string |
| `StartAsync(CancellationToken)` | Task | Start PostgreSQL asynchronously |
| `StopAsync()` | Task | Stop PostgreSQL asynchronously |
| `Dispose()` | void | Cleanup resources |
| `DisposeAsync()` | ValueTask | Cleanup resources asynchronously |

#### Static Methods

| Method | Return Type | Description |
|--------|-------------|-------------|
| `IsPortInUse(int)` | bool | Check if port is in use by any instance |
| `GetActiveInstances()` | IReadOnlyList | Get all active instances |
| `CleanupDeadReferences()` | void | Remove dead WeakReferences from registry |

#### Example

```csharp
using var postgres = new EmbeddedPostgresBuilder()
    .WithPort(5432)
    .Build();

postgres.Start();  // Blocking until ready

var connectionString = postgres.GetConnectionString();
// Host=localhost;Port=5432;Username=postgres;Password=postgres;Database=postgres

postgres.Stop();   // Blocking until stopped
```

---

## Models

### `PostgresVersion`

Enumeration of supported PostgreSQL versions.

| Value | Version String |
|-------|---------------|
| `V18` | 18.3.0 |
| `V17` | 17.5.0 |
| `V16` | 16.9.0 |
| `V15` | 15.13.0 |
| `V14` | 14.18.0 |
| `V13` | 13.21.0 |
| `V12` | 12.22.0 |
| `V11` | 11.22.0 |
| `V10` | 10.23.0 |
| `V9` | 9.6.24 |

---

### `EmbeddedPostgresConfig`

Configuration model for `EmbeddedPostgres`.

#### Properties

All properties have default values as listed in `EmbeddedPostgresBuilder` table above.

---

### `PlatformInfo`

Platform detection result.

| Property | Type | Description |
|----------|------|-------------|
| `OperatingSystem` | string | OS name (windows/linux/darwin) |
| `Architecture` | string | Architecture (amd64/arm64v8/arm32v7) |
| `IsAlpineLinux` | bool | Whether running on Alpine Linux |

---

## Exceptions

### `EmbeddedPostgresException`

Base exception for all embedded PostgreSQL errors.

```csharp
catch (EmbeddedPostgresException ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
```

---

### `PortConflictException`

Thrown when port is already in use.

| Property | Type | Description |
|----------|------|-------------|
| `Port` | int | Conflicting port number |

```csharp
catch (PortConflictException ex)
{
    Console.WriteLine($"Port {ex.Port} is already in use");
}
```

---

### `BinaryNotFoundException`

Thrown when PostgreSQL binary not found.

| Property | Type | Description |
|----------|------|-------------|
| `ExpectedPath` | string | Expected binary location |

---

### `DatabaseInitException`

Thrown when database initialization fails.

---

### `ProcessStartException`

Thrown when PostgreSQL process fails to start.

---

## Services

### `PlatformDetector`

Static class for platform detection.

```csharp
var platformInfo = PlatformDetector.Detect();
// Returns: PlatformInfo { OperatingSystem = "windows", Architecture = "amd64" }
```

---

### `BinaryDownloader`

Downloads PostgreSQL binaries from Maven repository.

| Method | Description |
|--------|-------------|
| `Download(string url, string path)` | Download binary synchronously |
| `DownloadChecksumAndVerify(string path, string url)` | Download and verify SHA256 checksum |
| `VerifyChecksum(string path, string expected)` | Verify SHA256 checksum |
| `BuildDownloadUrl(PostgresVersion, PlatformInfo)` | Build Maven download URL |

---

### `ArchiveExtractor`

Extracts PostgreSQL archives.

| Method | Description |
|--------|-------------|
| `ExtractTarXz(string archivePath, string destinationPath)` | Extract tar.xz archive |
| `ExtractJar(string jarPath, string destinationPath)` | Extract .txz from JAR |

---

### `DatabaseInitializer`

Initializes PostgreSQL database.

| Method | Description |
|--------|-------------|
| `Initialize(...)` | Run initdb with configuration |
| `CreateDatabase(...)` | Create custom database (if not postgres) |

---

### `ProcessManager`

Manages PostgreSQL process lifecycle.

| Method | Description |
|--------|-------------|
| `StartPostgres(...)` | Start PostgreSQL process using pg_ctl |

---

### `HealthChecker`

Checks database readiness.

| Method | Description |
|--------|-------------|
| `WaitForReady(...)` | Poll database until ready or timeout |

---

## Usage Patterns

### Basic Usage

```csharp
using var postgres = new EmbeddedPostgresBuilder()
    .Build();

postgres.Start();
var connectionString = postgres.GetConnectionString();
// Use database...
postgres.Stop();
```

### Test Fixture

```csharp
public class DatabaseTests : IDisposable
{
    private readonly EmbeddedPostgres _postgres;
    
    public DatabaseTests()
    {
        _postgres = new EmbeddedPostgresBuilder()
            .WithPort(5433)
            .Build();
        _postgres.Start();
    }
    
    public void Dispose()
    {
        _postgres.Stop();
        _postgres.Dispose();
    }
}
```

### Multiple Instances

```csharp
var db1 = new EmbeddedPostgresBuilder()
    .WithPort(5432)
    .Build();

var db2 = new EmbeddedPostgresBuilder()
    .WithPort(5433)
    .Build();

db1.Start();
db2.Start();  // Both run simultaneously
```

### Auto-Restart

```csharp
var postgres = new EmbeddedPostgresBuilder()
    .WithPort(5432)
    .Build();

postgres.Start();  // First start
postgres.Start();  // Auto-stops and restarts (no exception)
```

### Error Handling

```csharp
try
{
    postgres.Start();
}
catch (PortConflictException ex)
{
    Console.WriteLine($"Port {ex.Port} already in use");
}
catch (ProcessStartException ex)
{
    Console.WriteLine($"Failed to start: {ex.Message}");
}
catch (EmbeddedPostgresException ex)
{
    Console.WriteLine($"General error: {ex.Message}");
}
```

---

## Connection String Format

Standard Npgsql connection string:

```
Host=localhost;Port={port};Username={username};Password={password};Database={database}
```

Example:
```
Host=localhost;Port=5432;Username=postgres;Password=postgres;Database=postgres
```

---

## Concurrency Control

### Global Port Registry

Process-level registry prevents concurrent conflicts:

```csharp
// Registry is static and shared across all instances
EmbeddedPostgres.PortRegistry // Internal ConcurrentDictionary<int, WeakReference>

// Check if port in use
bool inUse = EmbeddedPostgres.IsPortInUse(5432);

// Get all active instances
var instances = EmbeddedPostgres.GetActiveInstances();

// Cleanup dead references
EmbeddedPostgres.CleanupDeadReferences();
```

### Auto-Cleanup

Failed instances are automatically cleaned up via `WeakReference`:

```csharp
var postgres = new EmbeddedPostgresBuilder()
    .WithPort(5432)
    .Build();

postgres.Start();

// If instance is garbage collected without Dispose
postgres = null;
GC.Collect();

// Port becomes available again (WeakReference.Target == null)
```

---

## Performance Notes

- **First startup**: Slow (~30-60s) due to binary download (~200MB)
- **Subsequent startups**: Fast (~2-5s) if binaries cached
- **Binary extraction**: Moderate (~10-20s for tar.xz)
- **Memory usage**: ~100MB overhead + PostgreSQL memory
- **Disk usage**: ~500MB (binaries + data directory)

---

## Thread Safety

- ✅ `EmbeddedPostgresBuilder` - Thread-safe (immutable)
- ✅ `EmbeddedPostgres.Start/Stop` - Thread-safe (global lock)
- ✅ `PortRegistry` - Thread-safe (`ConcurrentDictionary`)
- ⚠️ `Dispose` - Should be called from same thread as `Start`
- ⚠️ `HealthChecker` - Blocking, not suitable for UI threads

---

## Best Practices

1. **Always Dispose**: Use `using` statement or call `Dispose()`
2. **Use Different Ports**: For multiple instances
3. **Set Timeout**: Increase for slow networks `WithStartTimeout(TimeSpan.FromSeconds(60))`
4. **Check Port Availability**: Use `IsPortInUse()` before creating instance
5. **Handle Exceptions**: Catch `PortConflictException` explicitly
6. **Reuse Instances**: Don't recreate for every test (use fixtures)
7. **Clean Cache**: Delete `~/.embedded-postgres-dotnet/cache` if corrupted
8. **Persistent Data**: Use `WithDataPath()` for persistent test data