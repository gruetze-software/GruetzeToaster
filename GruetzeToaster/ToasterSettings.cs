using System;
using System.IO;
using System.Text.Json;

namespace GruetzeToaster;

public class ToasterSettings
{
    public const int MinCount = 1, MaxCount = 200;
    public const double MinSpeed = 0.2, MaxSpeed = 5;
    public int ToasterCount { get; set; } = 35;
    public double SpeedMultiplier { get; set; } = 1;
    public bool ShowFps { get; set; }
    public bool ShowC64 { get; set; }
    public bool ShowAmiga { get; set; }

    public void Validate()
    {
        ToasterCount = Math.Clamp(ToasterCount, MinCount, MaxCount);
        SpeedMultiplier = double.IsFinite(SpeedMultiplier)
            ? Math.Clamp(SpeedMultiplier, MinSpeed, MaxSpeed) : 1;
    }
}

public class ConfigManager
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    public ToasterSettings Settings { get; set; } = new();
    public string ConfigPath { get; }
    public string? LastError { get; private set; }

    public ConfigManager(string? configPath = null)
    {
        ConfigPath = configPath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "GruetzeToaster", "settings.json");
        LoadSettings();
    }

    public void LoadSettings()
    {
        LastError = null;
        Settings = new();
        try
        {
            if (File.Exists(ConfigPath))
                Settings = JsonSerializer.Deserialize<ToasterSettings>(File.ReadAllText(ConfigPath)) ?? new();
            Settings.Validate();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        { LastError = Tools.GetExcMsg(ex); }
    }

    public bool SaveSettings()
    {
        LastError = null;
        string? temporaryPath = null;
        try
        {
            Settings.Validate();
            string fullPath = Path.GetFullPath(ConfigPath);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
            temporaryPath = fullPath + "." + Guid.NewGuid().ToString("N") + ".tmp";
            File.WriteAllText(temporaryPath, JsonSerializer.Serialize(Settings, JsonOptions));
            File.Move(temporaryPath, fullPath, overwrite: true);
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        { LastError = Tools.GetExcMsg(ex); return false; }
        finally
        {
            if (temporaryPath != null)
            {
                try { File.Delete(temporaryPath); }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { }
            }
        }
    }
}
