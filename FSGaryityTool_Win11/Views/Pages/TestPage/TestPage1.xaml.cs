using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

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
    }

    private void Timer_Tick(object sender, object e)
    {
        //TestCustomTextBox.Text += "Test\n";
    }
}
