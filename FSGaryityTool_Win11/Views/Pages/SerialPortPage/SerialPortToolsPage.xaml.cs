using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Tommy;
using static FSGaryityTool_Win11.Page1;
using static FSGaryityTool_Win11.Views.Pages.SerialPortPage.MainPage1;
using System.IO.Ports;
using System.Management;
using Application = Microsoft.UI.Xaml.Application;
using System.Threading.Tasks;
using System.Threading;
using System.Text;
using FSGaryityTool_Win11.McuToolpage;
using System.Diagnostics;
using FSGaryityTool_Win11.Views.McuToolpage;
using Microsoft.UI.Xaml.Media.Animation;

namespace FSGaryityTool_Win11.Views.Pages.SerialPortPage;

public sealed partial class SerialPortToolsPage : Page
{
    public static int GetPortInfo { get; set; }

    /// <summary>
    /// Con
    /// </summary>
    public static int PortIsConnect { get; set; }

    public static string ConCom { get; set; } = "";

    /// <summary>
    /// TXHEX
    /// </summary>
    public static int TxHex { get; set; }

    /// <summary>
    /// RXHEX
    /// </summary>
    public static int RxHex { get; set; }

    /// <summary>
    /// FTR
    /// </summary>
    public static int Dtr { get; set; }

    /// <summary>
    /// RTS
    /// </summary>
    public static int Rts { get; set; }

    /// <summary>
    /// ShowTime
    /// </summary>
    public static int ShTime { get; set; }

    /// <summary>
    /// AUTOScroll
    /// </summary>
    public static int AutoTr { get; set; }

    public static int AutoSaveSet { get; set; }

    public static int AutoSerCom { get; set; }

    public static int AutoConnect { get; set; }

    public static int TxNewLine { get; set; }

    /// <summary>
    /// 定义字符串数组，数组名为 <see cref="ArryPort"/>
    /// </summary>
    public static string[] ArryPort { get; set; }

    public static int Baudrate { get; set; }

    public Timer Timer { get; set; }

    public Timer TimerSerialPort { get; set; }

    private bool _isLoaded;

    public class ParityOption
    {
        public string DisplayText { get; set; }

        public string Value { get; set; }
    }
    public class StopBitsOption
    {
        public string DisplayText { get; set; }

        public string Value { get; set; }
    }
    public class McuTool
    {
        public string Name { get; set; }

        public string Description { get; set; }
    }

    public static SerialPortToolsPage Current { get; private set; }

    public SerialPortToolsPage()
    {
        InitializeComponent();
        Current = this;
        Loaded += SerialPortToolsPage_Loaded;

        HideTimer = new() { Interval = TimeSpan.FromMilliseconds(750) };
        HideTimer.Tick += HideTimer_Tick;
    }

