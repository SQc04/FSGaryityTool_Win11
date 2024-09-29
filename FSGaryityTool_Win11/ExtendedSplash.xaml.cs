using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;
using Microsoft.UI.Xaml.Media.Imaging;

namespace FSGaryityTool_Win11;

public sealed partial class ExtendedSplash : UserControl
{
    private Window mainWindow;

    private Rect splashImageRect;

    public ExtendedSplash(Window mainWindow)
    {
        InitializeComponent();
        this.mainWindow = mainWindow;

        // 设置初始屏幕图像的源
        ExtendedSplashImage.Source = new BitmapImage(new("ms-appx:///Assets/SplashScreen.scale-400.png"));

        // 将扩展初始屏幕图像定位在与系统初始屏幕图像相同的位置
        splashImageRect = new(0, 0, mainWindow.Bounds.Width, mainWindow.Bounds.Height);
        PositionImage();
    }

    // 定位图像
    private void PositionImage()
    {
        ExtendedSplashImage.SetValue(Canvas.LeftProperty, splashImageRect.X);
        ExtendedSplashImage.SetValue(Canvas.TopProperty, splashImageRect.Y);
        ExtendedSplashImage.Height = splashImageRect.Height;
        ExtendedSplashImage.Width = splashImageRect.Width;
    }

    // 当窗口大小更改时，更新初始屏幕图像的坐标
    public void ExtendedSplash_OnResize(object sender, WindowSizeChangedEventArgs e)
    {
        // 安全地更新扩展初始屏幕图像的坐标。此函数可能会在响应应用程序视图状态更改或窗口大小更改事件时被调用
        if (mainWindow is not null)
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
