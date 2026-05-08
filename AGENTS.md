# Agent Instructions

## Build & Test Commands

```bash
dotnet restore
dotnet build
dotnet test
```

**Important**: The CI pipeline filters tests by category (`--filter "Category=Unit"` and `--filter "Category=Integration"`), but test files currently lack `[Trait("Category", "...")]` attributes. This will cause CI test runs to skip all tests.

## Architecture

- **Main library**: `src/Postgres.Embedded/` - Embedded PostgreSQL for .NET testing
- **Tests**: `tests/Postgres.Embedded.Tests/` with `Unit/` and `Integration/` folders
- **Examples**: `examples/Postgres.Embedded.Examples/`

## Key Requirements

- **Target Framework**: .NET 10.0 (preview SDK required)
- **Warnings**: Treated as errors (`TreatWarningsAsErrors=true`)
- **Analyzers**: .NET analyzers enabled at latest level
- **Documentation**: XML docs generated in Release builds

## Code Style (enforced via .editorconfig)

- CRLF line endings
- 4-space indentation for C#, 2-space for config files
- Private fields: `_camelCase` prefix required
- PascalCase for types, properties, methods
- Interfaces: `IPascalCase` prefix required

## Namespace Mismatch

Test namespaces use `DotNet.EmbeddedPostgres.Tests.*` but project names use `Postgres.Embedded.Tests`. This is intentional - follow existing patterns.

## CI Pipeline Issues

The `azure-pipelines.yml` references incorrect paths:
- Line 57, 95, 114, 134: `tests/DotNet.EmbeddedPostgres.Tests/...` should be `tests/Postgres.Embedded.Tests/...`
- Line 64: `src/DotNet.EmbeddedPostgres/...` should be `src/Postgres.Embedded/...`

## Test Behavior

Integration tests start real PostgreSQL instances:
- First run downloads PostgreSQL binaries (~50-100MB)
- Tests need network access to Maven repository for binaries
- Default port 5432 - tests should use different ports to avoid conflicts