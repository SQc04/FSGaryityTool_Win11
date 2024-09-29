using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.InteropServices;
using Windows.Foundation;
using Windows.Foundation.Collections;


using System.Text;
using Windows.Networking.Sockets;
using System.Collections.ObjectModel;
using Microsoft.UI.Composition.SystemBackdrops;

using Windows.UI.Popups;
using System.Threading;

using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams;
using System.Threading.Tasks;
using System.IO.Ports;
using Microsoft.UI;           // Needed for WindowId.
using Microsoft.UI.Windowing; // Needed for AppWindow.
using WinRT;
using WinRT.Interop;
using Windows.UI;          // Needed for XAML/HWND interop.
using Windows.ApplicationModel.Core;
using Windows.UI.ViewManagement;
using Windows.ApplicationModel;
using Windows.Graphics;

using System.Diagnostics;
using System.IO;

using Tommy;
using System.Reflection.PortableExecutable;
using Windows.ApplicationModel.Activation;
using System.Xml.Linq;
using Microsoft.UI.Xaml.Media.Animation;

using FSGaryityTool_Win11.Core.Settings;
using FSGaryityTool_Win11.Views.Pages.KsyboardPage;
using FSGaryityTool_Win11.Views.Pages.MousePage;
using FSGaryityTool_Win11.Views.Pages.FanControlPage;
using FSGaryityTool_Win11.Views.Pages.FlashDownloadPage;
using FSGaryityTool_Win11.Views.Pages.FairingStudioPage;
using FSGaryityTool_Win11.Views.Pages.SerialPortPage;
using Windows.System;
using FSGaryityTool_Win11.Views.Pages.CameraControlPage;
using Microsoft.UI.Xaml.Hosting;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FSGaryityTool_Win11
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>

    

    public sealed partial class MainWindow : Window
    {

        public static string FSSoftVersion = "0.3.6";
        public static string FSSoftName = "FSGravityTool Dev";
        public static int FsPage = 0;
        public static int defWindowBackGround = 0;
        public static bool defaultNavigationViewPaneOpen;

        public static MainWindow mainWindow;

        //public static Page1 page1Instance;

        private Dictionary<string, Type> pageTypeMap = new Dictionary<string, Type>
        {
            {"MainPage1", typeof(MainPage1)},
            {"Page2", typeof(Page2)},
            {"Page3", typeof(Page3)},
            {"Page4", typeof(Page4)},
            {"Page5", typeof(Page5)},
            {"CameraControlMainPage", typeof(CameraControlMainPage)},
            {"FSPage", typeof(FSPage)},
            
        };

        private Dictionary<int, Action> backRequestedMap = new Dictionary<int, Action>
        {
            {0, () => Debug.WriteLine("SerialPortPage")},
            {1, () => Debug.WriteLine("FlashDownloadPage")},
            {2, () => Debug.WriteLine("KeyboardPage")},
            {3, () => Debug.WriteLine("MousePage")},
            {4, () => Debug.WriteLine("FanControlPage")},
            {5, () => Debug.WriteLine("CameraControlPage")},
            {6, () => { if (FSPage.fSPage.MyWebView2.CanGoBack) FSPage.fSPage.MyWebView2.GoBack(); }},
            {7, () => Debug.WriteLine("SettingsPage")},
        };

        void CanvasControl_Draw(
        Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl sender,
        Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs args)
        {
            args.DrawingSession.DrawEllipse(155, 115, 80, 30, Microsoft.UI.Colors.Black, 3);
            args.DrawingSession.DrawText("Hello, Win2D World!", 100, 100, Microsoft.UI.Colors.Yellow);
        }

        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            var selectedItem = (NavigationViewItem)args.SelectedItem;
            string tag = (string)selectedItem.Tag;

            if (pageTypeMap.ContainsKey(tag))
            {
                FSnvf.Navigate(pageTypeMap[tag]);
                FsPage = Array.IndexOf(pageTypeMap.Keys.ToArray(), tag);

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

        private AppWindow m_AppWindow;

        public NavigationFailedEventHandler OnNavigationFailed { get; private set; }
        public static MainWindow Instance { get; private set; }

        public new static Microsoft.UI.Windowing.AppWindow AppWindow { get; set; }
        ///*
        // 窗口的最小宽度和高度
        private const int MinWidth = 642;
        private const int MinHeight = 409;

        // 窗口的默认宽度和高度
        private const int DefaultWidth = 1840;
        private const int DefaultHeight = 960;
        public static bool Resize(Window window, int width, int height)
        {
            try
            {
                IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
                Microsoft.UI.WindowId windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
                AppWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
                AppWindow.Resize(new Windows.Graphics.SizeInt32 { Width = width, Height = height });
                AppWindow.Changed += AppWindow_Changed;

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
            return false;
        }

        private static void AppWindow_Changed(Microsoft.UI.Windowing.AppWindow sender, Microsoft.UI.Windowing.AppWindowChangedEventArgs args)
        {
            try
            {
                if (AppWindow == null)
                    return;

                if (AppWindow.Size.Height < MinHeight && AppWindow.Size.Width < MinWidth)
                    AppWindow.Resize(new Windows.Graphics.SizeInt32 { Width = MinWidth, Height = MinHeight });
                else if (AppWindow.Size.Height < MinHeight)
                    AppWindow.Resize(new Windows.Graphics.SizeInt32 { Width = AppWindow.Size.Width, Height = MinHeight });
                else if (AppWindow.Size.Width < MinWidth)
                    AppWindow.Resize(new Windows.Graphics.SizeInt32 { Width = MinWidth, Height = AppWindow.Size.Height });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }
        //*/

        public void DelayedInitialize(UIElement mainContent)
        {
            Task.Run(() =>
            {
                Thread.Sleep(500);  // 延时改为0.5秒

                // 创建淡出动画
                ///*
                DispatcherQueue.TryEnqueue(() =>
                {
                    Storyboard storyboard = new Storyboard();
                    DoubleAnimation fadeOutAnimation = new DoubleAnimation()
                    {
                        From = 1.0,
                        To = 0.0,
                        Duration = new Duration(TimeSpan.FromMilliseconds(50))
                    };
                    Storyboard.SetTarget(fadeOutAnimation, (ExtendedSplash)App.m_window.Content);
                    Storyboard.SetTargetProperty(fadeOutAnimation, "Opacity");
                    storyboard.Children.Add(fadeOutAnimation);

                    // 播放淡出动画
                    //storyboard.Begin();
                });
                //*/

                Thread.Sleep(1);  // 等待淡出动画完成

                // 移除 ExtendedSplash
                DispatcherQueue.TryEnqueue(() =>
                {
                    ((App)Application.Current).RemoveExtendedSplash(mainContent);
                });
            });
        }


        public MainWindow()
        {
            this.InitializeComponent();
            Instance = this;

            ((FrameworkElement)this.Content).ActualThemeChanged += Window_ThemeChanged;

            bool isFirstActivation = true;
            UIElement mainContent = this.Content;

            ///*
            Resize(this, DefaultWidth, DefaultHeight);
            //*/


            // 将窗口的标题栏设置为自定义标题栏
            this.ExtendsContentIntoTitleBar = true;  // enable custom titlebar
            SetTitleBar(AppTitleBara);

            m_AppWindow = GetAppWindowForCurrentWindow();
            m_AppWindow.Title = FSSoftName;//Set AppWindow
            m_AppWindow.SetIcon("FSFSoftH.ico");

            // 在窗口激活后注册 SizeChanged 事件处理器
            this.Activated += (sender, e) =>
            {
                if (isFirstActivation)
                {
                    ExtendedSplash extendedSplash = new ExtendedSplash(this);
                    this.Content = extendedSplash;
                    if (Window.Current != null)
                    {
                        Window.Current.SizeChanged += extendedSplash.ExtendedSplash_OnResize;
                    }
                }
            };

            using (StreamReader reader = File.OpenText(SettingsCoreServices.FSGravityToolsSettingsToml))
            {
                TomlTable settingstomlr = TOML.Parse(reader);
                string Value = settingstomlr["FSGravitySettings"]["DefaultNavigationViewPaneOpen"];
                defaultNavigationViewPaneOpen = System.Convert.ToBoolean(Value);
                FSnv.IsPaneOpen = defaultNavigationViewPaneOpen;
                Debug.WriteLine("Pane" + defaultNavigationViewPaneOpen);
            }

            //page1Instance = new Page1(); // 初始化Page1实例

            Task.Run(() =>
            {
                SettingsCoreServices.CheckSettingFolder();
                SettingsCoreServices.AddTomlFile();
                SettingsCoreServices.CheckSettingsFileVersion();

                // 在初始化完成后，回到 UI 线程移除 ExtendedSplash
                this.DispatcherQueue.TryEnqueue(() =>
                {
                    LaunageSetting();

                    //设置默认页面
                    using (StreamReader reader = File.OpenText(SettingsCoreServices.FSGravityToolsSettingsToml))
                    {
                        TomlTable settingstomlr = TOML.Parse(reader);
                        Debug.WriteLine("Print:" + settingstomlr["FSGravitySettings"]["DefaultNvPage"]);
                        int NvPage = int.Parse(settingstomlr["FSGravitySettings"]["DefaultNvPage"]);
                        FSnv.SelectedItem = FSnv.MenuItems[NvPage];             //设置默认页面
                        FsPage = NvPage;
                    }
                    using (StreamReader reader = File.OpenText(SettingsCoreServices.FSGravityToolsSettingsToml))
                    {
                        TomlTable settingstomlr = TOML.Parse(reader);
                        Debug.WriteLine("Print:" + settingstomlr["FSGravitySettings"]["SoftBackground"]);
                        int defPageBackground = int.Parse(settingstomlr["FSGravitySettings"]["SoftBackground"]);
                        defWindowBackGround = defPageBackground;
                        lastdefWindowBackGround = defPageBackground;
                    }
                    TitleBarTextBlock.Text = FSSoftName;

                    if (Microsoft.UI.Composition.SystemBackdrops.DesktopAcrylicController.IsSupported())
                    {
                        // 连接策略对象。
                        m_configurationSource = new Microsoft.UI.Composition.SystemBackdrops.SystemBackdropConfiguration();
                        this.Activated += Window_Activated;  // 当窗口被激活时的事件处理。
                        this.Closed += Window_Closed;  // 当窗口被关闭时的事件处理。

                        // 初始配置状态。
                        m_configurationSource.IsInputActive = true;  // 设置输入活动状态为真。
                        SetConfigurationSourceTheme();  // 设置配置源主题。


                        // 根据defWindowBackGround的值选择背景效果的类型。
                        switch (defWindowBackGround)
                        {
                            case 0:
                                m_acrylicController = new Microsoft.UI.Composition.SystemBackdrops.DesktopAcrylicController();
                                m_acrylicController.Kind = Microsoft.UI.Composition.SystemBackdrops.DesktopAcrylicKind.Thin;
                                m_acrylicController.AddSystemBackdropTarget(this.As<Microsoft.UI.Composition.ICompositionSupportsSystemBackdrop>());
                                m_acrylicController.SetSystemBackdropConfiguration(m_configurationSource);
                                break;
                            case 1:
                                m_acrylicController = new Microsoft.UI.Composition.SystemBackdrops.DesktopAcrylicController();
                                m_acrylicController.Kind = Microsoft.UI.Composition.SystemBackdrops.DesktopAcrylicKind.Base;
                                m_acrylicController.AddSystemBackdropTarget(this.As<Microsoft.UI.Composition.ICompositionSupportsSystemBackdrop>());
                                m_acrylicController.SetSystemBackdropConfiguration(m_configurationSource);
                                break;
                            case 2:
                                m_micaController = new Microsoft.UI.Composition.SystemBackdrops.MicaController();
                                m_micaController.Kind = Microsoft.UI.Composition.SystemBackdrops.MicaKind.Base;
                                m_micaController.AddSystemBackdropTarget(this.As<Microsoft.UI.Composition.ICompositionSupportsSystemBackdrop>());
                                m_micaController.SetSystemBackdropConfiguration(m_configurationSource);
                                break;
                            case 3:
                                m_micaController = new Microsoft.UI.Composition.SystemBackdrops.MicaController();
                                m_micaController.Kind = Microsoft.UI.Composition.SystemBackdrops.MicaKind.BaseAlt;
                                m_micaController.AddSystemBackdropTarget(this.As<Microsoft.UI.Composition.ICompositionSupportsSystemBackdrop>());
                                m_micaController.SetSystemBackdropConfiguration(m_configurationSource);
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


            this.Activated += (sender, e) =>
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
        int lastdefWindowBackGround = 0;

        public static bool IsFirstLoadding = true;

        public void LoadingEnd()
        {
            if (IsFirstLoadding) Loadingprg();
        }

        public void Loadingprg()
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
                    IsFirstLoadding = false;
                });

            });
        }

        public void LaunageSetting()
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
                using (StreamReader reader = File.OpenText(SettingsCoreServices.FSGravityToolsSettingsToml)) // 打开TOML文件
                {
                    TomlTable settingstomlr = TOML.Parse(reader);
                    Debug.WriteLine("Print:" + settingstomlr["FSGravitySettings"]["SoftBackground"]);
                    int defPageBackground = int.Parse(settingstomlr["FSGravitySettings"]["SoftBackground"]);
                    if (defPageBackground != lastdefWindowBackGround)
                    {
                        defWindowBackGround = defPageBackground;
                    }
                    lastdefWindowBackGround = defPageBackground;
                }

                SetConfigurationSourceTheme(); // 设置配置源主题。

                // 根据defWindowBackGround的值选择背景效果的类型。
                switch (defWindowBackGround)
                {
                    case 0:
                        if (m_acrylicController == null)
                        {
                            m_acrylicController = new Microsoft.UI.Composition.SystemBackdrops.DesktopAcrylicController();
                        }
                        m_acrylicController.Kind = Microsoft.UI.Composition.SystemBackdrops.DesktopAcrylicKind.Thin;
                        m_acrylicController.AddSystemBackdropTarget(this.As<Microsoft.UI.Composition.ICompositionSupportsSystemBackdrop>());
                        m_acrylicController.SetSystemBackdropConfiguration(m_configurationSource);
                        break;
                    case 1:
                        if (m_acrylicController == null)
                        {
                            m_acrylicController = new Microsoft.UI.Composition.SystemBackdrops.DesktopAcrylicController();
                        }
                        m_acrylicController.Kind = Microsoft.UI.Composition.SystemBackdrops.DesktopAcrylicKind.Base;
                        m_acrylicController.AddSystemBackdropTarget(this.As<Microsoft.UI.Composition.ICompositionSupportsSystemBackdrop>());
                        m_acrylicController.SetSystemBackdropConfiguration(m_configurationSource);
                        break;
                    case 2:
                        if (m_micaController == null)
                        {
                            m_micaController = new Microsoft.UI.Composition.SystemBackdrops.MicaController();
                        }
                        m_micaController.Kind = Microsoft.UI.Composition.SystemBackdrops.MicaKind.Base;
                        m_micaController.AddSystemBackdropTarget(this.As<Microsoft.UI.Composition.ICompositionSupportsSystemBackdrop>());
                        m_micaController.SetSystemBackdropConfiguration(m_configurationSource);
                        break;
                    case 3:
                        if (m_micaController == null)
                        {
                            m_micaController = new Microsoft.UI.Composition.SystemBackdrops.MicaController();
                        }
                        m_micaController.Kind = Microsoft.UI.Composition.SystemBackdrops.MicaKind.BaseAlt;
                        m_micaController.AddSystemBackdropTarget(this.As<Microsoft.UI.Composition.ICompositionSupportsSystemBackdrop>());
                        m_micaController.SetSystemBackdropConfiguration(m_configurationSource);
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
                if (m_acrylicController != null)
                {
                    if (defWindowBackGround == 2 || defWindowBackGround == 3)
                    {
                        //Thread.Sleep(100);
                        m_acrylicController.Dispose();
                        m_acrylicController = null;
                    }
                }
                // 释放m_micaController的资源，并将其设置为null
                if (m_micaController != null)
                {
                    if (defWindowBackGround == 0 || defWindowBackGround == 1)
                    {
                        //Thread.Sleep(100);
                        m_micaController.Dispose();
                        m_micaController = null;
                    }
                }
            });
        }


        // 定义一个DesktopAcrylicController对象，用于控制窗口的背景
        Microsoft.UI.Composition.SystemBackdrops.DesktopAcrylicController m_acrylicController;
        // 定义一个MicaController对象，用于控制云母效果
        Microsoft.UI.Composition.SystemBackdrops.MicaController m_micaController;
        // 定义一个SystemBackdropConfiguration对象，用于配置窗口的背景
        Microsoft.UI.Composition.SystemBackdrops.SystemBackdropConfiguration m_configurationSource;


        // 当窗口被激活时，此方法会被调用
        private void Window_Activated(object sender, WindowActivatedEventArgs args)
        {
            // 根据窗口的激活状态来更新m_configurationSource.IsInputActive的值
            m_configurationSource.IsInputActive = args.WindowActivationState != WindowActivationState.Deactivated;
            // 根据窗口的激活状态来设置背景效果
            if (m_configurationSource.IsInputActive)
            {
                // 使用云母效果
                if (defWindowBackGround == 2 || defWindowBackGround == 3)
                {
                    if (m_micaController == null)
                    {
                        m_micaController = new Microsoft.UI.Composition.SystemBackdrops.MicaController();
                    }
                    m_micaController.Kind = (defWindowBackGround == 2) ? Microsoft.UI.Composition.SystemBackdrops.MicaKind.Base : Microsoft.UI.Composition.SystemBackdrops.MicaKind.BaseAlt;
                    m_micaController.AddSystemBackdropTarget(this.As<Microsoft.UI.Composition.ICompositionSupportsSystemBackdrop>());
                    m_micaController.SetSystemBackdropConfiguration(m_configurationSource);
                }

                // 使用亚克力效果
                if (defWindowBackGround == 0 || defWindowBackGround == 1)
                {
                    if (m_acrylicController == null)
                    {
                        m_acrylicController = new Microsoft.UI.Composition.SystemBackdrops.DesktopAcrylicController();
                    }

                    m_acrylicController.Kind = (defWindowBackGround == 0) ? Microsoft.UI.Composition.SystemBackdrops.DesktopAcrylicKind.Thin : Microsoft.UI.Composition.SystemBackdrops.DesktopAcrylicKind.Base;
                    m_acrylicController.AddSystemBackdropTarget(this.As<Microsoft.UI.Composition.ICompositionSupportsSystemBackdrop>());
                    m_acrylicController.SetSystemBackdropConfiguration(m_configurationSource);
                }
                    
            }

        }

        // 当窗口被关闭时，此方法会被调用
        private void Window_Closed(object sender, WindowEventArgs args)
        {
            // 释放m_acrylicController的资源，并将其设置为null
            if (m_acrylicController != null)
            {
                m_acrylicController.Dispose();
                m_acrylicController = null;
            }
            // 释放m_micaController的资源，并将其设置为null
            if (m_micaController != null)
            {
                m_micaController.Dispose();
                m_micaController = null;
            }
            // 移除窗口激活事件的处理方法
            this.Activated -= Window_Activated;
            // 将配置对象设置为null
            m_configurationSource = null;
        }

        // 当窗口的主题改变时，此方法会被调用
        private void Window_ThemeChanged(FrameworkElement sender, object args)
        {
            Debug.WriteLine("Change");
            // 如果配置对象不为null，则根据当前的主题来更新m_configurationSource.Theme的值
            if (m_configurationSource != null)
            {
                SetConfigurationSourceTheme();
                WindowBackSetting();
            }
        }

        // 根据当前的主题来设置m_configurationSource.Theme的值
        private void SetConfigurationSourceTheme()
        {
            var theme = ((FrameworkElement)this.Content).ActualTheme;
            switch (theme)
            {
                case ElementTheme.Dark:
                    // 如果主题是深色，SystemBackdropTheme会被设置为Dark
                    m_configurationSource.Theme = Microsoft.UI.Composition.SystemBackdrops.SystemBackdropTheme.Dark;
                    break;
                case ElementTheme.Light:
                    // 如果主题是浅色，SystemBackdropTheme会被设置为Light
                    m_configurationSource.Theme = Microsoft.UI.Composition.SystemBackdrops.SystemBackdropTheme.Light;
                    break;
                case ElementTheme.Default:
                    // 如果主题是默认的，SystemBackdropTheme会被设置为Default
                    m_configurationSource.Theme = Microsoft.UI.Composition.SystemBackdrops.SystemBackdropTheme.Default;
                    break;
            }

            var titleBar = m_AppWindow.TitleBar;
            if (theme == ElementTheme.Dark)
            {
                titleBar.ButtonForegroundColor = Colors.White;
                titleBar.ButtonBackgroundColor = Colors.Transparent;
                titleBar.ButtonHoverBackgroundColor = Color.FromArgb(64, 128, 128, 128);
            }
            else
            {
                titleBar.ButtonForegroundColor = Colors.Black;
                titleBar.ButtonBackgroundColor = Colors.Transparent;
                titleBar.ButtonHoverBackgroundColor = Color.FromArgb(64, 128, 128, 128);
            }
        }

        // Call your extend acrylic code in the OnLaunched event, after
        // calling Window.Current.Activate.


        private AppWindow GetAppWindowForCurrentWindow()
        {
            IntPtr hWnd = WindowNative.GetWindowHandle(this);
            Microsoft.UI.WindowId wndId = Win32Interop.GetWindowIdFromWindow(hWnd);
            return AppWindow.GetFromWindowId(wndId);
        }
        private void NavigationView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            if (backRequestedMap.ContainsKey(FsPage))
            {
                backRequestedMap[FsPage]();
            }
        }

        private void NavigationView_DisplayModeChanged(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewDisplayModeChangedEventArgs args)
        {
            if (args.DisplayMode == Microsoft.UI.Xaml.Controls.NavigationViewDisplayMode.Minimal)
            {
                AppTitleBara.Margin = new Thickness(48, 0, 0, 0);
            }
            else
            {
                AppTitleBara.Margin = new Thickness(0, 0, 0, 0);
            }

            UpdateSplitViewDisplayMode();
        }

        private void UpdateSplitViewDisplayMode()
        {
            var currentPage = this.FSnvf.Content as MainPage1;
            if (currentPage != null)
            {
                if (this.FSnv.DisplayMode == NavigationViewDisplayMode.Expanded)
                {
                    currentPage.ChangeSplitViewDisplayMode(SplitViewDisplayMode.CompactInline);
                }
                else
                {
                    currentPage.ChangeSplitViewDisplayMode(SplitViewDisplayMode.CompactOverlay);
                }
            }
        }

        private void FSnv_PaneOpened(NavigationView sender, object args)
        {
            Debug.WriteLine(defaultNavigationViewPaneOpen);
            SettingsCoreServices.SaveSetting("FSGravitySettings", "DefaultNavigationViewPaneOpen", "true");
        }

        private void FSnv_PaneClosed(NavigationView sender, object args)
        {
            Debug.WriteLine(defaultNavigationViewPaneOpen);
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

}
