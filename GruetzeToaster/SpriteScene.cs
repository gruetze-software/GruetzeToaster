using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace GruetzeToaster;

// One retained visual for the entire scene. Updating positions does not invalidate layout.
public sealed class SpriteScene : Control
{
    private readonly List<FlyingObject> _objects = new();
    private FlyingObject[] _renderOrder = [];
    private Random _random = Random.Shared;
    internal int ObjectCount => _objects.Count;
    internal int CountKind(SpriteKind kind)
    {
        int count = 0;
        foreach (var sprite in _objects) if (sprite.Kind == kind) count++;
        return count;
    }
    internal double AnimationSeconds { get; private set; }

    public SpriteScene()
    {
        IsHitTestVisible = false;
        ClipToBounds = true;
        // Bilinear sampling allows smooth subpixel travel at fractional sprite scales.
        RenderOptions.SetBitmapInterpolationMode(this, BitmapInterpolationMode.LowQuality);
    }

    internal void Initialize(ToasterSettings settings, bool preview, double width, double height, Random random)
    {
        Clear();
        _random = random;
        SpriteAssets.Instance.PrepareScene();
        var pool = SpriteCatalog.GetSpawnPool(settings);
        int count = preview ? 12 : settings.ToasterCount;
        var kinds = new SpriteKind[count];
        for (int i = 0; i < count; i++) kinds[i] = pool[random.Next(pool.Count)];
        // A scene keeps its selected kinds on respawn, so a missing computer would never appear.
        // Reserve distinct slots only when the random selection omitted an enabled model.
        int reserved = 0;
        EnsureComputer(SpriteKind.C64, settings.ShowC64);
        EnsureComputer(SpriteKind.Amiga, settings.ShowAmiga);
        void EnsureComputer(SpriteKind kind, bool enabled)
        {
            if (!enabled || reserved >= count) return;
            int index = Array.IndexOf(kinds, kind, reserved);
            if (index < 0) kinds[reserved] = kind;
            else (kinds[reserved], kinds[index]) = (kinds[index], kinds[reserved]);
            reserved++;
        }
        for (int i = 0; i < count; i++)
        {
            SpriteKind kind = kinds[i];
            double depth = 0.4 + random.NextDouble() * 1.2;
            double x = random.NextDouble() * width;
            double y = random.NextDouble() * height;
            var sprite = new FlyingObject
            {
                Kind = kind,
                Depth = depth,
                Size = SpriteAssets.BaseSize(kind) * (preview ? 0.3 : 1) * depth,
                Opacity = depth < 0.8 ? 0.6 : 1,
                AnimationOffset = random.NextDouble() * 4
            };
            sprite.State.X = x;
            sprite.State.Y = y;
            sprite.State.Speed = 2.5 * depth * settings.SpeedMultiplier * (preview ? 0.3 : 1);
            _objects.Add(sprite);
        }
        _renderOrder = _objects.ToArray();
        Array.Sort(_renderOrder, (a, b) => a.Depth.CompareTo(b.Depth));
        InvalidateVisual();
    }

    internal void Advance(double width, double height, double seconds)
    {
        if (!double.IsFinite(seconds) || seconds <= 0) return;
        AnimationSeconds += seconds;
        for (int i = 0; i < _objects.Count; i++)
            _objects[i].State.Advance(width, height, seconds, 0, _random);
        InvalidateVisual();
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        if (_renderOrder.Length == 0) return;
        var assets = SpriteAssets.Instance;
        foreach (var sprite in _renderOrder)
        {
            var bounds = new Rect(sprite.State.X, sprite.State.Y, sprite.Size, sprite.Size);
            if (bounds.Right <= 0 || bounds.Bottom <= 0 || bounds.X >= Bounds.Width || bounds.Y >= Bounds.Height)
                continue;
            using (context.PushOpacity(sprite.Opacity))
                assets.Draw(context, sprite.Kind, bounds, AnimationSeconds + sprite.AnimationOffset);
        }
    }

    internal void Clear()
    {
        _objects.Clear();
        _renderOrder = [];
        AnimationSeconds = 0;
        InvalidateVisual();
    }
}
