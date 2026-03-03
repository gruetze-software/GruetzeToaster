using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace GruetzeToaster;

public partial class SettingsWindow : Window
{
    private readonly ConfigManager _config;
    private string _gitUrl = "https://github.com/gruetze-software/GruetzeToaster"; // Fallback

    public SettingsWindow() : this(new ConfigManager()) // Default-Manager für den normalen Start
    {  }

    public SettingsWindow(ConfigManager configManager)
    {
        InitializeComponent();
        _config = configManager;
        
        // WICHTIG: Die Slider-Werte setzen, BEVOR wir die Events abonnieren,
        // damit die Labels beim Öffnen direkt richtig ausgefüllt sind.
        LoadCurrentSettings();
        
        LoadAboutInfo();

        // Logo laden
        // Falls MainWindow.GetLogoBitmap() statisch ist, passt das so:
        LogoImage.Source = MainWindow.GetLogoBitmap(); 

        // Live-Update der Labels beim Schieben (Property-Check)
        CountSlider.PropertyChanged += (s, e) => { 
            if(e.Property.Name == nameof(Slider.Value)) 
                CountLabel.Text = $"{(int)CountSlider.Value}"; 
        };
        
        SpeedSlider.PropertyChanged += (s, e) => { 
            if(e.Property.Name == nameof(Slider.Value)) 
                SpeedLabel.Text = $"{SpeedSlider.Value:F1}x"; 
        };
    }

    private void LoadCurrentSettings()
    {
        // Hier nutzen wir die Namen aus deiner ConfigManager-Klasse
        _config.LoadSettings(); 
        CountSlider.Value = _config.Settings.ToasterCount;
        SpeedSlider.Value = _config.Settings.SpeedMultiplier;
        
        // Labels initial füllen
        CountLabel.Text = $"{(int)CountSlider.Value}";
        SpeedLabel.Text = $"{SpeedSlider.Value:F1}x";
    }

    private void GitLink_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start(new ProcessStartInfo(_gitUrl) { UseShellExecute = true });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", _gitUrl);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", _gitUrl);
            }
        }
        catch
        {
            // Falls das Öffnen des Browsers fehlschlägt, ignorieren wir es leise
        }
    }

    private void LoadAboutInfo()
    {
        var assembly = Assembly.GetExecutingAssembly();
        
        // Version auslesen
        var version = assembly.GetName().Version;
        VersionLabel.Text = $"v{version?.Major}.{version?.Minor}.{version?.Build}";

        // Copyright & Description aus den Attributen holen
        var copyright = assembly.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright;
        var description = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description;
        // Git-URL aus den Assembly-Metadaten auslesen
        var attributes = assembly.GetCustomAttributes<AssemblyMetadataAttribute>();
        var repoUrlAttribute = attributes.FirstOrDefault(a => a.Key == "RepositoryUrl");
        
        if (repoUrlAttribute != null && !string.IsNullOrEmpty(repoUrlAttribute.Value))
        {
            _gitUrl = repoUrlAttribute.Value;
        }

        if (copyright != null) CopyrightLabel.Text = copyright;
        if (description != null) DescriptionLabel.Text = description;
        
        // Falls du den Maintainer oder die URL auch anzeigen willst:
        // Diese liegen oft im AssemblyMetadataAttribute
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        _config.Settings.ToasterCount = (int)CountSlider.Value;
        _config.Settings.SpeedMultiplier = SpeedSlider.Value;
        _config.SaveSettings(); // Name korrigiert
        this.Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => this.Close();

    // Der versprochene RESET-Button
    private void Reset_Click(object sender, RoutedEventArgs e)
    {
        var defaults = new ToasterSettings();
        CountSlider.Value = defaults.ToasterCount;
        SpeedSlider.Value = defaults.SpeedMultiplier;
    }
}