    private void FsBorderIsChecked(int isChecked, Border border, TextBlock textBlock)
    {
        var foregroundColor = (SolidColorBrush)Application.Current.Resources["TextFillColorPrimaryBrush"];
        var backgroundColor = (SolidColorBrush)Application.Current.Resources["CardBackgroundFillColorDefaultBrush"];
        var foreCheckColor = (SolidColorBrush)Application.Current.Resources["TextOnAccentFillColorPrimaryBrush"];
        var darkAccentColor = (Windows.UI.Color)Application.Current.Resources["SystemAccentColorLight2"];
        var lightAccentColor = (Windows.UI.Color)Application.Current.Resources["SystemAccentColorDark1"];
        var theme = Application.Current.RequestedTheme;

        if (isChecked is 1)
        {
            border.Background = theme switch
            {
                ApplicationTheme.Dark => new SolidColorBrush(darkAccentColor),
                ApplicationTheme.Light => new SolidColorBrush(lightAccentColor),
                _ => border.Background
            };
            textBlock.Foreground = foreCheckColor;
        }
        else
        {
            border.Background = backgroundColor;
            textBlock.Foreground = foregroundColor;
        }
    }
    private T TomlGetValueOrDefault<T>(TomlTable table, string menu, string name, T defaultValue)
    {
        if (table[menu][name] != "Tommy.TomlLazy")
        {
            var value = table[menu][name].AsString.Value;
            return (T)Convert.ChangeType(value, typeof(T));
        }
        else
        {
            return defaultValue;
        }
    }
    private void ToggleButtonIsChecked(int isChecked, ToggleButton toggleButton)
    {
        toggleButton.IsChecked = isChecked is 1;
    }
    private void SerialPortToolsPage_Loaded(object sender, RoutedEventArgs e)
    {
        if (!_isLoaded)
        {
            _isLoaded = true;

            string defaultBaud;
            string defaultPart;
            string defaultStop;
            int defaultData;
            string defaultEncoding;

            using (var reader = File.OpenText(FsSetToml))
            {
                var sPsettingstomlr = TOML.Parse(reader);             //读取TOML
                //Debug.WriteLine("Print:" + SPsettingstomlr["FSGravitySettings"]["DefaultNvPage"]);
                //NvPage = int.Parse(settingstomlr["FSGravitySettings"]["DefaultNvPage"]);
                var spSettings = "SerialPortSettings";
                //检查设置是否为NULL
                defaultBaud = TomlGetValueOrDefault(sPsettingstomlr, spSettings, "DefaultBAUD", "115200");
                defaultPart = TomlGetValueOrDefault(sPsettingstomlr, spSettings, "DefaultParity", "None");
                defaultStop = TomlGetValueOrDefault(sPsettingstomlr, spSettings, "DefaultSTOP", "One");
                defaultData = int.Parse(TomlGetValueOrDefault(sPsettingstomlr, spSettings, "DefaultDATA", "8"));
                defaultEncoding = TomlGetValueOrDefault(sPsettingstomlr, spSettings, "DefaultEncoding", "utf-8");

                TxHex = int.Parse(TomlGetValueOrDefault(sPsettingstomlr, spSettings, "DefaultTXHEX", "0"));
                RxHex = int.Parse(TomlGetValueOrDefault(sPsettingstomlr, spSettings, "DefaultRXHEX", "0"));
                Dtr = int.Parse(TomlGetValueOrDefault(sPsettingstomlr, spSettings, "DefaultDTR", "1"));
                Rts = int.Parse(TomlGetValueOrDefault(sPsettingstomlr, spSettings, "DefaultRTS", "0"));
                ShTime = int.Parse(TomlGetValueOrDefault(sPsettingstomlr, spSettings, "DefaultSTime", "0"));
                AutoTr = int.Parse(TomlGetValueOrDefault(sPsettingstomlr, spSettings, "DefaultAUTOSco", "1"));
                AutoSaveSet = int.Parse(TomlGetValueOrDefault(sPsettingstomlr, spSettings, "AutoDaveSet", "1"));
                AutoSerCom = int.Parse(TomlGetValueOrDefault(sPsettingstomlr, spSettings, "AutoSerichCom", "1"));
                AutoConnect = int.Parse(TomlGetValueOrDefault(sPsettingstomlr, spSettings, "AutoConnect", "1"));
                TxNewLine = int.Parse(TomlGetValueOrDefault(sPsettingstomlr, spSettings, "DefaultTXNewLine", "0"));

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
            LanguageSetting();
            // 在你的代码后台，定义一个List<string>作为数据源
            var baudRates = new List<string>
            {
                "75", "110", "134", "150", "300", "600", "1200", "1800", "2400", "4800", "7200", "9600", "14400", "19200", "38400", "57600", "74880","115200", "128000", "230400", "250000", "500000", "1000000", "2000000"
            };
            // 将ComboBox的ItemsSource属性绑定到这个数据源
            BandComboBox.ItemsSource = baudRates;
            // 设置默认选项
            BandComboBox.SelectedItem = defaultBaud; // 将"9600"设置为默认选项

            var parRates = new List<ParityOption>
            {
                new() { DisplayText = LanguageText("parityNone"), Value = "None" },
                new() { DisplayText = LanguageText("parityOdd"), Value = "Odd" },
                new() { DisplayText = LanguageText("parityEven"), Value = "Even" },
                new() { DisplayText = LanguageText("parityMark"), Value = "Mark" },
                new() { DisplayText = LanguageText("paritySpace"), Value = "Space" }
            };
            ParComboBox.ItemsSource = parRates;
            ParComboBox.DisplayMemberPath = "DisplayText";
            ParComboBox.SelectedValuePath = "Value";
            ParComboBox.SelectedValue = defaultPart;

            var stopRates = new List<StopBitsOption>
            {
                //new StopBitsOption { DisplayText = LanguageText("stopNone"), Value = "None" },
                new() { DisplayText = LanguageText("stopOne"), Value = "One" },
                new() { DisplayText = LanguageText("stopOnePointFive"), Value = "OnePointFive" },
                new() { DisplayText = LanguageText("stopTwo"), Value = "Two" }
            };
            StopComboBox.ItemsSource = stopRates;
            StopComboBox.DisplayMemberPath = "DisplayText";
            StopComboBox.SelectedValuePath = "Value";
            StopComboBox.SelectedValue = defaultStop;

            for (var j = 5; j < 9; ++j)
            {
                DataComboBox.Items.Add(j);
            }
            DataComboBox.SelectedItem = defaultData;
            DataNumberBox.Value = defaultData;

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            // 创建一个 List<string> 来存储编码名称
            var encodings = new List<string> { "gb2312" };

            // 将编码名称添加到 List<string> 中
            encodings.AddRange(Encoding.GetEncodings().Select(encodingInfo => encodingInfo.Name));

            EncodingComboBox.ItemsSource = encodings;
            EncodingComboBox.SelectedItem = defaultEncoding;

            COMButton_Click(this, new());

            ToggleButtonIsChecked(RxHex, RxhexButton);
            ToggleButtonIsChecked(TxHex, TxhexButton);

            ToggleButtonIsChecked(Dtr, DtrButton);
            CommonRes.SerialPort.DtrEnable = Dtr is 1;

            ToggleButtonIsChecked(Rts, RtsButton);
            CommonRes.SerialPort.RtsEnable = Rts is 1;

            ToggleButtonIsChecked(ShTime, ShowTimeButton);
            ToggleButtonIsChecked(AutoTr, AutoScrollButton);

            //BorderBackRX.Background = backgroundColor;

            ToggleButtonIsChecked(AutoSaveSet, SaveSetButton);
            ToggleButtonIsChecked(AutoConnect, AutoConnectButton);
            ToggleButtonIsChecked(TxNewLine, TxNewLineButton);

            ToggleButtonIsChecked(AutoSerCom, AutoComButton);
            if (AutoSerCom is 1)
            {
                TimerSerialPort = new(TimerSerialPortTick, null, 0, 1500);
                AutoSerchComProgressRing.IsActive = true;
            }
            else AutoSerchComProgressRing.IsActive = false;

            ToggleButtonIsChecked(AutoConnect, AutoConnectButton);
            //ToggleButtonIsChecked();

            //EncodingTest();
        }
    }

    public void EncodingTest()
    {
        // Print the header.
        Debug.Write("CodePage identifier and name     ");
        Debug.Write("BrDisp   BrSave   ");
        Debug.Write("MNDisp   MNSave   ");
        Debug.WriteLine("1-Byte   ReadOnly ");

        // For every encoding, get the property values.
        foreach (var ei in Encoding.GetEncodings())
        {
            var e = ei.GetEncoding();

            Debug.Write($"{ei.CodePage,-6} {ei.Name,-25} ");
            Debug.Write($"{e.IsBrowserDisplay,-8} {e.IsBrowserSave,-8} ");
            Debug.Write($"{e.IsMailNewsDisplay,-8} {e.IsMailNewsSave,-8} ");
            Debug.WriteLine($"{e.IsSingleByte,-8} {e.IsReadOnly,-8} ");
        }
    }

    public void LanguageSetting()
    {
        BaudTextBlock.Text = LanguageText("baudRatel");
        PartTextBlock.Text = LanguageText("parityl");
        StopTextBlock.Text = LanguageText("stopBits");
        DataTextBlock.Text = LanguageText("dataBits");
        EncodingTextBlock.Text = LanguageText("encoding");
        RxhexButton.Content = LanguageText("rxHexl");
        TxhexButton.Content = LanguageText("txHexl");
        TxNewLineButton.Content = LanguageText("txNewLinel");
        SaveSetButton.Content = LanguageText("autoSaveSetl");
        AutoScrollButton.Content = LanguageText("autoScrolll");
        AutoComButton.Content = LanguageText("autoSerichComl");
        AutoConnectButton.Content = LanguageText("autoConnectl");
            
        //COMRstInfoBar.Message = LanguageText("comRstInfoBar");

        var mcuTools = new List<McuTool>
        {
            new() { Name = "None", Description = LanguageText("mcuToolNone") },
            new() { Name = "ESP8266", Description = LanguageText("mcuToolEsp8266") },
            new() { Name = "RP2040        M", Description = LanguageText("mcuToolRP2040MPY") },
            new() { Name = "LPC1768        SM", Description = LanguageText("mcuToolLPC1768SMOOTH") },
        };

        ChipToolKitComboBox.ItemsSource = mcuTools;
        ChipToolKitComboBox.SelectedItem = mcuTools[1];
    }
    public void TimerTick(object stateInfo)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            //RXDATA_ClickAsync(null, null);
            Page1.Current.CurrentTime = DateTime.Now;     //获取当前时间

            if (RunT is 0) RunPbt += 2;

            else RunPbt -= 2;

            RunTProgressBar.Value = RunPbt;
            RunT = RunPbt switch
            {
                100 => 1,
                0 => 0,
                _ => RunT
            };

            if (CommonRes.SerialPort.IsOpen)
            {
                FsBorderIsChecked(CommonRes.SerialPort.DsrHolding ? 1 : 0, DsrBorder, DsrTextBlock);

                FsBorderIsChecked(CommonRes.SerialPort.CtsHolding ? 1 : 0, CtsBorder, CtsTextBlock);

                FsBorderIsChecked(CommonRes.SerialPort.CDHolding ? 1 : 0, CdhBorder, CdhTextBlock);
            }
        });
    }
        
    private void PinChanged(object sender, SerialPinChangedEventArgs e)
    {
        if (e.EventType == SerialPinChange.Ring)
        {
            var ri = 0;
            // RI 信号使能
            // RI 信号未使能
            ri = ri is 0 ? 1 : 0;

            FsBorderIsChecked(ri, RiBorder, RiTextBlock);
        }
    }

    public class SerialPortInfo
    {
        public string Name { get; protected set; }

        public string Description { get; protected set; }

        public string Manufacturer { get; protected set; }

        private static Dictionary<string, SerialPortInfo> _portInfoDictionary = new();

        static SerialPortInfo()
        {
            // 在应用程序启动时获取所有串口的设备描述
            RefreshPortInfo();
            GetPortInfo = 1;
        }

        public static void RefreshPortInfo(string portName = null)
        {
            var queryString = "SELECT * FROM Win32_PnPEntity";
            if (portName is not null)
            {
                queryString += $" WHERE Name LIKE '%{portName}%'";
            }

            using var searcher = new ManagementObjectSearcher(queryString);
            var hardInfos = searcher.Get();
            foreach (var hardInfo in hardInfos)
            {
                if (hardInfo.Properties["Name"].Value is not null &&
                    hardInfo.Properties["Name"].Value.ToString().Contains("COM"))
                {
                    var temp = new SerialPortInfo();
                    var s = hardInfo.Properties["Name"].Value.ToString();
                    var p = s.IndexOf('(');
                    temp.Description = s[..p];
                    temp.Name = s.Substring(p + 1, s.Length - p - 2);
                    temp.Manufacturer = hardInfo.Properties[nameof(Manufacturer)].Value.ToString();
                    _portInfoDictionary[temp.Name] = temp;
                }
            }
        }

        public static SerialPortInfo GetPort(string portName)
        {
            if (!_portInfoDictionary.ContainsKey(portName))
            {
                // 如果字典中没有指定的串口名称，刷新串口信息
                RefreshPortInfo(portName);
            }

            return _portInfoDictionary.GetValueOrDefault(portName);  // 如果没有找到匹配的串口，返回null
        }
    }

    public void TimerSerialPortTick(object stateInfo)       //串口热插拔检测
    {
        if (GetPortInfo is 0) return;
        var nowPort = SerialPort.GetPortNames(); // 获取当前所有可用的串口名称
        nowPort = new HashSet<string>(nowPort).ToArray(); // 移除可能的重复项

        var lastPort = ArryPort ?? nowPort; // 获取上一次检测到的串口名称，如果没有则使用当前串口名称
        ArryPort = nowPort; // 更新上一次检测到的串口名称

        var lastPortSet = new HashSet<string>(lastPort); // 创建一个包含上一次串口名称的HashSet
        var nowPortSet = new HashSet<string>(nowPort); // 创建一个包含当前串口名称的HashSet

        var insertedPorts = nowPortSet.Except(lastPortSet).ToArray(); // 找出新插入的串口
        var removedPorts = lastPortSet.Except(nowPortSet).ToArray(); // 找出被拔出的串口

        if (insertedPorts.Length > 0 || removedPorts.Length > 0) // 如果有新插入的串口或者有串口被拔出
        {
            DispatcherQueue.TryEnqueue(() => // 在UI线程中执行以下操作
            {
                var selectedPort = (string)ComComboBox.SelectedItem; // 获取当前选中的串口

                foreach (var port in insertedPorts) // 遍历所有新插入的串口
                {
                    var info = SerialPortInfo.GetPort(port); // 获取串口的信息
                    Page1.Current.RxTextBox.Text += $"{port}{LanguageText("spPlogin")}\r\n"; // 更新文本框的内容

                }

                foreach (var port in removedPorts) // 遍历所有被拔出的串口
                {
                    Page1.Current.RxTextBox.Text += $"{port}{LanguageText("spPullout")}\r\n"; // 更新文本框的内容
                    if (PortIsConnect is 1 && port == selectedPort) // 如果当前连接的串口被拔出，则断开连接
                    {
                        MainPage1.Current.SerialPortConnectToggleButton_Click(null, null);
                    }
                }

                ComComboBox.Items.Clear(); // 清空组合框的内容
                ComListview.Items.Clear(); // 清空列表视图的内容

                foreach (var port in nowPort) // 遍历当前所有可用的串口
                {
                    ComComboBox.Items.Add(port); // 将串口名称添加到组合框中
                    ComListview.Items.Add(port); // 将串口名称添加到列表视图中
                }

                ComComboBox.SelectedItem = selectedPort; // 将之前选中的串口重新选中
                ComListview.SelectedItem = selectedPort; // 将之前选中的串口重新选中

                if (PortIsConnect is 0 && ComComboBox.SelectedItem is null && insertedPorts.Length > 0) // 如果没有选中的串口，并且有新插入的串口
                {
                    ComComboBox.SelectedItem = insertedPorts[0]; // 选中新插入的串口
                    ComListview.SelectedItem = insertedPorts[0]; // 选中新插入的串口
                    if (AutoConnectButton.IsChecked == true) // 如果设置了自动连接，则尝试连接新插入的串口
                    {
                        MainPage1.Current.SerialPortConnectToggleButton_Click(null, null);
                    }
                }
            });
        }
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

        if (LastPort is null)
        {
            LastPort = SerialPort.GetPortNames();
        }
        if (Enumerable.SequenceEqual(LastPort, NowPort) == false || ArryPort is null)
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

            if (InOut is 1)
            {
                InOutCom = NowPort[i];
            }
            else
            {
                InOutCom = LastPort[j];
            }
            DispatcherQueue.TryEnqueue(() =>
            {
                if (COMComboBox.SelectedItem is not null)
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
                    if (portIsConnect is 0)
                    {
                        if (COMComboBox.SelectedItem is null)
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
                    if (portIsConnect is 1)                                                   //自动断开已拔出的设备串口连接
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
    private async void COMButton_Click(object sender, RoutedEventArgs e)
    {
        //COMButton.Content = "Clicked";
        await SearchAndAddSerialToComboBoxAsync(CommonRes.SerialPort, ComComboBox);           //扫描并将串口添加至下拉列表

        async Task SearchAndAddSerialToComboBoxAsync(SerialPort myPort, ComboBox myBox)
        {
            Page1.Current.RxTextBox.Text = Page1.Current.RxTextBox.Text + LanguageText("startSerichSP") + "\r\n";
            var comMem = (string)ComComboBox.SelectedItem;           //记忆串口名
            ArryPort = SerialPort.GetPortNames();                       //SerialPort.GetPortNames()函数功能为获取计算机所有可用串口，以字符串数组形式输出
            ArryPort = new HashSet<string>(ArryPort).ToArray(); // 移除可能的重复项
            var scom = string.Join("\r\n", ArryPort);
            //RXTextBox.Text = RXTextBox.Text + scom + "\r\n";
            myBox.Items.Clear();                                        //清除当前组合框下拉菜单内容
            ComListview.Items.Clear();
            //COMListview.ItemsSource = null;
            //COMListview.ItemsSource = new ObservableCollection<ComDataItem>();

            foreach (var t in ArryPort)
            {
                myBox.Items.Add(t);                           //将所有的可用串口号添加到端口对应的组合框中
                ComListview.Items.Add(t/* + (portDescription is not null ? " | " + portDescription : "")*/);
                var info = await Task.Run(() => SerialPortInfo.GetPort(t));
                if (info is not null)
                {
                    Page1.Current.RxTextBox.Text += t + ": " + info.Description + "\r\n";
                }
                else
                {
                    Page1.Current.RxTextBox.Text += t + "\r\n";
                }
                //RXTextBox.Text += t + "\r\n" + GetPortDescription(t) + "\r\n";
            }
            //MyBox.Items.Add("COM0");
            Page1.Current.RxTextBox.Text = Page1.Current.RxTextBox.Text + LanguageText("overSerichSP") + "\r\n";
            ComComboBox.SelectedItem = comMem;
            ComListview.SelectedItem = comMem;
        }
            
        var comButtonIconRotation = new Thread(COMButtonIcon_Rotation);
        comButtonIconRotation.Start();
    }

    private void COMButtonIcon_Rotation(object name)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            ComButtonIcon.Rotation = -60;
            ComButtonIconScalar.Duration = TimeSpan.FromMilliseconds(250);
        });
        Thread.Sleep(300);
        DispatcherQueue.TryEnqueue(() =>
        {
            ComButtonIcon.Rotation = 420;
        });
        Thread.Sleep(250);
        DispatcherQueue.TryEnqueue(() =>
        {
            ComButtonIconScalar.Duration = TimeSpan.FromMilliseconds(0);
            ComButtonIcon.Rotation = 60;
            ComButtonIconScalar.Duration = TimeSpan.FromMilliseconds(250);
        });
        Thread.Sleep(60);
        DispatcherQueue.TryEnqueue(() =>
        {
            ComButtonIcon.Rotation = 0;
        });
    }

    public void SerialPortConnect()
    {
        ConCom = (string)ComComboBox.SelectedItem;

        var portName = (string)ComComboBox.SelectedItem;
        var bandRate = Convert.ToInt32(BandComboBox.SelectedItem);
        var parity = ((ParityOption)ParComboBox.SelectedItem).Value;
        var stopBits = ((StopBitsOption)StopComboBox.SelectedItem).Value;
        var dataBits = Convert.ToInt32(DataComboBox.SelectedItem);
        var encoding = (string)EncodingComboBox.SelectedItem;

        Page1.Current.SerialPortConnect(portName, bandRate, parity, stopBits, dataBits, 1500, encoding);

        Page1.Current.RxTextBox.Text = Page1.Current.RxTextBox.Text + "BaudRate = " + Convert.ToInt32(BandComboBox.SelectedItem) + "\r\n";
        Page1.Current.RxTextBox.Text = Page1.Current.RxTextBox.Text + "Parity = " + (Parity)Enum.Parse(typeof(Parity), ((ParityOption)ParComboBox.SelectedItem).Value) + "\r\n";
        Page1.Current.RxTextBox.Text = Page1.Current.RxTextBox.Text + "StopBits = " + (StopBits)Enum.Parse(typeof(StopBits), ((StopBitsOption)StopComboBox.SelectedItem).Value) + "\r\n";
        Page1.Current.RxTextBox.Text = Page1.Current.RxTextBox.Text + "DataBits = " + Convert.ToInt32(DataComboBox.SelectedItem) + "\r\n";
        Page1.Current.RxTextBox.Text = Page1.Current.RxTextBox.Text + "Encoding = " + (string)EncodingComboBox.SelectedItem + "\r\n";
        Page1.Current.RxTextBox.Text = Page1.Current.RxTextBox.Text + LanguageText("serialPortl") + " " + ComComboBox.SelectedItem + LanguageText("spConnect") + "\r\n";

        Timer = new(TimerTick, null, 0, 250); // 每秒触发8次

        PortIsConnect = 1;
    }

    public void SerialPortConnectCatch()
    {
        Page1.Current.RxTextBox.Text = Page1.Current.RxTextBox.Text + LanguageText("openSPErr") + "\r\n";
        //MessageBox.Show("打开串口失败，请检查相关设置", "错误");

        PortIsConnect = 0;
        //CONTButton.Content = "CONNECT";
    }
    public void SerialPortClose()
    {
        CommonRes.SerialPort.Close();                                                                              //关闭串口
        Page1.Current.RxTextBox.Text = Page1.Current.RxTextBox.Text + "\n" + LanguageText("serialPortl") + " " + ComComboBox.SelectedItem + LanguageText("spClose") + "\r\n";
    }
    public void SerialPortDisconnect()
    {
        PortIsConnect = 0;
        Timer.Dispose();
    }

    private void COMComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var comIs = (string)ComComboBox.SelectedItem;
        ComListview.SelectedItem = comIs;
        if (PortIsConnect is 1)
        {
            if (ConCom != comIs)
                Page1.Current.ComRstInfoBar.IsOpen = true;
            else
                Page1.Current.ComRstInfoBar.IsOpen = false;
        }
    }
    private void ClearCOMCombobox_Click(object sender, RoutedEventArgs e)
    {
        ComComboBox.SelectedItem = null;
        ComListview.SelectedItem = null;
    }

    private void ComboboxSaveSetting(string menuName, string name, string settingItem)
    {
        //打开TOML文件
        using var reader = File.OpenText(FsSetToml);
        SettingsTomlr = TOML.Parse(reader);
        SettingsTomlr[menuName][name] = settingItem;

        //将设置写入TOML文件
        using var writer = File.CreateText(FsSetToml);
        SettingsTomlr.WriteTo(writer);
        writer.Flush();
    }

    private void AutoComButton_Click(object sender, RoutedEventArgs e)
    {
        if (AutoSerCom is 0)
        {
            TimerSerialPort = new(TimerSerialPortTick, null, 0, 1500);
            AutoSerchComProgressRing.IsActive = true;
            AutoSerCom = 1;
        }
        else
        {
            TimerSerialPort.Dispose();
            AutoSerchComProgressRing.IsActive = false;
            AutoSerCom = 0;
        }
        if (AutoSaveSet is 1)
        {
            ComboboxSaveSetting("SerialPortSettings", "AutoSerichCom", Convert.ToString(AutoSerCom));
        }
    }
    private void AutoConnectButton_Click(object sender, RoutedEventArgs e)
    {
        AutoConnect = AutoConnect is 0 ? 1 : 0;
        if (AutoSaveSet is 1)
        {
            ComboboxSaveSetting("SerialPortSettings", "AutoConnect", Convert.ToString(AutoConnect));
        }
    }
    private void COMListview_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var comIs = (string)ComListview.SelectedItem;
        ComComboBox.SelectedItem = comIs;
    }
    private void BANDComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // 检查输入的是否为数字
        if (!int.TryParse((string)BandComboBox.SelectedItem, out var baudRate) || baudRate is 0)
        {
            // 如果输入的不是数字，使用设置文件中的数字覆盖它
            using var reader = File.OpenText(FsSetToml);
            SettingsTomlr = TOML.Parse(reader);
            BandComboBox.SelectedItem = ((TomlString)SettingsTomlr["SerialPortSettings"]["DefaultBAUD"]).Value;
        }
        else
        {
            if (AutoSaveSet is 1)
            {
                ComboboxSaveSetting("SerialPortSettings", "DefaultBAUD", (string)BandComboBox.SelectedItem);
            }
            if (PortIsConnect is 1)
            {
                CommonRes.SerialPort.BaudRate = Convert.ToInt32(BandComboBox.SelectedItem);
                Page1.Current.RxTextBox.Text = Page1.Current.RxTextBox.Text + "BaudRate = " + Convert.ToInt32(BandComboBox.SelectedItem) + "\r\n";
            }
            Baudrate = Convert.ToInt32(BandComboBox.SelectedItem);
        }

        BaudrateIcon.Glyph = Convert.ToInt32(BandComboBox.SelectedItem) switch
        {
            <= 7200 => "\uEC48",
            > 7200 and < 128000 => "\uEC49",
            _ => "\uEC4A"
        };
    }

    private void PARComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (AutoSaveSet is 1)
        {
            ComboboxSaveSetting("SerialPortSettings", "DefaultParity", ((ParityOption)ParComboBox.SelectedItem).Value);
        }
        if (PortIsConnect is 1)
        {
            CommonRes.SerialPort.Parity = (Parity)Enum.Parse(typeof(Parity), ((ParityOption)ParComboBox.SelectedItem).Value);
            Page1.Current.RxTextBox.Text = Page1.Current.RxTextBox.Text + "Parity = " + (Parity)Enum.Parse(typeof(Parity), ((ParityOption)ParComboBox.SelectedItem).Value) + "\r\n";
        }
    }
    private void STOPComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (AutoSaveSet is 1)
        {
            ComboboxSaveSetting("SerialPortSettings", "DefaultSTOP", ((StopBitsOption)StopComboBox.SelectedItem).Value);
        }
        if (PortIsConnect is 1)
        {
            CommonRes.SerialPort.StopBits = (StopBits)Enum.Parse(typeof(StopBits), ((StopBitsOption)StopComboBox.SelectedItem).Value);
            Page1.Current.RxTextBox.Text = Page1.Current.RxTextBox.Text + "StopBits = " + (StopBits)Enum.Parse(typeof(StopBits), ((StopBitsOption)StopComboBox.SelectedItem).Value) + "\r\n";
        }
        if (StopComboBox.SelectedItem is StopBitsOption selectedOption)
        {
            if (float.TryParse(selectedOption.DisplayText, out var stopBits))
            {
                StopBorder.Scale = new(stopBits, 1, 1);
            }
            else
            {
                // 处理解析失败的情况
                StopBorder.Scale = new(1, 1, 1);
            }
        }
    }
    private void DATAComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (AutoSaveSet is 1)
        {
            ComboboxSaveSetting("SerialPortSettings", "DefaultDATA", Convert.ToString(DataComboBox.SelectedItem));
        }
        if (PortIsConnect is 1)
        {
            CommonRes.SerialPort.DataBits = Convert.ToInt32(DataComboBox.SelectedItem);
            Page1.Current.RxTextBox.Text = Page1.Current.RxTextBox.Text + "DataBits = " + Convert.ToInt32(DataComboBox.SelectedItem) + "\r\n";
        }
        DatainfoBadge.Value = Convert.ToInt32(DataComboBox.SelectedItem);
    }

    private void EncodingComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (AutoSaveSet is 1)
        {
            ComboboxSaveSetting("SerialPortSettings", "DefaultEncoding", (string)EncodingComboBox.SelectedItem);
        }
        if (PortIsConnect is 1)
        {
            CommonRes.SerialPort.Encoding = Encoding.GetEncoding((string)EncodingComboBox.SelectedItem);
            Page1.Current.RxTextBox.Text = Page1.Current.RxTextBox.Text + "Encoding = " + (string)EncodingComboBox.SelectedItem + "\r\n";
        }
    }

    private void RXHEXButton_Click(object sender, RoutedEventArgs e)    //接收以十六进制数显示
    {
        if (RxHex is 0)
        {
            RxHex = 1;
            RxhexButton.IsChecked = true;
        }
        else
        {
            RxHex = 0;
            RxhexButton.IsChecked = false;
        }
        if (AutoSaveSet is 1)
        {
            ComboboxSaveSetting("SerialPortSettings", "DefaultRXHEX", Convert.ToString(RxHex));
        }
    }

    private void TXHEXButton_Click(object sender, RoutedEventArgs e)    //发送以十六进制数显示
    {

        if (TxHex is 0)
        {
            TxHex = 1;
            TxhexButton.IsChecked = true;
        }
        else
        {
            TxHex = 0;
            TxhexButton.IsChecked = false;
        }
        if (AutoSaveSet is 1)
        {
            ComboboxSaveSetting("SerialPortSettings", "DefaultTXHEX", Convert.ToString(TxHex));
        }
    }
    private void DTRButton_Click(object sender, RoutedEventArgs e)      //DTR信号使能
    {
        //FsButtonChecked(dtr, DTRButton);

        if (Dtr is 0)
        {
            CommonRes.SerialPort.DtrEnable = true;
            Dtr = 1;
        }
        else
        {
            CommonRes.SerialPort.DtrEnable = false;
            Dtr = 0;
        }
        if (AutoSaveSet is 1)
        {
            ComboboxSaveSetting("SerialPortSettings", "DefaultDTR", Convert.ToString(Dtr));
        }
    }
    private void RTSButton_Click(object sender, RoutedEventArgs e)      //RTS信号使能
    {
        //FsButtonChecked(rts, RTSButton);

        if (Rts is 0)
        {
            CommonRes.SerialPort.RtsEnable = true;
            Rts = 1;
        }
        else
        {
            CommonRes.SerialPort.RtsEnable = false;
            Rts = 0;
        }
        if (AutoSaveSet is 1)
        {
            ComboboxSaveSetting("SerialPortSettings", "DefaultRTS", Convert.ToString(Rts));
        }
    }
    private void TXNewLineButton_Click(object sender, RoutedEventArgs e)
    {
        TxNewLine = TxNewLine is 0 ? 1 : 0;
        if (AutoSaveSet is 1)
        {
            ComboboxSaveSetting("SerialPortSettings", "DefaultTXNewLine", Convert.ToString(TxNewLine));
        }
    }
    private void SaveSetButton_Click(object sender, RoutedEventArgs e)
    {
        AutoSaveSet = AutoSaveSet is 0 ? 1 : 0;
        ComboboxSaveSetting("SerialPortSettings", "AutoDaveSet", Convert.ToString(AutoSaveSet));
    }
    private void AUTOScrollButton_Click(object sender, RoutedEventArgs e)
    {
        //FsButtonChecked(autotr, AUTOScrollButton);

        AutoTr = AutoTr is 0 ? 1 : 0;
        if (AutoSaveSet is 1)
        {
            ComboboxSaveSetting("SerialPortSettings", "DefaultAUTOSco", Convert.ToString(AutoTr));
        }
    }
    private void ChipToolKitComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {

        var selectedTool = (McuTool)ChipToolKitComboBox.SelectedItem;

        if (selectedTool is not null)
        {
            switch (selectedTool.Name)
            {
                case "None":
                    McuToolsFrame.Navigate(typeof(NoneTools), null, new DrillInNavigationTransitionInfo());
                    break;
                case "ESP8266":
                    McuToolsFrame.Navigate(typeof(ESP8266Tools), null, new DrillInNavigationTransitionInfo());
                    break;
                case "RP2040        M":
                    McuToolsFrame.Navigate(typeof(RP2040MPYTools), null, new DrillInNavigationTransitionInfo());
                    break;
                case "LPC1768        SM":
                    McuToolsFrame.Navigate(typeof(Lpc1768FsPnPTools), null, new DrillInNavigationTransitionInfo());
                    break;
                // 在这里添加更多的case语句来处理其他工具
                default:
                    break;
            }
        }
    }

    private void RXDataButton_Click(object sender, RoutedEventArgs e)
    {
    }
    private Task RXDATA_ClickAsync(object sender, RoutedEventArgs e)
    {
        // 在这里添加你的异步代码
        // 例如：await SomeAsyncMethod();
            
        //RXTextBox.Text = RXTextBox.Text + current_time.ToString("HH:mm:ss") + "  ";
        //Timesr = current_time.ToString("HH:mm:ss");

        //rxpstr = System.Text.Encoding.UTF8.GetString(datapwate);
        //rxpstr = datapwate.ToString();                          //将缓冲区赋值到输出
        //page1.RXTextBox.Text = page1.RXTextBox.Text + rxpstr + "";          //输出接收的数据
        //datapwate.Clear();                                      //清空缓冲区

        return Task.CompletedTask;
    }

    private void ShowTimeButton_Click(object sender, RoutedEventArgs e)
    {
    }

    public DispatcherTimer HideTimer { get; }

    private void Grid_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        HideTimer.Stop();
        MainPage1.Current.SerialPortToolsToggleButton.IsChecked = true;
    }

    private void Grid_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        if (MainPage1.Current.SerialPortToolsToggleButton.IsChecked == true)
        {
            if (HideTimer is not null && !HideTimer.IsEnabled)
            {
                HideTimer.Start();
            }
        }
    }

    private void HideTimer_Tick(object sender, object e)
    {
        HideTimer.Stop();
        if (PortIsConnect is 1)
        {
            MainPage1.Current.SerialPortToolsToggleButton.IsChecked = false;
        }
    }
}
