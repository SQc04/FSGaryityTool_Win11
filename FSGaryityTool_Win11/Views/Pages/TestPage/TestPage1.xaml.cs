using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Hosting;
using System;
using System.Numerics;
using Windows.Foundation;
using Windows.Graphics;
using Windows.UI;

namespace FSGaryityTool_Win11.Views.Pages.TestPage;

public sealed partial class TestPage1 : Page
{
    private DispatcherTimer _timer;
    private int _currentStep;
    private Random _random = new Random();

    public TestPage1()
    {
        InitializeComponent();
        SetLedBoardColors();
        // 初始化定时器
        _timer = new DispatcherTimer();
        _timer.Interval = TimeSpan.FromMilliseconds(100);
        _timer.Tick += Timer_Tick;
        _timer.Start();

        //Uri uri1 = new(uriCopilot + launage + darkschemeovr);
        //Webview1.Source = uri1;
        /*
        Uri uri2 = new Uri("https://live.bilibili.com/22409163");
        Uri uri3 = new Uri("https://live.bilibili.com/390508");
        Uri uri4 = new Uri("https://live.bilibili.com/1570342");
        
        Webview2.Source = uri2;
        Webview3.Source = uri3;
        Webview4.Source = uri4;
        */

    }

    private void Timer_Tick(object sender, object e)
    {
        UpdateLedBoardColors();
    }

