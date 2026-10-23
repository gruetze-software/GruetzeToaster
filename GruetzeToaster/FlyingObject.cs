namespace GruetzeToaster;

// Simulation data only: no per-sprite controls, transforms or property notifications.
internal sealed class FlyingObject
{
    public AnimationState State { get; } = new() { Animated = false };
    public SpriteKind Kind { get; init; }
    public double Size { get; init; }
    public double Depth { get; init; }
    public double Opacity { get; init; }
    public double AnimationOffset { get; init; }
}
