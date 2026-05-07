using FluentAssertions;
using Xunit;
using Postgres.Embedded;
using Postgres.Embedded.Models;

namespace DotNet.EmbeddedPostgres.Tests.Unit;

public class EmbeddedPostgresBuilderTests
{
    [Fact]
    public void WithPort_ShouldSetPort()
    {
        var builder = new EmbeddedPostgresBuilder();
        
        builder.WithPort(5433);
        
        var postgres = builder.Build();
        postgres.Port.Should().Be(5433);
    }
    
    [Fact]
    public void WithPort_ShouldThrowOnInvalidPort()
    {
        var builder = new EmbeddedPostgresBuilder();
        
        var act = () => builder.WithPort(0);
        act.Should().Throw<ArgumentException>();
        
        act = () => builder.WithPort(65536);
        act.Should().Throw<ArgumentException>();
    }
    
    [Fact]
    public void WithVersion_ShouldSetVersion()
    {
        var builder = new EmbeddedPostgresBuilder();
        
        builder.WithVersion(PostgresVersion.V16);
        
        var postgres = builder.Build();
        postgres.Should().NotBeNull();
    }
    
    [Fact]
    public void WithUsername_ShouldThrowOnNull()
    {
        var builder = new EmbeddedPostgresBuilder();
        
        var act = () => builder.WithUsername(null!);
        act.Should().Throw<ArgumentNullException>();
    }
    
    [Fact]
    public void WithPassword_ShouldThrowOnNull()
    {
        var builder = new EmbeddedPostgresBuilder();
        
        var act = () => builder.WithPassword(null!);
        act.Should().Throw<ArgumentNullException>();
    }
    
    [Fact]
    public void WithDatabase_ShouldThrowOnNull()
    {
        var builder = new EmbeddedPostgresBuilder();
        
        var act = () => builder.WithDatabase(null!);
        act.Should().Throw<ArgumentNullException>();
    }
    
    [Fact]
    public void WithStartTimeout_ShouldThrowOnZero()
    {
        var builder = new EmbeddedPostgresBuilder();
        
        var act = () => builder.WithStartTimeout(TimeSpan.Zero);
        act.Should().Throw<ArgumentException>();
    }
    
    [Fact]
    public void Build_ShouldCreateInstance()
    {
        var postgres = new EmbeddedPostgresBuilder()
            .WithPort(5432)
            .Build();
        
        postgres.Should().NotBeNull();
        postgres.Port.Should().Be(5432);
    }
}