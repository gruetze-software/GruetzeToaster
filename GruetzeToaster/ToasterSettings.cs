using System;
using System.IO;
using System.Text.Json;

namespace GruetzeToaster;

public class ToasterSettings
{
    public int ToasterCount { get; set; } = 35;
    public double SpeedMultiplier { get; set; } = 1.0;
    public bool ShowFps { get; set; } = false;
}

public class ConfigManager
{
    public ToasterSettings Settings { get; set; } = new ToasterSettings();
    
    // Statischer Pfad für den einfachen Zugriff
    private static readonly string ConfigPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
        "GruetzeToaster", "settings.json");

    public ConfigManager()
    {
        LoadSettings();
    }

    public void LoadSettings()
    {
        try 
        {
            if (File.Exists(ConfigPath)) 
            {
                string json = File.ReadAllText(ConfigPath);
                Settings = JsonSerializer.Deserialize<ToasterSettings>(json) ?? new ToasterSettings();
            }
        } 
        catch 
        { 
            Settings = new ToasterSettings(); 
        }
    }

    public void SaveSettings()
    {
        try 
        {
            string? directory = Path.GetDirectoryName(ConfigPath);
            if (directory != null) Directory.CreateDirectory(directory);

            string json = JsonSerializer.Serialize(Settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(ConfigPath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler beim Speichern: {ex.Message}");
        }
    }
}