namespace GruetzeToaster;

public static class AppInfo
{
    public const string Name = "GruetzeToaster";
    public static string Version => typeof(AppInfo).Assembly.GetName().Version?.ToString(3) ?? "1.1.0";
    public static string DisplayName => $"{Name} {Version}";
    public static string AvaloniaVersion => typeof(Avalonia.Application).Assembly.GetName().Version?.ToString(3) ?? "unknown";
    public const string RepositoryUrl = "https://github.com/gruetze-software/GruetzeToaster";
    public const string CompanyUrl = "https://github.com/gruetze-software";
    public const string KofiUrl = "https://ko-fi.com/gruetzesoftware";
}
