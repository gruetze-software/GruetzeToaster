using System;

namespace GruetzeToaster;

// UI-independent motion allows deterministic frame-rate and respawn tests.
public sealed class AnimationState
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Speed { get; set; }
    public int Frame { get; set; }
    public bool Animated { get; set; } = true;
    public double FrameDurationSeconds { get; set; } = 0.12;
    private double _animationTime;

    public void Advance(double width, double height, double seconds, int frameCount, Random random)
    {
        if (!double.IsFinite(seconds) || seconds <= 0) return;
        X -= Speed * 120 * seconds;
        Y += Speed * 60 * seconds;
        if (Y > height + 150 || X < -200)
        {
            if (random.Next(2) == 0)
            { X = random.NextDouble() * (Math.Max(0, width) + 600); Y = -200; }
            else
            { X = width + 200; Y = -200 + random.NextDouble() * (Math.Max(0, height) + 200); }
        }
        if (!Animated || frameCount <= 0) return;
        _animationTime += seconds;
        double interval = double.IsFinite(FrameDurationSeconds) && FrameDurationSeconds > 0
            ? FrameDurationSeconds : 0.12;
        int steps = (int)Math.Floor((_animationTime + 1e-10) / interval);
        if (steps > 0)
        {
            Frame = (Frame + steps) % frameCount;
            _animationTime = Math.Max(0, _animationTime - steps * interval);
        }
    }
}
