using Xunit;

namespace GruetzeToaster.Tests;

public class SpriteCatalogTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void OptionalComputersAreIndependentlySelectable(bool c64, bool amiga)
    {
        var pool = SpriteCatalog.GetSpawnPool(new() { ShowC64 = c64, ShowAmiga = amiga });
        Assert.Equal(c64, pool.Contains(SpriteKind.C64));
        Assert.Equal(amiga, pool.Contains(SpriteKind.Amiga));
        Assert.Contains(SpriteKind.Toaster, pool);
        Assert.Contains(SpriteKind.Toast, pool);
        Assert.Contains(SpriteKind.Floppy, pool);
        Assert.Contains(SpriteKind.Logo, pool);
    }

    [Fact]
    public void ComputersAreRareAndDisksAreAsCommonAsToast()
    {
        var pool = SpriteCatalog.GetSpawnPool(new() { ShowC64 = true, ShowAmiga = true });
        int computers = pool.Count(kind => kind is SpriteKind.C64 or SpriteKind.Amiga);
        Assert.True((double)computers / pool.Count < 0.10);
        Assert.Equal(pool.Count(kind => kind == SpriteKind.Toast), pool.Count(kind => kind == SpriteKind.Floppy));
    }

    [Fact]
    public void DifferentAnimationRatesRemainRefreshRateIndependent()
    {
        foreach (double interval in new[] { 0.12, 0.16, 0.55 })
        {
            int FrameAt(int hz)
            {
                var state = new AnimationState { FrameDurationSeconds = interval };
                var random = new Random(1);
                for (int i = 0; i < hz * 3; i++) state.Advance(1920, 1080, 1.0 / hz, 12, random);
                return state.Frame;
            }
            Assert.Equal(FrameAt(30), FrameAt(144));
        }
    }
}
