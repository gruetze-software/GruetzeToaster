using System.Text.Json;
using Xunit;

namespace GruetzeToaster.Tests;

public sealed class SettingsTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "GruetzeToaster.Tests-" + Guid.NewGuid().ToString("N"));
    private string ConfigPath => Path.Combine(_directory, "settings.json");
    public SettingsTests() => Directory.CreateDirectory(_directory);
    public void Dispose() => Directory.Delete(_directory, true);

    [Fact]
    public void MissingFileUsesDefaultsWithoutWriting()
    {
        var manager = new ConfigManager(ConfigPath);
        Assert.Equal(35, manager.Settings.ToasterCount);
        Assert.Null(manager.LastError);
        Assert.False(File.Exists(ConfigPath));
        Assert.True(manager.Settings.ShowC64);
        Assert.True(manager.Settings.ShowAmiga);
        Assert.True(manager.Settings.ShowToasters && manager.Settings.ShowToast && manager.Settings.ShowFloppies && manager.Settings.ShowLogo);
        Assert.False(manager.Settings.ShowStars);
    }

    [Theory]
    [InlineData("{invalid")]
    [InlineData("{\"SpeedMultiplier\": \"fast\"}")]
    public void CorruptFileIsPreservedAndReported(string json)
    {
        File.WriteAllText(ConfigPath, json);
        var manager = new ConfigManager(ConfigPath);
        Assert.NotNull(manager.LastError);
        Assert.Equal(35, manager.Settings.ToasterCount);
        Assert.Equal(json, File.ReadAllText(ConfigPath));
    }

    [Fact]
    public void LoadedValuesAreValidated()
    {
        File.WriteAllText(ConfigPath, "{\"ToasterCount\":2147483647,\"SpeedMultiplier\":-3}");
        var manager = new ConfigManager(ConfigPath);
        Assert.Equal(200, manager.Settings.ToasterCount);
        Assert.Equal(0.2, manager.Settings.SpeedMultiplier);
    }

    [Fact]
    public void NonFiniteSpeedAndCountAreNormalized()
    {
        var settings = new ToasterSettings { ToasterCount = -5, SpeedMultiplier = double.NaN };
        settings.Validate();
        Assert.Equal(1, settings.ToasterCount);
        Assert.Equal(1, settings.SpeedMultiplier);
    }

    [Fact]
    public void SaveRoundTripsAndLeavesNoTemporaryFiles()
    {
        var manager = new ConfigManager(ConfigPath);
        manager.Settings = new() { ToasterCount = 42, SpeedMultiplier = 2.5, ShowFps = true, ShowC64 = true, ShowAmiga = true,
            ShowToasters = false, ShowToast = false, ShowFloppies = false, ShowLogo = false, ShowStars = true };
        Assert.True(manager.SaveSettings(), manager.LastError);
        manager.Settings.ToasterCount = 43;
        Assert.True(manager.SaveSettings(), manager.LastError);
        var loaded = new ConfigManager(ConfigPath);
        Assert.Equal(43, loaded.Settings.ToasterCount);
        Assert.Equal(2.5, loaded.Settings.SpeedMultiplier);
        Assert.True(loaded.Settings.ShowFps);
        Assert.True(loaded.Settings.ShowC64);
        Assert.True(loaded.Settings.ShowAmiga);
        Assert.False(loaded.Settings.ShowToasters || loaded.Settings.ShowToast || loaded.Settings.ShowFloppies || loaded.Settings.ShowLogo);
        Assert.True(loaded.Settings.ShowStars);
        Assert.Single(Directory.GetFiles(_directory));
    }

    [Fact]
    public void FailedSaveIsReportedAndExistingFileIsPreserved()
    {
        File.WriteAllText(ConfigPath, "{}");
        using var locked = File.Open(ConfigPath, FileMode.Open, FileAccess.Read, FileShare.None);
        var manager = new ConfigManager(Path.Combine(ConfigPath, "settings.json"));
        Assert.False(manager.SaveSettings());
        Assert.NotNull(manager.LastError);
        Assert.Single(Directory.GetFiles(_directory));
    }
}
