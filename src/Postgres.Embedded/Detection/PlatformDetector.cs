using System.Runtime.InteropServices;
using Postgres.Embedded.Models;

namespace Postgres.Embedded.Detection;

public static class PlatformDetector
{
    public static PlatformInfo Detect()
    {
        var os = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "windows"
            : RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "linux"
            : RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "darwin"
            : throw new PlatformNotSupportedException($"Unsupported OS: {RuntimeInformation.OSDescription}");
        
        var arch = RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.X64 => "amd64",
            Architecture.Arm64 => os == "linux" ? "arm64v8" : "arm64",
            Architecture.Arm => "arm32v7",
            _ => throw new PlatformNotSupportedException($"Unsupported architecture: {RuntimeInformation.ProcessArchitecture}")
        };
        
        var isAlpine = false;
        if (os == "linux")
        {
            isAlpine = IsAlpineLinux();
            if (isAlpine)
                arch += "-alpine";
        }
        
        return new PlatformInfo(os, arch, isAlpine);
    }
    
    private static bool IsAlpineLinux()
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.Linux) 
            && File.Exists("/etc/alpine-release");
    }
}