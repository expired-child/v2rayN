using System.Windows.Media;

namespace v2rayN.Common;

/// <summary>Semantic surface colors for the desktop workspace.</summary>
public static class WorkspaceTheme
{
    public static void Apply(bool dark, Color? accent = null)
    {
        var colors = new Dictionary<string, string>
        {
            ["WorkspaceCanvas"] = dark ? "#111D2B" : "#EEF2F7",
            ["WorkspaceSidebar"] = dark ? "#182738" : "#E5EBF3",
            ["WorkspaceSurface"] = dark ? "#203144" : "#FFFFFF",
            ["WorkspaceSurfaceAlt"] = dark ? "#293C50" : "#F4F7FB",
            ["WorkspaceStroke"] = dark ? "#3B5269" : "#DBE2EB",
            ["WorkspaceText"] = dark ? "#EDF4FB" : "#19293B",
            ["WorkspaceMuted"] = dark ? "#B3C5D6" : "#52677D",
            ["WorkspaceAccent"] = dark ? "#80B3FF" : "#246DE8",
            ["WorkspaceAccentSoft"] = dark ? "#293F5A" : "#E8F0FF",
            ["WorkspaceAccentForeground"] = dark ? "#101D2C" : "#FFFFFF",
            ["DesignBackgroundPrimary"] = dark ? "#111D2B" : "#EEF2F7",
            ["DesignBackgroundSecondary"] = dark ? "#182738" : "#E5EBF3",
            ["DesignSurfacePrimary"] = dark ? "#203144" : "#FFFFFF",
            ["DesignSurfaceSecondary"] = dark ? "#293C50" : "#F4F7FB",
            ["DesignSurfaceElevated"] = dark ? "#293C50" : "#FFFFFF",
            ["DesignControlBackground"] = dark ? "#293C50" : "#F4F7FB",
            ["DesignTextPrimary"] = dark ? "#EDF4FB" : "#19293B",
            ["DesignTextSecondary"] = dark ? "#B3C5D6" : "#52677D",
            ["DesignTextTertiary"] = dark ? "#A9BDCF" : "#5D7187",
            ["DesignAccentPrimary"] = dark ? "#80B3FF" : "#246DE8",
            ["DesignAccentSubtle"] = dark ? "#293F5A" : "#E8F0FF",
            ["DesignBorderSubtle"] = dark ? "#3B5269" : "#DBE2EB",
            ["DesignSeparator"] = dark ? "#3B5269" : "#DBE2EB",
            ["DesignSuccess"] = dark ? "#81DFC0" : "#087A62",
            ["DesignWarning"] = dark ? "#FFC16B" : "#905200",
            ["DesignError"] = dark ? "#FF8580" : "#BA2525",
            ["DesignSelectedBackground"] = dark ? "#293F5A" : "#E8F0FF",
            ["DesignHoverBackground"] = dark ? "#30485F" : "#E6EDF7",
            ["DesignPressedBackground"] = dark ? "#3A5570" : "#D7E4F6",
            ["DesignDisabledForeground"] = dark ? "#9BAFC2" : "#66798D",
            ["DesignDisabledBackground"] = dark ? "#293C50" : "#E7EDF4",
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
            Application.Current.Resources["DesignAccentPrimary"] = new SolidColorBrush(color);
        }
    }
}
