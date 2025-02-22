using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI;           // Needed for WindowId.
using Microsoft.UI.Windowing; // Needed for AppWindow.
using WinRT;
using WinRT.Interop;
using Windows.UI;          // Needed for XAML/HWND interop.
using System.Diagnostics;
using System.IO;
using Tommy;
using Microsoft.UI.Xaml.Media.Animation;
using FSGaryityTool_Win11.Core.Settings;
using FSGaryityTool_Win11.Views.Pages.KsyboardPage;
using FSGaryityTool_Win11.Views.Pages.MousePage;
using FSGaryityTool_Win11.Views.Pages.FanControlPage;
using FSGaryityTool_Win11.Views.Pages.FlashDownloadPage;
using FSGaryityTool_Win11.Views.Pages.FairingStudioPage;
using FSGaryityTool_Win11.Views.Pages.SerialPortPage;
using FSGaryityTool_Win11.Views.Pages.CameraControlPage;
using FSGaryityTool_Win11.Controls;

namespace FSGaryityTool_Win11;

public sealed partial class MainWindow : Window
{
    public const string FSSoftVersion = "0.3.10";
    public const string FSSoftName = "FSGravityTool";

    public static int FsPage { get; set; }

    public static int DefWindowBackGround { get; set; }


    // public static Page1 page1Instance { get; set; }

    private readonly Dictionary<string, Type> _pageTypeMap = new()
    {
        [nameof(MainPage1)] = typeof(MainPage1),
        [nameof(Page2)] = typeof(Page2),
        [nameof(Page3)] = typeof(Page3),
        [nameof(Page4)] = typeof(Page4),
        [nameof(Page5)] = typeof(Page5),
        [nameof(CameraControlMainPage)] = typeof(CameraControlMainPage),
        [nameof(FSPage)] = typeof(FSPage)
    };

    private readonly Dictionary<int, Action> _backRequestedMap = new()
    {
        [0] = () => Debug.WriteLine("SerialPortPage"),
        [1] = () => Debug.WriteLine("FlashDownloadPage"),
        [2] = () => Debug.WriteLine("KeyboardPage"),
        [3] = () => Debug.WriteLine("MousePage"),
        [4] = () => Debug.WriteLine("FanControlPage"),
        [5] = () => Debug.WriteLine("CameraControlPage"),
        [6] = () => { if (FSPage.fSPage.MyWebView2.CanGoBack) FSPage.fSPage.MyWebView2.GoBack(); },
        [7] = () => Debug.WriteLine("SettingsPage")
    };

    private void CanvasControl_Draw(
        Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl sender,
        Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs args)
    {
        args.DrawingSession.DrawEllipse(155, 115, 80, 30, Colors.Black, 3);
        args.DrawingSession.DrawText("Hello, Win2D World!", 100, 100, Colors.Yellow);
    }

    private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        var selectedItem = (NavigationViewItem)args.SelectedItem;
        var tag = (string)selectedItem.Tag;

        if (_pageTypeMap.TryGetValue(tag, out var value))
        {
            FSnvf.Navigate(value);
            FsPage = Array.IndexOf(_pageTypeMap.Keys.ToArray(), tag);

            if (tag == "MainPage1")
            {
                UpdateSplitViewDisplayMode();
            }
        }

