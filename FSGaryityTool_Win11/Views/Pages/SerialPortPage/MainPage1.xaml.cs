using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using System.Threading;
using System.Threading.Tasks;
using FSGaryityTool_Win11.Views.Pages.FlashDownloadPage;
using FSGaryityTool_Win11.Views.Pages.TestPage;
using static FSGaryityTool_Win11.Page1;
using static FSGaryityTool_Win11.Views.Pages.SerialPortPage.SerialPortToolsPage;

namespace FSGaryityTool_Win11.Views.Pages.SerialPortPage;

public sealed partial class MainPage1 : Page
{
    public static int Pge { get; set; } = 0;

    public const int SelectorBarDefaultNumber = 0;

    private int _previousSelectedIndex = SelectorBarDefaultNumber;

    public static MainPage1 Current { get; private set; }

    private readonly ITaskbarList3 _taskbarInstance;

    private CancellationTokenSource _delayCts = new();

    public MainPage1()
    {
        InitializeComponent();

        Current = this;

        SerialPortTextBlock.Text = LanguageText("serialPort");
        SerialPlotterTextBlock.Text = LanguageText("serialPlotter");

        SerialPortToolsFrame.Navigate(typeof(SerialPortToolsPage));

        FssPagf.Navigate(typeof(Page1), null, null);//初始化Page1

        SerialPortPageNavigationView.SelectedItem = SelectorBarDefaultNumber switch
        {
            0 => SerialPort,
            1 => SerialPlotter,
            2 => Test2,
            _ => SerialPortPageNavigationView.SelectedItem
        };

        _taskbarInstance = (ITaskbarList3)new TaskbarList();
        _taskbarInstance.HrInit();

        SerialPortConnectToggleButtonText.Text = LanguageText("connectl");
        RunProgressBar.Visibility = Visibility.Collapsed;
    }

    private async void SerialPortPageNavigationView_SelectionChanged(NavigationView sender,
        NavigationViewSelectionChangedEventArgs args)
    {
        var selectedItem = args.SelectedItem as NavigationViewItem;
        var currentSelectionIndex = sender.MenuItems.IndexOf(selectedItem);

        var pageType = currentSelectionIndex switch
        {
            0 => typeof(Page1),
            1 => typeof(SerialPlotterPage),
            2 => typeof(TestPage1),
            _ => typeof(Page2)
        };

        var slideNavigationTransitionEffect = currentSelectionIndex - _previousSelectedIndex > 0
            ? SlideNavigationTransitionEffect.FromRight
            : SlideNavigationTransitionEffect.FromLeft;

        if (currentSelectionIndex == _previousSelectedIndex)
            slideNavigationTransitionEffect = SlideNavigationTransitionEffect.FromBottom;

        FssPagf.Navigate(pageType, null,
            new SlideNavigationTransitionInfo { Effect = slideNavigationTransitionEffect });

        _previousSelectedIndex = currentSelectionIndex;

        // 取消之前的延时
        await _delayCts.CancelAsync();
        _delayCts = new();

        try
        {
            // 等待2秒，如果在这期间被取消，将抛出 OperationCanceledException
            await Task.Delay(500, _delayCts.Token);

            // 然后导航到同一个页面，但不播放过渡动画
            FssPagf.Navigate(pageType, null, null);
        }
        catch (OperationCanceledException)
        {
            // 如果延时被取消，不做任何事情
        }
    }

    private bool _toolsToggleButtonIsChecked;

    private void SerialPortToolsToggleButton_Click(object sender, RoutedEventArgs e)
    {

    }

    private void SerialPortToolsToggleButtonFontIcon_Rotation(object name)
    {

        if (_toolsToggleButtonIsChecked)
        {
            Thread.Sleep(100);
            DispatcherQueue.TryEnqueue(() =>
            {
                SerialPortToolsToggleButtonFontIcon.Rotation = 90;
            });
        }
        else
        {
            Thread.Sleep(100);
            DispatcherQueue.TryEnqueue(() =>
            {
                SerialPortToolsToggleButtonFontIcon.Rotation = 0;
            });
        }
    }

