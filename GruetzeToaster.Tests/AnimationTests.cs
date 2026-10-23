using Xunit;

namespace GruetzeToaster.Tests;

public class AnimationTests
{
    [Fact]
    public void MotionAndAnimationAreIndependentOfRefreshRate()
    {
        AnimationState Simulate(int hz)
        {
            var state = new AnimationState { X = 1000, Y = 100, Speed = 1 };
            var random = new Random(17);
            for (int i = 0; i < hz; i++) state.Advance(1920, 1080, 1.0 / hz, 4, random);
            return state;
        }
        var reference = Simulate(30);
        Assert.Equal(880, reference.X, 8);
        Assert.Equal(160, reference.Y, 8);
        Assert.Equal(0, reference.Frame);
        foreach (int hz in new[] { 60, 120, 144 })
        {
            var state = Simulate(hz);
            Assert.Equal(reference.X, state.X, 8);
            Assert.Equal(reference.Y, state.Y, 8);
            Assert.Equal(reference.Frame, state.Frame);
        }
    }

    [Fact]
    public void AnimationKeepsRemainderAndAdvancesMultipleFrames()
    {
        var state = new AnimationState();
        state.Advance(1920, 1080, 0.25, 4, new Random(1));
        Assert.Equal(2, state.Frame);
        state.Advance(1920, 1080, 0.11, 4, new Random(1));
        Assert.Equal(3, state.Frame);
    }

    [Fact]
    public void StaticSpritesDoNotChangeFrame()
    {
        var state = new AnimationState { Animated = false, Frame = 2 };
        state.Advance(1920, 1080, 0.5, 4, new Random(1));
        Assert.Equal(2, state.Frame);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void InvalidTimeDoesNotMoveObjects(double seconds)
    {
        var state = new AnimationState { X = 100, Y = 100, Speed = 1 };
        state.Advance(1920, 1080, seconds, 4, new Random(1));
        Assert.Equal(100, state.X);
        Assert.Equal(100, state.Y);
    }

    [Fact]
    public void RespawnUsesTopAndRightEdgesIncludingSmallPreviews()
    {
        bool top = false, right = false;
        for (int seed = 0; seed < 100; seed++)
        {
            var state = new AnimationState { X = -201 };
            state.Advance(160, 120, 0.01, 4, new Random(seed));
            top |= state.Y == -200;
            right |= state.X == 360;
            Assert.True(state.Y == -200 || state.X == 360);
            Assert.InRange(state.X, 0, 760);
            Assert.InRange(state.Y, -200, 120);
        }
        Assert.True(top && right);
    }
}
