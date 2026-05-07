# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Initial project structure and implementation
- Core `EmbeddedPostgres` class with lifecycle management
- Fluent Builder API (`EmbeddedPostgresBuilder`)
- Global port registry with `WeakReference` cleanup
- Auto-restart on second `Start()` call
- Port conflict detection and prevention
- Platform detection for Windows, Linux, macOS (x64 & ARM64)
- Binary downloader with Maven repository integration
- Archive extractor (tar.xz and JAR) using SharpCompress
- Database initializer (initdb)
- Process manager (pg_ctl)
- Health checker with timeout
- Synchronous-first API design
- Optional async methods (`StartAsync`, `StopAsync`)
- Custom exception types:
  - `PortConflictException`
  - `BinaryNotFoundException`
  - `DatabaseInitException`
  - `ProcessStartException`
  - `EmbeddedPostgresException`
- Unit tests for platform detection, builder, and downloader
- Integration tests for lifecycle and concurrent control
- Usage examples with Dapper and Npgsql
- License: GNU Affero General Public License v3.0 (AGPL-3.0)
- Package name: Postgres.Embedded
- Comprehensive README with API documentation
- EditorConfig for consistent code style
- Directory.Build.props for global build settings

### Supported PostgreSQL Versions
- 18.3.0 (V18)
- 17.5.0 (V17)
- 16.9.0 (V16)
- 15.13.0 (V15)
- 14.18.0 (V14)
- 13.21.0 (V13)
- 12.22.0 (V12)
- 11.22.0 (V11)
- 10.23.0 (V10)
- 9.6.24 (V9)

### Platform Support
- Windows (x64, ARM64)
- Linux (x64, ARM64, Alpine)
- macOS (x64, ARM64/M1)

## [1.0.0] - 2026-05-07

### Added
- Initial release
- Complete implementation of embedded PostgreSQL for .NET 10.0
- Port from `go-embedded-postgres` with improvements:
  - Global port registry (process-level concurrency control)
  - Auto-restart capability
  - WeakReference cleanup for failed instances
- License: GNU Affero General Public License v3.0 (AGPL-3.0)
- Package name: Postgres.Embedded (NuGet package identifier)