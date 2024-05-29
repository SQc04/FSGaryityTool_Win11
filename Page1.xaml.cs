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
using System.IO.Ports;
using System.Management;
using System.Text;
using Windows.Networking.Sockets;
using System.Collections.ObjectModel;
using Microsoft.UI.Composition.SystemBackdrops;

using Windows.UI.Popups;
using System.Threading;

using System.Resources;
using System.Reflection;
using Microsoft.Win32;

using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams;
using System.Threading.Tasks;
using Microsoft.UI;           // Needed for WindowId.
using Microsoft.UI.Windowing; // Needed for AppWindow.
using Microsoft.UI.Dispatching;
using WinRT.Interop;
using Windows.UI;          // Needed for XAML/HWND interop.
using Windows.ApplicationModel.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Core;
using System.Xml.Linq;
using System.Diagnostics.Metrics;

using Tommy;
using System.Diagnostics;
using static System.Runtime.CompilerServices.RuntimeHelpers;

using System.Windows.Input;
using Windows.ApplicationModel.Contacts;
using System.Reflection.Metadata.Ecma335;
using Windows.Devices.Sensors;
using Windows.ApplicationModel.DataTransfer;
using System.Reflection.Metadata;
using System.Globalization;
using Microsoft.Windows.ApplicationModel.Resources;
using System.Timers;
using System.Text.RegularExpressions;
using CommunityToolkit.WinUI;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FSGaryityTool_Win11
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    /// 
    public class DataItem
    {
        public string Timesr { get; set; }
        public string Rxstr { get; set; }
    }
    /*
    public class ComDataItem
    {
        public string ComName { get; set; }
    }
    */

    public sealed partial class Page1 : Page
    {
        public static int Con = 0;
        public static string ConCom = "";
        public static int txf = 0;
        public static int tx = 0; //TXHEX
        public static int rx = 0; //RXHEX
        public static int dtr = 0;//FTR
        public static int rts = 0;//RTS
        public static int shtime = 0;//ShowTime
        public static int autotr = 0;//AUTOScroll
        public static int autosaveset;
        public static int autosercom;
        public static int autoconnect;
        public static int txnewline = 0;

        public static int rxs = 0;
        public static string[] ArryPort; //定义字符串数组，数组名为 ArryPort
        public static string rxpstr;
        public static StringBuilder datapwate = new StringBuilder(2048);

        public static int Rollta = 0;

        public static int RunPBT = 0;
        public static int RunT = 0;

        public static string SYSAPLOCAL = Environment.GetFolderPath(folder: Environment.SpecialFolder.LocalApplicationData);
        public static string FSFolder = Path.Combine(SYSAPLOCAL, "FAIRINGSTUDIO");
        public static string FSGravif = Path.Combine(FSFolder, "FSGravityTool");
        public static string FSSetJson = Path.Combine(FSGravif, "Settings.json");
        public static string FSSetToml = Path.Combine(FSGravif, "Settings.toml");


        public static TomlTable settingstomlr;

        public System.Threading.Timer timer;
        public System.Threading.Timer timerSerialPort;

        private bool _isLoaded;
        public static string str;
        public DateTime current_time = new DateTime();

        public static class CommonRes
        {
            public static SerialPort _serialPort = new SerialPort();
            public static SerialPort serialPort2 = new SerialPort();

        }



        private ITaskbarList3 taskbarInstance;

        public Page1()
        {
            this.InitializeComponent();

            this.Loaded += Page1_Loaded;

            this.taskbarInstance = (ITaskbarList3)new TaskbarList();
            this.taskbarInstance.HrInit();

        }

        public static string LanguageText(string laugtext)
        {
            var culture = System.Globalization.CultureInfo.CurrentUICulture;
            string lang = culture.Name;

            // 创建一个字典来存储不同语言和对应的资源文件之间的映射
            var resourceFiles = new Dictionary<string, string>
            {
                { "zh-CN", "FSGaryityTool_Win11.Resources.zh_CN.resource" },
                { "en-US", "FSGaryityTool_Win11.Resources.en_US.resource" },
            //  { "xx-xx", "FSGaryityTool_Win11.Resources.xx_xx.resource" },
            };

            // 如果当前语言不在字典中，则默认使用 "zh-CN" 的资源文件
            if (!resourceFiles.ContainsKey(lang))
            {
                lang = "zh-CN";
            }

            var rm = new System.Resources.ResourceManager(resourceFiles[lang], Assembly.GetExecutingAssembly());
            string text = rm.GetString(laugtext);
            return text;
        }


        private void ToggleButtonIsChecked(int isChecked, ToggleButton toggleButton)
        {
            if (isChecked == 1)
            {
                toggleButton.IsChecked = true;
            }
            else
            {
                toggleButton.IsChecked = false;
            }
        }

        private void FsButtonIsChecked(int isChecked, Button button)
        {
            var foregroundColor = COMButton.Foreground as SolidColorBrush;
            var backgroundColor = COMButton.Background as SolidColorBrush;
            var darkaccentColor = (Windows.UI.Color)Application.Current.Resources["SystemAccentColorLight2"];
            var ligtaccentColor = (Windows.UI.Color)Application.Current.Resources["SystemAccentColorDark1"];
            var theme = Application.Current.RequestedTheme;

            if (isChecked == 1)
            {
                if (theme == ApplicationTheme.Dark)
                {
                    // 当前处于深色模式
                    button.Background = new SolidColorBrush(darkaccentColor);
                    button.Foreground = new SolidColorBrush(Colors.Black);
                }
                else if (theme == ApplicationTheme.Light)
                {
                    // 当前处于浅色模式
                    button.Background = new SolidColorBrush(ligtaccentColor);
                    button.Foreground = new SolidColorBrush(Colors.White);
                }
            }
            else
            {
                button.Background = backgroundColor;
                button.Foreground = foregroundColor;
            }
        }

        private void FsBorderIsChecked(int isChecked, Border border, TextBlock textBlock)
        {
            var foregroundColor = COMButton.Foreground as SolidColorBrush;
            var backgroundColor = (SolidColorBrush)Application.Current.Resources["LayerOnAcrylicFillColorDefaultBrush"];
            var darkaccentColor = (Windows.UI.Color)Application.Current.Resources["SystemAccentColorLight2"];
            var ligtaccentColor = (Windows.UI.Color)Application.Current.Resources["SystemAccentColorDark1"];
            var theme = Application.Current.RequestedTheme;

            if (isChecked == 1)
            {
                if (theme == ApplicationTheme.Dark)
                {
                    // 当前处于深色模式
                    border.Background = new SolidColorBrush(darkaccentColor);
                    textBlock.Foreground = new SolidColorBrush(Colors.Black);
                }
                else if (theme == ApplicationTheme.Light)
                {
                    // 当前处于浅色模式
                    border.Background = new SolidColorBrush(ligtaccentColor);
                    textBlock.Foreground = new SolidColorBrush(Colors.White);
                }
            }
            else
            {
                border.Background = backgroundColor;
                textBlock.Foreground = foregroundColor;
            }
        }

        private int TomlCheckNull(string Menu, string Name)
        {
            int data = 0;
            using (StreamReader reader = File.OpenText(FSSetToml))
            {
                TomlTable SPsettingstomlr = TOML.Parse(reader);             //读取TOML

                if (SPsettingstomlr[Menu][Name] != "Tommy.TomlLazy") data = int.Parse(SPsettingstomlr[Menu][Name]);
                else data = 0;
            }
            return data;
        }

        private void Page1_Loaded(object sender, RoutedEventArgs e)
        {
            if (!_isLoaded)
            {
                _isLoaded = true;

                string DefaultBAUD;
                string DefaultPart;
                string DefaultSTOP;
                int DefaultDATA;

                using (StreamReader reader = File.OpenText(FSSetToml))
                {
                    TomlTable SPsettingstomlr = TOML.Parse(reader);             //读取TOML
                                                                                //Debug.WriteLine("Print:" + SPsettingstomlr["FSGravitySettings"]["DefaultNvPage"]);
                                                                                //NvPage = int.Parse(settingstomlr["FSGravitySettings"]["DefaultNvPage"]);
                    string spSettings = "SerialPortSettings";
                                                                                //检查设置是否为NULL
                    if (SPsettingstomlr[spSettings]["DefaultBAUD"] != "Tommy.TomlLazy") DefaultBAUD = SPsettingstomlr[spSettings]["DefaultBAUD"];
                    else DefaultBAUD = "115200";
                    if (SPsettingstomlr[spSettings]["DefaultParity"] != "Tommy.TomlLazy") DefaultPart = SPsettingstomlr[spSettings]["DefaultParity"];
                    else DefaultPart = "None";
                    if (SPsettingstomlr[spSettings]["DefaultSTOP"] != "Tommy.TomlLazy") DefaultSTOP = SPsettingstomlr[spSettings]["DefaultSTOP"];
                    else DefaultSTOP = "One";
                    if (SPsettingstomlr[spSettings]["DefaultDATA"] != "Tommy.TomlLazy") DefaultDATA = int.Parse(SPsettingstomlr[spSettings]["DefaultDATA"]);
                    else DefaultDATA = 8;

                    tx = TomlCheckNull(spSettings, "DefaultTXHEX");
                    rx = TomlCheckNull(spSettings, "DefaultRXHEX");
                    dtr = TomlCheckNull(spSettings, "DefaultDTR");
                    rts = TomlCheckNull(spSettings, "DefaultRTS");
                    shtime = TomlCheckNull(spSettings, "DefaultSTime");
                    autotr = TomlCheckNull(spSettings, "DefaultAUTOSco");
                    autosaveset = TomlCheckNull(spSettings, "AutoDaveSet");
                    autosercom = TomlCheckNull(spSettings, "AutoSerichCom");
                    autoconnect = TomlCheckNull(spSettings, "AutoConnect");
                    txnewline = TomlCheckNull(spSettings, "DefaultTXNewLine");

                    /*
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
                    */
                }

                RXListView.ItemsSource = new ObservableCollection<DataItem>();

                //COMListview.ItemsSource = new ObservableCollection<ComDataItem>();

                BaudTextBlock.Text = LanguageText("baudRatel");
                PartTextBlock.Text = LanguageText("parityl");
                StopTextBlock.Text = LanguageText("stopBits");
                DataTextBlock.Text = LanguageText("dataBits");
                RXHEXButton.Content = LanguageText("rxHexl");
                TXHEXButton.Content = LanguageText("txHexl");
                TXNewLineButton.Content = LanguageText("txNewLinel");
                SaveSetButton.Content = LanguageText("autoSaveSetl");
                AUTOScrollButton.Content = LanguageText("autoScrolll");
                AutoComButton.Content = LanguageText("autoSerichComl");
                AutoConnectButton.Content = LanguageText("autoConnectl");
                CONTButton.Content = LanguageText("connectl");
                COMRstInfoBar.Message = LanguageText("comRstInfoBar");

                CommonRes._serialPort.DataReceived += _serialPort_DataReceived;
                // 在你的代码后台，定义一个List<string>作为数据源
                List<string> BaudRates = new List<string>()
                {
                    "75", "110", "134", "150", "300", "600", "1200", "1800", "2400", "4800", "7200", "9600", "14400", "19200", "38400", "57600", "74880","115200", "128000", "230400", "250000", "500000", "1000000", "2000000"
                };
                // 将ComboBox的ItemsSource属性绑定到这个数据源
                BANDComboBox.ItemsSource = BaudRates;
                // 设置默认选项
                BANDComboBox.SelectedItem = DefaultBAUD; // 将"9600"设置为默认选项

                List<string> ParRates = new List<string>()
                {
                    "None", "Odd", "Even", "Mark", "Space"
                };
                PARComboBox.ItemsSource = ParRates;
                PARComboBox.SelectedItem = DefaultPart;

                List<string> StopRates = new List<string>()
                {
                    "None", "One", "OnePointFive", "Two"
                };
                STOPComboBox.ItemsSource = StopRates;
                STOPComboBox.SelectedItem = DefaultSTOP;

                for (int j = 5; j < 10; ++j)
                {
                    DATAComboBox.Items.Add(j);
                }
                DATAComboBox.SelectedItem = DefaultDATA;

                var foregroundColor = COMButton.Foreground as SolidColorBrush;
                var backgroundColor = COMButton.Background as SolidColorBrush;
                //var darkaccentColor = (Windows.UI.Color)Application.Current.Resources["SystemAccentColorLight2"];
                //var ligtaccentColor = (Windows.UI.Color)Application.Current.Resources["SystemAccentColorDark1"];
                //var theme = Application.Current.RequestedTheme;

                /*
                if (theme == ApplicationTheme.Dark)
                {
                    // 当前处于深色模式
                    DTRButton.Background = new SolidColorBrush(darkaccentColor);
                    DTRButton.Foreground = new SolidColorBrush(Colors.Black);
                }
                else if (theme == ApplicationTheme.Light)
                {
                    // 当前处于浅色模式
                    DTRButton.Background = new SolidColorBrush(ligtaccentColor);
                    DTRButton.Foreground = new SolidColorBrush(Colors.White);
                }
                CommonRes._serialPort.DtrEnable = true;

                if (theme == ApplicationTheme.Dark)
                {
                    // 当前处于深色模式
                    AUTOScrollButton.Background = new SolidColorBrush(darkaccentColor);
                    AUTOScrollButton.Foreground = new SolidColorBrush(Colors.Black);
                }
                else if (theme == ApplicationTheme.Light)
                {
                    // 当前处于浅色模式
                    AUTOScrollButton.Background = new SolidColorBrush(ligtaccentColor);
                    AUTOScrollButton.Foreground = new SolidColorBrush(Colors.White);
                }
                */
                ToggleButtonIsChecked(rx, RXHEXButton);
                ToggleButtonIsChecked(tx, TXHEXButton);

                ToggleButtonIsChecked(dtr, DTRButton);
                if (dtr == 1)
                {
                    CommonRes._serialPort.DtrEnable = true;
                }
                else
                {
                    CommonRes._serialPort.DtrEnable = false;
                }

                ToggleButtonIsChecked(rts, RTSButton);
                if (rts == 1)
                {
                    CommonRes._serialPort.RtsEnable = true;
                }
                else
                {
                    CommonRes._serialPort.RtsEnable = false;
                }

                ToggleButtonIsChecked(shtime, ShowTimeButton);
                ToggleButtonIsChecked(autotr, AUTOScrollButton);

                RunProgressBar.Visibility = Visibility.Collapsed;

                //BorderBackRX.Background = backgroundColor;

                ToggleButtonIsChecked(autosaveset, SaveSetButton);
                ToggleButtonIsChecked(autoconnect, AutoConnectButton);
                ToggleButtonIsChecked(txnewline, TXNewLineButton);

                
                COMButton_Click(this, new RoutedEventArgs());
            }


            /*
            // 创建一个DispatcherQueueTimer对象
            DispatcherQueueTimer timer = DispatcherQueue.GetForCurrentThread().CreateTimer();

            // 在你的代码中初始化这个DispatcherQueueTimer
            timer.Interval = TimeSpan.FromMilliseconds(500); // 注意这里的间隔时间是250毫秒，也就是每秒触发四次
            timer.Tick += (sender, args) =>
            {
                // 在这里调用你的按钮点击事件
                AUTOScrollButton_ClickAsync(null, null);
            };
            timer.Start();
            */

            ToggleButtonIsChecked(autosercom, AutoComButton);
            if (autosercom == 1)
            {
                timerSerialPort = new System.Threading.Timer(TimerSerialPortTick, null, 0, 1500);
                AutoSerchComProgressRing.IsActive = true;
            }
            else AutoSerchComProgressRing.IsActive = false;

            ToggleButtonIsChecked(autoconnect, AutoConnectButton);
            //ToggleButtonIsChecked();


        }

        public void TimerTick(Object stateInfo)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                RXDATA_ClickAsync(null, null);

                if (RunT == 0) RunPBT += 2;

                else RunPBT -= 2;

                RunTProgressBar.Value = RunPBT;
                if (RunPBT == 100)
                {
                    RunT = 1;
                }
                else if (RunPBT == 0)
                {
                    RunT = 0;
                }


                if (CommonRes._serialPort.IsOpen == true)
                {
                    if (CommonRes._serialPort.DsrHolding == true)
                    {
                        FsBorderIsChecked(1, DSRBorder, DSRTextBlock);
                    }
                    else
                    {
                        FsBorderIsChecked(0, DSRBorder, DSRTextBlock);
                    }
                    if (CommonRes._serialPort.CtsHolding == true)
                    {
                        FsBorderIsChecked(1, CTSBorder, CTSTextBlock);
                    }
                    else
                    {
                        FsBorderIsChecked(0, CTSBorder, CTSTextBlock);
                    }
                    if (CommonRes._serialPort.CDHolding == true)
                    {
                        FsBorderIsChecked(1, CDHBorder, CDHTextBlock);
                    }
                    else
                    {
                        FsBorderIsChecked(0, CDHBorder, CDHTextBlock);
                    }
                }

            });

        }
        private void PinChanged(object sender, SerialPinChangedEventArgs e)
        {
            if (e.EventType == System.IO.Ports.SerialPinChange.Ring)
            {
                int RI = 0;
                if(RI == 0)
                {
                    // RI 信号使能
                    FsBorderIsChecked(1, RIBorder, RITextBlock);
                    RI = 1;
                }
                else
                {
                    // RI 信号未使能
                    FsBorderIsChecked(0, RIBorder, RITextBlock);
                    RI = 0;
                }
            }
        }

        public class SerialPortInfo
        {
            public string Name { get; protected set; }
            public string Description { get; protected set; }
            public string Manufacturer { get; protected set; }

            private static Dictionary<string, SerialPortInfo> portInfoDictionary = new Dictionary<string, SerialPortInfo>();

            static SerialPortInfo()
            {
                // 在应用程序启动时获取所有串口的设备描述
                RefreshPortInfo();
            }

            public static void RefreshPortInfo(string portName = null)
            {
                string queryString = "SELECT * FROM Win32_PnPEntity";
                if (portName != null)
                {
                    queryString += $" WHERE Name LIKE '%{portName}%'";
                }

                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(queryString))
                {
                    var hardInfos = searcher.Get();
                    foreach (var hardInfo in hardInfos)
                    {
                        if ((hardInfo.Properties["Name"].Value != null) &&
                            (hardInfo.Properties["Name"].Value.ToString().Contains("COM")))
                        {
                            SerialPortInfo temp = new SerialPortInfo();
                            string s = hardInfo.Properties["Name"].Value.ToString();
                            int p = s.IndexOf('(');
                            temp.Description = s.Substring(0, p);
                            temp.Name = s.Substring(p + 1, s.Length - p - 2);
                            temp.Manufacturer = hardInfo.Properties["Manufacturer"].Value.ToString();
                            portInfoDictionary[temp.Name] = temp;
                        }
                    }
                }
            }

            public static SerialPortInfo GetPort(string portName)
            {
                if (!portInfoDictionary.ContainsKey(portName))
                {
                    // 如果字典中没有指定的串口名称，刷新串口信息
                    RefreshPortInfo(portName);
                }

                if (portInfoDictionary.ContainsKey(portName))
                {
                    return portInfoDictionary[portName];
                }

                return null;  // 如果没有找到匹配的串口，返回null
            }
        }


        /*
        public static string GetPortDescription(string portName)
        {
            string description = null;

            RegistryKey localMachineRegistry = Registry.LocalMachine;
            RegistryKey hardwareRegistry = localMachineRegistry.OpenSubKey("HARDWARE\\DEVICEMAP\\SERIALCOMM");

            if (hardwareRegistry != null)
            {
                string[] deviceNames = hardwareRegistry.GetValueNames();
                foreach (string deviceName in deviceNames)
                {
                    if (hardwareRegistry.GetValue(deviceName).ToString().Equals(portName))
                    {
                        string instanceName = deviceName.Substring(deviceName.IndexOf("\\Device\\") + "\\Device\\".Length);
                        RegistryKey deviceRegistry = localMachineRegistry.OpenSubKey("SYSTEM\\CurrentControlSet\\Enum\\" + instanceName + "\\Device Parameters");
                        if (deviceRegistry != null)
                        {
                            description = (string)deviceRegistry.GetValue("PortName");
                        }
                    }
                }
            }

            return description;
        }
        */

        public void TimerSerialPortTick(Object stateInfo)       //串口热插拔检测
        {
            string[] NowPort = SerialPort.GetPortNames(); // 获取当前所有可用的串口名称
            string[] LastPort = ArryPort ?? NowPort; // 获取上一次检测到的串口名称，如果没有则使用当前串口名称
            ArryPort = NowPort; // 更新上一次检测到的串口名称

            var lastPortSet = new HashSet<string>(LastPort); // 创建一个包含上一次串口名称的HashSet
            var nowPortSet = new HashSet<string>(NowPort); // 创建一个包含当前串口名称的HashSet

            var insertedPorts = nowPortSet.Except(lastPortSet).ToArray(); // 找出新插入的串口
            var removedPorts = lastPortSet.Except(nowPortSet).ToArray(); // 找出被拔出的串口

            DispatcherQueue.TryEnqueue(() => // 在UI线程中执行以下操作
            {
                string selectedPort = (string)COMComboBox.SelectedItem; // 获取当前选中的串口

                foreach (var port in insertedPorts) // 遍历所有新插入的串口
                {
                    SerialPortInfo info = SerialPortInfo.GetPort(port); // 获取串口的信息
                    RXTextBox.Text += $"{port}: {info.Description} {LanguageText("spPlogin")}\r\n"; // 更新文本框的内容
                }

                foreach (var port in removedPorts) // 遍历所有被拔出的串口
                {
                    RXTextBox.Text += $"{port}{LanguageText("spPullout")}\r\n"; // 更新文本框的内容
                    if (Con == 1 && port == selectedPort) // 如果当前连接的串口被拔出，则断开连接
                    {
                        CONTButton_Click(null, null);
                    }
                }

                COMComboBox.Items.Clear(); // 清空组合框的内容
                COMListview.Items.Clear(); // 清空列表视图的内容

                foreach (var port in NowPort) // 遍历当前所有可用的串口
                {
                    COMComboBox.Items.Add(port); // 将串口名称添加到组合框中
                    COMListview.Items.Add(port); // 将串口名称添加到列表视图中
                }

                COMComboBox.SelectedItem = selectedPort; // 将之前选中的串口重新选中
                COMListview.SelectedItem = selectedPort; // 将之前选中的串口重新选中

                if (Con == 0 && COMComboBox.SelectedItem == null && insertedPorts.Length > 0) // 如果没有选中的串口，并且有新插入的串口
                {
                    COMComboBox.SelectedItem = insertedPorts[0]; // 选中新插入的串口
                    COMListview.SelectedItem = insertedPorts[0]; // 选中新插入的串口
                    if (AutoConnectButton.IsChecked == true) // 如果设置了自动连接，则尝试连接新插入的串口
                    {
                        CONTButton_Click(null, null);
                    }
                }
            });
        }

        /*
        public void TimerSerialPortTick(Object stateInfo)       //串口热插拔检测
        {
            int InOut = 0;
            int i = 0;
            int j = 0;
            string[] LastPort = ArryPort;
            string[] NowPort = SerialPort.GetPortNames();
            string InOutCom;
            string commne = "";

            if (LastPort == null)
            {
                LastPort = SerialPort.GetPortNames();
            }
            if (Enumerable.SequenceEqual(LastPort, NowPort) == false || ArryPort == null)
            {
                if (LastPort.Length < NowPort.Length)
                {
                    InOut = 1;
                    for (j = 0; j < NowPort.Length; j++)         //遍历插入的设备
                    {
                        Debug.WriteLine("SER J " + j);
                        for (i = 0; i < LastPort.Length; i++)
                        {
                            if (NowPort[j] == LastPort[i])
                            {
                                Debug.WriteLine("SER I " + i);
                                break;
                            }
                        }
                        Debug.WriteLine("Now" + i);
                    }
                    Debug.WriteLine("=" + i);
                }
                else if (LastPort.Length > NowPort.Length)
                {
                    InOut = 0;
                    for (i = 0; i < LastPort.Length; i++)       //遍历拔出的设备
                    {
                        for (j = 0; j < NowPort.Length; j++)
                        {
                            if (LastPort[i] == NowPort[j])
                            {
                                break;
                            }
                        }
                    }
                    Debug.WriteLine("Last" + j);
                }
                Debug.WriteLine("INOUT" + InOut);


                if (InOut == 1)
                {
                    InOutCom = NowPort[i];
                }
                else
                {
                    InOutCom = LastPort[j];
                }
                DispatcherQueue.TryEnqueue(() =>
                {
                    if (COMComboBox.SelectedItem != null)
                    {
                        commne = (string)COMComboBox.SelectedItem;
                    }
                    if (InOut != 0)
                    {
                        string commme = (string)COMComboBox.SelectedItem;
                        SerialPortInfo info = SerialPortInfo.GetPort(InOutCom);
                        RXTextBox.Text = RXTextBox.Text + InOutCom + ": " + info.Description + " " + LanguageText("spPlogin") + "\r\n";
                        COMComboBox.Items.Clear();
                        COMListview.Items.Clear();
                        //COMListview.ItemsSource = null;
                        //COMListview.ItemsSource = new ObservableCollection<ComDataItem>();
                        ArryPort = SerialPort.GetPortNames();
                        
                        for (int k = 0; k < NowPort.Length; k++)
                        {
                            //string portDescription = ports.Find(p => p.Name == ArryPort[k])?.Description;  // 查找对应串口的设备描述
                            
                            COMComboBox.Items.Add(ArryPort[k]);                           //将所有的可用串口号添加到端口对应的组合框中
                            COMListview.Items.Add(ArryPort[k]);
                        }
                        COMComboBox.SelectedItem = commme;
                        COMListview.SelectedItem = commne;
                        if (Con == 0)
                        {
                            if (COMComboBox.SelectedItem == null)
                            {
                                COMComboBox.SelectedItem = InOutCom;
                                COMListview.SelectedItem = InOutCom;
                                if (AutoConnectButton.IsChecked == true)
                                {
                                    CONTButton_Click(null, null);
                                }
                            }
                        }
                    }
                    else
                    {
                        RXTextBox.Text = RXTextBox.Text + InOutCom + LanguageText("spPullout") + "\r\n";
                        if (Con == 1)                                                   //自动断开已拔出的设备串口连接
                        {
                            if (InOutCom == (string)COMComboBox.SelectedItem)
                            {
                                CONTButton_Click(null, null);
                            }
                        }

                        COMComboBox.Items.Clear();
                        COMListview.Items.Clear();
                        //COMListview.ItemsSource = null;
                        //COMListview.ItemsSource = new ObservableCollection<ComDataItem>();
                        ArryPort = SerialPort.GetPortNames();
                        for (int k = 0; k < NowPort.Length; k++)
                        {
                            COMComboBox.Items.Add(ArryPort[k]);                           //将所有的可用串口号添加到端口对应的组合框中
                            COMListview.Items.Add(ArryPort[k]);
                        }
                        COMComboBox.SelectedItem = commne;
                        COMListview.SelectedItem = commne;
                    }
                });
            }
        }
        */

        //public event SerialDataReceivedEventHandler DataReceived;

        private async void COMButton_Click(object sender, RoutedEventArgs e)
        {
            //COMButton.Content = "Clicked";
            await SearchAndAddSerialToComboBoxAsync(CommonRes._serialPort, COMComboBox);           //扫描并将串口添加至下拉列表

            async Task SearchAndAddSerialToComboBoxAsync(SerialPort MyPort, ComboBox MyBox)
            {
                RXTextBox.Text = RXTextBox.Text + LanguageText("startSerichSP") + "\r\n";
                string commme = (string)COMComboBox.SelectedItem;           //记忆串口名
                ArryPort = SerialPort.GetPortNames();                       //SerialPort.GetPortNames()函数功能为获取计算机所有可用串口，以字符串数组形式输出
                string scom = String.Join("\r\n", ArryPort);
                //RXTextBox.Text = RXTextBox.Text + scom + "\r\n";
                MyBox.Items.Clear();                                        //清除当前组合框下拉菜单内容
                COMListview.Items.Clear();
                //COMListview.ItemsSource = null;
                //COMListview.ItemsSource = new ObservableCollection<ComDataItem>();

                for (int i = 0; i < ArryPort.Length; i++)
                {
                    MyBox.Items.Add(ArryPort[i]);                           //将所有的可用串口号添加到端口对应的组合框中
                    COMListview.Items.Add(ArryPort[i]/* + (portDescription != null ? " | " + portDescription : "")*/);
                    SerialPortInfo info = await Task.Run(() => SerialPortInfo.GetPort(ArryPort[i]));
                    RXTextBox.Text += ArryPort[i] + ": " + info.Description + "\r\n";
                    //RXTextBox.Text += ArryPort[i] + "\r\n" + GetPortDescription(ArryPort[i]) + "\r\n";
                }
                //MyBox.Items.Add("COM0");
                RXTextBox.Text = RXTextBox.Text + LanguageText("overSerichSP") + "\r\n";
                COMComboBox.SelectedItem = commme;
                COMListview.SelectedItem = commme;
            }
            //COMComboBox.SelectedItem = "COM0";
            /*
            if (Rollta == 360)
            {
                Rollta = 0;
            }
            else
            {
                Rollta = Rollta + 90;
            }
            COMButtonIcon.Rotation = Rollta;
            */
            Thread COMButtonIconRotation = new Thread(COMButtonIcon_Rotation);
            COMButtonIconRotation.Start();
        }


        private void COMButtonIcon_Rotation(object name)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                COMButtonIcon.Rotation = -60;
                COMButtonIconScalar.Duration = TimeSpan.FromMilliseconds(250);
            });
            Thread.Sleep(300);
            DispatcherQueue.TryEnqueue(() =>
            {
                COMButtonIcon.Rotation = 420;
            });
            Thread.Sleep(250);
            DispatcherQueue.TryEnqueue(() =>
            {
                COMButtonIconScalar.Duration = TimeSpan.FromMilliseconds(0);
                COMButtonIcon.Rotation = 60;
                COMButtonIconScalar.Duration = TimeSpan.FromMilliseconds(250);
            });
            Thread.Sleep(60);
            DispatcherQueue.TryEnqueue(() =>
            {
                COMButtonIcon.Rotation = 0;
            });


        }

        private void Settings_ColorValuesChanged(Windows.UI.ViewManagement.UISettings sender, object args)
        {
            var color = sender.GetColorValue(Windows.UI.ViewManagement.UIColorType.Accent);
            CONTButton.Background = new SolidColorBrush(color);
        }

        private void CONTButton_Click(object sender, RoutedEventArgs e)
        {
            var settings = new Windows.UI.ViewManagement.UISettings();
            var color = settings.GetColorValue(Windows.UI.ViewManagement.UIColorType.Accent);


            var foregroundColor = COMButton.Foreground as SolidColorBrush;
            var backgroundColor = COMButton.Background as SolidColorBrush;
            var darkaccentColor = (Windows.UI.Color)Application.Current.Resources["SystemAccentColorLight2"];
            var ligtaccentColor = (Windows.UI.Color)Application.Current.Resources["SystemAccentColorDark1"];
            var theme = Application.Current.RequestedTheme;


            settings.ColorValuesChanged += Settings_ColorValuesChanged;

            // 获取当前窗口的句柄

            if (Con == 0)
            {
                var app = (Application.Current as App);             // 尝试将当前应用程序实例转换为App类型
                if (app != null)                                    // 检查转换是否成功
                {
                    var hWnd = app.MainWindowHandle;                // 获取主窗口的句柄
                    this.taskbarInstance.SetProgressState(hWnd, FSGaryityTool_Win11.TBPFLAG.TBPF_INDETERMINATE);//开始任务栏加载动画
                }
                try
                {
                    CommonRes._serialPort.PortName = (string)COMComboBox.SelectedItem;                  //开启的串口名称为选择串口的ComboBox组件中的内容
                    ConCom = (string)COMComboBox.SelectedItem;
                    CommonRes._serialPort.BaudRate = Convert.ToInt32(BANDComboBox.SelectedItem);        //将选择波特率ComboBox组件中的数据转为Int型，并且进行波特率的设置

                    //RXTextBox.Foreground = foregroundColor;
                    /*
                    if (theme == ApplicationTheme.Dark)
                    {
                        // 当前处于深色模式
                        RXTextBox.Foreground = new SolidColorBrush(darkaccentColor);
                    }
                    else if (theme == ApplicationTheme.Light)
                    {
                        // 当前处于浅色模式
                        RXTextBox.Foreground = new SolidColorBrush(ligtaccentColor);
                    }*/

                    RXTextBox.Text = RXTextBox.Text + "BaudRate = " + Convert.ToInt32(BANDComboBox.SelectedItem) + "\r\n";

                    CommonRes._serialPort.Parity = (Parity)Enum.Parse(typeof(Parity), (string)PARComboBox.SelectedItem);        //校验位
                    CommonRes._serialPort.StopBits = (StopBits)Enum.Parse(typeof(StopBits), (string)STOPComboBox.SelectedItem); //停止位

                    RXTextBox.Text = RXTextBox.Text + "Parity = " + (Parity)Enum.Parse(typeof(Parity), (string)PARComboBox.SelectedItem) + "\r\n";
                    RXTextBox.Text = RXTextBox.Text + "StopBits = " + (StopBits)Enum.Parse(typeof(StopBits), (string)STOPComboBox.SelectedItem) + "\r\n";

                    CommonRes._serialPort.DataBits = Convert.ToInt32(DATAComboBox.SelectedItem);                                //数据位

                    RXTextBox.Text = RXTextBox.Text + "DataBits = " + Convert.ToInt32(DATAComboBox.SelectedItem) + "\r\n";

                    CommonRes._serialPort.ReadTimeout = 1500;
                    //_SerialPort.DtrEnable = true;                                                                             //启用数据终端就绪信息
                    CommonRes._serialPort.Encoding = Encoding.UTF8;
                    CommonRes._serialPort.ReceivedBytesThreshold = 1;                                               //DataReceived触发前内部输入缓冲器的字节数

                    //RXTextBox.Foreground = foregroundColor;

                    RXTextBox.Text = RXTextBox.Text + LanguageText("serialPortl") + " " + COMComboBox.SelectedItem + LanguageText("spConnect") + "\r\n";

                    CommonRes._serialPort.Open();                                                                               //打开串口

                    timer = new System.Threading.Timer(TimerTick, null, 0, 250); // 每秒触发8次

                    //CONTButton.Content = "DISCONNECT";
                    CONTButton.Content = LanguageText("disconnectl");
                    Con = 1;
                    RunProgressBar.ShowPaused = false;
                    RunProgressBar.IsIndeterminate = true;
                    RunProgressBar.Visibility = Visibility.Visible;
                    //CONTButton.Background = new SolidColorBrush(color);
                    if (theme == ApplicationTheme.Dark)                                                                         //设置连接按钮背景颜色
                    {
                        // 当前处于深色模式
                        CONTButton.Background = new SolidColorBrush(darkaccentColor);
                        CONTButton.Foreground = new SolidColorBrush(Colors.Black);
                    }
                    else if (theme == ApplicationTheme.Light)
                    {
                        // 当前处于浅色模式
                        CONTButton.Background = new SolidColorBrush(ligtaccentColor);
                        CONTButton.Foreground = new SolidColorBrush(Colors.White);
                    }


                }
                catch                                                                                                     //如果打开串口失败 需要做如下警示
                {
                    RXTextBox.Text = RXTextBox.Text + LanguageText("openSPErr") + "\r\n";
                    //MessageBox.Show("打开串口失败，请检查相关设置", "错误");
                    if (app != null)
                    {
                        var hWnd = app.MainWindowHandle;
                        this.taskbarInstance.SetProgressState(hWnd, FSGaryityTool_Win11.TBPFLAG.TBPF_NOPROGRESS);//停止任务栏加载动画
                    }
                    Con = 0;
                    //CONTButton.Content = "CONNECT";
                    CONTButton.Content = LanguageText("connectl");
                    CONTButton.Background = backgroundColor;
                    CONTButton.Foreground = foregroundColor;
                    RunProgressBar.IsIndeterminate = true;
                    RunProgressBar.ShowPaused = true;
                    RunProgressBar.Visibility = Visibility.Visible;
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
                //RXTextBox.Text = RXTextBox.Text + "SerialPort IS DISCONNECT\r\n";

                try
                {
                    CommonRes._serialPort.Close();                                                                              //关闭串口
                    RXTextBox.Text = RXTextBox.Text + "\n" + LanguageText("serialPortl") + " " + COMComboBox.SelectedItem + LanguageText("spClose") + "\r\n";
                }
                catch (Exception err)                                                                       //一般情况下关闭串口不会出错，所以不需要加处理程序
                {
                    RXTextBox.Text = RXTextBox.Text + err + "\r\n";
                }
                //CONTButton.Content = "CONNECT";
                CONTButton.Content = LanguageText("connectl");
                Con = 0;
                CONTButton.Background = backgroundColor;
                CONTButton.Foreground = foregroundColor;
                RunProgressBar.IsIndeterminate = false;
                RunProgressBar.ShowPaused = false;
                RunProgressBar.Visibility = Visibility.Collapsed;
                timer.Dispose();
            }
        }


        /*
        
        */


        private void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            Thread.Sleep(500);
            /*
            if (txf == 1)
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    RXTextBox.Text += "\r\n";
                });
                
                txf = 0;
            }
            */


            string rxstr;
            string Timesr = current_time.ToString("yyyy-MM-dd HH:mm:ss:ff   "); //显示时间
            string output;
            //StringBuilder datawate = new StringBuilder(1024);


            if (rx == 0)                                                        // 如果以字符串形式读取
            {

                rxstr = CommonRes._serialPort.ReadExisting();                   // 读取串口接收缓冲区字符串
                string content = rxstr;//.Replace("\n", "").Replace("\r", ""); // 将StringBuilder的内容转换为字符串，并去除换行符
                if (!string.IsNullOrWhiteSpace(content)) // 如果content不为空
                {
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        UpdateItemsRepeater(Timesr, content);                       // 在接收listview中进行显示
                        output = rxstr.Replace("\n", "\\n").Replace("\r", "\\r"); // 将换行符替换为其转义序列
                        Debug.WriteLine(output); // 在Debug.WriteLine中显示处理后的字符串
                        Debug.WriteLine("content" + content);
                    });
                }

                if (shtime == 1)
                {
                    //rxstr = string.Concat(Timesr, rxstr);                     //显示时间
                }
                //datawate.Append(rxstr);

                DispatcherQueue.TryEnqueue(() =>
                {
                    //datapwate.Append(rxstr);                                    // 在接收文本框中进行显示
                });


                if (autotr == 1)
                {
                    //RXTextBox.ScrollToEnd();
                }




            }
            else                                                            // 以数值形式读取
            {
                int length = CommonRes._serialPort.BytesToRead; // 读取串口接收缓冲区字节数

                byte[] Data = new byte[length]; // 定义相同字节的数组

                CommonRes._serialPort.Read(Data, 0, length); // 串口读取缓冲区数据到数组中
                DispatcherQueue.TryEnqueue(() =>
                {
                    UpdateItemsRepeater(Timesr, str);
                });
                for (int i = 0; i < length; i += 16)
                {
                    string str = "";
                    for (int j = i; j < i + 16 && j < length; j++)
                    {
                        str += Data[j].ToString("X2") + " ";
                    }

                    DispatcherQueue.TryEnqueue(() =>
                    {
                        UpdateItemsRepeater("|                                     |", str);
                    });
                }


                DispatcherQueue.TryEnqueue(() =>
                {
                    //datapwate.Append("\r\n");
                });



            }

            /*
            ++rxs;                                          //接收自动清空(已弃用)
            if (rxs == 200)
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    RXTextBox.Text = "";
                });
                rxs = 0;
            }
            */

        }

        private void UpdateItemsRepeater(string Timesr, string Rxstr)
        {
            // 假设你的ItemsRepeater的名字是RXListView
            DataItem item = new DataItem { Timesr = Timesr, Rxstr = Rxstr };
            (RXListView.ItemsSource as ObservableCollection<DataItem>).Add(item);

            if (autotr == 1)
            {
                RXListView.ScrollIntoView(item);
            }
        }

        /*
        private void ComItemsRepeater(string Com)
        {
            ComDataItem citem = new ComDataItem { ComName = Com };
            (COMListview.ItemsSource as ObservableCollection<ComDataItem>).Add(citem);
        }
        */

        private void COMComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string ComIs;
            ComIs = (string)COMComboBox.SelectedItem;
            COMListview.SelectedItem = ComIs;
            if(Con == 1)
            {
                if(ConCom != ComIs)
                    COMRstInfoBar.IsOpen = true;
                else
                    COMRstInfoBar.IsOpen = false;
            }
        }



        private void TXTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        ///*
        // 当点击发送按钮时执行的操作
        private void TXButton_Click(object sender, RoutedEventArgs e)
        {
            // 如果串口设备已经打开了
            if (!CommonRes._serialPort.IsOpen) return;

            try
            {
                // 根据发送模式选择发送数据的方式
                SendData();
                // 发送完成后清空发送文本框的内容
                TXTextBox.Text = "";
            }
            catch (Exception ex)
            {
                // 如果发送过程中出现错误，显示错误信息并断开串口连接
                RXTextBox.Text += $"{ex.Message}\r\n";
                CONTButton_Click(sender, e);
            }
        }

        // 根据发送模式选择发送数据的方式
        private void SendData()
        {
            // 如果是以字符的形式发送数据
            if (tx == 0)
            {
                SendStringData();
            }
            else // 如果以数值的形式发送
            {
                SendHexData();
            }
        }

        // 以字符形式发送数据
        private void SendStringData()
        {
            try
            {
                // 获取要发送的字符串
                string str = TXTextBox.Text;
                // 通过串口发送字符串
                CommonRes._serialPort.Write(str);
                // 如果需要在每条消息后添加换行符
                AppendNewLineIfRequired();
                // 更新接收文本框的内容
                RXTextBox.Text += $"TX: {str}\r\n";
            }
            catch
            {
                // 如果串口字符写入出错，显示错误信息
                RXTextBox.Text += $"{LanguageText("txStringErr")}\r\n";
                // 抛出异常以便外层捕获
                throw;
            }
        }

        // 以十六进制数值形式发送数据
        private void SendHexData()
        {
            try
            {
                // 获取要发送的十六进制字符串，并进行必要的预处理
                string input = PrepareHexString();
                // 将十六进制字符串转换为字节数组
                byte[] bytes = ConvertHexStringToByteArray(input);
                // 通过串口发送字节数组
                CommonRes._serialPort.Write(bytes, 0, bytes.Length);
                // 如果需要在每条消息后添加换行符
                AppendNewLineIfRequired();
                // 更新接收文本框的内容
                RXTextBox.Text += $"TX: 0x {string.Join(" ", bytes.Select(b => b.ToString("X2")))}\r\n";
            }
            catch (FormatException)
            {
                // 如果输入的字符串不是有效的十六进制数，显示错误信息
                RXTextBox.Text += $"{LanguageText("txHexErr")}\r\n";
                // 抛出异常以便外层捕获
                throw;
            }
        }

        // 对输入的十六进制字符串进行预处理
        private string PrepareHexString()
        {
            // 获取要发送的十六进制字符串，并去除所有空格
            string input = TXTextBox.Text.Trim().Replace(" ", "");
            // 如果长度为奇数，前面添加一个 '0'
            if (input.Length % 2 != 0)
            {
                input = "0" + input;
            }
            return input;
        }

        // 将十六进制字符串转换为字节数组
        private byte[] ConvertHexStringToByteArray(string input)
        {
            byte[] bytes = new byte[input.Length / 2];
            for (int i = 0; i < input.Length; i += 2)
            {
                // 尝试将每两个字符转换为一个字节
                if (!byte.TryParse(input.Substring(i, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out bytes[i / 2]))
                {
                    // 如果转换失败，抛出异常
                    throw new FormatException();
                }
            }
            return bytes;
        }

        // 如果需要在每条消息后添加换行符
        private void AppendNewLineIfRequired()
        {
            if (txnewline == 1)
            {
                CommonRes._serialPort.Write("\r\n");
            }
        }
        //*/
        private void CLEARButton_Click(object sender, RoutedEventArgs e)
        {
            RXListView.ItemsSource = null;
            RXTextBox.Text = "";    //清除文本框内容
            RXListView.ItemsSource = new ObservableCollection<DataItem>();
        }

        private void RXTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
        private void BANDComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (autosaveset == 1)
            {
                using (StreamReader reader = File.OpenText(FSSetToml))                    //打开TOML文件
                {
                    settingstomlr = TOML.Parse(reader);

                    settingstomlr["SerialPortSettings"]["DefaultBAUD"] = (string)BANDComboBox.SelectedItem;
                }

                using (StreamWriter writer = File.CreateText(FSSetToml))                  //将设置写入TOML文件
                {
                    settingstomlr.WriteTo(writer);
                    writer.Flush();
                }
            }
            if (Con == 1)
            {
                CommonRes._serialPort.BaudRate = Convert.ToInt32(BANDComboBox.SelectedItem);
                RXTextBox.Text = RXTextBox.Text + "BaudRate = " + Convert.ToInt32(BANDComboBox.SelectedItem) + "\r\n";
            }
        }
        private void PARComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (autosaveset == 1)
            {
                using (StreamReader reader = File.OpenText(FSSetToml))                    //打开TOML文件
                {
                    settingstomlr = TOML.Parse(reader);

                    settingstomlr["SerialPortSettings"]["DefaultParity"] = (string)PARComboBox.SelectedItem;
                }

                using (StreamWriter writer = File.CreateText(FSSetToml))                  //将设置写入TOML文件
                {
                    settingstomlr.WriteTo(writer);
                    writer.Flush();
                }
            }
            if (Con == 1)
            {
                CommonRes._serialPort.Parity = (Parity)Enum.Parse(typeof(Parity), (string)PARComboBox.SelectedItem);
                RXTextBox.Text = RXTextBox.Text + "Parity = " + (Parity)Enum.Parse(typeof(Parity), (string)PARComboBox.SelectedItem) + "\r\n";
            }
        }
        private void STOPComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (autosaveset == 1)
            {
                using (StreamReader reader = File.OpenText(FSSetToml))                    //打开TOML文件
                {
                    settingstomlr = TOML.Parse(reader);

                    settingstomlr["SerialPortSettings"]["DefaultSTOP"] = (string)STOPComboBox.SelectedItem;
                }

                using (StreamWriter writer = File.CreateText(FSSetToml))                  //将设置写入TOML文件
                {
                    settingstomlr.WriteTo(writer);
                    writer.Flush();
                }
            }
            if (Con == 1)
            {
                CommonRes._serialPort.StopBits = (StopBits)Enum.Parse(typeof(StopBits), (string)STOPComboBox.SelectedItem);
                RXTextBox.Text = RXTextBox.Text + "StopBits = " + (StopBits)Enum.Parse(typeof(StopBits), (string)STOPComboBox.SelectedItem) + "\r\n";
            }
        }
        private void DATAComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (autosaveset == 1)
            {
                using (StreamReader reader = File.OpenText(FSSetToml))                    //打开TOML文件
                {
                    settingstomlr = TOML.Parse(reader);

                    settingstomlr["SerialPortSettings"]["DefaultDATA"] = Convert.ToString(DATAComboBox.SelectedItem);
                }

                using (StreamWriter writer = File.CreateText(FSSetToml))                  //将设置写入TOML文件
                {
                    settingstomlr.WriteTo(writer);
                    writer.Flush();
                }
            }
            if (Con == 1)
            {
                CommonRes._serialPort.DataBits = Convert.ToInt32(DATAComboBox.SelectedItem);
                RXTextBox.Text = RXTextBox.Text + "DataBits = " + Convert.ToInt32(DATAComboBox.SelectedItem) + "\r\n";
            }
        }


        private void RXHEXButton_Click(object sender, RoutedEventArgs e)    //接收以十六进制数显示
        {
            if (rx == 0)
            {
                rx = 1;
            }
            else
            {
                rx = 0;
            }
            if (autosaveset == 1)
            {
                using (StreamReader reader = File.OpenText(FSSetToml))                    //打开TOML文件
                {
                    settingstomlr = TOML.Parse(reader);

                    settingstomlr["SerialPortSettings"]["DefaultRXHEX"] = Convert.ToString(rx);
                }

                using (StreamWriter writer = File.CreateText(FSSetToml))                  //将设置写入TOML文件
                {
                    settingstomlr.WriteTo(writer);
                    writer.Flush();
                }
            }
        }

        private void TXHEXButton_Click(object sender, RoutedEventArgs e)    //发送以十六进制数显示
        {

            if (tx == 0)
            {
                tx = 1;
            }
            else
            {
                tx = 0;
            }
            if (autosaveset == 1)
            {
                using (StreamReader reader = File.OpenText(FSSetToml))                    //打开TOML文件
                {
                    settingstomlr = TOML.Parse(reader);

                    settingstomlr["SerialPortSettings"]["DefaultTXHEX"] = Convert.ToString(tx);
                }

                using (StreamWriter writer = File.CreateText(FSSetToml))                  //将设置写入TOML文件
                {
                    settingstomlr.WriteTo(writer);
                    writer.Flush();
                }
            }
        }


        private void RSTButton_Click(object sender, RoutedEventArgs e)      //自动重启
        {
            CommonRes._serialPort.BaudRate = 74880;// BANDComboBox.SelectedItem = "74880";//ESP12F

            CommonRes._serialPort.RtsEnable = true;
            Thread.Sleep(10);
            CommonRes._serialPort.DtrEnable = true;
            Thread.Sleep(10);
            CommonRes._serialPort.DtrEnable = false;
            Thread.Sleep(10);
            CommonRes._serialPort.RtsEnable = false;

            RSTButton_ClickAsync(null, null);
        }

        private Task RSTButton_ClickAsync(object sender, RoutedEventArgs e)
        {
            Thread.Sleep(10);
            if (dtr == 1)
            {
                DTRButton_Click(sender, e);
                Thread.Sleep(1);
                DTRButton_Click(sender, e);
            }
            else
            {
                Thread.Sleep(1);
                DTRButton_Click(sender, e);
            }

            //CommonRes._serialPort.DtrEnable = true;
            Thread.Sleep(1);
            CommonRes._serialPort.BaudRate = Convert.ToInt32(BANDComboBox.SelectedItem);
            return Task.CompletedTask;
        }

        private void FsButtonChecked(int isChecked, Button button)
        {
            var foregroundColor = COMButton.Foreground as SolidColorBrush;
            var backgroundColor = COMButton.Background as SolidColorBrush;
            var darkaccentColor = (Windows.UI.Color)Application.Current.Resources["SystemAccentColorLight2"];
            var ligtaccentColor = (Windows.UI.Color)Application.Current.Resources["SystemAccentColorDark1"];
            var theme = Application.Current.RequestedTheme;
            if (isChecked == 0)
            {
                if (theme == ApplicationTheme.Dark)
                {
                    // 当前处于深色模式
                    button.Background = new SolidColorBrush(darkaccentColor);
                    button.Foreground = new SolidColorBrush(Colors.Black);
                }
                else if (theme == ApplicationTheme.Light)
                {
                    // 当前处于浅色模式
                    button.Background = new SolidColorBrush(ligtaccentColor);
                    button.Foreground = new SolidColorBrush(Colors.White);
                }
            }
            else
            {
                button.Background = backgroundColor;
                button.Foreground = foregroundColor;
            }
        }

        private void DTRButton_Click(object sender, RoutedEventArgs e)      //DTR信号使能
        {
            //FsButtonChecked(dtr, DTRButton);

            if (dtr == 0)
            {
                CommonRes._serialPort.DtrEnable = true;
                dtr = 1;
            }
            else
            {
                CommonRes._serialPort.DtrEnable = false;
                dtr = 0;
            }
            if (autosaveset == 1)
            {
                using (StreamReader reader = File.OpenText(FSSetToml))                    //打开TOML文件
                {
                    settingstomlr = TOML.Parse(reader);

                    settingstomlr["SerialPortSettings"]["DefaultDTR"] = Convert.ToString(dtr);
                }

                using (StreamWriter writer = File.CreateText(FSSetToml))                  //将设置写入TOML文件
                {
                    settingstomlr.WriteTo(writer);
                    writer.Flush();
                }
            }
        }

        private void RTSButton_Click(object sender, RoutedEventArgs e)      //RTS信号使能
        {
            //FsButtonChecked(rts, RTSButton);

            if (rts == 0)
            {
                CommonRes._serialPort.RtsEnable = true;
                rts = 1;
            }
            else
            {
                CommonRes._serialPort.RtsEnable = false;
                rts = 0;
            }
            if (autosaveset == 1)
            {
                using (StreamReader reader = File.OpenText(FSSetToml))                    //打开TOML文件
                {
                    settingstomlr = TOML.Parse(reader);

                    settingstomlr["SerialPortSettings"]["DefaultRTS"] = Convert.ToString(rts);
                }

                using (StreamWriter writer = File.CreateText(FSSetToml))                  //将设置写入TOML文件
                {
                    settingstomlr.WriteTo(writer);
                    writer.Flush();
                }
            }
        }

        private void ShowTimeButton_Click(object sender, RoutedEventArgs e)
        {
            //FsButtonChecked(shtime, ShowTimeButton);

            if (shtime == 0)
            {
                shtime = 1;

                //显示时间
                //current_time = System.DateTime.Now;     //获取当前时间
                /*
                DispatcherQueue.TryEnqueue(() =>
                {
                    RXTextBox.Text = RXTextBox.Text + current_time.ToString("HH:mm:ss") + "  ";                          // 在接收文本框中进行显示
                });
                */
            }
            else
            {
                shtime = 0;
            }
        }

        private void AUTOScrollButton_Click(object sender, RoutedEventArgs e)
        {
            //FsButtonChecked(autotr, AUTOScrollButton);

            if (autotr == 0)
            {
                autotr = 1;
            }
            else
            {
                autotr = 0;
            }
            if (autosaveset == 1)
            {
                using (StreamReader reader = File.OpenText(FSSetToml))                    //打开TOML文件
                {
                    settingstomlr = TOML.Parse(reader);

                    settingstomlr["SerialPortSettings"]["DefaultAUTOSco"] = Convert.ToString(autotr);
                }

                using (StreamWriter writer = File.CreateText(FSSetToml))                  //将设置写入TOML文件
                {
                    settingstomlr.WriteTo(writer);
                    writer.Flush();
                }
            }
        }

        private void RXDataButton_Click(object sender, RoutedEventArgs e)
        {

        }
        /*
        private async void RXDataButton_Click(object sender, RoutedEventArgs e)
        {
            await AUTOScrollButton_ClickAsync(sender, e);
        }
        */

        private Task RXDATA_ClickAsync(object sender, RoutedEventArgs e)
        {
            // 在这里添加你的异步代码
            // 例如：await SomeAsyncMethod();
            current_time = System.DateTime.Now;     //获取当前时间
            //RXTextBox.Text = RXTextBox.Text + current_time.ToString("HH:mm:ss") + "  ";
            //Timesr = current_time.ToString("HH:mm:ss");



            //rxpstr = System.Text.Encoding.UTF8.GetString(datapwate);
            rxpstr = datapwate.ToString();                          //将缓冲区赋值到输出
            RXTextBox.Text = RXTextBox.Text + rxpstr + "";          //输出接收的数据
            datapwate.Clear();                                      //清空缓冲区

            return Task.CompletedTask;
        }

        private void TXTextBox_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                TXButton_Click(this, new RoutedEventArgs());
            }
        }

        private void SaveSetButton_Click(object sender, RoutedEventArgs e)
        {
            if (autosaveset == 0)
            {
                autosaveset = 1;
            }
            else 
            {
                autosaveset = 0; 
            }
            using (StreamReader reader = File.OpenText(FSSetToml))                    //打开TOML文件
            {
                settingstomlr = TOML.Parse(reader);

                settingstomlr["SerialPortSettings"]["AutoDaveSet"] = Convert.ToString(autosaveset);
            }

            using (StreamWriter writer = File.CreateText(FSSetToml))                  //将设置写入TOML文件
            {
                settingstomlr.WriteTo(writer);
                writer.Flush();
            }
        }


        private void COMListview_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string ComIs;
            ComIs = (string)COMListview.SelectedItem;
            COMComboBox.SelectedItem = ComIs;

            
        }


        private void AutoComButton_Click(object sender, RoutedEventArgs e)
        {
            if (autosercom == 0)
            {
                timerSerialPort = new System.Threading.Timer(TimerSerialPortTick, null, 0, 1500);
                AutoSerchComProgressRing.IsActive = true;
                autosercom = 1;
            }
            else
            {
                timerSerialPort.Dispose();
                AutoSerchComProgressRing.IsActive = false;
                autosercom = 0;
            }
            if (autosaveset == 1)
            {
                using (StreamReader reader = File.OpenText(FSSetToml))                    //打开TOML文件
                {
                    settingstomlr = TOML.Parse(reader);

                    settingstomlr["SerialPortSettings"]["AutoSerichCom"] = Convert.ToString(autosercom);
                }

                using (StreamWriter writer = File.CreateText(FSSetToml))                  //将设置写入TOML文件
                {
                    settingstomlr.WriteTo(writer);
                    writer.Flush();
                }
            }
        }
        //=======================================================================
        /*
        private void RXListView_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                RXListView_RightTapped(this, new RightTappedRoutedEventArgs());
            }
        }
        */
        private void RXListView_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            // 获取鼠标右键点击的位置
            Point point = e.GetPosition(null);

            // 根据位置找到对应的ListViewItem
            ListViewItem listViewItem = VisualTreeHelper.FindElementsInHostCoordinates(point, RXListView).FirstOrDefault(x => x is ListViewItem) as ListViewItem;

            // 如果找到了ListViewItem，将其设置为选中状态
            if (listViewItem != null)
            {
                listViewItem.IsSelected = true;
            }

            MenuFlyoutItem copyItem = new MenuFlyoutItem 
            { 
                Text = LanguageText("copyAlll"),
                Icon = new FontIcon
                {
                    Glyph = "\uE8C8"
                }
            };
            copyItem.Click += CopyItem_Click;


            MenuFlyoutItem copyTimestampItem = new MenuFlyoutItem 
            { 
                Text = LanguageText("copyTimel"),
                Icon = new FontIcon
                {
                    Glyph = "\uE823"
                }
            };
            copyTimestampItem.Click += CopyTimestampItem_Click;

            MenuFlyoutItem copyDataItem = new MenuFlyoutItem 
            { 
                Text = LanguageText("copyDatal"),
                Icon = new FontIcon
                {
                    Glyph = "\uE8A4"
                }
            };
            copyDataItem.Click += CopyDataItem_Click;

            // 创建一个新的菜单飞出（MenuFlyout）并添加菜单项
            MenuFlyout menuFlyout = new MenuFlyout();

            menuFlyout.Items.Add(copyItem);
            menuFlyout.Items.Add(copyTimestampItem);
            menuFlyout.Items.Add(copyDataItem);

            // 显示菜单
            menuFlyout.ShowAt(sender as UIElement, e.GetPosition(sender as UIElement));
        }

        private void CopyItem_Click(object sender, RoutedEventArgs e)
        {
            // 获取当前选中的项
            var selectedItem = RXListView.SelectedItem as DataItem;

            // 确保选中的项不为空
            if (selectedItem != null)
            {
                // 获取选中项的内容
                var content1 = selectedItem.Timesr;
                var content2 = selectedItem.Rxstr;

                // 将两个内容合并，你可以根据需要添加适当的分隔符
                var combinedContent = content1 + " " + content2;

                // 将内容复制到剪贴板
                DataPackage dataPackage = new DataPackage();
                dataPackage.SetText(combinedContent);
                Clipboard.SetContent(dataPackage);
            }
        }
        private void CopyTimestampItem_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = RXListView.SelectedItem as DataItem;

            // 确保选中的项不为空
            if (selectedItem != null)
            {
                // 获取选中项的内容
                var content1 = selectedItem.Timesr;

                // 将两个内容合并，你可以根据需要添加适当的分隔符
                var combinedContent = content1;

                // 将内容复制到剪贴板
                DataPackage dataPackage = new DataPackage();
                dataPackage.SetText(combinedContent);
                Clipboard.SetContent(dataPackage);
            }
        }
        private void CopyDataItem_Click(object sender, RoutedEventArgs e)
        {
            // 获取当前选中的项
            var selectedItem = RXListView.SelectedItem as DataItem;

            // 确保选中的项不为空
            if (selectedItem != null)
            {
                // 获取选中项的内容
                var content2 = selectedItem.Rxstr;

                // 将两个内容合并，你可以根据需要添加适当的分隔符
                var combinedContent = content2;

                // 将内容复制到剪贴板
                DataPackage dataPackage = new DataPackage();
                dataPackage.SetText(combinedContent);
                Clipboard.SetContent(dataPackage);
            }
        }

        private void TXNewLineButton_Click(object sender, RoutedEventArgs e)
        {
            if (txnewline == 0)
            {
                txnewline = 1;
            }
            else
            {
                txnewline = 0;
            }
            if (autosaveset == 1)
            {
                using (StreamReader reader = File.OpenText(FSSetToml))                    //打开TOML文件
                {
                    settingstomlr = TOML.Parse(reader);

                    settingstomlr["SerialPortSettings"]["DefaultTXNewLine"] = Convert.ToString(txnewline);
                }

                using (StreamWriter writer = File.CreateText(FSSetToml))                  //将设置写入TOML文件
                {
                    settingstomlr.WriteTo(writer);
                    writer.Flush();
                }
            }
        }

        private void AutoConnectButton_Click(object sender, RoutedEventArgs e)
        {
            if (autoconnect == 0)
            {
                autoconnect = 1;
            }
            else
            {
                autoconnect = 0;
            }
            if (autosaveset == 1)
            {
                using (StreamReader reader = File.OpenText(FSSetToml))                    //打开TOML文件
                {
                    settingstomlr = TOML.Parse(reader);

                    settingstomlr["SerialPortSettings"]["AutoConnect"] = Convert.ToString(autoconnect);
                }

                using (StreamWriter writer = File.CreateText(FSSetToml))                  //将设置写入TOML文件
                {
                    settingstomlr.WriteTo(writer);
                    writer.Flush();
                }
            }
        }

        private void ClearCOMCombobox_Click(object sender, RoutedEventArgs e)
        {
            COMComboBox.SelectedItem = null;
            COMListview.SelectedItem = null;
        }

        private void ChipToolKitComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
