# Troubleshooting Guide

## Common Issues

### 1. Port Already in Use

**Error**:
```
PortConflictException: Port 5432 is already in use by an external process.
```

**Causes**:
- Another PostgreSQL instance running
- Previous test didn't cleanup
- External application using the port

**Solutions**:

```bash
# Find process using port (Windows)
netstat -ano | findstr :5432

# Find process using port (Linux/macOS)
lsof -i :5432

# Kill process
kill -9 <PID>
```

**Prevention**:
```csharp
// Check before creating
if (EmbeddedPostgres.IsPortInUse(5432))
{
    Console.WriteLine("Port in use, choosing different port");
}

// Use different port
var postgres = new EmbeddedPostgresBuilder()
    .WithPort(5433)  // Use available port
    .Build();
```

---

### 2. Download Timeout

**Error**:
```
EmbeddedPostgresException: Failed to download PostgreSQL binaries from https://repo1.maven.org/maven2...
```

**Causes**:
- Slow network connection
- Maven repository unavailable
- Firewall blocking downloads

**Solutions**:

```csharp
// Increase timeout
var postgres = new EmbeddedPostgresBuilder()
    .WithStartTimeout(TimeSpan.FromSeconds(120))  // 2 minutes
    .Build();

// Use Maven mirror
var postgres = new EmbeddedPostgresBuilder()
    .WithBinaryRepositoryUrl("https://repo.local/mirror")
    .Build();
```

**Manual Download**:
```bash
# Download manually
cd ~/.embedded-postgres-dotnet/cache
wget https://repo1.maven.org/maven2/io/zonky/test/postgres/embedded-postgres-binaries-windows-amd64/16.9.0/embedded-postgres-binaries-windows-amd64-16.9.0.jar
```

---

### 3. Extraction Failure

**Error**:
```
EmbeddedPostgresException: Failed to extract PostgreSQL binaries from /path/to/archive.txz
```

**Causes**:
- Corrupted download
- Insufficient disk space
- Permission denied
- SharpCompress library issue

**Solutions**:

```bash
# Check disk space
df -h  # Linux/macOS
dir    # Windows

# Clear cache and redownload
rm -rf ~/.embedded-postgres-dotnet/cache
```

---

### 4. Database Initialization Failure

**Error**:
```
DatabaseInitException: initdb failed with exit code 1: ...
```

**Causes**:
- Invalid locale setting
- Invalid encoding
- Permission denied on data directory
- Data directory already exists with wrong version

**Solutions**:

```csharp
// Use valid locale
var postgres = new EmbeddedPostgresBuilder()
    .WithLocale("C")  // Universal locale
    .WithEncoding("UTF8")
    .Build();

// Clear corrupted data directory
rm -rf ~/.embedded-postgres-dotnet/extracted/data
```

---

### 5. Process Start Failure

**Error**:
```
ProcessStartException: pg_ctl start failed with exit code 1: ...
```

**Causes**:
- Data directory corruption
- Insufficient system resources
- PostgreSQL configuration error
- Missing binaries

**Solutions**:

```bash
# Check PostgreSQL logs
cat ~/.embedded-postgres-dotnet/extracted/data/logfile

# Verify binaries exist
ls ~/.embedded-postgres-dotnet/extracted/bin/pg_ctl

# Clear everything and restart
rm -rf ~/.embedded-postgres-dotnet
```

---

### 6. Health Check Timeout

**Error**:
```
ProcessStartException: Database not ready within 15 seconds
```

**Causes**:
- PostgreSQL startup slow
- System overload
- Insufficient timeout

**Solutions**:

```csharp
// Increase timeout
var postgres = new EmbeddedPostgresBuilder()
    .WithStartTimeout(TimeSpan.FromSeconds(30))
    .Build();
```

---

### 7. macOS Rosetta 2 Requirement

**Error** (on M1/M2):
```
ProcessStartException: pg_ctl start failed...
```

**Cause**: PostgreSQL < 14.2 on macOS ARM64 requires Rosetta 2

**Solutions**:

```bash
# Install Rosetta 2
softwareupdate --install-rosetta
```

```csharp
// Use PostgreSQL 14.2+ (native ARM64 binaries)
var postgres = new EmbeddedPostgresBuilder()
    .WithVersion(PostgresVersion.V14)  // or V15, V16, V17, V18
    .Build();
```

---

### 8. Alpine Linux Compatibility

**Error** (on Alpine):
```
ProcessStartException: pg_ctl execution failed...
```

**Cause**: Standard binaries don't work on Alpine (musl libc)

**Solution**: Platform detection automatically uses Alpine binaries if `/etc/alpine-release` exists.

**Manual Override**: (if detection fails)
```bash
# Verify Alpine detection
cat /etc/alpine-release
```

---

### 9. Permission Denied

**Error**:
```
UnauthorizedAccessException: Access to the path '/path/to/data' is denied.
```

