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
using static System.Net.Mime.MediaTypeNames;
using Tommy;
using static FSGaryityTool_Win11.Page1;
using static FSGaryityTool_Win11.Views.Pages.SerialPortPage.MainPage1;
using System.IO.Ports;
using System.Management;
using Microsoft.UI;
using Application = Microsoft.UI.Xaml.Application;
using System.Threading.Tasks;
using System.Threading;
using System.Text;
using FSGaryityTool_Win11.McuToolpage;
using System.Numerics;
using System.Diagnostics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FSGaryityTool_Win11.Views.Pages.SerialPortPage
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SerialPortToolsPage : Page
    {
        public static int getPortInfo = 0;
        public static int portIsConnect = 0;//Con
        public static string conCom = "";
        public static int txHex = 0; //TXHEX
        public static int rxHex = 0; //RXHEX
        public static int dtr = 0;//FTR
        public static int rts = 0;//RTS
        public static int shtime = 0;//ShowTime
        public static int autotr = 0;//AUTOScroll
        public static int autosaveset;
        public static int autosercom;
        public static int autoconnect;
        public static int txnewline = 0;

        public static string[] ArryPort; //定义字符串数组，数组名为 ArryPort

        public static int baudrate = 0;

        public System.Threading.Timer timer;
        public System.Threading.Timer timerSerialPort;

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
        public class MCUTool
        {
            public string Name { get; set; }
            public string Description { get; set; }
        }

        public static SerialPortToolsPage serialPortToolsPage;
        public SerialPortToolsPage()
        {
            this.InitializeComponent();
            serialPortToolsPage = this;
            this.Loaded += SerialPortToolsPage_Loaded;

        }
        private void FsBorderIsChecked(int isChecked, Border border, TextBlock textBlock)
        {
            var foregroundColor = (SolidColorBrush)Application.Current.Resources["TextFillColorPrimaryBrush"];
            var backgroundColor = (SolidColorBrush)Application.Current.Resources["CardBackgroundFillColorDefaultBrush"];
            var foreCheckColor = (SolidColorBrush)Application.Current.Resources["TextOnAccentFillColorPrimaryBrush"];
            var darkaccentColor = (Windows.UI.Color)Application.Current.Resources["SystemAccentColorLight2"];
            var ligtaccentColor = (Windows.UI.Color)Application.Current.Resources["SystemAccentColorDark1"];
            var theme = Application.Current.RequestedTheme;

            if (isChecked == 1)
            {
                if (theme == ApplicationTheme.Dark)
                {
                    // 当前处于深色模式
                    border.Background = new SolidColorBrush(darkaccentColor);
                }
                else if (theme == ApplicationTheme.Light)
                {
                    // 当前处于浅色模式
                    border.Background = new SolidColorBrush(ligtaccentColor);
                }
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
            if (isChecked == 1)
            {
                toggleButton.IsChecked = true;
            }
            else
            {
                toggleButton.IsChecked = false;
            }
        }
        private void SerialPortToolsPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!_isLoaded)
            {
                _isLoaded = true;

                string DefaultBAUD;
                string DefaultPart;
                string DefaultSTOP;
                int DefaultDATA;
                string DefaultEncoding;

                using (StreamReader reader = File.OpenText(Page1.FSSetToml))
                {
                    TomlTable SPsettingstomlr = TOML.Parse(reader);             //读取TOML
                                                                                //Debug.WriteLine("Print:" + SPsettingstomlr["FSGravitySettings"]["DefaultNvPage"]);
                                                                                //NvPage = int.Parse(settingstomlr["FSGravitySettings"]["DefaultNvPage"]);
                    string spSettings = "SerialPortSettings";
                    //检查设置是否为NULL
                    DefaultBAUD = TomlGetValueOrDefault(SPsettingstomlr, spSettings, "DefaultBAUD", "115200");
                    DefaultPart = TomlGetValueOrDefault(SPsettingstomlr, spSettings, "DefaultParity", "None");
                    DefaultSTOP = TomlGetValueOrDefault(SPsettingstomlr, spSettings, "DefaultSTOP", "One");
                    DefaultDATA = int.Parse(TomlGetValueOrDefault(SPsettingstomlr, spSettings, "DefaultDATA", "8"));
                    DefaultEncoding = TomlGetValueOrDefault(SPsettingstomlr, spSettings, "DefaultEncoding", "utf-8");

                    txHex = int.Parse(TomlGetValueOrDefault(SPsettingstomlr, spSettings, "DefaultTXHEX", "0"));
                    rxHex = int.Parse(TomlGetValueOrDefault(SPsettingstomlr, spSettings, "DefaultRXHEX", "0"));
                    dtr = int.Parse(TomlGetValueOrDefault(SPsettingstomlr, spSettings, "DefaultDTR", "1"));
                    rts = int.Parse(TomlGetValueOrDefault(SPsettingstomlr, spSettings, "DefaultRTS", "0"));
                    shtime = int.Parse(TomlGetValueOrDefault(SPsettingstomlr, spSettings, "DefaultSTime", "0"));
                    autotr = int.Parse(TomlGetValueOrDefault(SPsettingstomlr, spSettings, "DefaultAUTOSco", "1"));
                    autosaveset = int.Parse(TomlGetValueOrDefault(SPsettingstomlr, spSettings, "AutoDaveSet", "1"));
                    autosercom = int.Parse(TomlGetValueOrDefault(SPsettingstomlr, spSettings, "AutoSerichCom", "1"));
                    autoconnect = int.Parse(TomlGetValueOrDefault(SPsettingstomlr, spSettings, "AutoConnect", "1"));
                    txnewline = int.Parse(TomlGetValueOrDefault(SPsettingstomlr, spSettings, "DefaultTXNewLine", "0"));

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
                LaunageSetting();
                // 在你的代码后台，定义一个List<string>作为数据源
                List<string> BaudRates = new List<string>()
                {
                    "75", "110", "134", "150", "300", "600", "1200", "1800", "2400", "4800", "7200", "9600", "14400", "19200", "38400", "57600", "74880","115200", "128000", "230400", "250000", "500000", "1000000", "2000000"
                };
                // 将ComboBox的ItemsSource属性绑定到这个数据源
                BANDComboBox.ItemsSource = BaudRates;
                // 设置默认选项
                BANDComboBox.SelectedItem = DefaultBAUD; // 将"9600"设置为默认选项

                List<ParityOption> ParRates = new List<ParityOption>()
                {
                    new ParityOption { DisplayText = LanguageText("parityNone"), Value = "None" },
                    new ParityOption { DisplayText = LanguageText("parityOdd"), Value = "Odd" },
                    new ParityOption { DisplayText = LanguageText("parityEven"), Value = "Even" },
                    new ParityOption { DisplayText = LanguageText("parityMark"), Value = "Mark" },
                    new ParityOption { DisplayText = LanguageText("paritySpace"), Value = "Space" }
                };
                PARComboBox.ItemsSource = ParRates;
                PARComboBox.DisplayMemberPath = "DisplayText";
                PARComboBox.SelectedValuePath = "Value";
                PARComboBox.SelectedValue = DefaultPart;

                List<StopBitsOption> StopRates = new List<StopBitsOption>()
                {
                    //new StopBitsOption { DisplayText = LanguageText("stopNone"), Value = "None" },
                    new StopBitsOption { DisplayText = LanguageText("stopOne"), Value = "One" },
                    new StopBitsOption { DisplayText = LanguageText("stopOnePointFive"), Value = "OnePointFive" },
                    new StopBitsOption { DisplayText = LanguageText("stopTwo"), Value = "Two" }
                };
                STOPComboBox.ItemsSource = StopRates;
                STOPComboBox.DisplayMemberPath = "DisplayText";
                STOPComboBox.SelectedValuePath = "Value";
                STOPComboBox.SelectedValue = DefaultSTOP;

                for (int j = 5; j < 9; ++j)
                {
                    DATAComboBox.Items.Add(j);
                }
                DATAComboBox.SelectedItem = DefaultDATA;
                DATANumberBox.Value = DefaultDATA;

                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                EncodingInfo[] encodings = Encoding.GetEncodings();
                // 创建一个 List<string> 来存储编码名称
                List<string> Encodings = new List<string>() 
                {
                    "gb2312",
                };
                // 将编码名称添加到 List<string> 中
                foreach (EncodingInfo encodingInfo in encodings)
                {
                    Encodings.Add(encodingInfo.Name);
                }

                EncodingComboBox.ItemsSource = Encodings;
                EncodingComboBox.SelectedItem = DefaultEncoding;

                COMButton_Click(this, new RoutedEventArgs());

                ToggleButtonIsChecked(rxHex, RXHEXButton);
                ToggleButtonIsChecked(txHex, TXHEXButton);

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

                //BorderBackRX.Background = backgroundColor;

                ToggleButtonIsChecked(autosaveset, SaveSetButton);
                ToggleButtonIsChecked(autoconnect, AutoConnectButton);
                ToggleButtonIsChecked(txnewline, TXNewLineButton);

                ToggleButtonIsChecked(autosercom, AutoComButton);
                if (autosercom == 1)
                {
                    timerSerialPort = new System.Threading.Timer(TimerSerialPortTick, null, 0, 1500);
                    AutoSerchComProgressRing.IsActive = true;
                }
                else AutoSerchComProgressRing.IsActive = false;

                ToggleButtonIsChecked(autoconnect, AutoConnectButton);
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
            foreach (EncodingInfo ei in Encoding.GetEncodings())
            {
                Encoding e = ei.GetEncoding();

                Debug.Write(string.Format("{0,-6} {1,-25} ", ei.CodePage, ei.Name));
                Debug.Write(string.Format("{0,-8} {1,-8} ", e.IsBrowserDisplay, e.IsBrowserSave));
                Debug.Write(string.Format("{0,-8} {1,-8} ", e.IsMailNewsDisplay, e.IsMailNewsSave));
                Debug.WriteLine(string.Format("{0,-8} {1,-8} ", e.IsSingleByte, e.IsReadOnly));
            }
        }

        public void LaunageSetting()
        {
            BaudTextBlock.Text = LanguageText("baudRatel");
            PartTextBlock.Text = LanguageText("parityl");
            StopTextBlock.Text = LanguageText("stopBits");
            DataTextBlock.Text = LanguageText("dataBits");
            EncodingTextBlock.Text = LanguageText("encoding");
            RXHEXButton.Content = LanguageText("rxHexl");
            TXHEXButton.Content = LanguageText("txHexl");
            TXNewLineButton.Content = LanguageText("txNewLinel");
            SaveSetButton.Content = LanguageText("autoSaveSetl");
            AUTOScrollButton.Content = LanguageText("autoScrolll");
            AutoComButton.Content = LanguageText("autoSerichComl");
            AutoConnectButton.Content = LanguageText("autoConnectl");
            
            //COMRstInfoBar.Message = LanguageText("comRstInfoBar");

            List<MCUTool> mcuTools = new List<MCUTool>()
                {
                    new MCUTool() { Name = "None", Description = LanguageText("mcuToolNone") },
                    new MCUTool() { Name = "ESP8266", Description = LanguageText("mcuToolEsp8266") },
                    new MCUTool() { Name = "RP2040        M", Description = LanguageText("mcuToolRP2040MPY") }
                };

            ChipToolKitComboBox.ItemsSource = mcuTools;
            ChipToolKitComboBox.SelectedItem = mcuTools[1];
        }
        public void TimerTick(Object stateInfo)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                //RXDATA_ClickAsync(null, null);
                page1.current_time = System.DateTime.Now;     //获取当前时间

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
                if (RI == 0)
                {
                    // RI 信号使能
                    RI = 1;
                    FsBorderIsChecked(RI, RIBorder, RITextBlock);

                }
                else
                {
                    // RI 信号未使能
                    RI = 0;
                    FsBorderIsChecked(RI, RIBorder, RITextBlock);

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
                getPortInfo = 1;
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

        public void TimerSerialPortTick(Object stateInfo)       //串口热插拔检测
        {
            if (getPortInfo == 0) return;
            string[] NowPort = SerialPort.GetPortNames(); // 获取当前所有可用的串口名称
            NowPort = new HashSet<string>(NowPort).ToArray(); // 移除可能的重复项

            string[] LastPort = ArryPort ?? NowPort; // 获取上一次检测到的串口名称，如果没有则使用当前串口名称
            ArryPort = NowPort; // 更新上一次检测到的串口名称

            var lastPortSet = new HashSet<string>(LastPort); // 创建一个包含上一次串口名称的HashSet
            var nowPortSet = new HashSet<string>(NowPort); // 创建一个包含当前串口名称的HashSet

            var insertedPorts = nowPortSet.Except(lastPortSet).ToArray(); // 找出新插入的串口
            var removedPorts = lastPortSet.Except(nowPortSet).ToArray(); // 找出被拔出的串口

            if (insertedPorts.Length > 0 || removedPorts.Length > 0) // 如果有新插入的串口或者有串口被拔出
            {
                DispatcherQueue.TryEnqueue(() => // 在UI线程中执行以下操作
                {
                    string selectedPort = (string)COMComboBox.SelectedItem; // 获取当前选中的串口

                    foreach (var port in insertedPorts) // 遍历所有新插入的串口
                    {
                        SerialPortInfo info = SerialPortInfo.GetPort(port); // 获取串口的信息
                        page1.RXTextBox.Text += $"{port}{LanguageText("spPlogin")}\r\n"; // 更新文本框的内容

                    }

                    foreach (var port in removedPorts) // 遍历所有被拔出的串口
                    {
                        page1.RXTextBox.Text += $"{port}{LanguageText("spPullout")}\r\n"; // 更新文本框的内容
                        if (portIsConnect == 1 && port == selectedPort) // 如果当前连接的串口被拔出，则断开连接
                        {
                            mainPage1.SerialPortConnectToggleButton_Click(null, null);
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

                    if (portIsConnect == 0 && COMComboBox.SelectedItem == null && insertedPorts.Length > 0) // 如果没有选中的串口，并且有新插入的串口
                    {
                        COMComboBox.SelectedItem = insertedPorts[0]; // 选中新插入的串口
                        COMListview.SelectedItem = insertedPorts[0]; // 选中新插入的串口
                        if (AutoConnectButton.IsChecked == true) // 如果设置了自动连接，则尝试连接新插入的串口
                        {
                            mainPage1.SerialPortConnectToggleButton_Click(null, null);
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
                        if (portIsConnect == 0)
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
                        if (portIsConnect == 1)                                                   //自动断开已拔出的设备串口连接
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
            await SearchAndAddSerialToComboBoxAsync(CommonRes._serialPort, COMComboBox);           //扫描并将串口添加至下拉列表

            async Task SearchAndAddSerialToComboBoxAsync(SerialPort MyPort, ComboBox MyBox)
            {
                page1.RXTextBox.Text = page1.RXTextBox.Text + LanguageText("startSerichSP") + "\r\n";
                string commme = (string)COMComboBox.SelectedItem;           //记忆串口名
                ArryPort = SerialPort.GetPortNames();                       //SerialPort.GetPortNames()函数功能为获取计算机所有可用串口，以字符串数组形式输出
                ArryPort = new HashSet<string>(ArryPort).ToArray(); // 移除可能的重复项
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
                    if (info != null)
                    {
                        page1.RXTextBox.Text += ArryPort[i] + ": " + info.Description + "\r\n";
                    }
                    else
                    {
                        page1.RXTextBox.Text += ArryPort[i] + "\r\n";
                    }
                    //RXTextBox.Text += ArryPort[i] + "\r\n" + GetPortDescription(ArryPort[i]) + "\r\n";
                }
                //MyBox.Items.Add("COM0");
                page1.RXTextBox.Text = page1.RXTextBox.Text + LanguageText("overSerichSP") + "\r\n";
                COMComboBox.SelectedItem = commme;
                COMListview.SelectedItem = commme;
            }
            
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


        public void SerialPortConnect()
        {
            conCom = (string)COMComboBox.SelectedItem;

            string portName = (string)COMComboBox.SelectedItem;
            int bandRate = Convert.ToInt32(BANDComboBox.SelectedItem);
            string parity = ((ParityOption)PARComboBox.SelectedItem).Value;
            string stopBits = ((StopBitsOption)STOPComboBox.SelectedItem).Value;
            int dataBits = Convert.ToInt32(DATAComboBox.SelectedItem);
            string encoding = (string)EncodingComboBox.SelectedItem;

            page1.SerialPortConnrct(portName, bandRate, parity, stopBits, dataBits, 1500, encoding);

            page1.RXTextBox.Text = page1.RXTextBox.Text + "BaudRate = " + Convert.ToInt32(BANDComboBox.SelectedItem) + "\r\n";
            page1.RXTextBox.Text = page1.RXTextBox.Text + "Parity = " + (Parity)Enum.Parse(typeof(Parity), ((ParityOption)PARComboBox.SelectedItem).Value) + "\r\n";
            page1.RXTextBox.Text = page1.RXTextBox.Text + "StopBits = " + (StopBits)Enum.Parse(typeof(StopBits), ((StopBitsOption)STOPComboBox.SelectedItem).Value) + "\r\n";
            page1.RXTextBox.Text = page1.RXTextBox.Text + "DataBits = " + Convert.ToInt32(DATAComboBox.SelectedItem) + "\r\n";
            page1.RXTextBox.Text = page1.RXTextBox.Text + "Encoding = " + (string)EncodingComboBox.SelectedItem + "\r\n";
            page1.RXTextBox.Text = page1.RXTextBox.Text + LanguageText("serialPortl") + " " + COMComboBox.SelectedItem + LanguageText("spConnect") + "\r\n";

            timer = new System.Threading.Timer(TimerTick, null, 0, 250); // 每秒触发8次

            portIsConnect = 1;

        }

        public void SerialPortConnectcatch()
        {
            page1.RXTextBox.Text = page1.RXTextBox.Text + LanguageText("openSPErr") + "\r\n";
            //MessageBox.Show("打开串口失败，请检查相关设置", "错误");

            portIsConnect = 0;
            //CONTButton.Content = "CONNECT";
        }
        public void SerialPortClose()
        {
            CommonRes._serialPort.Close();                                                                              //关闭串口
            page1.RXTextBox.Text = page1.RXTextBox.Text + "\n" + LanguageText("serialPortl") + " " + COMComboBox.SelectedItem + LanguageText("spClose") + "\r\n";
        }
        public void SerialPortDisconnect()
        {
            portIsConnect = 0;
            timer.Dispose();
        }

        private void COMComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string ComIs;
            ComIs = (string)COMComboBox.SelectedItem;
            COMListview.SelectedItem = ComIs;
            if (portIsConnect == 1)
            {
                if (conCom != ComIs)
                    page1.COMRstInfoBar.IsOpen = true;
                else
                    page1.COMRstInfoBar.IsOpen = false;
            }
        }
        private void ClearCOMCombobox_Click(object sender, RoutedEventArgs e)
        {
            COMComboBox.SelectedItem = null;
            COMListview.SelectedItem = null;
        }

        private void ComboboxSaveSetting(string menuName, string name, string settingItem)
        {
            using (StreamReader reader = File.OpenText(FSSetToml))                    //打开TOML文件
            {
                settingstomlr = TOML.Parse(reader);

                settingstomlr[menuName][name] = settingItem;
            }

            using (StreamWriter writer = File.CreateText(FSSetToml))                  //将设置写入TOML文件
            {
                settingstomlr.WriteTo(writer);
                writer.Flush();
            }
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
                ComboboxSaveSetting("SerialPortSettings", "AutoSerichCom", Convert.ToString(autosercom));
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
                ComboboxSaveSetting("SerialPortSettings", "AutoConnect", Convert.ToString(autoconnect));
            }
        }
        private void COMListview_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string ComIs;
            ComIs = (string)COMListview.SelectedItem;
            COMComboBox.SelectedItem = ComIs;

        }
        private void BANDComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // 检查输入的是否为数字
            if (!int.TryParse((string)BANDComboBox.SelectedItem, out int baudRate) || baudRate == 0)
            {
                // 如果输入的不是数字，使用设置文件中的数字覆盖它
                using (StreamReader reader = File.OpenText(FSSetToml))                    //打开TOML文件
                {
                    settingstomlr = TOML.Parse(reader);
                    BANDComboBox.SelectedItem = ((Tommy.TomlString)settingstomlr["SerialPortSettings"]["DefaultBAUD"]).Value;
                }
            }
            else
            {
                if (autosaveset == 1)
                {
                    ComboboxSaveSetting("SerialPortSettings", "DefaultBAUD", (string)BANDComboBox.SelectedItem);
                }
                if (portIsConnect == 1)
                {
                    CommonRes._serialPort.BaudRate = Convert.ToInt32(BANDComboBox.SelectedItem);
                    page1.RXTextBox.Text = page1.RXTextBox.Text + "BaudRate = " + Convert.ToInt32(BANDComboBox.SelectedItem) + "\r\n";
                }
                baudrate = Convert.ToInt32(BANDComboBox.SelectedItem);
            }
            if (Convert.ToInt32(BANDComboBox.SelectedItem) <= 7200)
            {
                BaudrateIcon.Glyph = "\uEC48";
            }
            else if (Convert.ToInt32(BANDComboBox.SelectedItem) > 7200 && Convert.ToInt32(BANDComboBox.SelectedItem) < 128000)
            {
                BaudrateIcon.Glyph = "\uEC49";
            }
            else
            {
                BaudrateIcon.Glyph = "\uEC4A";
            }
        }

        private void PARComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (autosaveset == 1)
            {
                ComboboxSaveSetting("SerialPortSettings", "DefaultParity", ((ParityOption)PARComboBox.SelectedItem).Value);
            }
            if (portIsConnect == 1)
            {
                CommonRes._serialPort.Parity = (Parity)Enum.Parse(typeof(Parity), ((ParityOption)PARComboBox.SelectedItem).Value);
                page1.RXTextBox.Text = page1.RXTextBox.Text + "Parity = " + (Parity)Enum.Parse(typeof(Parity), ((ParityOption)PARComboBox.SelectedItem).Value) + "\r\n";
            }
        }
        private void STOPComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (autosaveset == 1)
            {
                ComboboxSaveSetting("SerialPortSettings", "DefaultSTOP", ((StopBitsOption)STOPComboBox.SelectedItem).Value);
            }
            if (portIsConnect == 1)
            {
                CommonRes._serialPort.StopBits = (StopBits)Enum.Parse(typeof(StopBits), ((StopBitsOption)STOPComboBox.SelectedItem).Value);
                page1.RXTextBox.Text = page1.RXTextBox.Text + "StopBits = " + (StopBits)Enum.Parse(typeof(StopBits), ((StopBitsOption)STOPComboBox.SelectedItem).Value) + "\r\n";
            }
            if (STOPComboBox.SelectedItem is StopBitsOption selectedOption)
            {
                if (float.TryParse(selectedOption.DisplayText, out float stopBits))
                {
                    StopBorder.Scale = new Vector3(stopBits, 1, 1);
                }
                else
                {
                    // 处理解析失败的情况
                    StopBorder.Scale = new Vector3(1, 1, 1);
                }
            }
        }
        private void DATAComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (autosaveset == 1)
            {
                ComboboxSaveSetting("SerialPortSettings", "DefaultDATA", Convert.ToString(DATAComboBox.SelectedItem));
            }
            if (portIsConnect == 1)
            {
                CommonRes._serialPort.DataBits = Convert.ToInt32(DATAComboBox.SelectedItem);
                page1.RXTextBox.Text = page1.RXTextBox.Text + "DataBits = " + Convert.ToInt32(DATAComboBox.SelectedItem) + "\r\n";
            }
            DatainfoBadge.Value = Convert.ToInt32(DATAComboBox.SelectedItem);
        }

        private void EncodingComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (autosaveset == 1)
            {
                ComboboxSaveSetting("SerialPortSettings", "DefaultEncoding", (string)EncodingComboBox.SelectedItem);
            }
            if (portIsConnect == 1)
            {
                CommonRes._serialPort.Encoding = Encoding.GetEncoding((string)EncodingComboBox.SelectedItem);
                page1.RXTextBox.Text = page1.RXTextBox.Text + "Encoding = " + (string)EncodingComboBox.SelectedItem + "\r\n";
            }
        }

        private void RXHEXButton_Click(object sender, RoutedEventArgs e)    //接收以十六进制数显示
        {
            if (rxHex == 0)
            {
                rxHex = 1;
                RXHEXButton.IsChecked = true;
            }
            else
            {
                rxHex = 0;
                RXHEXButton.IsChecked = false;
            }
            if (autosaveset == 1)
            {
                ComboboxSaveSetting("SerialPortSettings", "DefaultRXHEX", Convert.ToString(rxHex));
            }
        }

        private void TXHEXButton_Click(object sender, RoutedEventArgs e)    //发送以十六进制数显示
        {

            if (txHex == 0)
            {
                txHex = 1;
                TXHEXButton.IsChecked = true;
            }
            else
            {
                txHex = 0;
                TXHEXButton.IsChecked = false;
            }
            if (autosaveset == 1)
            {
                ComboboxSaveSetting("SerialPortSettings", "DefaultTXHEX", Convert.ToString(txHex));
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
                ComboboxSaveSetting("SerialPortSettings", "DefaultDTR", Convert.ToString(dtr));
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
                ComboboxSaveSetting("SerialPortSettings", "DefaultRTS", Convert.ToString(rts));
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
                ComboboxSaveSetting("SerialPortSettings", "DefaultTXNewLine", Convert.ToString(txnewline));
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
            ComboboxSaveSetting("SerialPortSettings", "AutoDaveSet", Convert.ToString(autosaveset));
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
                ComboboxSaveSetting("SerialPortSettings", "DefaultAUTOSco", Convert.ToString(autotr));
            }
        }
        private void ChipToolKitComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            MCUTool selectedTool = (MCUTool)ChipToolKitComboBox.SelectedItem;

            if (selectedTool != null)
            {
                switch (selectedTool.Name)
                {
                    case "None":
                        McuToolsFrame.Navigate(typeof(NoneTools));
                        break;
                    case "ESP8266":
                        McuToolsFrame.Navigate(typeof(ESP8266Tools));
                        break;
                    case "RP2040        M":
                        McuToolsFrame.Navigate(typeof(RP2040MPYTools));
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

    }
}
