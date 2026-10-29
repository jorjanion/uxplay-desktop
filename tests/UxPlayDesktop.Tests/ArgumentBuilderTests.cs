using System.Linq;
using UxPlayDesktop.Models;
using UxPlayDesktop.Services;
using Xunit;

namespace UxPlayDesktop.Tests;

public class ArgumentBuilderTests
{
    private readonly ArgumentBuilder _builder = new();

    [Fact]
    public void BuildArguments_TeachingPreset_ContainsExpectedArguments()
    {
        var settings = new AppSettings
        {
            ServerName = "Math Lecture Mirror",
            Preset = PerformancePreset.TeachingAndPresentation,
            DisplayWidth = 1920,
            DisplayHeight = 1080,
            TargetFps = 60,
            EnableH265 = true,
            InhibitSleepAndScreensaver = true
        };

        var args = _builder.BuildArguments(settings).ToList();

        Assert.Contains("-n", args);
        Assert.Contains("Math Lecture Mirror", args);
        Assert.Contains("-nh", args);
        Assert.Contains("-s", args);
        Assert.Contains("1920x1080", args);
        Assert.Contains("-fps", args);
        Assert.Contains("60", args);
        Assert.Contains("-vs", args);
        Assert.Contains("d3d11videosink", args);
        Assert.Contains("-h265", args);
        Assert.Contains("-scrsv", args);
        Assert.Contains("1", args);
    }

    [Fact]
    public void BuildArguments_AudioOnlyPreset_DisablesVideoSink()
    {
        var settings = new AppSettings
        {
            Preset = PerformancePreset.AudioOnly
        };

        var args = _builder.BuildArguments(settings).ToList();

        var vsIndex = args.IndexOf("-vs");
        Assert.True(vsIndex >= 0);
        Assert.Equal("0", args[vsIndex + 1]);
        Assert.DoesNotContain("-fs", args);
    }

    [Fact]
    public void BuildArguments_LowLatencyGamingPreset_AddsVsyncNo()
    {
        var settings = new AppSettings
        {
            Preset = PerformancePreset.LowLatencyGaming
        };

        var args = _builder.BuildArguments(settings).ToList();

        var vsyncIndex = args.IndexOf("-vsync");
        Assert.True(vsyncIndex >= 0);
        Assert.Equal("no", args[vsyncIndex + 1]);
    }

    [Fact]
    public void BuildArguments_FixedPinAuthentication_PassesPinValue()
    {
        var settings = new AppSettings
        {
            RequirePin = true,
            FixedPin = "4821"
        };

        var args = _builder.BuildArguments(settings).ToList();

        var pinIndex = args.IndexOf("-pin");
        Assert.True(pinIndex >= 0);
        Assert.Equal("4821", args[pinIndex + 1]);
    }

    [Fact]
    public void BuildArguments_AudioDelayCalibration_PassesDelayMs()
    {
        var settings = new AppSettings
        {
            EnableAudio = true,
            AudioDelayMs = 75
        };

        var args = _builder.BuildArguments(settings).ToList();

        var delayIndex = args.IndexOf("-delay");
        Assert.True(delayIndex >= 0);
        Assert.Equal("75", args[delayIndex + 1]);
    }

    [Fact]
    public void BuildCommandLineString_ProperlyQuotesArgumentsWithSpaces()
    {
        var settings = new AppSettings
        {
            ServerName = "Jorge's Living Room PC"
        };

        var cmdLine = _builder.BuildCommandLineString(settings);

        Assert.Contains("-n \"Jorge's Living Room PC\"", cmdLine);
    }

    [Fact]
    public void BuildArguments_EmptyServerName_FallsBackToMachineName()
    {
        var settings = new AppSettings
        {
            ServerName = "   "
        };

        var args = _builder.BuildArguments(settings).ToList();
        var nIndex = args.IndexOf("-n");
        Assert.True(nIndex >= 0);
        Assert.Equal(System.Environment.MachineName, args[nIndex + 1]);
    }

    [Fact]
    public void BuildArguments_NegativeAudioDelay_AppliesNegativeMs()
    {
        var settings = new AppSettings
        {
            AudioDelayMs = -120
        };

        var args = _builder.BuildArguments(settings).ToList();
        var delayIndex = args.IndexOf("-delay");
        Assert.True(delayIndex >= 0);
        Assert.Equal("-120", args[delayIndex + 1]);
    }
}