    private void SerialPortToolsSplitView_PaneOpening(SplitView sender, object args)
    {
            
        _toolsToggleButtonIsChecked = true;
        var serialPortToolsToggleButtonFontIconRotation = new Thread(SerialPortToolsToggleButtonFontIcon_Rotation);
        serialPortToolsToggleButtonFontIconRotation.Start();
        SerialPortToolsSplitView.PaneBackground = SerialPortToolsSplitView.DisplayMode switch
        {
            SplitViewDisplayMode.CompactOverlay => Resources["CustomAcrylicBrush"] as AcrylicBrush,
            SplitViewDisplayMode.CompactInline => Application.Current.Resources["AcrylicBackgroundFillColorBaseBrush"] as SolidColorBrush,
            _ => SerialPortToolsSplitView.PaneBackground
        };
        SerialPortConnectToggleButtonGrid.Width = 304;
        SerialPortToolsPage.Current.RunTProgressBar.Width = 285;
    }

    private void SerialPortToolsSplitView_PaneClosing(SplitView sender, SplitViewPaneClosingEventArgs args)
    {
        _toolsToggleButtonIsChecked = false;
        var serialPortToolsToggleButtonFontIconRotation = new Thread(SerialPortToolsToggleButtonFontIcon_Rotation);
        serialPortToolsToggleButtonFontIconRotation.Start();
        SerialPortToolsSplitView.PaneBackground = SerialPortToolsSplitView.DisplayMode switch
        {
            SplitViewDisplayMode.CompactOverlay or SplitViewDisplayMode.CompactInline =>
                Application.Current.Resources["AcrylicBackgroundFillColorBaseBrush"] as SolidColorBrush,
            _ => SerialPortToolsSplitView.PaneBackground
        };

        SerialPortConnectToggleButtonGrid.Width = 51;
        SerialPortToolsPage.Current.RunTProgressBar.Width = 32;
    }

    public void ChangeSplitViewDisplayMode(SplitViewDisplayMode mode)
    {
        SerialPortToolsSplitView.DisplayMode = mode;
    }

    public void SerialPortConnectToggleButton_Click(object sender, RoutedEventArgs e)
    {
        var hWnd = (nint)App.MainWindowHandle;                // 获取主窗口的句柄
        if (PortIsConnect is 0)
        {
            var app = Application.Current as App;             // 尝试将当前应用程序实例转换为App类型
            if (app is not null)                                    // 检查转换是否成功
            {
                _taskbarInstance.SetProgressState(hWnd, TBPFLAG.TBPF_INDETERMINATE);//开始任务栏加载动画
            }
            try
            {
                SerialPortToolsPage.Current.SerialPortConnect();
                SerialPortConnectToggleButtonText.Text = LanguageText("disconnectl");
                SerialPortConnectToggleButton.IsChecked = true;
                RunProgressBar.IsIndeterminate = true;
                RunProgressBar.ShowPaused = false;
                RunProgressBar.Visibility = Visibility.Visible;
                SerialPortToolsPage.Current.HideTimer.Start();
            }
            catch 
            {
                SerialPortToolsPage.Current.SerialPortConnectCatch();
                if (app is not null)
                {
                    _taskbarInstance.SetProgressState(hWnd, TBPFLAG.TBPF_NOPROGRESS);//停止任务栏加载动画
                }
                SerialPortConnectToggleButtonText.Text = LanguageText("connectl");
                SerialPortConnectToggleButton.IsChecked = false;
                RunProgressBar.IsIndeterminate = true;
                RunProgressBar.ShowPaused = true;
                RunProgressBar.Visibility = Visibility.Visible;
                SerialPortToolsToggleButton.IsChecked = true;
            }
        }
        else
        {
            if (Application.Current is App app)
            {
                _taskbarInstance.SetProgressState(hWnd, TBPFLAG.TBPF_NOPROGRESS);//停止任务栏加载动画
            }
            try
            {
                SerialPortToolsPage.Current.SerialPortClose();
            }
            catch (Exception err)                                                                       //一般情况下关闭串口不会出错，所以不需要加处理程序
            {
                Page1.Current.RxTextBox.Text = Page1.Current.RxTextBox.Text + err + "\r\n";
            }
            SerialPortConnectToggleButtonText.Text = LanguageText("connectl");
            SerialPortToolsPage.Current.SerialPortDisconnect();
            SerialPortConnectToggleButton.IsChecked = false;
            RunProgressBar.IsIndeterminate = false;
            RunProgressBar.ShowPaused = false;
            RunProgressBar.Visibility = Visibility.Collapsed;
            SerialPortToolsToggleButton.IsChecked = true;
        }
    }
}
