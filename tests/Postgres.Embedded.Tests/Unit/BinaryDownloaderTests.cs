using FluentAssertions;
using Xunit;
using Postgres.Embedded.Services;
using Postgres.Embedded.Models;

namespace DotNet.EmbeddedPostgres.Tests.Unit;

public class BinaryDownloaderTests
{
    [Fact]
    public void BuildDownloadUrl_ShouldReturnValidUrl()
    {
        var downloader = new BinaryDownloader(Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance);
        var platformInfo = new PlatformInfo("windows", "amd64");
        
        var url = downloader.BuildDownloadUrl(PostgresVersion.V16, platformInfo);
        
        url.Should().Contain("repo1.maven.org");
        url.Should().Contain("embedded-postgres-binaries");
        url.Should().Contain("windows-amd64");
        url.Should().Contain("16.9.0");
        url.Should().EndWith(".jar");
    }
    
    [Theory]
    [InlineData(PostgresVersion.V18, "18.3.0")]
    [InlineData(PostgresVersion.V17, "17.5.0")]
    [InlineData(PostgresVersion.V16, "16.9.0")]
    [InlineData(PostgresVersion.V15, "15.13.0")]
    [InlineData(PostgresVersion.V14, "14.18.0")]
    public void BuildDownloadUrl_ShouldIncludeCorrectVersion(PostgresVersion version, string expectedVersion)
    {
        var downloader = new BinaryDownloader(Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance);
        var platformInfo = new PlatformInfo("linux", "amd64");
        
        var url = downloader.BuildDownloadUrl(version, platformInfo);
        
        url.Should().Contain(expectedVersion);
    }
    
    [Fact]
    public void BuildDownloadUrl_ShouldIncludeAlpineSuffix()
    {
        var downloader = new BinaryDownloader(Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance);
        var platformInfo = new PlatformInfo("linux", "amd64-alpine", true);
        
        var url = downloader.BuildDownloadUrl(PostgresVersion.V16, platformInfo);
        
        url.Should().Contain("linux-amd64-alpine");
    }
}