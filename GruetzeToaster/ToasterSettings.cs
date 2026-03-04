using System;
using System.Diagnostics;
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
    private static string ConfigPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
        "GruetzeToaster", "settings.json");

    public ConfigManager()
    {
        ConfigPath = SetConfigPath();
        LoadSettings();
    }

    private static string SetConfigPath()
    {
        // Versuche den Standard-Pfad (~/.config unter Linux, %AppData% unter Windows)
        string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        // Fallback für WSL/Linux, falls SpecialFolder leer zurückkommt
        if (string.IsNullOrEmpty(appData))
        {
            // Nutzt $HOME/.config als Standard-Linux-Konvention
            appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config");
        }

        return Path.Combine(appData, "GruetzeToaster", "settings.json");
    }

    public void LoadSettings()
    {
        try 
        {
            if (File.Exists(ConfigPath)) 
            {
                Trace.WriteLine("");
                Trace.WriteLine($"Lade Einstellungen von: {ConfigPath}");
                string json = File.ReadAllText(ConfigPath);
                Settings = JsonSerializer.Deserialize<ToasterSettings>(json) ?? new ToasterSettings();
            }
            else
            {
                Trace.WriteLine("");
                Trace.WriteLine("Keine Einstellungen gefunden, verwende Standardwerte.");
                Settings = new ToasterSettings();
                SaveSettings(); // Speichern der Standardwerte für die Zukunft
            }
        } 
        catch (Exception ex) 
        {
            Trace.WriteLine($"Fehler beim Laden: {Tools.GetExcMsg(ex)}");
            Settings = new ToasterSettings(); 
        }
    }

    public void SaveSettings()
    {
        try 
        {
            string? directory = Path.GetDirectoryName(ConfigPath);
            if (directory != null) Directory.CreateDirectory(directory);
            Trace.WriteLine($"Speichere Einstellungen nach: {ConfigPath}");
            string json = JsonSerializer.Serialize(Settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(ConfigPath, json);
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"Fehler beim Speichern: {Tools.GetExcMsg(ex)}");
        }
    }
}