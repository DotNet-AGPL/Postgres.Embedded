# Platform Support Matrix

## Operating Systems

| OS | Supported | Notes |
|----|-----------|-------|
| **Windows** | ✅ | Windows 10+ (x64, ARM64) |
| **Linux** | ✅ | Most distributions (x64, ARM64) |
| **Linux Alpine** | ✅ | Alpine Linux (musl libc) |
| **macOS** | ✅ | macOS 10.15+ (x64, ARM64/M1) |
| **FreeBSD** | ❌ | Not supported (no binaries) |
| **Other Unix** | ❌ | Not supported |

---

## Architectures

| Architecture | Binary Name | Platforms |
|--------------|-------------|-----------|
| **x64 (amd64)** | `amd64` | Windows, Linux, macOS |
| **ARM64 (arm64v8)** | `arm64v8` | Linux, macOS (M1/M2) |
| **ARM64** | `arm64` | Windows ARM64, macOS |
| **ARM32 (arm32v7)** | `arm32v7` | Linux ARM32 |
| **ARM32 (arm32v6)** | `arm32v6` | Linux ARM32 (older) |

---

## PostgreSQL Version Support

| Version | Windows x64 | Windows ARM64 | Linux x64 | Linux ARM64 | Linux Alpine | macOS x64 | macOS ARM64 |
|---------|-------------|---------------|-----------|-------------|--------------|-----------|-------------|
| **18.3.0** | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ (native) |
| **17.5.0** | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ (native) |
| **16.9.0** | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ (native) |
| **15.13.0** | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ (native) |
| **14.18.0** | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ (native) |
| **14.2-14.17** | ✅ | ⚠️ | ✅ | ✅ | ✅ | ✅ | ✅ (native) |
| **14.0-14.1** | ✅ | ⚠️ | ✅ | ✅ | ✅ | ✅ | ⚠️ (Rosetta) |
| **13.21.0** | ✅ | ⚠️ | ✅ | ✅ | ✅ | ✅ | ⚠️ (Rosetta) |
| **12.22.0** | ✅ | ⚠️ | ✅ | ✅ | ✅ | ✅ | ⚠️ (Rosetta) |
| **11.22.0** | ✅ | ⚠️ | ✅ | ⚠️ | ✅ | ✅ | ⚠️ (Rosetta) |
| **10.23.0** | ✅ | ⚠️ | ✅ | ⚠️ | ✅ | ✅ | ⚠️ (Rosetta) |
| **9.6.24** | ✅ | ⚠️ | ✅ | ⚠️ | ✅ | ✅ | ⚠️ (Rosetta) |

**Legend**:
- ✅ = Full support (native binaries)
- ⚠️ = Limited support (may require Rosetta 2 on macOS ARM64)
- ❌ = Not supported

---

## macOS ARM64 (M1/M2/M3) Special Notes

### Native Support (PG 14.2+)

PostgreSQL 14.2 and later include **universal binaries** that work natively on Apple Silicon:

```csharp
// Recommended: Use PG 14+ for native ARM64 support
var postgres = new EmbeddedPostgresBuilder()
    .WithVersion(PostgresVersion.V14)  // or V15, V16, V17, V18
    .Build();
```

### Rosetta 2 Required (PG < 14.2)

PostgreSQL versions before 14.2 only have x86_64 binaries on macOS. On Apple Silicon:

```bash
# Install Rosetta 2
softwareupdate --install-rosetta
```

```csharp
// PG < 14.2 will use Rosetta 2 (x86_64 emulation)
var postgres = new EmbeddedPostgresBuilder()
    .WithVersion(PostgresVersion.V13)
    .Build();  // Works via Rosetta 2
```

**Performance Impact**: Rosetta 2 emulation is slower than native ARM64 binaries.

---

## Linux Alpine Support

### Detection

Alpine Linux is automatically detected by checking `/etc/alpine-release`.

### Binary Names

Alpine binaries use `musl libc` instead of `glibc`:

```
embedded-postgres-binaries-linux-amd64-alpine-16.9.0.jar
embedded-postgres-binaries-linux-arm64v8-alpine-16.9.0.jar
```

### Compatibility

| Distribution | libc | Compatible |
|--------------|------|------------|
| Alpine Linux | musl | ✅ Alpine binaries |
| Ubuntu/Debian | glibc | ✅ Standard binaries |
| Fedora/RHEL | glibc | ✅ Standard binaries |
| Arch Linux | glibc | ✅ Standard binaries |

**Warning**: Standard binaries DO NOT work on Alpine (missing glibc). Alpine binaries DO NOT work on standard Linux (missing musl).

---

## Windows ARM64 Support

