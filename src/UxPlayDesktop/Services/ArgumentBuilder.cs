using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UxPlayDesktop.Models;

namespace UxPlayDesktop.Services;

public interface IArgumentBuilder
{
    IReadOnlyList<string> BuildArguments(AppSettings settings, string? bleStatusPath = null);
    string BuildCommandLineString(AppSettings settings, string? bleStatusPath = null);
}

public class ArgumentBuilder : IArgumentBuilder
{
    public IReadOnlyList<string> BuildArguments(AppSettings settings, string? bleStatusPath = null)
    {
        var args = new List<string>();

        // Server display name (airplay device name)
        var serverName = string.IsNullOrWhiteSpace(settings.ServerName)
            ? Environment.MachineName
            : settings.ServerName.Trim();
        args.Add("-n");
        args.Add(serverName);

        // Do not hold connection if new device attempts to connect (-nh)
        args.Add("-nh");

        // Apply Preset specific configurations
        switch (settings.Preset)
        {
            case PerformancePreset.TeachingAndPresentation:
                // Optimized for crisp static text, 1080p, 60fps, D3D11 sink, screensaver inhibitor
                args.Add("-s");
                args.Add($"{settings.DisplayWidth}x{settings.DisplayHeight}");
                args.Add("-fps");
                args.Add($"{settings.TargetFps}");
                args.Add("-vs");
                args.Add(string.IsNullOrWhiteSpace(settings.VideoSink) ? "d3d11videosink" : settings.VideoSink);
                if (settings.EnableH265) args.Add("-h265");
                break;

            case PerformancePreset.LowLatencyGaming:
                // Low latency, 60fps, sync=false
                args.Add("-s");
                args.Add($"{settings.DisplayWidth}x{settings.DisplayHeight}");
                args.Add("-fps");
                args.Add("60");
                args.Add("-vsync");
                args.Add("no");
                args.Add("-vs");
                args.Add(string.IsNullOrWhiteSpace(settings.VideoSink) ? "d3d11videosink" : settings.VideoSink);
                break;

            case PerformancePreset.HighQualityCinema:
                // 4K capable, H.265 decode enabled
                args.Add("-s");
                args.Add($"{Math.Max(settings.DisplayWidth, 3840)}x{Math.Max(settings.DisplayHeight, 2160)}");
                args.Add("-fps");
                args.Add("60");
                args.Add("-h265");
                args.Add("-vs");
                args.Add(string.IsNullOrWhiteSpace(settings.VideoSink) ? "d3d11videosink" : settings.VideoSink);
                break;

            case PerformancePreset.AudioOnly:
                // Audio only: disable video sink (-vs 0)
                args.Add("-vs");
                args.Add("0");
                break;

            case PerformancePreset.Custom:
                args.Add("-s");
                args.Add($"{settings.DisplayWidth}x{settings.DisplayHeight}");
                args.Add("-fps");
                args.Add($"{settings.TargetFps}");
                if (!string.IsNullOrWhiteSpace(settings.VideoSink))
                {
                    args.Add("-vs");
                    args.Add(settings.VideoSink);
                }
                if (settings.EnableH265) args.Add("-h265");
                break;
        }

        // Fullscreen
        if (settings.ForceFullscreen && settings.Preset != PerformancePreset.AudioOnly)
        {
            args.Add("-fs");
        }

        // Audio
        if (!settings.EnableAudio)
        {
            args.Add("-as");
            args.Add("0");
        }
        else
        {
            if (settings.Preset != PerformancePreset.LowLatencyGaming && settings.AudioDelayMs != 0)
            {
                var clampedDelay = Math.Clamp(settings.AudioDelayMs, -999, 999);
                args.Add("-vsync");
                args.Add(clampedDelay.ToString());
            }
            if (!string.IsNullOrWhiteSpace(settings.AudioSink) && settings.AudioSink != "autoaudiosink")
            {
                args.Add("-as");
                args.Add(settings.AudioSink);
            }
        }

        // Screensaver / Display Sleep Inhibition
        if (settings.InhibitSleepAndScreensaver && settings.Preset != PerformancePreset.AudioOnly)
        {
            args.Add("-scrsv");
            args.Add("1"); // Inhibit while actively mirroring
        }

        // Security / Pairing
        if (settings.RequirePin)
        {
            args.Add("-pin");
            if (!string.IsNullOrWhiteSpace(settings.FixedPin) &&
                settings.FixedPin.Length == 4 &&
                int.TryParse(settings.FixedPin, out var pinVal) &&
                pinVal >= 1 && pinVal <= 9999)
            {
                args.Add(settings.FixedPin);
            }
        }
        else if (!string.IsNullOrWhiteSpace(settings.Password))
        {
            args.Add("-pw");
            args.Add(settings.Password);
        }

        // Network port
        if (settings.NetworkPort > 0)
        {
            args.Add("-p");
            args.Add(settings.NetworkPort.ToString());
        }

        // Bluetooth Low Energy Beacon
        if (settings.EnableBluetoothBeacon && !string.IsNullOrWhiteSpace(bleStatusPath))
        {
            args.Add("-ble");
            args.Add(bleStatusPath);
        }

        // Additional user arguments
        if (!string.IsNullOrWhiteSpace(settings.AdditionalArguments))
        {
            var parsed = SplitCommandLine(settings.AdditionalArguments);
            args.AddRange(parsed);
        }

        return args.AsReadOnly();
    }

    public string BuildCommandLineString(AppSettings settings, string? bleStatusPath = null)
    {
        var args = BuildArguments(settings, bleStatusPath);
        var sb = new StringBuilder();
        foreach (var arg in args)
        {
            if (sb.Length > 0) sb.Append(' ');
            if (arg.Contains(' ') || arg.Contains('"'))
            {
                sb.Append('"').Append(arg.Replace("\"", "\\\"")).Append('"');
            }
            else
            {
                sb.Append(arg);
            }
        }
        return sb.ToString();
    }

    public static IReadOnlyList<string> SplitCommandLine(string commandLine)
    {
        var list = new List<string>();
        if (string.IsNullOrWhiteSpace(commandLine)) return list;

        var sb = new StringBuilder();
        bool inQuotes = false;
        bool escapeNext = false;

        for (int i = 0; i < commandLine.Length; i++)
        {
            char c = commandLine[i];
            if (escapeNext)
            {
                sb.Append(c);
                escapeNext = false;
            }
            else if (c == '\\')
            {
                if (inQuotes && i + 1 < commandLine.Length && commandLine[i + 1] == '"')
                {
                    escapeNext = true;
                }
                else
                {
                    sb.Append(c);
                }
            }
            else if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (char.IsWhiteSpace(c) && !inQuotes)
            {
                if (sb.Length > 0)
                {
                    list.Add(sb.ToString());
                    sb.Clear();
                }
            }
            else
            {
                sb.Append(c);
            }
        }

        if (sb.Length > 0)
        {
            list.Add(sb.ToString());
        }

        return list;
    }
}
