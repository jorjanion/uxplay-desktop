using UxPlayDesktop.Models;
using UxPlayDesktop.Services;
using Xunit;

namespace UxPlayDesktop.Tests;

public class LogParserTests
{
    private readonly LogParser _parser = new();

    [Fact]
    public void ParseLine_StreamingEvent_SetsStateStreaming()
    {
        var line = "Begin streaming to GStreamer video pipeline";
        var result = _parser.ParseLine(line);

        Assert.Equal(ServerState.Streaming, result.NewState);
        Assert.Equal(LogLevel.Info, result.LogEntry.Level);
    }

    [Fact]
    public void ParseLine_ClientConnected_ExtractsClientIpAndState()
    {
        var line = "raop: AirPlay client connected: 192.168.1.150";
        var result = _parser.ParseLine(line);

        Assert.Equal("192.168.1.150", result.DetectedClientIp);
        Assert.Equal(ServerState.Connecting, result.NewState);
    }

    [Fact]
    public void ParseLine_CodecAndGeometry_ExtractsFormatAndResolution()
    {
        var line = "*** codec is h264: geometry is (w,h)=(1920,1080) (source); (x,y)=(0,0); (w,h)=(1920,1080)";
        var result = _parser.ParseLine(line);

        Assert.Equal("H.264", result.DetectedCodec);
        Assert.Equal("1920x1080", result.DetectedResolution);
    }

    [Fact]
    public void ParseLine_H265CodecAndGeometry_ExtractsHEVC()
    {
        var line = "*** codec is h265: geometry is (w,h)=(3840,2160)";
        var result = _parser.ParseLine(line);

        Assert.Equal("H.265 (HEVC)", result.DetectedCodec);
        Assert.Equal("3840x2160", result.DetectedResolution);
    }

    [Fact]
    public void ParseLine_PinRequest_ExtractsPinCode()
    {
        var line = "create_pin_display: AirPlay PIN is: 8492";
        var result = _parser.ParseLine(line);

        Assert.Equal("8492", result.DetectedPin);
    }

    [Fact]
    public void ParseLine_DeviceModel_MapsToFriendlyName()
    {
        var line = "AirPlay device model: iPad13,8 registered";
        var result = _parser.ParseLine(line);

        Assert.NotNull(result.DetectedClientName);
        Assert.Contains("iPad", result.DetectedClientName);
    }

    [Fact]
    public void ParseLine_ClientDisconnect_SetsDisconnectedFlag()
    {
        var line = "video_reset: type = RTP_Shutdown";
        var result = _parser.ParseLine(line);

        Assert.True(result.ClientDisconnected);
        Assert.Equal(ServerState.Listening, result.NewState);
    }

    [Fact]
    public void ParseLine_ErrorLevel_DetectsError()
    {
        var line = "*** ERROR: Failed to bind to socket port 7000";
        var result = _parser.ParseLine(line);

        Assert.Equal(LogLevel.Error, result.LogEntry.Level);
    }

    [Fact]
    public void ParseLine_WarningLevel_DetectsWarning()
    {
        var line = "*** WARNING: Windows display power request could not be set";
        var result = _parser.ParseLine(line);

        Assert.Equal(LogLevel.Warning, result.LogEntry.Level);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void ParseLine_EmptyOrNull_DoesNotThrow(string? input)
    {
        var result = _parser.ParseLine(input!);
        Assert.NotNull(result);
        Assert.NotNull(result.LogEntry);
    }

    [Fact]
    public void MapDeviceModel_UnknownModel_ReturnsRawString()
    {
        var model = "CustomAndroidSimulator";
        var result = LogParser.MapDeviceModel(model);
        Assert.Equal("CustomAndroidSimulator", result);
    }
}
