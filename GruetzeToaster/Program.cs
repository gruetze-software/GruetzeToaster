using Avalonia;
using Avalonia.Controls;
using System;
using System.Diagnostics;
using System.IO;

namespace GruetzeToaster;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    { 
        if (args.Length > 0)
            {
                // Wir nehmen die ersten zwei Zeichen (z.B. /p, /s, /c)
                string arg = args[0].ToLower().Trim().Substring(0, 2);

                if (arg == "/c")
                {
                    // EINSTELLUNGEN
                    BuildAvaloniaApp().Start(AppSettings, args);
                    return;
                }
                else if (arg == "/p")
                {
                    // VORSCHAU
                    // args[1] enthält das Handle des Windows-Vorschaufensters
                    BuildAvaloniaApp().Start(AppPreview, args);
                    return;
                }
            }

            // NORMALER START (Vollbild /s oder kein Argument)
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }


    private static void AppPreview(Application app, string[] args)
    {
        // Wir brauchen das Handle aus args[1]
        if (args.Length > 1 && long.TryParse(args[1], out long parentHandle))
        {
            var window = new MainWindow(true, new IntPtr(parentHandle));
             app.Run(window);
        }
    }

    // Diese Methode wird aufgerufen, wenn /c (Settings) genutzt wird
    private static void AppSettings(Application app, string[] args)
    {
        try
        {
            // 1. Initialisiere den ConfigManager (lädt automatisch die settings.json)
            var configManager = new ConfigManager();
            
            // 2. Erstelle das echte SettingsWindow und übergib den Manager
            var settingsWin = new SettingsWindow(configManager)
            {
                // Wir können hier noch Standard-Fenster-Eigenschaften setzen
                Title = "Grütze-Software Toaster Setup",
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                CanResize = false
            };

            // 3. Starte die App-Schleife mit dem Einstellungsfenster
            app.Run(settingsWin);
        }
        catch (Exception ex)
        {
            Trace.WriteLine( $"Fehler im Settings-Modus: {Tools.GetExcMsg(ex)}");
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .With(new Win32PlatformOptions
            {
                // Wir nutzen die modernen Namen für Windows 11
                RenderingMode = new[] { Win32RenderingMode.AngleEgl, Win32RenderingMode.Software }
            })
            .WithInterFont()
            .LogToTrace();
}