    private void SetLedBoardColors()
    {
        var colors1 = new Color[8, 8];
        var colors2 = new Color[8, 8];
        var colors3 = new Color[8, 8];

        var color = Colors.Green;

        // 初始化颜色数据
        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                // 第一个控件按照列表顺序渲染
                int position1 = GetSnakePosition(row, col);
                colors1[row, col] = GetRainbowColor(position1, 64);

                // 第二个控件按照对角线渲染
                int position2 = GetDiagonalPosition(row, col);
                colors2[row, col] = GetRainbowColor(position2, 15);

                // 第三个控件随机渲染
                colors3[row, col] = GetRandomColor();
            }
        }

        // 设置 WS64LedBoardBox 控件的 Colors 属性
        ledBoard1.Colors = colors1;
        ledBoard2.Colors = colors2;
        ledBoard3.Colors = colors3;
    }

    private void UpdateLedBoardColors()
    {
        var colors1 = new Color[8, 8];
        var colors2 = new Color[8, 8];
        var colors3 = new Color[8, 8];

        // 更新颜色数据
        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                // 第一个控件按照列表顺序渲染
                int position1 = (GetSnakePosition(row, col) + _currentStep) % 64;
                colors1[row, col] = GetRainbowColor(position1, 64);

                // 第二个控件按照对角线渲染
                int position2 = (GetDiagonalPosition(row, col) + _currentStep) % 15;
                colors2[row, col] = GetRainbowColor(position2, 15);

                // 第三个控件随机渲染
                colors3[row, col] = GetRandomColor();
            }
        }

        // 设置 WS64LedBoardBox 控件的 Colors 属性
        ledBoard1.Colors = colors1;
        ledBoard2.Colors = colors2;
        ledBoard3.Colors = colors3;

        _currentStep++;
    }

    private int GetDiagonalPosition(int row, int col)
    {
        return row + col;
    }

    private int GetSnakePosition(int row, int col)
    {
        if (row % 2 == 0)
        {
            return row * 8 + col;
        }
        else
        {
            return row * 8 + (7 - col);
        }
    }

    private Color GetRandomColor()
    {
        return Color.FromArgb(255, (byte)_random.Next(256), (byte)_random.Next(256), (byte)_random.Next(256));
    }

    private Color GetRainbowColor(int position, int total)
    {
        double ratio = (double)position / total;
        int r = (int)(Math.Sin(ratio * Math.PI * 2) * 127 + 128);
        int g = (int)(Math.Sin(ratio * Math.PI * 2 + 2 * Math.PI / 3) * 127 + 128);
        int b = (int)(Math.Sin(ratio * Math.PI * 2 + 4 * Math.PI / 3) * 127 + 128);
        return Color.FromArgb(255, (byte)r, (byte)g, (byte)b);
    }

    private Window _starWindow;

    private void OpenStarWindowButton_Click(object sender, RoutedEventArgs e)
    {
        if (_starWindow == null)
        {
            _starWindow = new StarWindow();
            _starWindow.Closed += (s, args) => _starWindow = null;
            _starWindow.Activate();
        }
        else
        {
            _starWindow.Activate();
        }
    }

    private void ScrollViewer_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        var scrollViewer = sender as ScrollViewer;
        if (scrollViewer != null)
        {
            // 开始拖动
            isDragging = true;
            currentScrollViewer = scrollViewer;
            lastMousePosition = e.GetCurrentPoint(scrollViewer).Position;
            scrollViewer.CapturePointer(e.Pointer); // 捕获指针
            e.Handled = true;
        }
    }

    private void ScrollViewer_PointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if (isDragging && currentScrollViewer != null)
        {
            var currentPosition = e.GetCurrentPoint(currentScrollViewer).Position;
            // 计算鼠标移动的偏移量
            double deltaX = lastMousePosition.X - currentPosition.X;
            double deltaY = lastMousePosition.Y - currentPosition.Y;

            // 更新 ScrollViewer 的偏移量
            currentScrollViewer.ChangeView(
                currentScrollViewer.HorizontalOffset + deltaX,
                currentScrollViewer.VerticalOffset + deltaY,
                null, // 保持当前缩放比例
                true // 禁用动画以提高流畅度
            );

            // 更新上一次鼠标位置
            lastMousePosition = currentPosition;
            e.Handled = true;
        }
    }

    private void ScrollViewer_PointerReleased(object sender, PointerRoutedEventArgs e)
    {
        if (isDragging)
        {
            isDragging = false;
            currentScrollViewer?.ReleasePointerCapture(e.Pointer); // 释放指针
            currentScrollViewer = null;
            e.Handled = true;
        }
    }

    private bool isDragging;
    private Point lastMousePosition;
    private ScrollViewer currentScrollViewer;
    private enum SyncSource { None, ScrollViewer1, ScrollViewer2 }
    private SyncSource syncSource = SyncSource.None;

    private void ScrollViewer1_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
    {
        if (syncSource == SyncSource.ScrollViewer2) return;

        float zoomDelta = Math.Abs(scrollViewer2.ZoomFactor - scrollViewer1.ZoomFactor);
        float hOffsetDelta = (float)Math.Abs(scrollViewer2.HorizontalOffset - scrollViewer1.HorizontalOffset);
        float vOffsetDelta = (float)Math.Abs(scrollViewer1.VerticalOffset - scrollViewer2.VerticalOffset);

        if (zoomDelta < 0.01f && hOffsetDelta < 1f && vOffsetDelta < 1f) return;

        syncSource = SyncSource.ScrollViewer1;

        var centerPoint = new Point(scrollViewer1.ActualWidth / 2, scrollViewer1.ActualHeight / 2);
        scrollViewer2.ChangeView(scrollViewer1.HorizontalOffset, scrollViewer1.VerticalOffset, scrollViewer1.ZoomFactor, true);

        syncSource = SyncSource.None;
    }

    private void ScrollViewer2_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
    {
        if (syncSource == SyncSource.ScrollViewer1) return;

        float zoomDelta = Math.Abs(scrollViewer1.ZoomFactor - scrollViewer2.ZoomFactor);
        float hOffsetDelta = (float)Math.Abs(scrollViewer1.HorizontalOffset - scrollViewer2.HorizontalOffset);
        float vOffsetDelta = (float)Math.Abs(scrollViewer2.VerticalOffset - scrollViewer1.VerticalOffset);

        if (zoomDelta < 0.01f && hOffsetDelta < 1f && vOffsetDelta < 1f) return;

        syncSource = SyncSource.ScrollViewer2;

        var centerPoint = new Point(scrollViewer2.ActualWidth / 2, scrollViewer2.ActualHeight / 2);
        scrollViewer1.ChangeView(scrollViewer2.HorizontalOffset, scrollViewer2.VerticalOffset, scrollViewer2.ZoomFactor, true);

        syncSource = SyncSource.None;
    }

}