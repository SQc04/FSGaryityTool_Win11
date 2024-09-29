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

namespace FSGaryityTool_Win11;

public sealed partial class MainWindow : Window
{
    public const string FSSoftVersion = "0.3.6";
    public const string FSSoftName = "FSGravityTool Dev";

    public static int FsPage { get; set; }

    public static int DefWindowBackGround { get; set; }

    public static bool DefaultNavigationViewPaneOpen { get; set; }

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
            FSnvf.Navigate(typeof(MainSettingsPage), null, new SuppressNavigationTransitionInfo());
            FsPage = 7;
        }
    }

    public NavigationFailedEventHandler OnNavigationFailed { get; private set; }
    public static MainWindow Instance { get; private set; }

    // 窗口的最小宽度和高度
    private const int MinWidth = 642;
    private const int MinHeight = 409;

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
            AppWindow.Changed += AppWindow_Changed;

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
        try
        {
            if (AppWindow is null)
                return;

            if (AppWindow.Size is { Height: < MinHeight, Width: < MinWidth })
                AppWindow.Resize(new() { Width = MinWidth, Height = MinHeight });
            else if (AppWindow.Size.Height < MinHeight)
                AppWindow.Resize(new() { Width = AppWindow.Size.Width, Height = MinHeight });
            else if (AppWindow.Size.Width < MinWidth)
                AppWindow.Resize(new() { Width = MinWidth, Height = AppWindow.Size.Height });
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
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

    public MainWindow()
    {
        InitializeComponent();
        Instance = this;

        ((FrameworkElement)Content).ActualThemeChanged += Window_ThemeChanged;

        var isFirstActivation = true;
        var mainContent = Content;

        Resize(this, DefaultWidth, DefaultHeight);

        // 将窗口的标题栏设置为自定义标题栏
        ExtendsContentIntoTitleBar = true;  // enable custom titlebar
        SetTitleBar(AppTitleBara);

        AppWindow.Title = FSSoftName;//Set AppWindow
        AppWindow.SetIcon("FSFSoftH.ico");

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

        using (var reader = File.OpenText(SettingsCoreServices.FSGravityToolsSettingsToml))
        {
            var settingsTomlr = TOML.Parse(reader);
            string value = settingsTomlr["FSGravitySettings"]["DefaultNavigationViewPaneOpen"];
            DefaultNavigationViewPaneOpen = Convert.ToBoolean(value);
            FSnv.IsPaneOpen = DefaultNavigationViewPaneOpen;
            Debug.WriteLine("Pane" + DefaultNavigationViewPaneOpen);
        }

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

                //设置默认页面
                using (var reader = File.OpenText(SettingsCoreServices.FSGravityToolsSettingsToml))
                {
                    var settingsTomlr = TOML.Parse(reader);
                    Debug.WriteLine("Print:" + settingsTomlr["FSGravitySettings"]["DefaultNvPage"]);
                    var nvPage = int.Parse(settingsTomlr["FSGravitySettings"]["DefaultNvPage"]);
                    FSnv.SelectedItem = FSnv.MenuItems[nvPage];             //设置默认页面
                    FsPage = nvPage;
                }
                using (var reader = File.OpenText(SettingsCoreServices.FSGravityToolsSettingsToml))
                {
                    var settingsTomlr = TOML.Parse(reader);
                    Debug.WriteLine("Print:" + settingsTomlr["FSGravitySettings"]["SoftBackground"]);
                    var defPageBackground = int.Parse(settingsTomlr["FSGravitySettings"]["SoftBackground"]);
                    DefWindowBackGround = defPageBackground;
                    _lastDefWindowBackGround = defPageBackground;
                }
                TitleBarTextBlock.Text = FSSoftName;

                if (Microsoft.UI.Composition.SystemBackdrops.DesktopAcrylicController.IsSupported())
                {
                    // 连接策略对象。
                    _configurationSource = new();
                    Activated += Window_Activated;  // 当窗口被激活时的事件处理。
                    Closed += Window_Closed;  // 当窗口被关闭时的事件处理。

                    // 初始配置状态。
                    _configurationSource.IsInputActive = true;  // 设置输入活动状态为真。
                    SetConfigurationSourceTheme();  // 设置配置源主题。

                    // 根据defWindowBackGround的值选择背景效果的类型。
                    switch (DefWindowBackGround)
                    {
                        case 0:
                            _acrylicController = new();
                            _acrylicController.Kind = Microsoft.UI.Composition.SystemBackdrops.DesktopAcrylicKind.Thin;
                            _acrylicController.AddSystemBackdropTarget(this.As<Microsoft.UI.Composition.ICompositionSupportsSystemBackdrop>());
                            _acrylicController.SetSystemBackdropConfiguration(_configurationSource);
                            break;
                        case 1:
                            _acrylicController = new();
                            _acrylicController.Kind = Microsoft.UI.Composition.SystemBackdrops.DesktopAcrylicKind.Base;
                            _acrylicController.AddSystemBackdropTarget(this.As<Microsoft.UI.Composition.ICompositionSupportsSystemBackdrop>());
                            _acrylicController.SetSystemBackdropConfiguration(_configurationSource);
                            break;
                        case 2:
                            _micaController = new();
                            _micaController.Kind = Microsoft.UI.Composition.SystemBackdrops.MicaKind.Base;
                            _micaController.AddSystemBackdropTarget(this.As<Microsoft.UI.Composition.ICompositionSupportsSystemBackdrop>());
                            _micaController.SetSystemBackdropConfiguration(_configurationSource);
                            break;
                        case 3:
                            _micaController = new();
                            _micaController.Kind = Microsoft.UI.Composition.SystemBackdrops.MicaKind.BaseAlt;
                            _micaController.AddSystemBackdropTarget(this.As<Microsoft.UI.Composition.ICompositionSupportsSystemBackdrop>());
                            _micaController.SetSystemBackdropConfiguration(_configurationSource);
                            break;
                    }
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
                case 4: Thread.Sleep(100);
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
            using (var reader = File.OpenText(SettingsCoreServices.FSGravityToolsSettingsToml)) // 打开TOML文件
            {
                var settingsTomlr = TOML.Parse(reader);
                Debug.WriteLine("Print:" + settingsTomlr["FSGravitySettings"]["SoftBackground"]);
                var defPageBackground = int.Parse(settingsTomlr["FSGravitySettings"]["SoftBackground"]);
                if (defPageBackground != _lastDefWindowBackGround)
                {
                    DefWindowBackGround = defPageBackground;
                }
                _lastDefWindowBackGround = defPageBackground;
            }

            SetConfigurationSourceTheme(); // 设置配置源主题。

            // 根据defWindowBackGround的值选择背景效果的类型。
            switch (DefWindowBackGround)
            {
                case 0:
                    _acrylicController ??= new();
                    _acrylicController.Kind = Microsoft.UI.Composition.SystemBackdrops.DesktopAcrylicKind.Thin;
                    _acrylicController.AddSystemBackdropTarget(this.As<Microsoft.UI.Composition.ICompositionSupportsSystemBackdrop>());
                    _acrylicController.SetSystemBackdropConfiguration(_configurationSource);
                    break;
                case 1:
                    _acrylicController ??= new();
                    _acrylicController.Kind = Microsoft.UI.Composition.SystemBackdrops.DesktopAcrylicKind.Base;
                    _acrylicController.AddSystemBackdropTarget(this.As<Microsoft.UI.Composition.ICompositionSupportsSystemBackdrop>());
                    _acrylicController.SetSystemBackdropConfiguration(_configurationSource);
                    break;
                case 2:
                    _micaController ??= new();
                    _micaController.Kind = Microsoft.UI.Composition.SystemBackdrops.MicaKind.Base;
                    _micaController.AddSystemBackdropTarget(this.As<Microsoft.UI.Composition.ICompositionSupportsSystemBackdrop>());
                    _micaController.SetSystemBackdropConfiguration(_configurationSource);
                    break;
                case 3:
                    _micaController ??= new();
                    _micaController.Kind = Microsoft.UI.Composition.SystemBackdrops.MicaKind.BaseAlt;
                    _micaController.AddSystemBackdropTarget(this.As<Microsoft.UI.Composition.ICompositionSupportsSystemBackdrop>());
                    _micaController.SetSystemBackdropConfiguration(_configurationSource);
                    break;
            }

            // 释放当前背景效果的资源
            ReleaseCurrentBackdropResources();
        }
    }
    private void ReleaseCurrentBackdropResources()
    {
        Task.Run(() =>
        {
            // 释放m_acrylicController的资源，并将其设置为null
            if (_acrylicController is not null)
            {
                if (DefWindowBackGround is 2 or 3)
                {
                    //Thread.Sleep(100);
                    _acrylicController.Dispose();
                    _acrylicController = null;
                }
            }
            // 释放m_micaController的资源，并将其设置为null
            if (_micaController is not null)
            {
                if (DefWindowBackGround is 0 or 1)
                {
                    //Thread.Sleep(100);
                    _micaController.Dispose();
                    _micaController = null;
                }
            }
        });
    }

    /// <summary>
    /// 定义一个DesktopAcrylicController对象，用于控制窗口的背景
    /// </summary>
    private Microsoft.UI.Composition.SystemBackdrops.DesktopAcrylicController _acrylicController;

    /// <summary>
    /// 定义一个MicaController对象，用于控制云母效果
    /// </summary>
    private Microsoft.UI.Composition.SystemBackdrops.MicaController _micaController;

    /// <summary>
    /// 定义一个SystemBackdropConfiguration对象，用于配置窗口的背景
    /// </summary>
    private Microsoft.UI.Composition.SystemBackdrops.SystemBackdropConfiguration _configurationSource;

    /// <summary>
    /// 当窗口被激活时，此方法会被调用
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void Window_Activated(object sender, WindowActivatedEventArgs args)
    {
        // 根据窗口的激活状态来更新m_configurationSource.IsInputActive的值
        _configurationSource.IsInputActive = args.WindowActivationState != WindowActivationState.Deactivated;
        // 根据窗口的激活状态来设置背景效果
        if (_configurationSource.IsInputActive)
        {
            switch (DefWindowBackGround)
            {
                // 使用云母效果
                case 2 or 3:
                {
                    _micaController ??= new();
                    _micaController.Kind = DefWindowBackGround is 2 ? Microsoft.UI.Composition.SystemBackdrops.MicaKind.Base : Microsoft.UI.Composition.SystemBackdrops.MicaKind.BaseAlt;
                    _micaController.AddSystemBackdropTarget(this.As<Microsoft.UI.Composition.ICompositionSupportsSystemBackdrop>());
                    _micaController.SetSystemBackdropConfiguration(_configurationSource);
                    break;
                }
                // 使用亚克力效果
                case 0 or 1:
                {
                    _acrylicController ??= new();
                    _acrylicController.Kind = DefWindowBackGround is 0 ? Microsoft.UI.Composition.SystemBackdrops.DesktopAcrylicKind.Thin : Microsoft.UI.Composition.SystemBackdrops.DesktopAcrylicKind.Base;
                    _acrylicController.AddSystemBackdropTarget(this.As<Microsoft.UI.Composition.ICompositionSupportsSystemBackdrop>());
                    _acrylicController.SetSystemBackdropConfiguration(_configurationSource);
                    break;
                }
            }
        }
    }

    // 当窗口被关闭时，此方法会被调用
    private void Window_Closed(object sender, WindowEventArgs args)
    {
        // 释放m_acrylicController的资源，并将其设置为null
        if (_acrylicController is not null)
        {
            _acrylicController.Dispose();
            _acrylicController = null;
        }
        // 释放m_micaController的资源，并将其设置为null
        if (_micaController is not null)
        {
            _micaController.Dispose();
            _micaController = null;
        }
        // 移除窗口激活事件的处理方法
        Activated -= Window_Activated;
        // 将配置对象设置为null
        _configurationSource = null;
    }

    // 当窗口的主题改变时，此方法会被调用
    private void Window_ThemeChanged(FrameworkElement sender, object args)
    {
        Debug.WriteLine("Change");
        // 如果配置对象不为null，则根据当前的主题来更新m_configurationSource.Theme的值
        if (_configurationSource is not null)
        {
            SetConfigurationSourceTheme();
            WindowBackSetting();
        }
    }

    // 根据当前的主题来设置m_configurationSource.Theme的值
    private void SetConfigurationSourceTheme()
    {
        var theme = ((FrameworkElement)Content).ActualTheme;
        _configurationSource.Theme = theme switch
        {
            ElementTheme.Dark =>
                // 如果主题是深色，SystemBackdropTheme会被设置为Dark
                Microsoft.UI.Composition.SystemBackdrops.SystemBackdropTheme.Dark,
            ElementTheme.Light =>
                // 如果主题是浅色，SystemBackdropTheme会被设置为Light
                Microsoft.UI.Composition.SystemBackdrops.SystemBackdropTheme.Light,
            ElementTheme.Default =>
                // 如果主题是默认的，SystemBackdropTheme会被设置为Default
                Microsoft.UI.Composition.SystemBackdrops.SystemBackdropTheme.Default,
            _ => _configurationSource.Theme
        };

        var titleBar = AppWindow.TitleBar;
        titleBar.ButtonForegroundColor = theme is ElementTheme.Dark ? Colors.White : Colors.Black;

        titleBar.ButtonBackgroundColor = Colors.Transparent;
        titleBar.ButtonHoverBackgroundColor = Color.FromArgb(64, 128, 128, 128);
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
        Debug.WriteLine(DefaultNavigationViewPaneOpen);
        SettingsCoreServices.SaveSetting("FSGravitySettings", "DefaultNavigationViewPaneOpen", "true");
    }

    private void FSnv_PaneClosed(NavigationView sender, object args)
    {
        Debug.WriteLine(DefaultNavigationViewPaneOpen);
        SettingsCoreServices.SaveSetting("FSGravitySettings", "DefaultNavigationViewPaneOpen", "false");
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
