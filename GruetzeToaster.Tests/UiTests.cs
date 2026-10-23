using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Interactivity;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Media;
using Avalonia.VisualTree;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]
[assembly: AvaloniaTestApplication(typeof(GruetzeToaster.Tests.HeadlessTestApp))]

namespace GruetzeToaster.Tests;

public static class HeadlessTestApp
{
    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>()
        .UseSkia().UseHarfBuzz().WithInterFont()
        .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false });
}

public class UiTests
{
    [AvaloniaFact]
    public void EnabledComputersCannotBeOmittedByRandomSelection()
    {
        var scene = new SpriteScene();
        foreach (int seed in Enumerable.Range(0, 50))
        {
            scene.Initialize(new() { ToasterCount = 2, ShowC64 = true, ShowAmiga = true },
                false, 960, 600, new Random(seed));
            Assert.Equal(2, scene.ObjectCount);
            Assert.Equal(1, scene.CountKind(SpriteKind.C64));
            Assert.Equal(1, scene.CountKind(SpriteKind.Amiga));
        }
        scene.Initialize(new() { ToasterCount = 35 }, false, 960, 600, new Random(42));
        Assert.Equal(0, scene.CountKind(SpriteKind.C64));
        Assert.Equal(0, scene.CountKind(SpriteKind.Amiga));
        scene.Clear();
    }

    [AvaloniaFact]
    public void SettingsLoadsAssetsAndReflectsSavedValues()
    {
        string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");
        var manager = new ConfigManager(path);
        manager.Settings = new() { ToasterCount = 47, SpeedMultiplier = 2.5, ShowFps = true, ShowC64 = true, ShowAmiga = true };
        var window = new SettingsWindow(manager);
        window.Show();
        Assert.Equal(47, window.FindControl<Slider>("CountSlider")!.Value);
        Assert.True(window.FindControl<CheckBox>("FpsCheckBox")!.IsChecked);
        Assert.Equal("2.5×", window.FindControl<TextBlock>("SpeedLabel")!.Text);
        Assert.True(window.FindControl<CheckBox>("C64CheckBox")!.IsChecked);
        Assert.True(window.FindControl<CheckBox>("AmigaCheckBox")!.IsChecked);
        Assert.NotNull(window.FindControl<Image>("LogoImage")!.Source);
        Capture(window, "settings");
        window.Close();
        Assert.False(File.Exists(path));
    }

    [AvaloniaFact]
    public void AboutContainsProductVersionAndLogo()
    {
        var window = new AboutWindow();
        window.Show();
        Assert.Equal(AppInfo.DisplayName, window.FindControl<TextBlock>("ProductName")!.Text);
        Assert.NotNull(window.FindControl<Image>("CompanyLogo")!.Source);
        Capture(window, "about");
        window.Close();
    }

    [AvaloniaFact]
    public void WindowedAnimationCreatesRequestedObjectsAndStopsOnClose()
    {
        var window = new MainWindow(false, IntPtr.Zero, true, new() { ToasterCount = 17, ShowFps = true, ShowC64 = true, ShowAmiga = true });
        window.Show();
        var scene = window.FindControl<SpriteScene>("Scene")!;
        Assert.Equal(17, scene.ObjectCount);
        Assert.Empty(scene.GetVisualChildren());
        Assert.True(window.FindControl<TextBlock>("FpsDisplay")!.IsVisible);
        Assert.Equal(WindowState.Normal, window.WindowState);
        Capture(window, "animation");
        window.Close();
        Assert.Equal(0, scene.ObjectCount);
    }

    [AvaloniaFact]
    public void WindowedModeIgnoresMouseAndClosesWithEscape()
    {
        var window = new MainWindow(false, IntPtr.Zero, true, new());
        window.Show();
        window.MouseMove(new Point(10, 10));
        window.MouseMove(new Point(200, 200));
        Assert.True(window.IsVisible);
        window.KeyPress(Key.Escape, RawInputModifiers.None, PhysicalKey.Escape, null);
        Assert.False(window.IsVisible);
    }

