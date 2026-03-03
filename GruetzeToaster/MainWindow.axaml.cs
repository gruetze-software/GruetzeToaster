using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;

namespace GruetzeToaster;

public partial class MainWindow : Window
{
    public bool IsPreviewMode { get; set; } = false;
    public IntPtr ParentWindowHandle { get; set; } = IntPtr.Zero;
    private ConfigManager _configManager = new ConfigManager();

    private List<FlyingObject> _objects = new();
    private IImage[] _toasterFrames = new IImage[4];
    private Bitmap _toasterSheet;
    private Bitmap _toast;
    private Bitmap _logo;
    private static Bitmap? _logogs = null; // Statischer Cache für das Logo, damit wir es in der Vorschau nutzen können
    private DateTime _lastTickTime = DateTime.Now;

    private int _frameCount = 0;
    private DateTime _lastFpsUpdate = DateTime.Now;
    private Point? _lastMousePos;

    public MainWindow() : this(false, IntPtr.Zero) // Standard-Konstruktor für normalen Start
    {  }

    public MainWindow(bool isPreview, IntPtr? parentHandle)
    {
        IsPreviewMode = isPreview;
        ParentWindowHandle = parentHandle ?? IntPtr.Zero;

        InitializeComponent();

        PointerMoved += (s, e) => 
        {
            if ( IsPreviewMode) return; // Im Vorschau-Modus ignorieren, damit die Mausbewegung die Anzeige nicht stört

            var currentPos = e.GetPosition(this);
            if (_lastMousePos == null) { _lastMousePos = currentPos; return; }

            // Distanz berechnen (Satz des Pythagoras)
            double dist = Math.Sqrt(Math.Pow(currentPos.X - _lastMousePos.Value.X, 2) + 
                                    Math.Pow(currentPos.Y - _lastMousePos.Value.Y, 2));

            if (dist > 10) Close(); // Erst bei 10 Pixel Bewegung schließen
        };

        // Event für Tastendrücke registrieren
        this.KeyDown += (sender, e) =>
        {
            // Nicht im Preview-Modus, damit wir die FPS-Anzeige nicht stören
            if (IsPreviewMode) return;

            // Wenn 'F' gedrückt wird (F wie Frames)
            if (e.Key == Avalonia.Input.Key.F)
            {
                // Schaltet zwischen True und False um
                FpsDisplay.IsVisible = !FpsDisplay.IsVisible;
            }
            
            // Bonus: Falls du mit ESC den Screensaver beenden willst
            if (e.Key == Avalonia.Input.Key.Escape)
            {
                this.Close();
            }
        };
        
        _toasterSheet = new Bitmap(AssetLoader.Open(new Uri("avares://GruetzeToaster/Assets/toaster-sprite.gif")));
        for (int i = 0; i < 4; i++)
        {       
            // Wir schneiden 64x64 Pixel große Stücke aus dem Sheet
            _toasterFrames[i] = new CroppedBitmap(_toasterSheet, new PixelRect(i * 64, 0, 64, 64));
        }

        _toast = new Bitmap(AssetLoader.Open(new Uri("avares://GruetzeToaster/Assets/toast1.gif")));
        _logo = new Bitmap(AssetLoader.Open(new Uri("avares://GruetzeToaster/Assets/logo.gif")));
        _logogs = new Bitmap(AssetLoader.Open(new Uri("avares://GruetzeToaster/Assets/logogs.png")));
        
        InitializeToasters();
                
        // Warte kurz, bis das Fenster bereit ist, dann starte den Loop
        Dispatcher.UIThread.Post(() => 
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel != null)
            {
                StartAnimationLoop(topLevel);
            }
        });
        
    }

    private void InitializeToasters()
    {
        var rand = new Random();
        var canvasWidth = this.Bounds.Width > 0 ? this.Bounds.Width : 1920;
        var canvasHeight = this.Bounds.Height > 0 ? this.Bounds.Height : 1080;
        // 1. Anzahl reduzieren: Im Preview reichen 10-15 Toaster völlig aus
        int countObj = IsPreviewMode ? 12 : _configManager.Settings.ToasterCount;
        // 2. BaseSize im Preview massiv senken (ca. 30%)
        // Das kompensiert den hohen depthFactor von bis zu 1.6
        double baseSize = IsPreviewMode ? 64 * 0.3 : 64;

        for (int i = 0; i < countObj; i++)
        {
            int typeRoll = rand.Next(0, 10);
            bool isLogo = (typeRoll == 0);
            bool isToast = (typeRoll == 1 || typeRoll == 2);

            // Der depthFactor bestimmt alles: 0.4 (hinten/klein) bis 1.6 (vorne/groß)
            double depthFactor = 0.4 + (rand.NextDouble() * 1.2);

            FlyingObject obj = new FlyingObject
            {
                // Initial-Verteilung über einen riesigen Bereich (auch weit außerhalb)
                Position = new Point(rand.Next(-500, (int)canvasWidth + 800), rand.Next(-800, (int)canvasHeight + 500)),
                Speed = 2.5 * depthFactor * _configManager.Settings.SpeedMultiplier, // Schnelle Objekte sind IMMER groß
                IsLogo = isLogo,
                IsToast = isToast,
                Frame = rand.Next(0, 4) // Zufälliger Start-Frame gegen "Gleichschritt"
            };

            // Größe basierend auf Tiefe
            double finalSize = baseSize * depthFactor;
            obj.DisplayImage.Width = finalSize;
            obj.DisplayImage.Height = finalSize;

            // Pixel-Art scharf halten (wichtig für C64 Look!)
            RenderOptions.SetBitmapInterpolationMode(obj.DisplayImage, BitmapInterpolationMode.None);

            // Z-Index: Vorne liegende Objekte verdecken hintere
            obj.DisplayImage.SetValue(Canvas.ZIndexProperty, (int)(depthFactor * 100));

            // Transparenz für Hintergrund-Objekte (Dunst-Effekt)
            if (depthFactor < 0.8) obj.DisplayImage.Opacity = 0.6;

            // Bild-Zuweisung
            if (isLogo) obj.DisplayImage.Source = _logo;
            else if (isToast) obj.DisplayImage.Source = _toast;
            else obj.DisplayImage.Source = _toasterFrames[obj.Frame];

            MainCanvas.Children.Add(obj.DisplayImage);
            _objects.Add(obj);
        }
    }

    private void StartAnimationLoop(TopLevel topLevel)
    {
        topLevel.RequestAnimationFrame(time =>
        {
            // 1. Zeitberechnung (DeltaTime)
            var now = DateTime.Now;
            double deltaTime = (now - _lastTickTime).TotalSeconds;
            
            // Schutz vor Rucklern nach einem System-Hänger
            if (deltaTime > 0.1) deltaTime = 0.016;
            _lastTickTime = now;

            // 2. Deine Toaster-Update Logik
            foreach (var obj in _objects) 
            {
                obj.Update(Bounds.Width, Bounds.Height, deltaTime, _toasterFrames);
            }

            // 3. FPS-Anzeige aktualisieren - NUR WENN SICHTBAR
            // Wir prüfen hier auf IsVisible, damit wir keine Rechenzeit verschwenden
            // und keinen Fehler bekommen, wenn die Anzeige aus ist.
            if (FpsDisplay != null && FpsDisplay.IsVisible)
            {
                _frameCount++;
                var elapsed = (now - _lastFpsUpdate).TotalSeconds;
                if (elapsed >= 0.5)
                {
                    FpsDisplay.Text = $"FPS: {(_frameCount / elapsed):F1}";
                    _frameCount = 0;
                    _lastFpsUpdate = now;
                }
            }

            // 4. DER WICHTIGE TEIL: Den nächsten Frame anfordern
            // Ohne diese Zeile würde die Animation nach einem Bild stehen bleiben.
            StartAnimationLoop(topLevel);
        });
    }

    // Windows spezifischer Code, um das Fenster in den Preview-Modus zu versetzen
    [DllImport("user32.dll")]
    private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    public static Avalonia.Media.Imaging.Bitmap? GetLogoBitmap()
    {
        // Falls das Logo noch nicht geladen wurde, laden wir es hier kurz nach
        if (_logogs == null)
        {
            // Passe den Pfad hier an deinen Asset-Pfad an!
            var assets = Avalonia.Platform.AssetLoader.Open(new Uri("avares://GruetzeToaster/Assets/logogs.png"));
            _logogs = new Avalonia.Media.Imaging.Bitmap(assets);
        }
        return _logogs;
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);

        if (IsPreviewMode && ParentWindowHandle != IntPtr.Zero)
        {
            var platformHandle = this.TryGetPlatformHandle();
            if (platformHandle != null)
            {
                // Win32 Magie: Das Fenster zum "Kind" des Windows-Dialogs machen
                SetWindowLong(platformHandle.Handle, -16, 0x40000000); // WS_CHILD
                SetParent(platformHandle.Handle, ParentWindowHandle);
                
                this.WindowState = WindowState.Normal;
                this.Position = new PixelPoint(0, 0);
                
                this.Width = 160; 
                this.Height = 120;
            }
        }
    }
}