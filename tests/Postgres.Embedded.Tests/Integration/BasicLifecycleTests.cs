using FluentAssertions;
using Xunit;
using Postgres.Embedded;
using Postgres.Embedded.Exceptions;
using Postgres.Embedded.Models;

namespace DotNet.EmbeddedPostgres.Tests.Integration;

public class BasicLifecycleTests
{
    [Fact]
    public void EmbeddedPostgres_ShouldStartAndStopSuccessfully()
    {
        var postgres = new EmbeddedPostgresBuilder()
            .WithPort(5432)
            .Build();
        
        try
        {
            postgres.Start();
            postgres.IsRunning.Should().BeTrue();
            
            postgres.Stop();
            postgres.IsRunning.Should().BeFalse();
        }
        finally
        {
            postgres.Dispose();
        }
    }
    
    [Fact]
    public void EmbeddedPostgres_ShouldReturnConnectionString()
    {
        var postgres = new EmbeddedPostgresBuilder()
            .WithPort(5433)
            .WithUsername("testuser")
            .WithPassword("testpass")
            .WithDatabase("testdb")
            .Build();
        
        try
        {
            postgres.Start();
            
            var connectionString = postgres.GetConnectionString();
            
            connectionString.Should().Contain("Host=localhost");
            connectionString.Should().Contain("Port=5433");
            connectionString.Should().Contain("Username=testuser");
            connectionString.Should().Contain("Password=testpass");
            connectionString.Should().Contain("Database=testdb");
        }
        finally
        {
            postgres.Stop();
            postgres.Dispose();
        }
    }
    
    [Fact]
    public void EmbeddedPostgres_ShouldCleanupOnDispose()
    {
        var postgres = new EmbeddedPostgresBuilder()
            .WithPort(5434)
            .Build();
        
        postgres.Start();
        postgres.Dispose();
        
        postgres.IsRunning.Should().BeFalse();
    }
}