namespace LogoffUsersTool.Models;

public class ApplicationInfo
{
    public string Version { get; set; } = "1.0";
    public DateTime LastRun { get; set; }
}

public class FullAppSettings
{
    public AppSettings DefaultSettings { get; set; } = new();
    public AppSettings LastUsedSettings { get; set; } = new();
    public ApplicationInfo Application { get; set; } = new();
}
