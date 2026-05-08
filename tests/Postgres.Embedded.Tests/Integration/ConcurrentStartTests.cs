using FluentAssertions;
using Xunit;
using Postgres.Embedded;
using Postgres.Embedded.Exceptions;
using Postgres.Embedded.Models;

namespace DotNet.EmbeddedPostgres.Tests.Integration;

public class ConcurrentStartTests
{
    [Fact]
    public void EmbeddedPostgres_ShouldThrowOnPortConflict()
    {
        var baseDir = AppContext.BaseDirectory;
        
        var postgres1 = new EmbeddedPostgresBuilder()
            .WithPort(5432)
            .WithCachePath(Path.Combine(baseDir, "pg-test-5432", "cache"))
            .WithRuntimePath(Path.Combine(baseDir, "pg-test-5432", "runtime"))
            .WithDataPath(Path.Combine(baseDir, "pg-test-5432", "data"))
            .WithBinariesPath(Path.Combine(baseDir, "pg-test-5432", "binaries"))
            .Build();
        
        var postgres2 = new EmbeddedPostgresBuilder()
            .WithPort(5432)
            .WithCachePath(Path.Combine(baseDir, "pg-test-5432-conflict", "cache"))
            .WithRuntimePath(Path.Combine(baseDir, "pg-test-5432-conflict", "runtime"))
            .WithDataPath(Path.Combine(baseDir, "pg-test-5432-conflict", "data"))
            .WithBinariesPath(Path.Combine(baseDir, "pg-test-5432-conflict", "binaries"))
            .Build();
        
        try
        {
            postgres1.Start();
            postgres1.IsRunning.Should().BeTrue();
            
            var act = () => postgres2.Start();
            act.Should().Throw<PortConflictException>()
                .Where(ex => ex.Port == 5432);
            
            postgres2.IsRunning.Should().BeFalse();
        }
        finally
        {
            postgres1.Stop();
            postgres1.Dispose();
            postgres2.Dispose();
        }
    }
    
    [Fact]
    public void EmbeddedPostgres_ShouldAllowDifferentPortsConcurrently()
    {
        var baseDir = AppContext.BaseDirectory;
        
        var postgres1 = new EmbeddedPostgresBuilder()
            .WithPort(5432)
            .WithCachePath(Path.Combine(baseDir, "pg-test-5432", "cache"))
            .WithRuntimePath(Path.Combine(baseDir, "pg-test-5432", "runtime"))
            .WithDataPath(Path.Combine(baseDir, "pg-test-5432", "data"))
            .WithBinariesPath(Path.Combine(baseDir, "pg-test-5432", "binaries"))
            .Build();
        
        var postgres2 = new EmbeddedPostgresBuilder()
            .WithPort(5433)
            .WithCachePath(Path.Combine(baseDir, "pg-test-5433", "cache"))
            .WithRuntimePath(Path.Combine(baseDir, "pg-test-5433", "runtime"))
            .WithDataPath(Path.Combine(baseDir, "pg-test-5433", "data"))
            .WithBinariesPath(Path.Combine(baseDir, "pg-test-5433", "binaries"))
            .Build();
        
        try
        {
            postgres1.Start();
            postgres2.Start();
            
            postgres1.IsRunning.Should().BeTrue();
            postgres2.IsRunning.Should().BeTrue();
        }
        finally
        {
            postgres1.Stop();
            postgres2.Stop();
            postgres1.Dispose();
            postgres2.Dispose();
        }
    }
}