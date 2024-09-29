using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Media.Animation;
using System.Security.AccessControl;
using System.Threading;
using System.Threading.Tasks;

using FSGaryityTool_Win11.Views.Pages.FlashDownloadPage;
using Microsoft.UI.Xaml.Documents;
using System.Diagnostics;
using FSGaryityTool_Win11.Views.Pages.TestPage;
using FSGaryityTool_Win11.Views.Pages.FairingStudioPage;
using System.Diagnostics.Eventing.Reader;
using Microsoft.UI;
using static FSGaryityTool_Win11.Page1;
using static FSGaryityTool_Win11.Views.Pages.SerialPortPage.SerialPortToolsPage;
using System.IO.Ports;
using System.Text;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FSGaryityTool_Win11.Views.Pages.SerialPortPage
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage1 : Page
    {
        
        public static int Pge = 0;

        public static int selectorBarDefaultNumber = 0;
        private int previousSelectedIndex = selectorBarDefaultNumber;

        public static MainPage1 mainPage1;

        private ITaskbarList3 taskbarInstance;

        private CancellationTokenSource delayCts = new CancellationTokenSource();
        public MainPage1()
        {
            this.InitializeComponent();

            mainPage1 = this;

            SerialPortTextBlock.Text = Page1.LanguageText("serialPort");
            SerialPlotterTextBlock.Text = Page1.LanguageText("serialPlotter");

            SerialPortToolsFrame.Navigate(typeof(SerialPortToolsPage));

            

            FSSPagf.Navigate(typeof(Page1), null, null);//初始化Page1

            switch (selectorBarDefaultNumber)
            {
                case 0:
                    SerialPortPageNavigationView.SelectedItem = SerialPort;
                    break;
                case 1:
                    SerialPortPageNavigationView.SelectedItem = SerialPlotter;
                    break;
                case 2:
                    SerialPortPageNavigationView.SelectedItem = Test2;
                    break;
            }

            this.taskbarInstance = (ITaskbarList3)new TaskbarList();
            this.taskbarInstance.HrInit();

            SerialPortConnectToggleButtonText.Text = LanguageText("connectl");
            RunProgressBar.Visibility = Visibility.Collapsed;
        }

        private async void SerialPortPageNavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            var selectedItem = args.SelectedItem as NavigationViewItem;
            int currentSelectionIndex = sender.MenuItems.IndexOf(selectedItem);

            System.Type pageType = typeof(Page2);

            switch (currentSelectionIndex)
            {
                case 0:
                    pageType = typeof(Page1);
                    break;
                case 1:
                    pageType = typeof(SerialPlotterPage);
                    break;
                case 2:
                    pageType = typeof(TestPage1);
                    break;
            }

            var slideNavigationTransitionEffect = currentSelectionIndex - previousSelectedIndex > 0 ? SlideNavigationTransitionEffect.FromRight : SlideNavigationTransitionEffect.FromLeft;

            if (currentSelectionIndex == previousSelectedIndex) slideNavigationTransitionEffect = SlideNavigationTransitionEffect.FromBottom;

            FSSPagf.Navigate(pageType, null, new SlideNavigationTransitionInfo() { Effect = slideNavigationTransitionEffect });

            previousSelectedIndex = currentSelectionIndex;

            // 取消之前的延时
            delayCts.Cancel();
            delayCts = new CancellationTokenSource();

            try
            {
                // 等待2秒，如果在这期间被取消，将抛出 OperationCanceledException
                await Task.Delay(500, delayCts.Token);

                // 然后导航到同一个页面，但不播放过渡动画
                FSSPagf.Navigate(pageType, null, null);
            }
            catch (OperationCanceledException)
            {
                // 如果延时被取消，不做任何事情
            }
        }

        bool toolsToggleButtonIsChecked = false;
        private void SerialPortToolsToggleButton_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void SerialPortToolsToggleButtonFontIcon_Rotation(object name)
        {
            
            if (toolsToggleButtonIsChecked)
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
            
            toolsToggleButtonIsChecked = true;
            Thread SerialPortToolsToggleButtonFontIconRotation = new Thread(SerialPortToolsToggleButtonFontIcon_Rotation);
            SerialPortToolsToggleButtonFontIconRotation.Start();
            if (SerialPortToolsSplitView.DisplayMode == SplitViewDisplayMode.CompactOverlay)
            {
                SerialPortToolsSplitView.PaneBackground = this.Resources["CustomAcrylicBrush"] as AcrylicBrush;
            }
            if (SerialPortToolsSplitView.DisplayMode == SplitViewDisplayMode.CompactInline)
            {
                SerialPortToolsSplitView.PaneBackground = Application.Current.Resources["AcrylicBackgroundFillColorBaseBrush"] as SolidColorBrush;
            }
            SerialPortConnectToggleButtonGrid.Width = 304;
            serialPortToolsPage.RunTProgressBar.Width = 285;
        }

        private void SerialPortToolsSplitView_PaneClosing(SplitView sender, SplitViewPaneClosingEventArgs args)
        {
            toolsToggleButtonIsChecked = false;
            Thread SerialPortToolsToggleButtonFontIconRotation = new Thread(SerialPortToolsToggleButtonFontIcon_Rotation);
            SerialPortToolsToggleButtonFontIconRotation.Start();
            if (SerialPortToolsSplitView.DisplayMode == SplitViewDisplayMode.CompactOverlay)
            {
                SerialPortToolsSplitView.PaneBackground = Application.Current.Resources["AcrylicBackgroundFillColorBaseBrush"] as SolidColorBrush;
            }
            if (SerialPortToolsSplitView.DisplayMode == SplitViewDisplayMode.CompactInline)
            {
                SerialPortToolsSplitView.PaneBackground = Application.Current.Resources["AcrylicBackgroundFillColorBaseBrush"] as SolidColorBrush;
            }
            SerialPortConnectToggleButtonGrid.Width = 51;
            serialPortToolsPage.RunTProgressBar.Width = 32;
        }

        public void ChangeSplitViewDisplayMode(Microsoft.UI.Xaml.Controls.SplitViewDisplayMode mode)
        {
            this.SerialPortToolsSplitView.DisplayMode = mode;
        }

        public void SerialPortConnectToggleButton_Click(object sender, RoutedEventArgs e)
        {
            if(SerialPortToolsPage.portIsConnect == 0)
            {
                var app = (Application.Current as App);             // 尝试将当前应用程序实例转换为App类型
                if (app != null)                                    // 检查转换是否成功
                {
                    var hWnd = app.MainWindowHandle;                // 获取主窗口的句柄
                    this.taskbarInstance.SetProgressState(hWnd, FSGaryityTool_Win11.TBPFLAG.TBPF_INDETERMINATE);//开始任务栏加载动画
                }
                try
                {
                    serialPortToolsPage.SerialPortConnect();
                    SerialPortConnectToggleButtonText.Text = LanguageText("disconnectl");
                    SerialPortConnectToggleButton.IsChecked = true;
                    RunProgressBar.IsIndeterminate = true;
                    RunProgressBar.ShowPaused = false;
                    RunProgressBar.Visibility = Visibility.Visible;
                    serialPortToolsPage._hideTimer.Start();
                }
                catch 
                {
                    serialPortToolsPage.SerialPortConnectcatch();
                    if (app != null)
                    {
                        var hWnd = app.MainWindowHandle;
                        this.taskbarInstance.SetProgressState(hWnd, FSGaryityTool_Win11.TBPFLAG.TBPF_NOPROGRESS);//停止任务栏加载动画
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
                var app = (Application.Current as App);
                if (app != null)
                {
                    var hWnd = app.MainWindowHandle;
                    this.taskbarInstance.SetProgressState(hWnd, FSGaryityTool_Win11.TBPFLAG.TBPF_NOPROGRESS);//停止任务栏加载动画
                }
                try
                {
                    serialPortToolsPage.SerialPortClose();
                }
                catch (Exception err)                                                                       //一般情况下关闭串口不会出错，所以不需要加处理程序
                {
                    page1.RXTextBox.Text = page1.RXTextBox.Text + err + "\r\n";
                }
                SerialPortConnectToggleButtonText.Text = LanguageText("connectl");
                serialPortToolsPage.SerialPortDisconnect();
                SerialPortConnectToggleButton.IsChecked = false;
                RunProgressBar.IsIndeterminate = false;
                RunProgressBar.ShowPaused = false;
                RunProgressBar.Visibility = Visibility.Collapsed;
                SerialPortToolsToggleButton.IsChecked = true;

            }
        }

    }
}
