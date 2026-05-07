namespace Postgres.Embedded.Models;

public class PlatformInfo
{
    public string OperatingSystem { get; init; } = string.Empty;
    public string Architecture { get; init; } = string.Empty;
    public bool IsAlpineLinux { get; init; }
    
    public PlatformInfo(string os, string arch, bool isAlpine = false)
    {
        OperatingSystem = os;
        Architecture = arch;
        IsAlpineLinux = isAlpine;
    }
    
    public override string ToString()
    {
        return $"{OperatingSystem}-{Architecture}";
    }
}