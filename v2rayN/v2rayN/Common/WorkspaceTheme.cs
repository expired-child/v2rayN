using System.Windows.Media;

namespace v2rayN.Common;

/// <summary>Semantic surface colors for the desktop workspace.</summary>
public static class WorkspaceTheme
{
    public static void Apply(bool dark, Color? accent = null)
    {
        var colors = new Dictionary<string, string>
        {
            ["WorkspaceCanvas"] = dark ? "#12141B" : "#F5F6FA",
            ["WorkspaceSidebar"] = dark ? "#181B24" : "#ECEEF4",
            ["WorkspaceSurface"] = dark ? "#1E222D" : "#FFFFFF",
            ["WorkspaceSurfaceAlt"] = dark ? "#252A37" : "#F7F8FC",
            ["WorkspaceStroke"] = dark ? "#303646" : "#E5E8F0",
            ["WorkspaceText"] = dark ? "#EDF0F8" : "#222838",
            ["WorkspaceMuted"] = dark ? "#9AA5BC" : "#727C91",
            ["WorkspaceAccent"] = dark ? "#78A5FF" : "#3268E8",
            ["WorkspaceAccentSoft"] = dark ? "#293A5B" : "#E7EEFF",
            ["WorkspaceAccentForeground"] = dark ? "#111827" : "#FFFFFF",
        };
        foreach (var (key, value) in colors)
        {
            var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(value));
            brush.Freeze();
            Application.Current.Resources[key] = brush;
        }
        if (accent is { } color)
        {
            static double Linear(byte value)
            {
                var channel = value / 255d;
                return channel <= 0.04045 ? channel / 12.92 : Math.Pow((channel + 0.055) / 1.055, 2.4);
            }
            var luminance = 0.2126 * Linear(color.R) + 0.7152 * Linear(color.G) + 0.0722 * Linear(color.B);
            Application.Current.Resources["WorkspaceAccent"] = new SolidColorBrush(color);
            Application.Current.Resources["WorkspaceAccentForeground"] = new SolidColorBrush(luminance > 0.179 ? Colors.Black : Colors.White);
            var surface = ((SolidColorBrush)Application.Current.Resources["WorkspaceCanvas"]).Color;
            var weight = dark ? 0.22 : 0.10;
            Application.Current.Resources["WorkspaceAccentSoft"] = new SolidColorBrush(Color.FromRgb(
                (byte)(surface.R * (1 - weight) + color.R * weight),
                (byte)(surface.G * (1 - weight) + color.G * weight),
                (byte)(surface.B * (1 - weight) + color.B * weight)));
        }
    }
}
