using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml.Media.Imaging;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FSGaryityTool_Win11
{
    public sealed partial class ExtendedSplash : UserControl
    {
        private Window mainWindow;
        private Rect splashImageRect;

        public ExtendedSplash(Window mainWindow)
        {
            this.InitializeComponent();
            this.mainWindow = mainWindow;

            // 设置初始屏幕图像的源
            extendedSplashImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/SplashScreen.scale-400.png"));

            // 将扩展初始屏幕图像定位在与系统初始屏幕图像相同的位置
            splashImageRect = new Rect(0, 0, mainWindow.Bounds.Width, mainWindow.Bounds.Height);
            PositionImage();
        }


        // 定位图像
        void PositionImage()
        {
            extendedSplashImage.SetValue(Canvas.LeftProperty, splashImageRect.X);
            extendedSplashImage.SetValue(Canvas.TopProperty, splashImageRect.Y);
            extendedSplashImage.Height = splashImageRect.Height;
            extendedSplashImage.Width = splashImageRect.Width;
        }

        // 当窗口大小更改时，更新初始屏幕图像的坐标
        public void ExtendedSplash_OnResize(Object sender, WindowSizeChangedEventArgs e)
        {
            // 安全地更新扩展初始屏幕图像的坐标。此函数可能会在响应应用程序视图状态更改或窗口大小更改事件时被调用
            if (mainWindow != null)
            {
                // 更新初始屏幕图像的坐标
                PositionImage();
            }
        }

        // 公共方法，用来注册 SizeChanged 事件处理器
        public void RegisterSizeChangedEvent()
        {
            Window.Current.SizeChanged += ExtendedSplash_OnResize;
        }
    }
}

