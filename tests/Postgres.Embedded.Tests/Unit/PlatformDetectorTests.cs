using FluentAssertions;
using Xunit;
using Postgres.Embedded.Models;
using Postgres.Embedded.Detection;

namespace DotNet.EmbeddedPostgres.Tests.Unit;

public class PlatformDetectorTests
{
    [Fact]
    public void Detect_ShouldReturnValidPlatformInfo()
    {
        var platformInfo = PlatformDetector.Detect();
        
        platformInfo.Should().NotBeNull();
        platformInfo.OperatingSystem.Should().NotBeNullOrEmpty();
        platformInfo.Architecture.Should().NotBeNullOrEmpty();
    }
    
    [Fact]
    public void Detect_ShouldReturnValidOsName()
    {
        var platformInfo = PlatformDetector.Detect();
        
        platformInfo.OperatingSystem.Should().BeOneOf("windows", "linux", "darwin");
    }
    
    [Fact]
    public void Detect_ShouldReturnValidArchitecture()
    {
        var platformInfo = PlatformDetector.Detect();
        
        platformInfo.Architecture.Should().MatchRegex(@"^(amd64|arm64|arm64v8|arm32v7)(-alpine)?$");
    }
}