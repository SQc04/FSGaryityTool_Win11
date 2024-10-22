using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace FSGaryityTool_Win11.Views.Pages.TestPage;

public sealed partial class TestPage1 : Page
{
    private DispatcherTimer _timer;

    public TestPage1()
    {
        InitializeComponent();

        // 初始化定时器
        //timer = new DispatcherTimer();
        //timer.Interval = TimeSpan.FromMilliseconds(100);
        //timer.Tick += Timer_Tick;
        //timer.Start();

        /*
        Uri uri1 = new Uri("https://live.bilibili.com/26312855");
        Uri uri2 = new Uri("https://live.bilibili.com/22409163");
        Uri uri3 = new Uri("https://live.bilibili.com/390508");
        Uri uri4 = new Uri("https://live.bilibili.com/1570342");
        Webview1.Source = uri1;
        Webview2.Source = uri2;
        Webview3.Source = uri3;
        Webview4.Source = uri4;
        */

    }

    private void Timer_Tick(object sender, object e)
    {
        //TestCustomTextBox.Text += "Test\n";
    }
}
