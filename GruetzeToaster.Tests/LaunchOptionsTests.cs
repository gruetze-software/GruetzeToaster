using Xunit;

namespace GruetzeToaster.Tests;

public class LaunchOptionsTests
{
    [Theory]
    [InlineData("--settings", LaunchMode.Settings)]
    [InlineData("/C:123", LaunchMode.Settings)]
    [InlineData("/s", LaunchMode.Fullscreen)]
    [InlineData("--windowed", LaunchMode.Windowed)]
    [InlineData("--about", LaunchMode.About)]
    [InlineData("--help", LaunchMode.Help)]
    public void ParsesModes(string argument, LaunchMode mode)
    {
        Assert.True(LaunchOptions.TryParse([argument], true, out var options, out var error), error);
        Assert.Equal(mode, options.Mode);
    }

    [Theory]
    [InlineData("/p:123")]
    [InlineData("/P:123")]
    public void ParsesInlinePreview(string argument)
    {
        Assert.True(LaunchOptions.TryParse([argument], true, out var options, out _));
        Assert.Equal(LaunchMode.Preview, options.Mode);
        Assert.Equal(123, options.ParentHandle);
    }

    [Fact]
    public void ParsesSeparatePreview()
    {
        Assert.True(LaunchOptions.TryParse(["/p", "123"], true, out var options, out _));
        Assert.Equal(123, options.ParentHandle);
    }

    [Theory]
    [InlineData("")]
    [InlineData("-")]
    [InlineData("/p")]
    [InlineData("/p:")]
    [InlineData("/p:0")]
    [InlineData("/p:-1")]
    [InlineData("/p:999999999999999999999")]
    [InlineData("--unknown")]
    public void RejectsBadArgumentsWithoutThrowing(string argument) =>
        Assert.False(LaunchOptions.TryParse([argument], true, out _, out _));

    [Fact]
    public void RejectsPreviewOnOtherPlatforms() =>
        Assert.False(LaunchOptions.TryParse(["/p:123"], false, out _, out _));

    [Fact]
    public void RejectsConflictingModesAndBackends()
    {
        Assert.False(LaunchOptions.TryParse(["--about", "--settings"], true, out _, out _));
        Assert.False(LaunchOptions.TryParse(["--wayland", "--x11"], false, out _, out _));
    }

    [Fact]
    public void BackendCanAppearBeforeOrAfterMode()
    {
        foreach (var args in new[] { new[] { "--wayland", "--settings" }, new[] { "--settings", "--wayland" } })
        {
            Assert.True(LaunchOptions.TryParse(args, false, out var options, out _));
            Assert.Equal(LinuxBackend.Wayland, options.Backend);
            Assert.Equal(LaunchMode.Settings, options.Mode);
        }
    }

    [Fact]
    public void DefaultIsFullscreen() =>
        Assert.True(LaunchOptions.TryParse([], true, out var options, out _) && options.Mode == LaunchMode.Fullscreen);
}
