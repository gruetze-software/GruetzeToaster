using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace GruetzeToaster;

// Small shared textures only. No large source sheets or DrawingImage frame graphs survive preparation.
internal sealed class SpriteAssets : IDisposable
{
    private static SpriteAssets? _instance;
    public static SpriteAssets Instance => _instance ??= new();
    private readonly List<Bitmap> _bitmaps = new();
    private Bitmap? _toasterBody, _rotor, _toast, _c64Atlas, _amiga, _disk;
    private readonly StreamGeometry _diskOpening = CreateDiskOpening();
    public Bitmap Logo { get; }
    public Bitmap AppLogo { get; }
    public Bitmap CompanyLogo { get; }
    internal long TextureBytes
    {
        get
        {
            long bytes = 0;
            foreach (var bitmap in _bitmaps) bytes += (long)bitmap.PixelSize.Width * bitmap.PixelSize.Height * 4;
            return bytes;
        }
    }

    private SpriteAssets()
    {
        Logo = Own(Decode("logo.gif"));
        AppLogo = Own(Decode("app-logo.png", 256));
        CompanyLogo = Own(Decode("logogs.png", 128));
    }

    internal static double BaseSize(SpriteKind kind) => kind switch
    {
        SpriteKind.Toaster => 88, SpriteKind.Toast => 58, SpriteKind.Logo => 64,
        SpriteKind.C64 => 112, SpriteKind.Amiga => 144, SpriteKind.Floppy => 48, _ => 88
    };

    internal void PrepareScene()
    {
        if (_toasterBody != null) return;
        using (var sheet = Decode("Artwork/rotor-toaster.png", 512))
        {
            var body = new RenderTargetBitmap(new PixelSize(192, 192), new Vector(96, 96));
            using (var context = body.CreateDrawingContext())
            {
                // Keep one fixed body; discard the generated rotor and the other three varying bodies.
                using (context.PushClip(new Rect(0, 60, 192, 132)))
                    context.DrawImage(sheet, new Rect(0, 0, sheet.Size.Width / 2, sheet.Size.Height / 2),
                        new Rect(0, 0, 192, 192));
                using (context.PushTransform(Matrix.CreateScale(192.0 / 88, 192.0 / 88)))
                {
                    context.DrawRectangle(new SolidColorBrush(Color.Parse("#101E3F")), null, new Rect(43.2, 22, 4.5, 12));
                    context.DrawRectangle(new SolidColorBrush(Color.Parse("#148C9E")), null, new Rect(44.2, 23, 2.5, 10));
                }
            }
            _toasterBody = Own(body);
        }
        var rotor = new RenderTargetBitmap(new PixelSize(128, 16), new Vector(96, 96));
        using (var context = rotor.CreateDrawingContext())
        {
            context.DrawRectangle(new SolidColorBrush(Color.Parse("#101E3F")), null, new Rect(0, 0, 128, 16));
            context.DrawRectangle(new SolidColorBrush(Color.Parse("#FF8218")), null, new Rect(4, 3, 120, 10));
            context.DrawRectangle(new SolidColorBrush(Color.Parse("#FFBB55")), null, new Rect(4, 3, 120, 3));
        }
        _rotor = Own(rotor);
        _toast = Own(Decode("Artwork/golden-toast.png", 128));
        _amiga = Own(Decode("Artwork/wedge-computer.png", 256));
        _disk = Own(Decode("Artwork/floppy-disk.png", 64));
        using (var computer = Decode("Artwork/breadbox-computer.png", 256))
        {
            var atlas = new RenderTargetBitmap(new PixelSize(512, 256), new Vector(96, 96));
            using (var context = atlas.CreateDrawingContext())
            {
                context.DrawImage(computer, new Rect(0, 0, 256, 256));
                context.DrawImage(computer, new Rect(256, 0, 256, 256));
                using (context.PushTransform(Matrix.CreateScale(256.0 / 112, 256.0 / 112)))
                    context.DrawEllipse(new SolidColorBrush(Color.Parse("#49131C")), null, new Rect(93.1, 44.6, 4.4, 3.2));
                using (context.PushTransform(Matrix.CreateScale(256.0 / 112, 256.0 / 112) * Matrix.CreateTranslation(256, 0)))
                {
                    context.DrawEllipse(new SolidColorBrush(Color.Parse("#FF323D")), null, new Rect(93.1, 44.6, 4.4, 3.2));
                    context.DrawRectangle(new SolidColorBrush(Color.Parse("#FFE0D8")), null, new Rect(94.2, 45.0, 1.3, 1.1));
                }
            }
            _c64Atlas = Own(atlas);
        }
    }