Windows ARM64 (Surface Pro X, etc.) is fully supported with native binaries from PostgreSQL 14.2+.

Earlier versions may have limited support or require emulation.

---

## Binary Download URLs

### URL Pattern

```
https://repo1.maven.org/maven2/io/zonky/test/postgres/
embedded-postgres-binaries-{os}-{arch}/{version}/
embedded-postgres-binaries-{os}-{arch}-{version}.jar
```

### Examples

| Platform | URL |
|----------|-----|
| Windows x64 | `embedded-postgres-binaries-windows-amd64-16.9.0.jar` |
| Linux x64 | `embedded-postgres-binaries-linux-amd64-16.9.0.jar` |
| Linux ARM64 | `embedded-postgres-binaries-linux-arm64v8-16.9.0.jar` |
| Linux Alpine x64 | `embedded-postgres-binaries-linux-amd64-alpine-16.9.0.jar` |
| macOS x64 | `embedded-postgres-binaries-darwin-amd64-16.9.0.jar` |
| macOS ARM64 | `embedded-postgres-binaries-darwin-arm64-16.9.0.jar` |

---

## Binary Sizes

| Platform | JAR Size | Extracted Size |
|----------|----------|----------------|
| Windows x64 | ~200 MB | ~400 MB |
| Linux x64 | ~180 MB | ~350 MB |
| Linux ARM64 | ~180 MB | ~350 MB |
| macOS Universal | ~200 MB | ~400 MB |

---

## System Requirements

### Minimum

- **CPU**: x64 or ARM64 processor
- **RAM**: 512 MB free (PostgreSQL overhead)
- **Disk**: 1 GB free (binaries + data)
- **Network**: Internet access (first download)

### Recommended

- **CPU**: Modern multi-core processor
- **RAM**: 2 GB free (for PostgreSQL + application)
- **Disk**: 5 GB free (multiple versions, test data)
- **Network**: Stable connection for initial download

---

## Runtime Dependencies

### .NET Runtime

- .NET 10.0 Runtime (or SDK for development)
- Npgsql 8.0.0+ (included as NuGet dependency)
- SharpCompress 0.37.0+ (included as NuGet dependency)

### System Dependencies

#### Windows
- No additional dependencies required
- Windows Firewall may prompt (allow PostgreSQL)

#### Linux
- No additional dependencies for standard binaries
- Alpine: musl libc (built into binaries)

#### macOS
- Rosetta 2 for PG < 14.2 on Apple Silicon
- No additional dependencies for PG 14.2+

---

## Testing Coverage

### Tested Platforms

| Platform | Architecture | PG Versions | Test Status |
|----------|--------------|-------------|-------------|
| Windows 10 | x64 | V12-V18 | ✅ Automated |
| Windows 11 | ARM64 | V14-V18 | ⚠️ Manual |
| Ubuntu 20.04 | x64 | V12-V18 | ✅ Automated |
| Ubuntu 22.04 | ARM64 | V14-V18 | ⚠️ Manual |
| Alpine 3.18 | x64 | V14-V18 | ✅ Automated |
| macOS 12 (Monterey) | x64 | V12-V18 | ✅ Automated |
| macOS 14 (Sonoma) | ARM64 | V14-V18 | ✅ Automated |

---

## Known Limitations

1. **No cross-architecture support**: x64 binaries don't work on ARM64
2. **No cross-OS support**: Linux binaries don't work on Windows
3. **Alpine incompatibility**: Standard binaries fail on Alpine
4. **macOS Rosetta**: PG < 14.2 requires Rosetta 2 on M1/M2
5. **Windows ARM64**: Limited testing, may have edge cases
6. **FreeBSD**: No binaries available, not supported
7. **32-bit Windows**: Not supported (no binaries)
8. **Solaris/AIX**: Not supported (no binaries)

---

## Future Platform Support

### Planned
- ✅ FreeBSD (if binaries become available)
- ✅ Windows 32-bit (unlikely, deprecated)

### Not Planned
- ❌ Solaris, AIX, HP-UX
- ❌ Mobile platforms (iOS, Android)
- ❌ WebAssembly

---

## Reporting Platform Issues

If you encounter platform-specific issues:

1. **Verify Platform**: Run `PlatformDetector.Detect()` to confirm detection
2. **Check Binary**: Verify downloaded JAR matches your platform
3. **Test Manually**: Run PostgreSQL manually from extracted binaries
4. **Provide Details**:
   - OS and version (e.g., Windows 11, Ubuntu 22.04)
   - Architecture (x64, ARM64)
   - PostgreSQL version
   - Exception message
   - Log output

Report at: https://github.com/DotNet-AGPL/Postgres.Embedded/issues