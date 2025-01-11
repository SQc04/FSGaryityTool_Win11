using Microsoft.UI;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using WinRT;
using WinRT.Interop;

namespace FSGaryityTool_Win11.Controls
{
    public class WindowBackgroundBrushControl
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_LAYERED = 0x80000;
        private const int LWA_COLORKEY = 0x1;
        private const int LWA_ALPHA = 0x2;


        public enum WindowBackgroundBrushKind
        {
            None,
            Mica,
            MicaAlt,
            AcrylicDesktop,
            AcrylicThin,
            AcrylicBase,
            Transparent
        }

        public static bool WindowBackgroundBrushActivatedEnable { get; set; }

        public enum WindowTheme
        {
            Light,
            Dark,
            System
        }
        
        private static DesktopAcrylicController acrylicController;
        private static MicaController micaController;
        private static SystemBackdropConfiguration configurationSource = new();

        public static Window window;
        public static AppWindow appWindow;

        public static WindowBackgroundBrushKind windowBackgroundBrushKind { get; set; }

        public static void SetWindowBackgroundBrush(WindowBackgroundBrushKind BackgroundBrushKind)
        {
            windowBackgroundBrushKind = BackgroundBrushKind;
            ApplyBackgroundBrush();
        }

        public static void SetWindowTheme(WindowTheme theme)
        {
            //currentTheme = theme;
            //ApplyTheme();
        }

        private static void ApplyBackgroundBrush()
        {
            var titleBar = appWindow.TitleBar; 
            var hwnd = WindowNative.GetWindowHandle(window);
            int extendedStyle;
            // 初始化或释放控制器
            switch (windowBackgroundBrushKind)
            {
                case WindowBackgroundBrushKind.Mica 
                or WindowBackgroundBrushKind.MicaAlt:
                    titleBar.BackgroundColor = null; 
                    extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
                    SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle & ~WS_EX_LAYERED);
                    micaController ??= new();
                    acrylicController?.Dispose();
                    acrylicController = null;
                    break;

                case WindowBackgroundBrushKind.AcrylicDesktop 
                or WindowBackgroundBrushKind.AcrylicThin 
                or WindowBackgroundBrushKind.AcrylicBase:
                    titleBar.BackgroundColor = null;
                    extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
                    SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle & ~WS_EX_LAYERED);
                    acrylicController ??= new();
                    micaController?.Dispose();
                    micaController = null;
                    break;

                case WindowBackgroundBrushKind.Transparent:
                    micaController?.Dispose();
                    acrylicController?.Dispose();
                    micaController = null;
                    acrylicController = null;
                    break;
                default:
                    titleBar.BackgroundColor = null;
                    extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
                    SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle & ~WS_EX_LAYERED);
                    micaController?.Dispose();
                    acrylicController?.Dispose();
                    micaController = null;
                    acrylicController = null;
                    break;
            }

            // 根据 windowBackgroundBrushKind 设置窗口背景
            switch (windowBackgroundBrushKind)
            {
                case WindowBackgroundBrushKind.None:
                    // 设置为无背景
                    break;
                case WindowBackgroundBrushKind.Mica:
                    // 设置为 Mica 背景
                    micaController.Kind = MicaKind.Base;
                    break;
                case WindowBackgroundBrushKind.MicaAlt:
                    // 设置为 MicaAlt 背景
                    micaController.Kind = MicaKind.BaseAlt;
                    break;
                case WindowBackgroundBrushKind.AcrylicDesktop:
                    // 设置为 AcrylicDesktop 背景
                    acrylicController.Kind = DesktopAcrylicKind.Default;
                    break;
                case WindowBackgroundBrushKind.AcrylicThin:
                    // 设置为 AcrylicThin 背景
                    acrylicController.Kind = DesktopAcrylicKind.Thin;
                    break;
                case WindowBackgroundBrushKind.AcrylicBase:
                    // 设置为 AcrylicBase 背景
                    acrylicController.Kind = DesktopAcrylicKind.Base;
                    break;
                case WindowBackgroundBrushKind.Transparent:
                    // 设置为透明背景
                    break;
            }

            ///*
            switch (windowBackgroundBrushKind)
            {
                case WindowBackgroundBrushKind.Mica
                or WindowBackgroundBrushKind.MicaAlt:
                    micaController.AddSystemBackdropTarget(window.As<Microsoft.UI.Composition.ICompositionSupportsSystemBackdrop>());
                    micaController.SetSystemBackdropConfiguration(configurationSource);
                    break;

                case WindowBackgroundBrushKind.AcrylicDesktop
                or WindowBackgroundBrushKind.AcrylicThin
                or WindowBackgroundBrushKind.AcrylicBase:
                    acrylicController.AddSystemBackdropTarget(window.As<Microsoft.UI.Composition.ICompositionSupportsSystemBackdrop>());
                    acrylicController.SetSystemBackdropConfiguration(configurationSource);
                    break;

                case WindowBackgroundBrushKind.Transparent:
                    //titleBar.ExtendsContentIntoTitleBar = true;
                    //titleBar.BackgroundColor = Colors.Transparent;
                    //titleBar.ButtonBackgroundColor = Colors.Transparent;
                    //titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
                    // 设置窗口背景为透明
                    extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
                    SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_LAYERED);
                    SetLayeredWindowAttributes(hwnd, 0, 0, LWA_ALPHA);
                    break;
                default:
                    
                    break;
            }
            //*/
        }

        public static void ActivatedBackgroundBrush(object sender, WindowActivatedEventArgs args)
        {
            if (!WindowBackgroundBrushActivatedEnable)
            {
                configurationSource.IsInputActive = args.WindowActivationState != WindowActivationState.Deactivated;
            }
        }
        public static void CloseBackgroundBrush(object sender, WindowEventArgs args)
        {
            // 释放m_acrylicController的资源，并将其设置为null
            if (acrylicController is not null)
            {
                acrylicController.Dispose();
                acrylicController = null;
            }
            // 释放m_micaController的资源，并将其设置为null
            if (micaController is not null)
            {
                micaController.Dispose();
                micaController = null;
            }
            configurationSource = null;
        }


        public static void ApplyTheme(FrameworkElement element, WindowTheme currentTheme)
        {
            if (element != null)
            {
                // 根据 currentTheme 设置窗口主题
                switch (currentTheme)
                {
                    case WindowTheme.Light:
                        // 设置为浅色主题
                        element.RequestedTheme = ElementTheme.Light;
                        break;
                    case WindowTheme.Dark:
                        // 设置为深色主题
                        element.RequestedTheme = ElementTheme.Dark;
                        break;
                    case WindowTheme.System:
                        // 设置为系统主题
                        element.RequestedTheme = ElementTheme.Default;
                        break;
                }
            }
        }

        public static void SetAppTitleBar(FrameworkElement element, WindowTheme currentTheme)
        {
            // 设置应用标题栏
            var titleBar = appWindow.TitleBar;
            var theme = element.ActualTheme;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            titleBar.ButtonHoverBackgroundColor = Color.FromArgb(64, 128, 128, 128);
            if (currentTheme == WindowTheme.System)
            {
                titleBar.ButtonForegroundColor = theme is ElementTheme.Dark ? Colors.White : Colors.Black;
            }
            else if(currentTheme == WindowTheme.Light)
            {
                titleBar.ButtonForegroundColor = Colors.Black;
            }
            else if (currentTheme == WindowTheme.Dark)
            {
                titleBar.ButtonForegroundColor = Colors.White;
            }
        }

    }
}