    [AvaloniaFact]
    public void FullscreenAllowsFpsToggleAndExitsOnOtherKeys()
    {
        var window = new MainWindow(false, IntPtr.Zero, settings: new());
        window.Show();
        window.KeyPress(Key.F, RawInputModifiers.None, PhysicalKey.F, "f");
        Assert.True(window.IsVisible);
        Assert.True(window.FindControl<TextBlock>("FpsDisplay")!.IsVisible);
        window.KeyPress(Key.Space, RawInputModifiers.None, PhysicalKey.Space, " ");
        Assert.False(window.IsVisible);
    }

    [AvaloniaFact]
    public void SaveFailureKeepsSettingsWindowOpen()
    {
        string directory = Path.Combine(Path.GetTempPath(), "GruetzeToaster.UiTests-" + Guid.NewGuid());
        Directory.CreateDirectory(directory);
        try
        {
            // A directory cannot be replaced with the configuration file.
            var window = new SettingsWindow(new ConfigManager(directory));
            window.Show();
            window.FindControl<Button>("SaveButton")!.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Assert.True(window.IsVisible);
            Assert.Contains("Could not save settings", window.FindControl<TextBlock>("StatusLabel")!.Text);
            window.Close();
        }
        finally { Directory.Delete(directory, true); }
    }

    private static void Capture(Window window, string name)
    {
        if (Environment.GetEnvironmentVariable("GRUETZETOASTER_SCREENSHOTS") is not { Length: > 0 } directory) return;
        Directory.CreateDirectory(directory);
        AvaloniaHeadlessPlatform.ForceRenderTimerTick();
        using var frame = window.CaptureRenderedFrame();
        Assert.NotNull(frame);
        frame.Save(Path.Combine(directory, name + ".png"), PngBitmapEncoderOptions.Default);
    }

    [AvaloniaFact]
    public void ContinuousAnimationsMoveWithoutRedrawingTheToasterBody()
    {
        var assets = SpriteAssets.Instance;
        assets.PrepareScene();
        byte[] RenderFrame(SpriteKind kind, double seconds, string name, bool bodyOnly = false, Rect? clip = null)
        {
            using var bitmap = new RenderTargetBitmap(new PixelSize(576, 576), new Vector(96, 96));
            using (var context = bitmap.CreateDrawingContext())
            {
                using (context.PushClip(clip ?? (bodyOnly ? new Rect(0, 240, 576, 336) : new Rect(0, 0, 576, 576))))
                    assets.Draw(context, kind, new Rect(0, 0, 576, 576), seconds);
            }
            using var stream = new MemoryStream();
            bitmap.Save(stream, PngBitmapEncoderOptions.Default);
            if (Environment.GetEnvironmentVariable("GRUETZETOASTER_SCREENSHOTS") is { Length: > 0 } directory)
            {
                Directory.CreateDirectory(directory);
                bitmap.Save(Path.Combine(directory, name + ".png"), PngBitmapEncoderOptions.Default);
            }
            return stream.ToArray();
        }

        Assert.False(RenderFrame(SpriteKind.C64, 0, "c64-unlit")
            .SequenceEqual(RenderFrame(SpriteKind.C64, 0.6, "c64-lit")));
        Assert.False(RenderFrame(SpriteKind.Amiga, 0, "amiga-ejected")
            .SequenceEqual(RenderFrame(SpriteKind.Amiga, 1.2, "amiga-inserted")));
        Assert.False(RenderFrame(SpriteKind.Amiga, 0.6, "amiga-inserting")
            .SequenceEqual(RenderFrame(SpriteKind.Amiga, 0.6 + 1.0 / 60, "amiga-next")));
        // The sliding disk must stay outside the slot rather than painting over the keyboard/case.
        Assert.Equal(RenderFrame(SpriteKind.Amiga, 0, "amiga-case-ejected", clip: new Rect(0, 0, 360, 576)),
            RenderFrame(SpriteKind.Amiga, 0.6, "amiga-case-inserting", clip: new Rect(0, 0, 360, 576)));
        Assert.False(RenderFrame(SpriteKind.Toaster, 0, "rotor-0")
            .SequenceEqual(RenderFrame(SpriteKind.Toaster, 1.0 / 60, "rotor-next")));
        RenderFrame(SpriteKind.Toast, 0, "toast-forward");
        Assert.Equal(RenderFrame(SpriteKind.Toaster, 0, "body-0", true),
            RenderFrame(SpriteKind.Toaster, 0.12, "body-next", true));
        Assert.InRange(assets.TextureBytes, 1, 2 * 1024 * 1024);
    }
}
