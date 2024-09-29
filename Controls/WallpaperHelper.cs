using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FSGaryityTool_Win11.Controls
{
    public static class WallpaperHelper
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SystemParametersInfo(int uAction, int uParam, StringBuilder lpvParam, int fuWinIni);

        private const int SPI_GETDESKWALLPAPER = 0x0073;

        public static string GetWallpaperPath()
        {
            StringBuilder wallpaperPath = new StringBuilder(260);
            SystemParametersInfo(SPI_GETDESKWALLPAPER, wallpaperPath.Capacity, wallpaperPath, 0);
            return wallpaperPath.ToString();
        }
    }
}
