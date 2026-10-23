using Xunit;

namespace GruetzeToaster.Tests;

public class FrameStatisticsTests
{
    [Fact]
    public void RevealsSlowFramesHiddenByAverageFps()
    {
        var stats = new FrameStatistics();
        for (int i = 0; i < 94; i++) stats.Add(1.0 / 60);
        for (int i = 0; i < 6; i++) stats.Add(0.05);
        var snapshot = stats.Snapshot();
        Assert.Equal(50, snapshot.P95Ms, 6);
        Assert.Equal(50, snapshot.MaxMs, 6);
    }

    [Fact]
    public void OldStallsAgeOutAndResetClearsTheWindow()
    {
        var stats = new FrameStatistics();
        stats.Add(0.5);
        for (int i = 0; i < 120; i++) stats.Add(1.0 / 60);
        Assert.Equal(1000.0 / 60, stats.Snapshot().MaxMs, 6);
        stats.Reset();
        stats.Add(double.NaN);
        stats.Add(-1);
        Assert.Equal((0.0, 0.0), stats.Snapshot());
    }

    [Fact]
    public void DiskMotionIsContinuousAndReturnsToTheDrive()
    {
        Assert.Equal(0, SpriteAssets.DiskInsertion(0), 8);
        Assert.Equal(1, SpriteAssets.DiskInsertion(1.2), 8);
        Assert.Equal(0, SpriteAssets.DiskInsertion(2.4), 8);
        Assert.InRange(SpriteAssets.DiskInsertion(0.6 + 1.0 / 60) - SpriteAssets.DiskInsertion(0.6), 0.001, 0.025);
    }
}
