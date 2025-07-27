using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using System;
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
}