    internal void Draw(DrawingContext context, SpriteKind kind, Rect bounds, double seconds)
    {
        if (_toasterBody == null) PrepareScene();
        double size = BaseSize(kind);
        double scale = bounds.Width / size;
        using (context.PushTransform(Matrix.CreateScale(scale, scale) * Matrix.CreateTranslation(bounds.X, bounds.Y)))
        {
            switch (kind)
            {
                case SpriteKind.Toaster:
                    context.DrawImage(_toasterBody!, new Rect(0, 0, 88, 88));
                    // Continuous rotation projected into the horizontal rotor plane.
                    using (context.PushTransform(Matrix.CreateRotation(seconds * (Math.PI * 2 / 0.9)) *
                        Matrix.CreateScale(1, 0.30) * Matrix.CreateTranslation(45.5, 23)))
                        context.DrawImage(_rotor!, new Rect(-30, -4, 60, 8));
                    break;
                case SpriteKind.Toast:
                    context.DrawImage(_toast!, new Rect(0, 0, 58, 58));
                    break;
                case SpriteKind.Logo:
                    context.DrawImage(Logo, new Rect(0, 0, 64, 64));
                    break;
                case SpriteKind.Floppy:
                    context.DrawImage(_disk!, new Rect(0, 0, 48, 48));
                    break;
                case SpriteKind.C64:
                    double sourceX = (long)(seconds / 0.55) % 2 == 0 ? 0 : 256;
                    context.DrawImage(_c64Atlas!, new Rect(sourceX, 0, 256, 256), new Rect(0, 0, 112, 112));
                    break;
                case SpriteKind.Amiga:
                    context.DrawImage(_amiga!, new Rect(0, 16, 112, 112));
                    double progress = DiskInsertion(seconds);
                    if (progress < 1)
                    {
                        // Match the side slot's diagonal and hide the inserted portion behind its lip.
                        // Map the disk's leading edge (23,11)..(59,23) into that slot;
                        // its trailing edge travels along the case's left/right axis.
                        using (context.PushGeometryClip(_diskOpening))
                        using (context.PushTransform(new Matrix(
                            96.0 / 1308, -267.0 / 1308, 1020.0 / 1308, 92.5 / 1308,
                            91.5 - 23 * (96.0 / 1308) - 11 * (1020.0 / 1308) - 22 * progress,
                            85.5 - 23 * (-267.0 / 1308) - 11 * (92.5 / 1308) - 6 * progress)))
                            context.DrawImage(_disk!, new Rect(0, 0, 64, 64));
                    }
                    break;
            }
        }
    }

    internal static double DiskInsertion(double seconds) =>
        0.5 - 0.5 * Math.Cos(seconds * (Math.PI * 2 / 2.4));

    private static StreamGeometry CreateDiskOpening()
    {
        var geometry = new StreamGeometry();
        using var path = geometry.Open();
        path.BeginFigure(new Point(91.5, 85.5), true);
        path.LineTo(new Point(103.5, 79));
        path.LineTo(new Point(147.5, 91));
        path.LineTo(new Point(135.5, 97.5));
        path.EndFigure(true);
        return geometry;
    }

    private Bitmap Own(Bitmap bitmap) { _bitmaps.Add(bitmap); return bitmap; }

    private static Bitmap Decode(string name, int? width = null)
    {
        using var stream = AssetLoader.Open(new Uri($"avares://gruetzetoaster/Assets/{name}"));
        return width.HasValue
            ? Bitmap.DecodeToWidth(stream, width.Value, BitmapInterpolationMode.HighQuality)
            : new Bitmap(stream);
    }

    public void Dispose()
    {
        foreach (var bitmap in _bitmaps) bitmap.Dispose();
        _bitmaps.Clear();
        _toasterBody = _rotor = _toast = _c64Atlas = _amiga = _disk = null;
        _instance = null;
    }

    public static void Release() => _instance?.Dispose();
}
