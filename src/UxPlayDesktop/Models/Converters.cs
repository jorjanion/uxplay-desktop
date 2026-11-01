using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace UxPlayDesktop.Models;

public class PresetDisplayConverter : IValueConverter
{
    public static readonly PresetDisplayConverter Instance = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is PerformancePreset preset)
        {
            return preset.GetDisplayName();
        }
        return value?.ToString() ?? string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

public class ObjectToVisibilityConverter : IValueConverter
{
    public static readonly ObjectToVisibilityConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return Visibility.Collapsed;
        if (value is string s && string.IsNullOrWhiteSpace(s)) return Visibility.Collapsed;
        if (value is bool b && !b) return Visibility.Collapsed;
        return Visibility.Visible;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

public class InverseBoolToVisibilityConverter : IValueConverter
{
    public static readonly InverseBoolToVisibilityConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b && b) return Visibility.Collapsed;
        return Visibility.Visible;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

public class BoolToMuteIconConverter : IValueConverter
{
    public static readonly BoolToMuteIconConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b && b) return "🔇";
        return "🔊";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}

public class SeverityToColorConverter : IValueConverter
{
    public static readonly SeverityToColorConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Services.DiagnosticSeverity sev)
        {
            return sev switch
            {
                Services.DiagnosticSeverity.Pass => "#22C55E",
                Services.DiagnosticSeverity.Warning => "#F59E0B",
                Services.DiagnosticSeverity.Fail => "#EF4444",
                _ => "#94A3B8"
            };
        }
        return "#94A3B8";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}

public class SeverityToIconConverter : IValueConverter
{
    public static readonly SeverityToIconConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Services.DiagnosticSeverity sev)
        {
            return sev switch
            {
                Services.DiagnosticSeverity.Pass => "✅",
                Services.DiagnosticSeverity.Warning => "⚠️",
                Services.DiagnosticSeverity.Fail => "❌",
                _ => "ℹ️"
            };
        }
        return "ℹ️";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}
