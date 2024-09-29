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
using FSGaryityTool_Win11.McuToolpage;
using System.Timers;
using System.Text.RegularExpressions;
using Windows.System;
using Microsoft.UI.Input;
using FSGaryityTool_Win11.Views.Pages.SerialPortPage;
using static FSGaryityTool_Win11.Views.Pages.SerialPortPage.MainPage1;


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
        public static string rxpstr;
        public static StringBuilder datapwate = new StringBuilder(2048);

        public ObservableCollection<DataItem> dataList { get; set; } = new ObservableCollection<DataItem>();


        public static int Rollta = 0;

        public static int RunPBT = 0;
        public static int RunT = 0;

        public static string SYSAPLOCAL = Environment.GetFolderPath(folder: Environment.SpecialFolder.LocalApplicationData);
        public static string FSFolder = Path.Combine(SYSAPLOCAL, "FAIRINGSTUDIO");
        public static string FSGravif = Path.Combine(FSFolder, "FSGravityTool");
        public static string FSSetJson = Path.Combine(FSGravif, "Settings.json");
        public static string FSSetToml = Path.Combine(FSGravif, "Settings.toml");

        public static int baudrate = 0;

        public static TomlTable settingstomlr;
        public static Page1 page1;

        

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
            page1 = this;
            this.Loaded += Page1_Loaded;

            this.taskbarInstance = (ITaskbarList3)new TaskbarList();
            this.taskbarInstance.HrInit();

        }

        public static string LanguageText(string laugtext)
        {
            //System.Globalization.CultureInfo.CurrentUICulture = new System.Globalization.CultureInfo("en-US");
            var culture = System.Globalization.CultureInfo.CurrentUICulture;
            string lang = culture.Name;

            var resourceManagerMap = new Dictionary<string, string>
            {
                {"zh-CN", "FSGaryityTool_Win11.Resources.zh_CN.resource"},
                {"en-US", "FSGaryityTool_Win11.Resources.en_US.resource"},
                // {"xx-xx", "FSGaryityTool_Win11.Resources.xx_xx.resource"}
            };

            string resourcePath = resourceManagerMap.ContainsKey(lang) ? resourceManagerMap[lang] : "FSGaryityTool_Win11.Resources.zh_CN.resource";
            var rm = new System.Resources.ResourceManager(resourcePath, Assembly.GetExecutingAssembly());

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
        /*==============================================================================================================
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
        */
        //============================================================================================================
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



        /*
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
        */
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
                    DefaultBAUD = TomlGetValueOrDefault(SPsettingstomlr, spSettings, "DefaultBAUD", "115200");
                    DefaultPart = TomlGetValueOrDefault(SPsettingstomlr, spSettings, "DefaultParity", "None");
                    DefaultSTOP = TomlGetValueOrDefault(SPsettingstomlr, spSettings, "DefaultSTOP", "One");
                    DefaultDATA = int.Parse(TomlGetValueOrDefault(SPsettingstomlr, spSettings, "DefaultDATA", "8"));

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


                CommonRes._serialPort.DataReceived += _serialPort_DataReceived;
                //COMListview.ItemsSource = new ObservableCollection<ComDataItem>();

                LaunageSetting();



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

            
        }
        public void LaunageSetting()
        {
            COMRstInfoBar.Message = LanguageText("comRstInfoBar");


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


        //public event SerialDataReceivedEventHandler DataReceived;

        public void SerialPortConnrct(string portName ,int baudRate ,string parity ,string stopBits ,int dataBits,int timeout, string encoding)
        {
            CommonRes._serialPort.PortName = portName;
            CommonRes._serialPort.BaudRate = baudRate;
            CommonRes._serialPort.Parity = (Parity)Enum.Parse(typeof(Parity), parity);        //校验位
            CommonRes._serialPort.StopBits = (StopBits)Enum.Parse(typeof(StopBits), stopBits); //停止位
            CommonRes._serialPort.DataBits = dataBits;                                //数据位
            CommonRes._serialPort.ReadTimeout = timeout;

            //_SerialPort.DtrEnable = true;                                                                             //启用数据终端就绪信息

            CommonRes._serialPort.Encoding = Encoding.GetEncoding(encoding);
            CommonRes._serialPort.ReceivedBytesThreshold = 1;

            CommonRes._serialPort.Open(); // 打开串口
        }

        /*
        private DateTime lastReceivedTime = DateTime.Now; // 添加这一行来声明lastReceivedTime变量
StringBuilder buffer = new StringBuilder();

private async void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
{
    // 在另一个线程中处理串口数据
    await Task.Run(() =>
    {
        // ... 其他代码

        if (rx == 0) // 如果以字符串形式读取
        {
            try
            {
                if (CommonRes._serialPort.IsOpen && CommonRes._serialPort.BytesToRead > 0)
                {
                    buffer.Append(CommonRes._serialPort.ReadExisting()); // 将新接收的数据添加到缓冲区

                    int newlineIndex;
                    string bufferStr = buffer.ToString();
                    while ((newlineIndex = bufferStr.IndexOf('\n')) != -1) // 只要缓冲区中还有换行符
                    {
                        string packet = bufferStr.Substring(0, newlineIndex).Replace("\r", ""); // 取出一个完整的数据包
                        buffer.Remove(0, newlineIndex + 1); // 从缓冲区中移除这个数据包
                        bufferStr = buffer.ToString(); // 更新bufferStr

                        if (!string.IsNullOrWhiteSpace(packet)) // 如果packet不为空
                        {
                            string Timesr = current_time.ToString("yyyy-MM-dd HH:mm:ss:ff   "); //显示时间
                            DataItem item = new DataItem { Timesr = Timesr, Rxstr = packet };

                            // 将操作排队到UI线程
                            tempDataList.AddLast(item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error reading from serial port: " + ex.Message);
            }
        }
        else // 以数值形式读取
        {
            int length = CommonRes._serialPort.BytesToRead; // 读取串口接收缓冲区字节数

            byte[] Data = new byte[length]; // 定义相同字节的数组

            CommonRes._serialPort.Read(Data, 0, length); // 串口读取缓冲区数据到数组中

            int byteCount = 0; // 添加一个计数器来跟踪已处理的字节数

            for (int i = 0; i < length; i++)
            {
                buffer.Append(Data[i].ToString("X2") + " ");
                byteCount++; // 增加字节计数器

                if (byteCount == 16) // 每16个字节作为一个元素
                {
                    string Timesr = current_time.ToString("yyyy-MM-dd HH:mm:ss:ff   "); //显示时间
                    DataItem item = new DataItem { Timesr = Timesr, Rxstr = buffer.ToString() };

                    // 将操作排队到UI线程
                    tempDataList.AddLast(item);

                    buffer.Clear(); // 清空buffer，开始新的一行
                    byteCount = 0; // 重置字节计数器
                }
            }

            // 如果buffer中还有剩余的数据，也添加到ListView中
            if (buffer.Length > 0 && (DateTime.Now - lastReceivedTime).TotalMilliseconds > 50)
            {
                string Timesr = current_time.ToString("yyyy-MM-dd HH:mm:ss:ff   "); //显示时间
                DataItem item = new DataItem { Timesr = Timesr, Rxstr = buffer.ToString() };

                // 将操作排队到UI线程
                tempDataList.AddLast(item);

                buffer.Clear(); // 清空buffer
            }

            lastReceivedTime = DateTime.Now; // 更新最后接收数据的时间
        }

        DispatcherQueue.TryEnqueue(() =>
        {
            UpdateItemsRepeater(tempDataList);
        });
        // ... 其他代码
    });
}

private void UpdateItemsRepeater(LinkedList<DataItem> items)
{
    foreach (var item in items)
    {
        dataList.AddLast(item);新的
        */
        private DateTime lastReceivedTime = DateTime.Now; // 添加这一行来声明lastReceivedTime变量
        StringBuilder buffer = new StringBuilder();

        private bool _isProcessing = false;
        private readonly object _lock = new object();

        private void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            lock (_lock)
            {
                string data = CommonRes._serialPort.ReadExisting();
                buffer.Append(data);

                if (!_isProcessing)
                {
                    _isProcessing = true;
                    Thread.Sleep(5);
                    Task.Run(ProcessData);
                }
            }
        }

        private void ProcessData()
        {
            try
            {
                string Timesr = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:ff   "); //显示时间

                while (true)
                {
                    string data = null;

                    lock (_lock)
                    {
                        if (buffer.Length > 0)
                        {
                            data = buffer.ToString();
                            buffer.Clear();
                        }
                        else
                        {
                            _isProcessing = false;
                            break;
                        }
                    }

                    if (data != null)
                    {
                        if (SerialPortToolsPage.rxHex == 0) // 如果以字符串形式读取
                        {
                            DataItem itemh = new DataItem { Timesr = Timesr, Rxstr = data };
                            DispatcherQueue.TryEnqueue(() =>
                            {
                                dataList.Add(itemh);
                            });
                            /*
                            int newlineIndex;
                            while ((newlineIndex = data.IndexOf('\n')) != -1) // 只要缓冲区中还有换行符
                            {
                                string packet = data.Substring(0, newlineIndex); // 取出一个完整的数据包 .Replace("\r", "")
                                data = data.Substring(newlineIndex + 1); // 从缓冲区中移除这个数据包

                                if (!string.IsNullOrWhiteSpace(packet)) // 如果packet不为空
                                {
                                    DataItem item = new DataItem { Timesr = Timesr, Rxstr = packet };

                                    // 将操作排队到UI线程
                                    DispatcherQueue.TryEnqueue(() =>
                                    {
                                        dataList.Add(item);
                                    });
                                }
                            }
                            
                            // 检查缓冲区中是否还有剩余数据
                            if (!string.IsNullOrWhiteSpace(data))
                            {
                                if (dataList.Count > 0)
                                {
                                    
                                    DispatcherQueue.TryEnqueue(() =>
                                    {
                                        // 将剩余数据添加到最后一个链表元素的Rxstr属性中
                                        dataList.Last().Rxstr += data;
                                    });
                                }
                                else
                                {
                                    // 如果链表为空，则新建一个链表元素
                                    DataItem item = new DataItem { Timesr = Timesr, Rxstr = data };
                                    DispatcherQueue.TryEnqueue(() =>
                                    {
                                        dataList.Add(item);
                                    });
                                }
                            }
                            */
                        }
                        else // 以数值形式读取
                        {
                            byte[] Data = Encoding.ASCII.GetBytes(data); // 将字符串转换为字节数组
                            int length = Data.Length;

                            DataItem itemh = new DataItem { Timesr = Timesr, Rxstr = data };
                            DispatcherQueue.TryEnqueue(() =>
                            {
                                dataList.Add(itemh);
                            });

                            for (int i = 0; i < length; i += 16)
                            {
                                string str = "";
                                for (int j = i; j < i + 16 && j < length; j++)
                                {
                                    str += Data[j].ToString("X2") + " ";
                                }

                                string Timesrh = "|                                     |"; //显示时间
                                DataItem item = new DataItem { Timesr = Timesrh, Rxstr = str };

                                DispatcherQueue.TryEnqueue(() =>
                                {
                                    dataList.Add(item);
                                    UpdateItemsRepeater();
                                });
                            }

                            //lastReceivedTime = DateTime.Now; // 更新最后接收数据的时间
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error processing data: " + ex.Message);
            }
            DispatcherQueue.TryEnqueue(() =>
            {
                UpdateItemsRepeater();
            });
        }

        private void UpdateItemsRepeater()
        {
            
            // 如果链表的长度超过1000，从头部删除元素
            //while (dataList.Count > 1000)
            //{
            //dataList.RemoveFirst();
            //}

            // 更新ListView的ItemsSource


            if (SerialPortToolsPage.autotr == 1 && dataList.Count > 0) // 检查dataList是否为空
            {
                RXListView.ScrollIntoView(dataList.Last()); // 滚动到最后一个元素
            }
        }

        private void TXTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

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
                mainPage1.SerialPortConnectToggleButton_Click(sender, e);
            }
        }

        // 根据发送模式选择发送数据的方式
        private void SendData()
        {
            // 如果是以字符的形式发送数据
            if (SerialPortToolsPage.txHex == 0)
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
                RXTextBox.Text += $"TX: {str}" + "\r\n";
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
                Task.Run(() =>
                {
                    // 将十六进制字符串转换为字节数组
                    byte[] bytes = ConvertHexStringToByteArray(input);
                    // 通过串口发送字节数组
                    CommonRes._serialPort.Write(bytes, 0, bytes.Length);
                    // 如果需要在每条消息后添加换行符
                    AppendNewLineIfRequired();

                    DispatcherQueue.TryEnqueue(() =>
                    {
                        // 更新接收文本框的内容
                        RXTextBox.Text += $"TX: 0x {string.Join(" ", bytes.Select(b => b.ToString("X2")))}\r\n";
                    });
                    input = "";
                });
                
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
            input = input.Replace("\r", "").Replace("\t", "");
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
            if (SerialPortToolsPage.txnewline == 1)
            {
                CommonRes._serialPort.Write("\r\n");
            }
        }

        private void CLEARButton_Click(object sender, RoutedEventArgs e)
        {
            
            RXTextBox.Text = "";    //清除文本框内容
            dataList.Clear();
        }

        private void RXTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
        
        

        public static void RSTButtonRes()
        {
            Task.Run(() =>
            {
                CommonRes._serialPort.BaudRate = 74880;// BANDComboBox.SelectedItem = "74880";//ESP12F

                //ESP8266 Reset
                CommonRes._serialPort.RtsEnable = true;
                Thread.Sleep(10);
                CommonRes._serialPort.DtrEnable = true;
                Thread.Sleep(10);
                CommonRes._serialPort.DtrEnable = false;
                Thread.Sleep(10);
                CommonRes._serialPort.RtsEnable = false;

                Thread.Sleep(150);

                CommonRes._serialPort.BaudRate = SerialPortToolsPage.baudrate;

                if (SerialPortToolsPage.dtr == 1)
                {
                    CommonRes._serialPort.DtrEnable = true;
                }
                if (SerialPortToolsPage.rts == 1)
                {
                    CommonRes._serialPort.DtrEnable = true;
                }
                //RSTButton_ClickAsync(null, null);
            });
        }
        /*
        private static Task RSTButton_ClickAsync(object sender, RoutedEventArgs e)
        {
            Thread.Sleep(200);


            //CommonRes._serialPort.DtrEnable = true;
            Thread.Sleep(1);

            CommonRes._serialPort.BaudRate = baudrate;

            if (dtr == 1)
            {
                CommonRes._serialPort.DtrEnable = true;
            }
            if (rts == 1)
            {
                CommonRes._serialPort.DtrEnable = true;
            }

            return Task.CompletedTask;
        }
        */

        


        private Task RXDATA_ClickAsync(object sender, RoutedEventArgs e)
        {
            // 在这里添加你的异步代码
            // 例如：await SomeAsyncMethod();
            //current_time = System.DateTime.Now;     //获取当前时间
            //RXTextBox.Text = RXTextBox.Text + current_time.ToString("HH:mm:ss") + "  ";
            //Timesr = current_time.ToString("HH:mm:ss");



            //rxpstr = System.Text.Encoding.UTF8.GetString(datapwate);
            //rxpstr = datapwate.ToString();                          //将缓冲区赋值到输出
            //RXTextBox.Text = RXTextBox.Text + rxpstr + "";          //输出接收的数据
            //datapwate.Clear();                                      //清空缓冲区

            return Task.CompletedTask;
        }

        public bool isCtrlDown = false;
        private void TXTextBox_PreviewKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Control)
            {
                isCtrlDown = true;
            }
            else if (e.Key == Windows.System.VirtualKey.Enter && isCtrlDown)
            {
                e.Handled = true;  // 阻止事件继续传递
            }
        }

        private void TXTextBox_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter && isCtrlDown)
            {
                TXButton_Click(this, new RoutedEventArgs());
            }
            if (e.Key == Windows.System.VirtualKey.Control)
            {
                isCtrlDown = false;
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

            if (RXListView.Items.Count >= 0)
            {
                // 根据位置找到对应的ListViewItem
                ListViewItem listViewItem = VisualTreeHelper.FindElementsInHostCoordinates(point, RXListView).FirstOrDefault(x => x is ListViewItem) as ListViewItem;
                // 如果找到了ListViewItem，将其设置为选中状态
                if (listViewItem != null)
                {
                    listViewItem.IsSelected = true;
                }
            }
            
            MenuFlyoutItem copyItem = new MenuFlyoutItem 
            { 
                Text = LanguageText("copyAlll"),
                Icon = new FontIcon
                {
                    Glyph = "\uE8C8"
                },
                Margin = new Thickness(2)
            };
            copyItem.Click += CopyItem_Click;


            MenuFlyoutItem copyTimestampItem = new MenuFlyoutItem 
            { 
                Text = LanguageText("copyTimel"),
                Icon = new FontIcon
                {
                    Glyph = "\uE823"
                },
                Margin = new Thickness(2)
            };
            copyTimestampItem.Click += CopyTimestampItem_Click;

            MenuFlyoutItem copyDataItem = new MenuFlyoutItem 
            { 
                Text = LanguageText("copyDatal"),
                Icon = new FontIcon
                {
                    Glyph = "\uE8A4"
                },
                Margin = new Thickness(2)
            };
            copyDataItem.Click += CopyDataItem_Click;

            // 创建一个新的菜单飞出（MenuFlyout）并添加菜单项
            MenuFlyout menuFlyout = new MenuFlyout();

            menuFlyout.SystemBackdrop = new DesktopAcrylicBackdrop();
            menuFlyout.Items.Add(copyItem);
            menuFlyout.Items.Add(copyTimestampItem);
            menuFlyout.Items.Add(copyDataItem);

            menuFlyout.Closed += MenuFlyout_Closed;
            // 显示菜单
            menuFlyout.ShowAt(sender as UIElement, e.GetPosition(sender as UIElement));
        }
        private void MenuFlyout_Closed(object sender, object e)
        {
            MenuFlyout menuFlyout = sender as MenuFlyout;
            if (menuFlyout != null)
            {
                foreach (var item in menuFlyout.Items)
                {
                    if (item is MenuFlyoutItem menuItem)
                    {
                        menuItem.Click -= CopyItem_Click;
                        menuItem.Click -= CopyTimestampItem_Click;
                        menuItem.Click -= CopyDataItem_Click;
                    }
                }
                menuFlyout.Items.Clear();
                menuFlyout.SystemBackdrop = null;
                Debug.WriteLine("MenuFlyoutClosed");
            }
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


        private void RxstrTextBlock_PointerCanceled(object sender, PointerRoutedEventArgs e)
        {
            if (sender is TextBlock textBlock)
            {
                // 获取TextBlock的父控件（即Grid）
                var grid = textBlock.Parent as Grid;
                if (grid != null)
                {
                    // 获取Grid的DataContext（即ListView的项）
                    var dataItem = grid.DataContext;
                    // 设置ListView的选中项
                    RXListView.SelectedItem = dataItem;
                }
            }
        }

        private void RxstrTextBlock_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (sender is TextBlock textBlock)
            {
                // 获取TextBlock的父控件（即Grid）
                var grid = textBlock.Parent as Grid;
                if (grid != null)
                {
                    // 获取Grid的DataContext（即ListView的项）
                    var dataItem = grid.DataContext;
                    // 设置ListView的选中项
                    RXListView.SelectedItem = dataItem;
                }
            }
        }

    }
}