        if (args.IsSettingsSelected)
        {
            FSnvf.Navigate(typeof(MainSettingsPage), null, new DrillInNavigationTransitionInfo());
            FsPage = 7;
        }
    }

    public NavigationFailedEventHandler OnNavigationFailed { get; private set; }
    public static MainWindow Instance { get; private set; }
    private static Win32WindowHelper win32WindowHelper;

    // 窗口的最小宽度和高度
    private const int MinWidth = 515;
    private const int MinHeight = 328;

    // 窗口的默认宽度和高度
    private const int DefaultWidth = 1840;
    private const int DefaultHeight = 960;

    public bool Resize(Window window, int width, int height)
    {
        try
        {
            var hWnd = WindowNative.GetWindowHandle(window);
            var windowId = Win32Interop.GetWindowIdFromWindow(hWnd);

            AppWindow.Resize(new() { Width = width, Height = height });


            win32WindowHelper = new Win32WindowHelper(window);
            win32WindowHelper.SetWindowMinMaxSize(new Win32WindowHelper.POINT() { x = MinWidth, y = MinHeight });

            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
        return false;
    }

    private void AppWindow_Changed(AppWindow sender, AppWindowChangedEventArgs args)
    {
        
    }

    public void DelayedInitialize(UIElement mainContent)
    {
        Task.Run(() =>
        {
            Thread.Sleep(500);  // 延时改为0.5秒

            // 创建淡出动画
            DispatcherQueue.TryEnqueue(() =>
            {
                var storyboard = new Storyboard();
                var fadeOutAnimation = new DoubleAnimation
                {
                    From = 1.0,
                    To = 0.0,
                    Duration = new(TimeSpan.FromMilliseconds(50))
                };
                Storyboard.SetTarget(fadeOutAnimation, (ExtendedSplash)App.Window.Content);
                Storyboard.SetTargetProperty(fadeOutAnimation, "Opacity");
                storyboard.Children.Add(fadeOutAnimation);

                // 播放淡出动画
                //storyboard.Begin();
            });

            Thread.Sleep(1);  // 等待淡出动画完成

            // 移除 ExtendedSplash
            DispatcherQueue.TryEnqueue(() =>
            {
                App.RemoveExtendedSplash(mainContent);
            });
        });
    }

    public WindowBackgroundBrushControl windowBackgroundBrushControl;
    public MainWindow()
    {
        InitializeComponent();
        Instance = this;
        
        var isFirstActivation = true;
        var mainContent = Content;

        Resize(this, DefaultWidth, DefaultHeight);//==================

        // 将窗口的标题栏设置为自定义标题栏
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBara);

        AppWindow.Title = FSSoftName;//Set AppWindow
        AppWindow.SetIcon("FSFSoftH.ico");

        AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Tall;

        // 在窗口激活后注册 SizeChanged 事件处理器
        Activated += (sender, e) =>
        {
            if (isFirstActivation)
            {
                var extendedSplash = new ExtendedSplash(this);
                Content = extendedSplash;
                if (Current is not null)
                {
                    Current.SizeChanged += extendedSplash.ExtendedSplash_OnResize;
                }
            }
        };


        //page1Instance = new Page1(); // 初始化Page1实例

        Task.Run(() =>
        {
            SettingsCoreServices.CheckSettingFolder();
            SettingsCoreServices.AddTomlFile();
            SettingsCoreServices.CheckSettingsFileVersion();

            // 在初始化完成后，回到 UI 线程移除 ExtendedSplash
            DispatcherQueue.TryEnqueue(() =>
            {
                LanguageSetting();

                FSnv.IsPaneOpen = SettingsCoreServices.GetMainWindowNavigationPaneInfo();
                var nvPage = int.Parse(SettingsCoreServices.GetStartPageSetting());
                FSnv.SelectedItem = FSnv.MenuItems[nvPage];             //设置默认页面
                FsPage = nvPage;

                WindowBackgroundBrushControl.appWindow = AppWindow;
                WindowBackgroundBrushControl.window = this;

                var defPageBackground = int.Parse(SettingsCoreServices.GetSoftBackgroundSetting());
                DefWindowBackGround = defPageBackground;
                _lastDefWindowBackGround = defPageBackground;

                TitleBarTextBlock.Text = FSSoftName;

                if (Microsoft.UI.Composition.SystemBackdrops.DesktopAcrylicController.IsSupported())
                {
                    Activated += WindowBackgroundBrushControl.ActivatedBackgroundBrush;  // 当窗口被激活时的事件处理。
                    Activated += Window_Activated;  // 当窗口被激活时的事件处理。

                    Closed += WindowBackgroundBrushControl.CloseBackgroundBrush;  // 当窗口被关闭时的事件处理。
                    Closed += Window_Closed;  // 当窗口被关闭时的事件处理。

                    // 初始配置状态。
                    
                    SetConfigurationSourceTheme();  // 设置配置源主题。
                    WindowBackgroundBrushControl.WindowBackgroundBrushKind BackgroundBrushKind;
                    // 根据defWindowBackGround的值选择背景效果的类型。
                    switch (DefWindowBackGround)
                    {
                        case 0:
                            BackgroundBrushKind = WindowBackgroundBrushControl.WindowBackgroundBrushKind.AcrylicThin;
                            break;
                        case 1:
                            BackgroundBrushKind = WindowBackgroundBrushControl.WindowBackgroundBrushKind.AcrylicBase;
                            break;
                        case 2:
                            BackgroundBrushKind = WindowBackgroundBrushControl.WindowBackgroundBrushKind.Mica;
                            break;
                        case 3:
                            BackgroundBrushKind = WindowBackgroundBrushControl.WindowBackgroundBrushKind.MicaAlt;
                            break;
                        case 4:
                            BackgroundBrushKind = WindowBackgroundBrushControl.WindowBackgroundBrushKind.AcrylicDesktop;
                            break;
                        case 5:
                            BackgroundBrushKind = WindowBackgroundBrushControl.WindowBackgroundBrushKind.Transparent;
                            break;
                        case 6:
                            BackgroundBrushKind = WindowBackgroundBrushControl.WindowBackgroundBrushKind.None;
                            break;
                        default:
                            BackgroundBrushKind = WindowBackgroundBrushControl.WindowBackgroundBrushKind.None;
                            break;

                    }
                    WindowBackgroundBrushControl.SetWindowBackgroundBrush(BackgroundBrushKind);

                    WindowBackgroundBrushControl.WindowBackgroundBrushActivatedEnable = bool.Parse(SettingsCoreServices.GetSoftBackgroundActivatedEnableSetting());
                }

                
            });
        });

        /*
        if (AppWindowTitleBar.IsCustomizationSupported())
        {
            AppWindowTitleBar m_TitleBar = m_AppWindow.TitleBar;

            // Set active window colors.
            // Note: No effect when app is running on Windows 10
            // because color customization is not supported.
            m_TitleBar.ForegroundColor = Color.FromArgb(255, 255, 255, 255);
            m_TitleBar.BackgroundColor = Color.FromArgb(255, 22, 22, 22);
            m_TitleBar.ButtonForegroundColor = Color.FromArgb(255, 255, 255, 255);
            m_TitleBar.ButtonBackgroundColor = Color.FromArgb(255, 22, 22, 22);
            m_TitleBar.ButtonHoverForegroundColor = Color.FromArgb(255, 0, 0, 0);
            m_TitleBar.ButtonHoverBackgroundColor = Color.FromArgb(255, 230, 224, 0);
            m_TitleBar.ButtonPressedForegroundColor = Color.FromArgb(255, 0, 0, 0);
            m_TitleBar.ButtonPressedBackgroundColor = Color.FromArgb(255, 230, 224, 0);

            // Set inactive window colors.
            // Note: No effect when app is running on Windows 10
            // because color customization is not supported.
            m_TitleBar.InactiveForegroundColor = Colors.Gainsboro;
            m_TitleBar.InactiveBackgroundColor = Color.FromArgb(255, 22, 22, 22);
            m_TitleBar.ButtonInactiveForegroundColor = Colors.Gainsboro;
            m_TitleBar.ButtonInactiveBackgroundColor = Color.FromArgb(255, 22, 22, 22);
        }*/


        TitleDev.Visibility = Visibility.Visible;
        //TitleBeta.Visibility = Visibility.Visible;

        ((FrameworkElement)Content).ActualThemeChanged += Window_ThemeChanged;
        
        Activated += (sender, e) =>
        {
            AppWindow.Changed += AppWindow_Changed;

            if (isFirstActivation)
            {

                DelayedInitialize(mainContent);
                isFirstActivation = false;
            }
            LoadingEnd();
        };
    }

    private void MainWindow_Closed(object sender, WindowEventArgs args)
    {
        throw new NotImplementedException();
    }

    private int _lastDefWindowBackGround;

    public static bool IsFirstLoading { get; set; } = true;

    public void LoadingEnd()
    {
        if (IsFirstLoading)
            LoadingPrg();
    }

    public void LoadingPrg()
    {
        Task.Run(() =>
        {
            switch (FsPage)
            {
                case 0: Thread.Sleep(1450);
                    break;
                case 4: Thread.Sleep(1500);
                    break;
            }
            DispatcherQueue.TryEnqueue(() =>
            {
                var fadeOutAnimation = new DoubleAnimation
                {
                    To = 0,
                    Duration = TimeSpan.FromMilliseconds(75)
                };

                var storyboard = new Storyboard();
                storyboard.Children.Add(fadeOutAnimation);

                Storyboard.SetTarget(fadeOutAnimation, FsStartImage);
                Storyboard.SetTargetProperty(fadeOutAnimation, "Opacity");

                storyboard.Begin();
            });
            Thread.Sleep(200);
            DispatcherQueue.TryEnqueue(() =>
            {
                FsStartImage.Visibility = Visibility.Collapsed;
                IsFirstLoading = false;
            });
        });
    }

    public void LanguageSetting()
    {
        SerialPortI.Content = Page1.LanguageText("serialPort");
        DownFlashI.Content = Page1.LanguageText("download Flash");
        KeyboardI.Content = Page1.LanguageText("keyboard");
        MouseI.Content = Page1.LanguageText("mouse");
    }

    public void WindowBackSetting()
    {
        if (Microsoft.UI.Composition.SystemBackdrops.DesktopAcrylicController.IsSupported())
        {
            var defPageBackground = int.Parse(SettingsCoreServices.GetSoftBackgroundSetting());
            if (defPageBackground != _lastDefWindowBackGround)
            {
                DefWindowBackGround = defPageBackground;
            }
            _lastDefWindowBackGround = defPageBackground;

            WindowBackgroundBrushControl.WindowBackgroundBrushKind BackgroundBrushKind;
            // 根据defWindowBackGround的值选择背景效果的类型。
            switch (DefWindowBackGround)
            {
                case 0:
                    BackgroundBrushKind = WindowBackgroundBrushControl.WindowBackgroundBrushKind.AcrylicThin;
                    break;
                case 1:
                    BackgroundBrushKind = WindowBackgroundBrushControl.WindowBackgroundBrushKind.AcrylicBase;
                    break;
                case 2:
                    BackgroundBrushKind = WindowBackgroundBrushControl.WindowBackgroundBrushKind.Mica;
                    break;
                case 3:
                    BackgroundBrushKind = WindowBackgroundBrushControl.WindowBackgroundBrushKind.MicaAlt;
                    break;
                case 4:
                    BackgroundBrushKind = WindowBackgroundBrushControl.WindowBackgroundBrushKind.AcrylicDesktop;
                    break;
                case 5:
                    BackgroundBrushKind = WindowBackgroundBrushControl.WindowBackgroundBrushKind.Transparent;
                    break;
                case 6:
                    BackgroundBrushKind = WindowBackgroundBrushControl.WindowBackgroundBrushKind.None;
                    break;
                default:
                    BackgroundBrushKind = WindowBackgroundBrushControl.WindowBackgroundBrushKind.None;
                    break;

            }
            WindowBackgroundBrushControl.SetWindowBackgroundBrush(BackgroundBrushKind);
        }
    }

    private void Window_Activated(object sender, WindowActivatedEventArgs args)
    {
        SetConfigurationSourceTheme();
    }

    // 当窗口被关闭时，此方法会被调用
    private void Window_Closed(object sender, WindowEventArgs args)
    {
        // 移除窗口激活事件的处理方法
        Activated -= WindowBackgroundBrushControl.ActivatedBackgroundBrush;
    }

    // 当窗口的主题改变时，此方法会被调用
    private void Window_ThemeChanged(FrameworkElement sender, object args)
    {
        SetConfigurationSourceTheme();
        // 如果配置对象不为null，则根据当前的主题来更新m_configurationSource.Theme的值
        WindowBackSetting();
    }

    // 根据当前的主题来设置m_configurationSource.Theme的值
    private void SetConfigurationSourceTheme()
    {
        try
        {
            WindowBackgroundBrushControl.ApplyTheme(((FrameworkElement)Content), WindowBackgroundBrushControl.WindowTheme.System);
            WindowBackgroundBrushControl.SetAppTitleBar(((FrameworkElement)Content), WindowBackgroundBrushControl.WindowTheme.System);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
    }

    private void NavigationView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
    {
        if (_backRequestedMap.TryGetValue(FsPage, out var value))
        {
            value();
        }
    }

    private void NavigationView_DisplayModeChanged(NavigationView sender, NavigationViewDisplayModeChangedEventArgs args)
    {
        AppTitleBara.Margin =
            args.DisplayMode == NavigationViewDisplayMode.Minimal ? new(48, 0, 0, 0) : new(0, 0, 0, 0);

        UpdateSplitViewDisplayMode();
    }

    private void UpdateSplitViewDisplayMode()
    {
        if (FSnvf.Content is MainPage1 currentPage)
        {
            currentPage.ChangeSplitViewDisplayMode(FSnv.DisplayMode == NavigationViewDisplayMode.Expanded
                ? SplitViewDisplayMode.CompactInline
                : SplitViewDisplayMode.CompactOverlay);
        }
    }

    private void FSnv_PaneOpened(NavigationView sender, object args)
    {
        //Debug.WriteLine(DefaultNavigationViewPaneOpen);
        SettingsCoreServices.SetMainWindowNavigationPaneInfo(true);
    }

    private void FSnv_PaneClosed(NavigationView sender, object args)
    {
        //Debug.WriteLine(DefaultNavigationViewPaneOpen);
        SettingsCoreServices.SetMainWindowNavigationPaneInfo(false);
    }
    /*
        public class Tab1
        {
        private SerialPort serialPort1;

        public Tab1()
        {
        this.serialPort1 = new SerialPort();
        // 配置并打开serialPort1
        }

        // 使用serialPort1进行通信
        }

        public class Tab2
        {
        private SerialPort serialPort2;

        public Tab2()
        {
        this.serialPort2 = new SerialPort();
        // 配置并打开serialPort2
        }

        // 使用serialPort2进行通信
        }
*/
}
