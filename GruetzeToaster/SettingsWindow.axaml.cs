using Avalonia.Controls;
using Avalonia.Interactivity;

namespace GruetzeToaster;

public partial class SettingsWindow : Window
{
    private readonly ConfigManager _config;
    public SettingsWindow() : this(new ConfigManager()) { }
    public SettingsWindow(ConfigManager configManager)
    {
        InitializeComponent();
        _config = configManager;
        LogoImage.Source = SpriteAssets.Instance.Logo;
        VersionLabel.Text = AppInfo.DisplayName;
        CountSlider.Value = _config.Settings.ToasterCount;
        SpeedSlider.Value = _config.Settings.SpeedMultiplier;
        FpsCheckBox.IsChecked = _config.Settings.ShowFps;
        C64CheckBox.IsChecked = _config.Settings.ShowC64;
        AmigaCheckBox.IsChecked = _config.Settings.ShowAmiga;
        UpdateLabels();
        if (_config.LastError != null) StatusLabel.Text = "Could not load settings. Default values are being used.";
        CountSlider.PropertyChanged += (_, e) => { if (e.Property.Name == nameof(Slider.Value)) UpdateLabels(); };
        SpeedSlider.PropertyChanged += (_, e) => { if (e.Property.Name == nameof(Slider.Value)) UpdateLabels(); };
    }

    private void UpdateLabels()
    {
        CountLabel.Text = $"{(int)CountSlider.Value}";
        SpeedLabel.Text = System.FormattableString.Invariant($"{SpeedSlider.Value:F1}×");
    }

    private void Save_Click(object? sender, RoutedEventArgs e)
    {
        _config.Settings.ToasterCount = (int)CountSlider.Value;
        _config.Settings.SpeedMultiplier = SpeedSlider.Value;
        _config.Settings.ShowFps = FpsCheckBox.IsChecked == true;
        _config.Settings.ShowC64 = C64CheckBox.IsChecked == true;
        _config.Settings.ShowAmiga = AmigaCheckBox.IsChecked == true;
        if (_config.SaveSettings()) Close();
        else StatusLabel.Text = $"Could not save settings: {_config.LastError}";
    }
    private void Cancel_Click(object? sender, RoutedEventArgs e) => Close();
    private async void About_Click(object? sender, RoutedEventArgs e) => await new AboutWindow().ShowDialog(this);
    private void Reset_Click(object? sender, RoutedEventArgs e)
    {
        var defaults = new ToasterSettings();
        CountSlider.Value = defaults.ToasterCount;
        SpeedSlider.Value = defaults.SpeedMultiplier;
        FpsCheckBox.IsChecked = defaults.ShowFps;
        C64CheckBox.IsChecked = defaults.ShowC64;
        AmigaCheckBox.IsChecked = defaults.ShowAmiga;
    }
}
