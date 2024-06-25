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
using Newtonsoft.Json;
using System.Reflection.PortableExecutable;
using Windows.ApplicationModel.Activation;
using System.Xml.Linq;
using Microsoft.UI.Xaml.Media.Animation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FSGaryityTool_Win11
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>

    

    public sealed partial class MainWindow : Window
    {

        public static string FSSoftVersion = "0.2.33";
        public static int FsPage = 0;
        public static TomlTable settingstomlSp;

        private Dictionary<string, Type> pageTypeMap = new Dictionary<string, Type>
        {
            {"MainPage1", typeof(MainPage1)},
            {"Page2", typeof(Page2)},
            {"Page3", typeof(Page3)},
            {"Page4", typeof(Page4)},
            {"Page5", typeof(Page5)},
            {"FSPage", typeof(FSPage)},
        };

        private Dictionary<int, Action> backRequestedMap = new Dictionary<int, Action>
        {
            {0, () => Debug.WriteLine("SerialPortPage")},
            {4, () => { if (FSPage.fSPage.MyWebView2.CanGoBack) FSPage.fSPage.MyWebView2.GoBack(); }},
        };

        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            var selectedItem = (NavigationViewItem)args.SelectedItem;
            string tag = (string)selectedItem.Tag;

            if (pageTypeMap.ContainsKey(tag))
            {
                FSnvf.Navigate(pageTypeMap[tag]);
                FsPage = Array.IndexOf(pageTypeMap.Keys.ToArray(), tag);
            }

            if (args.IsSettingsSelected)
            {
                FSnvf.Navigate(typeof(MainSettingsPage));
                FsPage = 6;
            }
        }

        private AppWindow m_AppWindow;

        public NavigationFailedEventHandler OnNavigationFailed { get; private set; }

        ///*
        [DllImport("user32.dll")]
        public static extern IntPtr DefWindowProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);

        [DllImport("comctl32.dll")]
        public static extern bool SetWindowSubclass(IntPtr hWnd, WndProcDelegate lpfnSubclass, IntPtr uIdSubclass, IntPtr dwRefData);

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int x;
            public int y;
        };

        public struct MINMAXINFO
        {
            public POINT ptReserved;
            public POINT ptMaxSize;
            public POINT ptMaxPosition;
            public POINT ptMinTrackSize;
            public POINT ptMaxTrackSize;
        };

        public const int WM_GETMINMAXINFO = 0x0024;

        public delegate IntPtr WndProcDelegate(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam, IntPtr uIdSubclass, IntPtr dwRefData);
        private static IntPtr WndProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam, IntPtr uIdSubclass, IntPtr dwRefData)
        {
            switch (uMsg)
            {
                case WM_GETMINMAXINFO:
                    MINMAXINFO mmi = Marshal.PtrToStructure<MINMAXINFO>(lParam);
                    mmi.ptMinTrackSize.x = 700;
                    mmi.ptMinTrackSize.y = 200;
                    Marshal.StructureToPtr(mmi, lParam, true);
                    return IntPtr.Zero;
            }

            return DefWindowProc(hWnd, uMsg, wParam, lParam);
        }
        //*/
        public async void DelayedInitialize(UIElement mainContent)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(500));  // 延时改为0.5秒

            // 创建淡出动画
            Storyboard storyboard = new Storyboard();
            DoubleAnimation fadeOutAnimation = new DoubleAnimation()
            {
                From = 1.0,
                To = 0.0,
                Duration = new Duration(TimeSpan.FromMilliseconds(100))
            };
            Storyboard.SetTarget(fadeOutAnimation, (ExtendedSplash)App.m_window.Content);
            Storyboard.SetTargetProperty(fadeOutAnimation, "Opacity");
            storyboard.Children.Add(fadeOutAnimation);

            // 播放淡出动画
            storyboard.Begin();
            await Task.Delay(TimeSpan.FromMilliseconds(1));  // 等待淡出动画完成
            // 移除 ExtendedSplash
            ((App)Application.Current).RemoveExtendedSplash(mainContent);
        }


        public MainWindow()
        {
            this.InitializeComponent();

            bool isFirstActivation = true;
            UIElement mainContent = this.Content;

            ///*
            var app = (Application.Current as App); // 尝试将当前应用程序实例转换为App类型
            if (app != null) // 检查转换是否成功
            {
                var hWnd = app.MainWindowHandle; // 获取主窗口的句柄
                //WindowHelper.SetMinWindowSize(hWnd, 700, 200); // 设置窗口的最小大小为500x500
                SetWindowSubclass(hWnd, WndProc, IntPtr.Zero, IntPtr.Zero);
            }
            //*/

            

            // 将窗口的标题栏设置为自定义标题栏
            this.ExtendsContentIntoTitleBar = true;  // enable custom titlebar
            SetTitleBar(AppTitleBara);

            m_AppWindow = GetAppWindowForCurrentWindow();
            m_AppWindow.Title = "FSGravityTool";//Set AppWindow
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

            string SYSAPLOCAL = Environment.GetFolderPath(folder: Environment.SpecialFolder.LocalApplicationData);
            string FSFolder = Path.Combine(SYSAPLOCAL, "FAIRINGSTUDIO");
            string FSGravif = Path.Combine(FSFolder, "FSGravityTool");
            string FSSetJson = Path.Combine(FSGravif, "Settings.json");
            string FSSetToml = Path.Combine(FSGravif, "Settings.toml");

            //Debug.WriteLine("开始搜索文件夹");
            Debug.WriteLine("开始搜索文件夹  " + FSFolder);            //新建FS文件夹

            
            if (Directory.Exists(FSFolder))
            {
                //Debug.WriteLine("找到文件夹,跳过新建文件夹");
            }
            else
            {
                //Debug.WriteLine("没有找到文件夹");
                Directory.CreateDirectory(FSFolder);
                //Debug.WriteLine("新建文件夹");
            }


            if (Directory.Exists(FSGravif))
            {
                //Debug.WriteLine("找到文件夹,跳过新建文件夹");
            }
            else
            {
                //Debug.WriteLine("没有找到文件夹");
                Directory.CreateDirectory(FSGravif);
                //Debug.WriteLine("新建文件夹");
            }
            

            /*
            if (File.Exists(FSSetJson))
            {
                Debug.WriteLine("找到JSON文件,跳过新建文件");
            }
            else
            {
                Debug.WriteLine("没有找到JSON文件");
                var SettJson = new              //创建对象
                {
                    FSGravity = "Tool",
                    SerialSettings = "1",
                    DefaultBAUD = "115200",
                    DefaultParity = "None",
                    DefaultSTOP = "One",
                    DefaultDATA = "8",
                    DefaultRXHEX = "0",
                    DefaultTXHEX = "0",
                    DefaultDTR = "1",
                    DefaultRTS = "0",
                    DefaultSTime = "0",
                    DefaultAUTOSco = "1",
                };
                var jsonstring1 = JsonConvert.SerializeObject(SettJson);        //序列化Json
                using (StreamWriter file = File.CreateText(FSSetJson))          //创建json
                {
                    file.WriteLine(jsonstring1);
                }
                Debug.WriteLine("新建JSON");
            }
            */

            if (File.Exists(FSSetToml))             //生成TOML
            {
                Debug.WriteLine("找到TOML文件,跳过新建文件");
            }
            else
            {
                Debug.WriteLine("没有找到TOML文件");
                string[] cOMSaveDeviceinf = { "0" };
                TomlTable settingstoml = new TomlTable
                {
                    ["Version"] = FSSoftVersion,

                    ["FSGravitySettings"] =
                    {
                        Comment =
                        "FSGaryityTool Settings:",
                        ["DefaultNvPage"] = "0",
                        ["SoftBackground"] = "0",
                        ["SoftDefLanguage"] = "zh-CN",
                    },

                    ["SerialPortSettings"] =
                    {
                        Comment = 
                        "FSGaryityTool SerialPort Settings:\r\n" +
                        "Parity:None,Odd,Even,Mark,Space\r\n" +
                        "STOPbits:None,One,OnePointFive,Two\r\n" +
                        "DATAbits:5~9",

                        ["DefaultBAUD"] = "115200",
                        ["DefaultParity"] = "None",
                        ["DefaultSTOP"] = "One",
                        ["DefaultDATA"] = "8",
                        ["DefaultRXHEX"] = "0",
                        ["DefaultTXHEX"] = "0",
                        ["DefaultDTR"] = "1",
                        ["DefaultRTS"] = "0",
                        ["DefaultSTime"] = "0",
                        ["DefaultAUTOSco"] = "1",
                        ["AutoDaveSet"] = "1",
                        ["AutoSerichCom"] = "1",
                        ["AutoConnect"] = "1",
                        ["DefaultTXNewLine"] = "1"
                    },

                    ["SerialPortCOMData"] =
                    {
                        Comment =
                        "This is a cache of information for all serial devices.\r\n" + 
                        "",
                        ["CheckTime"] = "2024-04-12 19:48:55",                  //串口设备信息更新的时间
                        ["CheckCounter"] = "0",                                 //串口设备信息更新次数
                        ["COMSaveDeviceinf"] = String.Join(",", cOMSaveDeviceinf),//已保存串口设备的映射表
                    },

                    ["COMData"] =
                    {
                        Comment =
                        "This is an example of cached serial device information.\r\n",
                        ["COM0"] =
                        {
                            ["Icon"] = "\uE88E",                            //串口设备自定义的图标
                            ["Description"] = "An example of a serial device format",                       //串口设备描述
                            ["Name"] = "example",                              //串口设备名字
                            ["Manufacturer"] = "FairingStudio",             //串口设备制造商
                            ["RSTBaudRate"] = "115200",                     //自动重启上电打印波特率
                            ["RSTTime"] = "300",                            //自动重启上电打印延时
                            ["RSTMode"] = "0",                              //重启模式
                        },

                    },

                };

                using (StreamWriter writer = File.CreateText(FSSetToml))
                {
                    settingstoml.WriteTo(writer);
                    Debug.WriteLine("写入Toml");
                    // Remember to flush the data if needed!
                    writer.Flush();
                }
                Debug.WriteLine("新建TOML");
            }



            string TomlfsVersion;       //版本号比较

            using (StreamReader reader = File.OpenText(FSSetToml))
            {
                TomlTable settingstomlr = TOML.Parse(reader);
                TomlfsVersion = settingstomlr["Version"];
            }

            Version TomlVersion = new Version(TomlfsVersion);
            Version FSGrVersion = new Version(FSSoftVersion);

            if (FSGrVersion > TomlVersion)              //Settings.Toml版本管理
            {
                Debug.WriteLine(">");

                //缓存设置
                string defpage,defPageBackground,defLaunage;
                string baud, party, stop, data, rxhex, txhex, dtr, rts, shtime, autosco, autosavrset, autosercom, autoconnect, txnewline;
                string checkTime, checkCounter;

                string[] cOMSaveDeviceinf = {"0","1"};
                string cOMDeviceinf;

                string serialPortSettings = "SerialPortSettings";

                string TomlCheckNulls(string Mode, string Menu , string Name)
                {
                    string data = "0";
                    using (StreamReader reader = File.OpenText(Page1.FSSetToml))
                    {
                        TomlTable SPsettingstomlr = TOML.Parse(reader);             //读取TOML

                        if (SPsettingstomlr[Menu][Name] != "Tommy.TomlLazy") data = SPsettingstomlr[Menu][Name];
                        else
                        {
                            data = Mode;
                        }
                    }
                    return data;
                }

                using (StreamReader reader = File.OpenText(Page1.FSSetToml))                    //打开TOML文件
                {
                    settingstomlSp = TOML.Parse(reader);

                    if ((string)settingstomlSp["FSGravitySettings"]["DefaultNvPage"] != "Tommy.TomlLazy") defpage = settingstomlSp["FSGravitySettings"]["DefaultNvPage"];
                    else defpage = "0";
                    if ((string)settingstomlSp["FSGravitySettings"]["SoftBackground"] != "Tommy.TomlLazy") defPageBackground = settingstomlSp["FSGravitySettings"]["SoftBackground"];
                    else defPageBackground = "0";

                    var culture = System.Globalization.CultureInfo.CurrentUICulture;
                    string lang = culture.Name;

                    if ((string)settingstomlSp["FSGravitySettings"]["SoftDefLanguage"] != "Tommy.TomlLazy") defLaunage = settingstomlSp["FSGravitySettings"]["SoftDefLanguage"];
                    else defLaunage = lang;

                    baud = TomlCheckNulls("115200", serialPortSettings, "DefaultBAUD");
                    party = TomlCheckNulls("None", serialPortSettings, "DefaultParity");
                    stop = TomlCheckNulls("One", serialPortSettings, "DefaultSTOP");
                    data = TomlCheckNulls("8", serialPortSettings, "DefaultDATA");
                    
                    rxhex = TomlCheckNulls("0", serialPortSettings, "DefaultRXHEX");
                    txhex = TomlCheckNulls("0", serialPortSettings, "DefaultTXHEX");
                    dtr = TomlCheckNulls("1", serialPortSettings, "DefaultDTR");
                    rts = TomlCheckNulls("0", serialPortSettings, "DefaultRTS");
                    shtime = TomlCheckNulls("0", serialPortSettings, "DefaultSTime");
                    autosco = TomlCheckNulls("1", serialPortSettings, "DefaultAUTOSco");
                    autosavrset = TomlCheckNulls("1", serialPortSettings, "AutoDaveSet");
                    autosercom = TomlCheckNulls("1", serialPortSettings, "AutoSerichCom");
                    autoconnect = TomlCheckNulls("1", serialPortSettings, "AutoConnect");
                    txnewline = TomlCheckNulls("1", serialPortSettings, "DefaultTXNewLine");

                    //if (settingstomlSp["SerialPortSettings"] != null)  = ;

                    if ((string)settingstomlSp["SerialPortCOMData"]["CheckTime"] != "Tommy.TomlLazy") checkTime = settingstomlSp["SerialPortCOMData"]["CheckTime"];
                    else checkTime = "2024-04-12 19:48:55";
                    if ((string)settingstomlSp["SerialPortCOMData"]["CheckCounter"] != "Tommy.TomlLazy") checkCounter = settingstomlSp["SerialPortCOMData"]["CheckCounter"];
                    else checkCounter = "0";
                    if ((string)settingstomlSp["SerialPortCOMData"]["COMSaveDeviceinf"] != "Tommy.TomlLazy") cOMDeviceinf = settingstomlSp["SerialPortCOMData"]["COMSaveDeviceinf"];
                    else cOMDeviceinf = "0";


                    settingstomlSp = new TomlTable
                    {
                        ["Version"] = FSSoftVersion,

                        ["FSGravitySettings"] =
                        {
                            Comment =
                            "FSGaryityTool Settings:",
                            ["DefaultNvPage"] = defpage,
                            ["SoftBackground"] = defPageBackground,
                            ["SoftDefLanguage"] = defLaunage,
                        },

                        ["SerialPortSettings"] =
                        {
                            Comment =
                            "FSGaryityTool SerialPort Settings:\r\n" +
                            "Parity:None,Odd,Even,Mark,Space\r\n" +
                            "STOPbits:None,One,OnePointFive,Two\r\n" +
                            "DATAbits:5~9",

                            ["DefaultBAUD"] = baud,
                            ["DefaultParity"] = party,
                            ["DefaultSTOP"] = stop,
                            ["DefaultDATA"] = data,
                            ["DefaultRXHEX"] = rxhex,
                            ["DefaultTXHEX"] = txhex,
                            ["DefaultDTR"] = dtr,
                            ["DefaultRTS"] = rts,
                            ["DefaultSTime"] = shtime,
                            ["DefaultAUTOSco"] = autosco,
                            ["AutoDaveSet"] = autosavrset,
                            ["AutoSerichCom"] = autosercom,
                            ["AutoConnect"] = autoconnect,
                            ["DefaultTXNewLine"] = txnewline,
                        },

                        ["SerialPortCOMData"] =
                        {
                            Comment =
                            "This is a cache of information for all serial devices.\r\n",

                            ["CheckTime"] = "2024-04-12 19:48:55",
                            ["CheckCounter"] = "0",
                            ["COMSaveDeviceinf"] = cOMDeviceinf//String.Join(",", cOMSaveDeviceinf)
                        },
                        ["COMData"] =
                        {
                            Comment =
                            "This is an example of cached serial device information.\r\n",
                            ["COM0"] =
                            {
                                ["Icon"] = "uE88E",
                                ["Description"] = "An example of a serial device format",
                                ["Name"] = "example",
                                ["Manufacturer"] = "FairingStudio",
                                ["RSTBaudRate"] = "115200",
                                ["RSTTime"] = "300",
                                ["RSTMode"] = "0",
                            },
                        },

                    };

                }
                //更新Toml
                using (StreamWriter writer = File.CreateText(Page1.FSSetToml))                  //将设置写入TOML文件
                {
                    settingstomlSp.WriteTo(writer);
                    //Debug.WriteLine("写入Toml" + settingstomlSp["FSGravitySettings"]["DefaultNvPage"]);
                    // Remember to flush the data if needed!
                    writer.Flush();
                }
            }
            else
            {
                Debug.WriteLine("<=");

            }

            SerialPortI.Content = Page1.LanguageText("serialPort");
            DownFlashI.Content = Page1.LanguageText("download Flash");
            KeyboardI.Content = Page1.LanguageText("keyboard");
            MouseI.Content = Page1.LanguageText("mouse");

            this.Activated += (sender, e) =>
            {
                if (isFirstActivation)
                {
                    DelayedInitialize(mainContent);
                    isFirstActivation = false;
                }
            };

            //设置默认页面
            using (StreamReader reader = File.OpenText(FSSetToml))
            {
                TomlTable settingstomlr = TOML.Parse(reader);
                Debug.WriteLine("Print:" + settingstomlr["FSGravitySettings"]["DefaultNvPage"]);
                int NvPage = int.Parse(settingstomlr["FSGravitySettings"]["DefaultNvPage"]);
                FSnv.SelectedItem = FSnv.MenuItems[NvPage];             //设置默认页面
                FsPage = NvPage;
            }

            

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

            TitleBarTextBlock.Text = "FSGravityTool";
            //SystemBackdrop = new MicaBackdrop()
            //{ Kind = MicaKind.BaseAlt };

            //SystemBackdrop = new DesktopAcrylicBackdrop();

            if (Microsoft.UI.Composition.SystemBackdrops.DesktopAcrylicController.IsSupported())
            {
                bool useAcrylicThin = true;
                // Hooking up the policy object.
                m_configurationSource = new Microsoft.UI.Composition.SystemBackdrops.SystemBackdropConfiguration();
                this.Activated += Window_Activated;
                this.Closed += Window_Closed;
                ((FrameworkElement)this.Content).ActualThemeChanged += Window_ThemeChanged;

                // Initial configuration state.
                m_configurationSource.IsInputActive = true;
                SetConfigurationSourceTheme();

                m_acrylicController = new Microsoft.UI.Composition.SystemBackdrops.DesktopAcrylicController();

                m_acrylicController.Kind = useAcrylicThin ? Microsoft.UI.Composition.SystemBackdrops.DesktopAcrylicKind.Thin : Microsoft.UI.Composition.SystemBackdrops.DesktopAcrylicKind.Base;

                // Enable the system backdrop.
                // Note: Be sure to have "using WinRT;" to support the Window.As<...>() call.
                m_acrylicController.AddSystemBackdropTarget(this.As<Microsoft.UI.Composition.ICompositionSupportsSystemBackdrop>());
                m_acrylicController.SetSystemBackdropConfiguration(m_configurationSource);
            }


        }

        // 定义一个DesktopAcrylicController对象，用于控制窗口的背景
        Microsoft.UI.Composition.SystemBackdrops.DesktopAcrylicController m_acrylicController;
        // 定义一个SystemBackdropConfiguration对象，用于配置窗口的背景
        Microsoft.UI.Composition.SystemBackdrops.SystemBackdropConfiguration m_configurationSource;

        // 当窗口被激活时，此方法会被调用
        private void Window_Activated(object sender, WindowActivatedEventArgs args)
        {
            // 根据窗口的激活状态来更新m_configurationSource.IsInputActive的值
            m_configurationSource.IsInputActive = args.WindowActivationState != WindowActivationState.Deactivated;
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
            // 移除窗口激活事件的处理方法
            this.Activated -= Window_Activated;
            // 将配置对象设置为null
            m_configurationSource = null;
        }

        // 当窗口的主题改变时，此方法会被调用
        private void Window_ThemeChanged(FrameworkElement sender, object args)
        {
            // 如果配置对象不为null，则根据当前的主题来更新m_configurationSource.Theme的值
            if (m_configurationSource != null)
            {
                SetConfigurationSourceTheme();
            }
        }

        // 根据当前的主题来设置m_configurationSource.Theme的值
        private void SetConfigurationSourceTheme()
        {
            switch (((FrameworkElement)this.Content).ActualTheme)
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
        }

        // Call your extend acrylic code in the OnLaunched event, after
        // calling Window.Current.Activate.


        private AppWindow GetAppWindowForCurrentWindow()
        {
            IntPtr hWnd = WindowNative.GetWindowHandle(this);
            WindowId wndId = Win32Interop.GetWindowIdFromWindow(hWnd);
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
