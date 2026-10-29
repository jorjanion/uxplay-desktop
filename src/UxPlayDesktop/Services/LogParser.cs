using System;
using System.Text.RegularExpressions;
using UxPlayDesktop.Models;

namespace UxPlayDesktop.Services;

public class LogParseResult
{
    public LogEntry LogEntry { get; }
    public ServerState? NewState { get; set; }
    public string? DetectedClientIp { get; set; }
    public string? DetectedClientName { get; set; }
    public string? DetectedResolution { get; set; }
    public string? DetectedCodec { get; set; }
    public double? DetectedFps { get; set; }
    public string? DetectedPin { get; set; }
    public bool ClientDisconnected { get; set; }

    public LogParseResult(LogEntry entry)
    {
        LogEntry = entry;
    }
}

public interface ILogParser
{
    LogParseResult ParseLine(string rawLine);
}

public class LogParser : ILogParser
{
    private static readonly Regex PinRegex = new(@"pin(?:\s+is)?\s*[:=]?\s*([0-9]{4})", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex ClientIpRegex = new(@"(?:connected from|client connected|from client|connection from)(?::|\s)+\s*([0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3})", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex GeometryRegex = new(@"codec is\s+(h26[45]|jpeg).*?\(w,h\)=\(([0-9]+),([0-9]+)\)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex FpsRegex = new(@"([0-9]+(?:\.[0-9]+)?)\s*fps", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex DeviceModelRegex = new(@"model\s*[:=]\s*([A-Za-z0-9,]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public LogParseResult ParseLine(string rawLine)
    {
        if (rawLine == null) rawLine = string.Empty;
        var trimmed = rawLine.Trim();

        var level = LogLevel.Info;
        if (trimmed.StartsWith("*** ERROR", StringComparison.OrdinalIgnoreCase))
        {
            level = LogLevel.Error;
        }
        else if (trimmed.StartsWith("*** WARNING", StringComparison.OrdinalIgnoreCase))
        {
            level = LogLevel.Warning;
        }
        else if (trimmed.IndexOf("DEBUG", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            level = LogLevel.Debug;
        }

        var entry = new LogEntry
        {
            Level = level,
            Message = trimmed
        };

        var result = new LogParseResult(entry);

        // State detection
        if (trimmed.Contains("Begin streaming to GStreamer video pipeline", StringComparison.OrdinalIgnoreCase))
        {
            result.NewState = ServerState.Streaming;
        }
        else if (trimmed.Contains("AirPlay server started", StringComparison.OrdinalIgnoreCase) ||
                 trimmed.Contains("Initialized AirPlay server", StringComparison.OrdinalIgnoreCase) ||
                 trimmed.Contains("Listening for connections", StringComparison.OrdinalIgnoreCase))
        {
            result.NewState = ServerState.Listening;
        }
        else if (trimmed.Contains("Stopping RAOP Server", StringComparison.OrdinalIgnoreCase) ||
                 trimmed.Contains("video_reset: type = RTP_Shutdown", StringComparison.OrdinalIgnoreCase))
        {
            result.NewState = ServerState.Listening;
            result.ClientDisconnected = true;
        }

        // IP detection
        var ipMatch = ClientIpRegex.Match(trimmed);
        if (ipMatch.Success)
        {
            result.DetectedClientIp = ipMatch.Groups[1].Value;
            if (result.NewState == null)
            {
                result.NewState = ServerState.Connecting;
            }
        }

        // Codec & Resolution detection
        var geomMatch = GeometryRegex.Match(trimmed);
        if (geomMatch.Success)
        {
            var codec = geomMatch.Groups[1].Value.ToUpperInvariant();
            var width = geomMatch.Groups[2].Value;
            var height = geomMatch.Groups[3].Value;
            result.DetectedCodec = codec == "H265" ? "H.265 (HEVC)" : (codec == "H264" ? "H.264" : codec);
            result.DetectedResolution = $"{width}x{height}";
        }

        // PIN detection
        var pinMatch = PinRegex.Match(trimmed);
        if (pinMatch.Success)
        {
            result.DetectedPin = pinMatch.Groups[1].Value;
        }

        // FPS detection
        var fpsMatch = FpsRegex.Match(trimmed);
        if (fpsMatch.Success && double.TryParse(fpsMatch.Groups[1].Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var fps))
        {
            result.DetectedFps = fps;
        }

        // Model detection
        var modelMatch = DeviceModelRegex.Match(trimmed);
        if (modelMatch.Success)
        {
            result.DetectedClientName = MapDeviceModel(modelMatch.Groups[1].Value);
        }

        return result;
    }

    public static string MapDeviceModel(string rawModel)
    {
        if (string.IsNullOrWhiteSpace(rawModel)) return "Apple Device";

        // Map common Apple internal identifiers to friendly names
        if (rawModel.StartsWith("iPad", StringComparison.OrdinalIgnoreCase))
        {
            return $"Apple iPad ({rawModel})";
        }
        if (rawModel.StartsWith("iPhone", StringComparison.OrdinalIgnoreCase))
        {
            return $"Apple iPhone ({rawModel})";
        }
        if (rawModel.StartsWith("MacBook", StringComparison.OrdinalIgnoreCase) ||
            rawModel.StartsWith("Mac", StringComparison.OrdinalIgnoreCase))
        {
            return $"Apple Mac ({rawModel})";
        }

        return rawModel;
    }
}
