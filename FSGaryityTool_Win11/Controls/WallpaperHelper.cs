using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Windows.UI;

namespace FSGaryityTool_Win11.Controls;

public static class WallpaperHelper
{
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern int SystemParametersInfo(int uAction, int uParam, StringBuilder lpvParam, int fuWinIni);

    private const int SPI_GETDESKWALLPAPER = 0x0073;

    public static string GetWallpaperPath()
    {
        var wallpaperPath = new StringBuilder(260);
        SystemParametersInfo(SPI_GETDESKWALLPAPER, wallpaperPath.Capacity, wallpaperPath, 0);
        return wallpaperPath.ToString();
    }

    public static Color? GetSolidColorBackground()
    {
        try
        {
            using (var key = Registry.CurrentUser.OpenSubKey("Control Panel\\Colors"))
            {
                if (key != null)
                {
                    var colorString = key.GetValue("Background") as string;
                    if (!string.IsNullOrEmpty(colorString))
                    {
                        var colorParts = colorString.Split(' ');
                        if (colorParts.Length == 3 &&
                            byte.TryParse(colorParts[0], out byte r) &&
                            byte.TryParse(colorParts[1], out byte g) &&
                            byte.TryParse(colorParts[2], out byte b))
                        {
                            return Color.FromArgb(255, r, g, b);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"获取纯色背景颜色时发生错误: {ex.Message}");
        }

        return null;
    }
}
