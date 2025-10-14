using FSGaryityTool_Win11.Controls;
using FSGaryityTool_Win11.Views.Pages.FlashDownloadPage;
using FSGaryityTool_Win11.Views.Pages.TestPage;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Threading;
using System.Threading.Tasks;
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
            2 => typeof(SerialMultipltxerPage),
            3 => typeof(TestPage1),
            4 => typeof(CopilotPage),
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

    public enum ProgressState
    {
        Running,
        Paused,
        Error,
        Value,
        Stop
    }

    public void SetRunProgressBarValue(int? value, ProgressState state)
    {
        switch (state)
        {
            case ProgressState.Running:
                RunProgressBar.IsIndeterminate = value is null;
                RunProgressBar.ShowPaused = false;
                RunProgressBar.ShowError = false;
                RunProgressBar.Visibility = Visibility.Visible;
                if (value is not null)
                {
                    RunProgressBar.Value = Math.Clamp(value.Value, 0, 100);
                }
                break;
            case ProgressState.Paused:
                RunProgressBar.IsIndeterminate = value is null;
                RunProgressBar.ShowPaused = true;
                RunProgressBar.ShowError = false;
                RunProgressBar.Visibility = Visibility.Visible;
                if (value is not null)
                {
                    RunProgressBar.Value = Math.Clamp(value.Value, 0, 100);
                }
                break;
            case ProgressState.Error:
                RunProgressBar.IsIndeterminate = value is null;
                RunProgressBar.ShowPaused = false;
                RunProgressBar.ShowError = true;
                RunProgressBar.Visibility = Visibility.Visible;
                RunProgressBar.Value = 0;
                break;
            case ProgressState.Value:
                RunProgressBar.IsIndeterminate = false;
                RunProgressBar.ShowPaused = false;
                RunProgressBar.ShowError = false;
                RunProgressBar.Visibility = Visibility.Visible;
                if (value is not null)
                {
                    RunProgressBar.Value = Math.Clamp(value.Value, 0, 100);
                }
                break;
            case ProgressState.Stop:
                RunProgressBar.IsIndeterminate = false;
                RunProgressBar.ShowPaused = false;
                RunProgressBar.ShowError = false;
                RunProgressBar.Value = 0;
                RunProgressBar.Visibility = Visibility.Collapsed;
                break;
            default:
                RunProgressBar.IsIndeterminate = false;
                RunProgressBar.ShowPaused = false;
                RunProgressBar.ShowError = false;
                RunProgressBar.Value = 0;
                RunProgressBar.Visibility = Visibility.Collapsed;
                break;
        }
    }

    

    public void SerialPortConnectToggleButton_Click(object sender, RoutedEventArgs e)
    {
        if (PortIsConnect is 0)
        {
            var app = Application.Current as App;             // 尝试将当前应用程序实例转换为App类型
            if (app is not null)                                    // 检查转换是否成功
            {
                MainWindow.Instance.taskbarProgress.SetTaskbarProgressValue(null, TaskbarProgress.TaskbarProgressState.Indeterminate);
            }
            try
            {
                SerialPortToolsPage.Current.SerialPortConnect();
                SerialPortConnectToggleButtonText.Text = LanguageText("disconnectl");
                SerialPortConnectToggleButton.IsChecked = true;
                SetRunProgressBarValue(null,ProgressState.Running);
                Page1.Current.SerialPortFlowInfoBoxLogicAnalyzerToggle(true);
                SerialPortToolsPage.Current.HideTimer.Start();

                Page1.Current.SerialPortOpen();
            }
            catch 
            {
                SerialPortToolsPage.Current.SerialPortConnectCatch();
                if (app is not null)
                {
                    MainWindow.Instance.taskbarProgress.SetTaskbarProgressValue(null, TaskbarProgress.TaskbarProgressState.NoProgress);
                }
                SerialPortConnectToggleButtonText.Text = LanguageText("connectl");
                SerialPortConnectToggleButton.IsChecked = false;
                SetRunProgressBarValue(null, ProgressState.Error);
                Page1.Current.SerialPortFlowInfoBoxLogicAnalyzerToggle(false);
                SerialPortToolsToggleButton.IsChecked = true;

                Page1.Current.SerialPortClose();
            }
        }
        else
        {
            if (Application.Current is App app)
            {
                MainWindow.Instance.taskbarProgress.SetTaskbarProgressValue(null, TaskbarProgress.TaskbarProgressState.NoProgress);
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

            Page1.Current.SerialPortFlowInfoBoxLogicAnalyzerToggle(false);
            SetRunProgressBarValue(null, ProgressState.Stop);
            SerialPortToolsToggleButton.IsChecked = true;
            Page1.Current.SerialPortClose();
        }
    }
}