**Causes**:
- Directory owned by different user
- Insufficient permissions
- Windows: UAC blocking

**Solutions**:

```bash
# Linux/macOS: Change ownership
sudo chown -R $USER:$USER ~/.embedded-postgres-dotnet

# Windows: Run as Administrator or use accessible path
```

```csharp
// Use accessible path
var postgres = new EmbeddedPostgresBuilder()
    .WithRuntimePath(Path.Combine(Path.GetTempPath(), "postgres"))
    .Build();
```

---

### 10. Multiple Instances Conflict

**Error**:
```
PortConflictException: Port 5432 is already in use by another EmbeddedPostgres instance (PID: 12345).
```

**Cause**: Multiple instances in same process using same port

**Solutions**:

```csharp
// Use different ports
var db1 = new EmbeddedPostgresBuilder()
    .WithPort(5432)
    .Build();

var db2 = new EmbeddedPostgresBuilder()
    .WithPort(5433)  // Different port
    .Build();

// Or check before creating
if (!EmbeddedPostgres.IsPortInUse(5432))
{
    var db = new EmbeddedPostgresBuilder()
        .WithPort(5432)
        .Build();
}
```

---

## Debugging Techniques

### Enable Debug Logging

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

### Check File System

```bash
# Verify cache
ls -la ~/.embedded-postgres-dotnet/cache

# Verify binaries
ls -la ~/.embedded-postgres-dotnet/extracted/bin

# Verify data directory
ls -la ~/.embedded-postgres-dotnet/extracted/data
```

### Manual PostgreSQL Commands

```bash
# Check PostgreSQL version
cat ~/.embedded-postgres-dotnet/extracted/data/PG_VERSION

# Start PostgreSQL manually
cd ~/.embedded-postgres-dotnet/extracted/bin
./pg_ctl start -D ../data -l ../data/logfile

# Check status
./pg_ctl status -D ../data

# Stop manually
./pg_ctl stop -D ../data
```

### Connection Test

```csharp
using var postgres = new EmbeddedPostgresBuilder().Build();
postgres.Start();

try
{
    using var conn = new NpgsqlConnection(postgres.GetConnectionString());
    conn.Open();
    Console.WriteLine("Connection successful!");
}
catch (Exception ex)
{
    Console.WriteLine($"Connection failed: {ex.Message}");
}
```

---

## Performance Issues

### Slow First Startup

**Normal**: First startup takes 30-60 seconds (download + extraction)

**Improvement**:
- Pre-download binaries in CI pipeline
- Use Maven mirror close to your region
- Cache `BinariesPath` across test runs

### Slow Subsequent Startup

**Expected**: 2-5 seconds if binaries cached and data reused

**If slower**:
- Check if `DataPath` is reused (should be fast)
- Verify binaries aren't re-downloaded (check cache)
- Clear and restart if corrupted

```bash
# Clear cache if issues
rm -rf ~/.embedded-postgres-dotnet/cache
rm -rf ~/.embedded-postgres-dotnet/extracted
```

---

## Platform-Specific Issues

### Windows

- **Windows Firewall**: May prompt on first PostgreSQL startup (allow it)
- **Antivirus**: May block PostgreSQL executable (whitelist `pg_ctl.exe`)
- **Path Length**: Avoid long paths (>260 characters)

### Linux

- **SELinux**: May restrict PostgreSQL execution
- **AppArmor**: Similar restrictions on Ubuntu
- **Systemd**: May interfere with port bindings

### macOS

- **Rosetta 2**: Required for PG < 14.2 on M1/M2
- **Gatekeeper**: May block unsigned binaries
- **Path Permissions**: Use user-writable paths

---

## Recovery Procedures

### Complete Reset

```bash
# Delete everything
rm -rf ~/.embedded-postgres-dotnet

# Start fresh
var postgres = new EmbeddedPostgresBuilder().Build();
postgres.Start();
```

### Kill Zombie Processes

```bash
# Find PostgreSQL processes
ps aux | grep postgres

# Kill all
pkill -9 postgres
```

### Fix Port Registry

```csharp
// Cleanup dead references in registry
EmbeddedPostgres.CleanupDeadReferences();

// Check active instances
var instances = EmbeddedPostgres.GetActiveInstances();
foreach (var instance in instances)
{
    Console.WriteLine($"Port {instance.Port}, Running: {instance.IsRunning}");
}
```

---

## Getting Help

1. **Check Logs**: Enable debug logging
2. **Verify Paths**: Check cache and binary directories
3. **Manual Test**: Run PostgreSQL manually from binaries
4. **Clean Slate**: Delete everything and retry
5. **Report Issue**: https://github.com/DotNet-AGPL/Postgres.Embedded/issues

Include in issue report:
- Platform (Windows/Linux/macOS)
- Architecture (x64/ARM64)
- PostgreSQL version
- Exception type and message
- Full stack trace
- Log output (if available)