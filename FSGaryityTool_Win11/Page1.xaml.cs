using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System.IO.Ports;
using System.Text;
using System.Collections.ObjectModel;
using System.Threading;
using System.Reflection;
using System.Threading.Tasks;
using Tommy;
using System.Diagnostics;
using Windows.ApplicationModel.DataTransfer;
using System.Globalization;
using FSGaryityTool_Win11.Views.Pages.SerialPortPage;
using Windows.System;
using System.ComponentModel;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static FSGaryityTool_Win11.Page1;

namespace FSGaryityTool_Win11;

public class DataItem : INotifyPropertyChanged
{
    private string _timesr;
    private string _rxstr;

    public string Timesr
    {
        get => _timesr;
        set
        {
            if (_timesr != value)
            {
                _timesr = value;
                OnPropertyChanged(nameof(Timesr));
            }
        }
    }

    public string Rxstr
    {
        get => _rxstr;
        set
        {
            if (_rxstr != value)
            {
                _rxstr = value;
                OnPropertyChanged(nameof(Rxstr));
            }
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

/*
public class ComDataItem
{
    public string ComName { get; set; }
}
*/

public sealed partial class Page1 : Page
{
    public static string Rxpstr { get; set; }

    public static StringBuilder Datapwate { get; set; } = new();
    public ObservableCollection<DataItem> DataList { get; set; } = new();
    public partial class ViewModel : INotifyPropertyChanged
    {
        private string _rxTextinfo;

        public string RxTextinfo
        {
            get => _rxTextinfo;
            set
            {
                if (_rxTextinfo != value)
                {
                    _rxTextinfo = value;
                    OnPropertyChanged(nameof(RxTextinfo));
                    // 通知订阅者 RxTextinfo 变化
                    RxTextinfoChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event EventHandler RxTextinfoChanged;

        public void AppendToRxTextinfo(string text)
        {
            RxTextinfo += text;
        }

        public void ClearRxTextinfo()
        {
            RxTextinfo = "";
        }
    }

    public ViewModel _viewModel = new ViewModel();
    public static int Rollta { get; set; } = 0;

    public static int RunPbt { get; set; } = 0;

    public static int RunT { get; set; } = 0;

    public static string Sysaplocal { get; set; } = Environment.GetFolderPath(folder: Environment.SpecialFolder.LocalApplicationData);

    public static string FsFolder { get; set; } = Path.Combine(Sysaplocal, "FAIRINGSTUDIO");

    public static string FsGravif { get; set; } = Path.Combine(FsFolder, "FSGravityTool");

    public static string FsSetJson { get; set; } = Path.Combine(FsGravif, "Settings.json");

    public static string FsSetToml { get; set; } = Path.Combine(FsGravif, "Settings.toml");

    public static int Baudrate { get; set; } = 0;

    public static TomlTable SettingsTomlr { get; set; }

    public static Page1 Current { get; private set; }

    private bool _isLoaded;

    public static string Str { get; set; }

    public static class CommonRes
    {
        public static SerialPort SerialPort { get; set; } = new();

        public static SerialPort SerialPort2 { get; set; } = new();
    }

    private ITaskbarList3 _taskbarInstance;

    private DispatcherTimer _resizeTimer;

    public Page1()
    {
        InitializeComponent();
        Current = this;
        Loaded += Page1_Loaded;

        _taskbarInstance = (ITaskbarList3)new TaskbarList();
        _taskbarInstance.HrInit();

        RxText.DataContext = _viewModel;
        _viewModel.RxTextinfoChanged += ViewModel_RxTextinfoChanged;

        //_resizeTimer = new DispatcherTimer();
        //_resizeTimer.Interval = TimeSpan.FromMilliseconds(500);
        //_resizeTimer.Tick += ResizeTimer_Tick;
    }

    public static string LanguageText(string laugtext)
    {
        //System.Globalization.CultureInfo.CurrentUICulture = new System.Globalization.CultureInfo("en-US");
        var culture = CultureInfo.CurrentUICulture;
        var lang = culture.Name;

        var resourceManagerMap = new Dictionary<string, string>
        {
            {"zh-CN", "FSGaryityTool_Win11.Resources.zh_CN.resource"},
            {"en-US", "FSGaryityTool_Win11.Resources.en_US.resource"},
            // {"xx-xx", "FSGaryityTool_Win11.Resources.xx_xx.resource"}
        };

        var resourcePath = resourceManagerMap.GetValueOrDefault(lang, "FSGaryityTool_Win11.Resources.zh_CN.resource");
        var rm = new System.Resources.ResourceManager(resourcePath, Assembly.GetExecutingAssembly());

        var text = rm.GetString(laugtext);
        return text;
    }

    private void ToggleButtonIsChecked(int isChecked, ToggleButton toggleButton)
    {
        toggleButton.IsChecked = isChecked is 1;
    }

    /*==============================================================================================================
    private void FsButtonIsChecked(int isChecked, Button button)
    {
        var foregroundColor = COMButton.Foreground as SolidColorBrush;
        var backgroundColor = COMButton.Background as SolidColorBrush;
        var darkaccentColor = (Windows.UI.Color)Application.Current.Resources["SystemAccentColorLight2"];
        var ligtaccentColor = (Windows.UI.Color)Application.Current.Resources["SystemAccentColorDark1"];
        var theme = Application.Current.RequestedTheme;

        if (isChecked is 1)
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

        if (isChecked is 1)
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

            string defaultBaud;
            string defaultPart;
            string defaultStop;
            int defaultData;

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

            CommonRes.SerialPort.DataReceived += _serialPort_DataReceived;

            //COMListview.ItemsSource = new ObservableCollection<ComDataItem>();

            LanguageSetting();
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

    public void LanguageSetting()
    {
        ComRstInfoBar.Message = LanguageText("comRstInfoBar");
    }
        
    /*
    public static string GetPortDescription(string portName)
    {
        string description = null;

        RegistryKey localMachineRegistry = Registry.LocalMachine;
        RegistryKey hardwareRegistry = localMachineRegistry.OpenSubKey("HARDWARE\\DEVICEMAP\\SERIALCOMM");

        if (hardwareRegistry is not null)
        {
            string[] deviceNames = hardwareRegistry.GetValueNames();
            foreach (string deviceName in deviceNames)
            {
                if (hardwareRegistry.GetValue(deviceName).ToString().Equals(portName))
                {
                    string instanceName = deviceName.Substring(deviceName.IndexOf("\\Device\\") + "\\Device\\".Length);
                    RegistryKey deviceRegistry = localMachineRegistry.OpenSubKey("SYSTEM\\CurrentControlSet\\Enum\\" + instanceName + "\\Device Parameters");
                    if (deviceRegistry is not null)
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

    public void SerialPortConnect(string portName ,int baudRate ,string parity ,string stopBits ,int dataBits,int timeout, string encoding)
    {
        CommonRes.SerialPort.PortName = portName;
        CommonRes.SerialPort.BaudRate = baudRate;
        CommonRes.SerialPort.Parity = (Parity)Enum.Parse(typeof(Parity), parity);        //校验位
        CommonRes.SerialPort.StopBits = (StopBits)Enum.Parse(typeof(StopBits), stopBits); //停止位
        CommonRes.SerialPort.DataBits = dataBits;                                //数据位
        CommonRes.SerialPort.ReadTimeout = timeout;

        //_SerialPort.DtrEnable = true;                                                                             //启用数据终端就绪信息

        CommonRes.SerialPort.Encoding = Encoding.GetEncoding(encoding);
        CommonRes.SerialPort.ReceivedBytesThreshold = 1;

        CommonRes.SerialPort.Open(); // 打开串口
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

    if (rx is 0) // 如果以字符串形式读取
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

    private StringBuilder _buffer = new();
    private List<byte> _bufferHex = [];

    private bool _isProcessing;
    private readonly object _lock = new();
    Encoding currentEncoding;

    private void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        lock (_lock)
        {
            int bytesToRead = CommonRes.SerialPort.BytesToRead;

            byte[] bytes = new byte[bytesToRead];

            CommonRes.SerialPort.Read(bytes, 0, bytesToRead);

            _bufferHex.AddRange(bytes);

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
            var timesr = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:ff   "); //显示时间

            if (SerialPortToolsPage.RxHex is 0)
            {

            }
            else
            {

            }


            while (true)
            {
                /*
                string data;
                byte[] bytes;

                lock (_lock)
                {
                    if (_buffer.Length > 0)
                    {
                        if (SerialPortToolsPage.RxHex is 0)
                        {
                            data = _buffer.ToString();
                        }
                        else
                        {
                            bytes = _bufferHex;
                        }
                 
                        _buffer.Clear();
                    }
                    else
                    {
                        _isProcessing = false;
                        break;
                    }
                }

                if (data is not null)
                {
                    if (SerialPortToolsPage.RxHex is 0) // 如果以字符串形式读取
                    {
                        ProcessDataString(timesr, data);
                    }
                    else // 以数值形式读取
                    {
                        currentEncoding = Encoding.GetEncoding(SerialPortToolsPage.EncodingSelectedItem);
                        ProcessDataHex(timesr, data, currentEncoding);
                    }
                }
                //*/
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Error processing data: " + ex.Message);
        }
        DispatcherQueue.TryEnqueue(UpdateItemsRepeater);
    }

    private void ProcessDataString(string timesr, string data)
    {
        DataItem itemh = new DataItem { Timesr = timesr, Rxstr = data };

        DispatcherQueue.TryEnqueue(() =>
        {
            DataList.Add(itemh);
        });
    }

    private void ProcessDataHex(string timesr, byte[] bytes, Encoding encoding)
    {
        int length = bytes.Length;

        for (var i = 0; i < length; i += 16)
        {
            string tmpStr = "";
            for (var j = i; j < i + 16 && j < length; j++)
            {
                tmpStr += bytes[j].ToString("X2") + " ";
            }

            string timesrh = "|                                     |"; //显示时间
            DataItem item = new DataItem { Timesr = timesrh, Rxstr = tmpStr };

            DispatcherQueue.TryEnqueue(() =>
            {
                DataList.Add(item);
                UpdateItemsRepeater();
            });
        }
    }


    private void UpdateItemsRepeater()
    {
        // 如果链表的长度超过1000，从头部删除元素
        //while (dataList.Count > 1000)
        //{
        //dataList.RemoveFirst();
        //}

        // 更新ListView的ItemsSource

        if (SerialPortToolsPage.AutoTr is 1 && DataList.Count > 0) // 检查dataList是否为空
        {
            RxListView.ScrollIntoView(DataList.Last()); // 滚动到最后一个元素
        }
    }

    private void TXTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
    }

    // 当点击发送按钮时执行的操作
    private void TXButton_Click(object sender, RoutedEventArgs e)
    {
        // 如果串口设备已经打开了
        if (!CommonRes.SerialPort.IsOpen) return;

        try
        {
            // 根据发送模式选择发送数据的方式
            SendData();
            // 发送完成后清空发送文本框的内容
            TxTextBox.Text = "";
        }
        catch (Exception ex)
        {
            // 如果发送过程中出现错误，显示错误信息并断开串口连接
            _viewModel.AppendToRxTextinfo($"{ex.Message}\r\n");
            MainPage1.Current.SerialPortConnectToggleButton_Click(sender, e);
        }
    }

    // 根据发送模式选择发送数据的方式
    private void SendData()
    {
        // 如果是以字符的形式发送数据
        if (SerialPortToolsPage.TxHex is 0)
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
            var str = TxTextBox.Text;
            // 通过串口发送字符串
            CommonRes.SerialPort.Write(str);
            // 如果需要在每条消息后添加换行符
            AppendNewLineIfRequired();
            // 更新接收文本框的内容
            _viewModel.AppendToRxTextinfo($"TX: {str}" + "\r\n");
        }
        catch
        {
            // 如果串口字符写入出错，显示错误信息
            _viewModel.AppendToRxTextinfo($"{LanguageText("txStringErr")}\r\n");
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
            var input = PrepareHexString();
            Task.Run(() =>
            {
                // 将十六进制字符串转换为字节数组
                var bytes = ConvertHexStringToByteArray(input);
                // 通过串口发送字节数组
                CommonRes.SerialPort.Write(bytes, 0, bytes.Length);
                // 如果需要在每条消息后添加换行符
                AppendNewLineIfRequired();

                DispatcherQueue.TryEnqueue(() =>
                {
                    // 更新接收文本框的内容
                    _viewModel.AppendToRxTextinfo($"TX: 0x {string.Join(" ", bytes.Select(b => b.ToString("X2")))}\r\n");
                });
                input = "";
            });
        }
        catch (FormatException)
        {
            // 如果输入的字符串不是有效的十六进制数，显示错误信息
            _viewModel.AppendToRxTextinfo($"{LanguageText("txHexErr")}\r\n");
            // 抛出异常以便外层捕获
            throw;
        }
    }

    // 对输入的十六进制字符串进行预处理
    private string PrepareHexString()
    {
        // 获取要发送的十六进制字符串，并去除所有空格
        var input = TxTextBox.Text.Trim().Replace(" ", "");
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
        var bytes = new byte[input.Length / 2];
        for (var i = 0; i < input.Length; i += 2)
        {
            // 尝试将每两个字符转换为一个字节
            if (!byte.TryParse(input.AsSpan(i, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out bytes[i / 2]))
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
        if (SerialPortToolsPage.TxNewLine is 1)
        {
            CommonRes.SerialPort.Write("\r\n");
        }
    }

    private void CLEARButton_Click(object sender, RoutedEventArgs e)
    {
            
        _viewModel.ClearRxTextinfo();    //清除文本框内容
        DataList.Clear();
        Datapwate.Clear();
    }

    private void RXTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
    }

    public static void RstButtonRes()
    {
        Task.Run(() =>
        {
            CommonRes.SerialPort.BaudRate = 74880;// BANDComboBox.SelectedItem = "74880";//ESP12F

            //ESP8266 Reset
            CommonRes.SerialPort.RtsEnable = true;
            Thread.Sleep(10);
            CommonRes.SerialPort.DtrEnable = true;
            Thread.Sleep(10);
            CommonRes.SerialPort.DtrEnable = false;
            Thread.Sleep(10);
            CommonRes.SerialPort.RtsEnable = false;

            Thread.Sleep(150);

            CommonRes.SerialPort.BaudRate = SerialPortToolsPage.Baudrate;

            if (SerialPortToolsPage.Dtr is 1)
            {
                CommonRes.SerialPort.DtrEnable = true;
            }
            if (SerialPortToolsPage.Rts is 1)
            {
                CommonRes.SerialPort.DtrEnable = true;
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

        if (dtr is 1)
        {
            CommonRes._serialPort.DtrEnable = true;
        }
        if (rts is 1)
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

    public bool IsCtrlDown { get; set; }

    private void TXTextBox_PreviewKeyDown(object sender, KeyRoutedEventArgs e)
    {
        switch (e.Key)
        {
            case VirtualKey.Control:
                IsCtrlDown = true;
                break;
            case VirtualKey.Enter when IsCtrlDown:
                e.Handled = true;  // 阻止事件继续传递
                break;
        }
    }

    private void TXTextBox_KeyUp(object sender, KeyRoutedEventArgs e)
    {
        switch (e.Key)
        {
            case VirtualKey.Enter when IsCtrlDown:
                TXButton_Click(this, new());
                break;
            case VirtualKey.Control:
                IsCtrlDown = false;
                break;
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
        var point = e.GetPosition(null);

        if (RxListView.Items.Count >= 0)
        {
            // 根据位置找到对应的ListViewItem
            // 如果找到了ListViewItem，将其设置为选中状态
            if (VisualTreeHelper.FindElementsInHostCoordinates(point, RxListView).FirstOrDefault(x => x is ListViewItem) is ListViewItem listViewItem)
            {
                listViewItem.IsSelected = true;
            }
        }
            
        var copyItem = new MenuFlyoutItem 
        { 
            Text = LanguageText("copyAlll"),
            Icon = new FontIcon
            {
                Glyph = "\uE8C8"
            },
            Margin = new(2)
        };
        copyItem.Click += CopyItem_Click;

        var copyTimestampItem = new MenuFlyoutItem 
        { 
            Text = LanguageText("copyTimel"),
            Icon = new FontIcon
            {
                Glyph = "\uE823"
            },
            Margin = new(2)
        };
        copyTimestampItem.Click += CopyTimestampItem_Click;

        var copyDataItem = new MenuFlyoutItem 
        { 
            Text = LanguageText("copyDatal"),
            Icon = new FontIcon
            {
                Glyph = "\uE8A4"
            },
            Margin = new(2)
        };
        copyDataItem.Click += CopyDataItem_Click;

        // 创建一个新的菜单飞出（MenuFlyout）并添加菜单项
        var menuFlyout = new MenuFlyout { SystemBackdrop = new DesktopAcrylicBackdrop() };

        menuFlyout.Items.Add(copyItem);
        menuFlyout.Items.Add(copyTimestampItem);
        menuFlyout.Items.Add(copyDataItem);

        menuFlyout.Closed += MenuFlyout_Closed;
        // 显示菜单
        menuFlyout.ShowAt(sender as UIElement, e.GetPosition(sender as UIElement));
    }
    private void MenuFlyout_Closed(object sender, object e)
    {
        if (sender is MenuFlyout menuFlyout)
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

        // 确保选中的项不为空
        if (RxListView.SelectedItem is DataItem selectedItem)
        {
            // 获取选中项的内容
            var content1 = selectedItem.Timesr;
            var content2 = selectedItem.Rxstr;

            // 将两个内容合并，你可以根据需要添加适当的分隔符
            var combinedContent = content1 + " " + content2;

            // 将内容复制到剪贴板
            var dataPackage = new DataPackage();
            dataPackage.SetText(combinedContent);
            Clipboard.SetContent(dataPackage);
        }
    }
    private void CopyTimestampItem_Click(object sender, RoutedEventArgs e)
    {
        // 确保选中的项不为空
        if (RxListView.SelectedItem is DataItem selectedItem)
        {
            // 获取选中项的内容
            var content1 = selectedItem.Timesr;

            // 将两个内容合并，你可以根据需要添加适当的分隔符

            // 将内容复制到剪贴板
            var dataPackage = new DataPackage();
            dataPackage.SetText(content1);
            Clipboard.SetContent(dataPackage);
        }
    }
    private void CopyDataItem_Click(object sender, RoutedEventArgs e)
    {
        // 获取当前选中的项

        // 确保选中的项不为空
        if (RxListView.SelectedItem is DataItem selectedItem)
        {
            // 获取选中项的内容
            var content2 = selectedItem.Rxstr;

            // 将两个内容合并，你可以根据需要添加适当的分隔符

            // 将内容复制到剪贴板
            var dataPackage = new DataPackage();
            dataPackage.SetText(content2);
            Clipboard.SetContent(dataPackage);
        }
    }

    private void RxstrTextBlock_PointerCanceled(object sender, PointerRoutedEventArgs e)
    {
        if (sender is TextBlock { Parent: Grid grid })
            // 获取TextBlock的父控件（即Grid）
        {
            // 获取Grid的DataContext（即ListView的项）
            var dataItem = grid.DataContext;
            // 设置ListView的选中项
            RxListView.SelectedItem = dataItem;
        }
    }

    private void RxstrTextBlock_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (sender is TextBlock { Parent: Grid grid })
            // 获取TextBlock的父控件（即Grid）
        {
            // 获取Grid的DataContext（即ListView的项）
            var dataItem = grid.DataContext;
            // 设置ListView的选中项
            RxListView.SelectedItem = dataItem;
        }
    }


    public void SerialPortOpen()
    {
        BorderBackRx.BorderBrush = (Brush)Application.Current.Resources["TextControlElevationBorderFocusedBrush"];
        RxTextBoxBorder.BorderBrush = (Brush)Application.Current.Resources["TextControlElevationBorderFocusedBrush"];
    }

    public void SerialPortClose()
    {
        BorderBackRx.BorderBrush = (Brush)Application.Current.Resources["TextControlElevationBorderBrush"];
        RxTextBoxBorder.BorderBrush = (Brush)Application.Current.Resources["TextControlElevationBorderBrush"];
    }

    private async void ViewModel_RxTextinfoChanged(object sender, EventArgs e)
    {
        await Task.Delay(10);
        // 获取 ScrollViewer 的内容高度
        var scrollViewer = RxTextBoxScrollViewer;
        var extentHeight = scrollViewer.ExtentHeight;
        var viewportHeight = scrollViewer.ViewportHeight;
        //await Task.Delay(100);
        // 滚动到最下面
        scrollViewer.ChangeView(null, extentHeight - viewportHeight, null);
    }
    
    private void RxText_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        //if(RxText.TextWrapping == TextWrapping.WrapWholeWords)
        //{
        //    RxText.TextWrapping = TextWrapping.NoWrap;
        //}
        //RxText.Foreground = (Brush)Application.Current.Resources["TextFillColorPrimaryBrush"];
        //_resizeTimer.Stop();
        //_resizeTimer.Start();
    }
    private void ResizeTimer_Tick(object sender, object e)
    {
        //RxText.TextWrapping = TextWrapping.WrapWholeWords;
        //_resizeTimer.Stop();
        // 这里可以添加重新计算自动换行的逻辑
        //RxText.Foreground = (Brush)Application.Current.Resources["SystemFillColorAttentionBrush"];
    }
}
