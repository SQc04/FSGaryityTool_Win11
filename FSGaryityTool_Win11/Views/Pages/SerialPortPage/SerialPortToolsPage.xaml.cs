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
using System.ComponentModel;

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
    /// DTR
    /// </summary>
    public static int Dtr { get; set; }

    /// <summary>
    /// RTS
    /// </summary>
    public static int Rts { get; set; }

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

        Page1.CommonRes.SerialPort.PinChanged += PinChanged;
    }



    private void FsBorderIsChecked(int isChecked, Border border, TextBlock textBlock)
    {
        SolidColorBrush foregroundColor = (SolidColorBrush)Application.Current.Resources["TextFillColorPrimaryBrush"];
        SolidColorBrush foreCheckColor = (SolidColorBrush)Application.Current.Resources["TextOnAccentFillColorPrimaryBrush"];

        SolidColorBrush backgroundColor = (SolidColorBrush)Application.Current.Resources["CardBackgroundFillColorDefaultBrush"];
        SolidColorBrush backCheckColor = (SolidColorBrush)Application.Current.Resources["AccentFillColorDefaultBrush"];


        //var darkAccentColor = (Windows.UI.Color)Application.Current.Resources["SystemAccentColorLight2"];
        //var lightAccentColor = (Windows.UI.Color)Application.Current.Resources["SystemAccentColorDark1"];
        //var theme = Application.Current.RequestedTheme;

        border.Background = isChecked == 1 ? backCheckColor : backgroundColor;

        textBlock.Foreground = isChecked == 1 ? foreCheckColor : foregroundColor;
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

            ///*
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

            ToolControlSerialPortMenuBox.SerialPortParity = (Parity)Enum.Parse(typeof(Parity), defaultPart);

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

            ToolControlSerialPortMenuBox.SerialPortStopBits = (StopBits)Enum.Parse(typeof(StopBits), defaultStop);

            for (var j = 5; j < 9; ++j)
            {
                DataComboBox.Items.Add(j);
            }
            DataComboBox.SelectedItem = defaultData;
            //DataBitSlider.Value = defaultData;
            //*/

            ToolControlSerialPortMenuBox.SerialPortDataBits = defaultData;

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            ToggleButtonIsChecked(RxHex, RxhexButton);
            ToggleButtonIsChecked(TxHex, TxhexButton);

            ToggleButtonIsChecked(Dtr, DtrButton);
            CommonRes.SerialPort.DtrEnable = Dtr is 1;

            ToggleButtonIsChecked(Rts, RtsButton);
            CommonRes.SerialPort.RtsEnable = Rts is 1;

            //ToggleButtonIsChecked(AutoSaveSet, SaveSetButton);
            ToggleButtonIsChecked(TxNewLine, TxNewLineButton);

            
            MainSerialPortLIstBox.AutoConnectSetting = AutoConnect is 1;

        }
    }

    public void LanguageSetting()
    {
        BaudTextBlock.Text = LanguageText("baudRatel");
        PartTextBlock.Text = LanguageText("parityl");
        StopTextBlock.Text = LanguageText("stopBits");
        DataTextBlock.Text = LanguageText("dataBits");
        RxhexButton.Content = LanguageText("rxHexl");
        TxhexButton.Content = LanguageText("txHexl");
        TxNewLineButton.Content = LanguageText("txNewLinel");// + "\uE751"
        //SaveSetButton.Content = LanguageText("autoSaveSetl");
        //AutoScrollButton.Content = LanguageText("autoScrolll");
        //AutoComButton.Content = LanguageText("autoSerichComl");
        //AutoConnectButton.Content = LanguageText("autoConnectl");

        ToolControlSerialPortMenuBox.BaudRateText = LanguageText("baudRatel");
        ToolControlSerialPortMenuBox.DataBitsText = LanguageText("dataBits");
        ToolControlSerialPortMenuBox.StopBitsText = LanguageText("stopBits");
        ToolControlSerialPortMenuBox.ParityText = LanguageText("parityl");
        ToolControlSerialPortMenuBox.EncodingText = LanguageText("encoding");

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

    private int ringHold = 0;
    private void PinChanged(object sender, SerialPinChangedEventArgs e)
    {
        SerialPort sp = (SerialPort)sender;
        SerialPinChange pinChange = e.EventType;

        switch (pinChange)
        {
            case SerialPinChange.CtsChanged:
                DispatcherQueue.TryEnqueue(() =>
                {
                    FsBorderIsChecked(sp.CtsHolding ? 1 : 0, CtsBorder, CtsTextBlock);
                    SetPinValue(sp.CtsHolding, "CTS");
                });
                break;
            case SerialPinChange.DsrChanged:
                DispatcherQueue.TryEnqueue(() =>
                {
                    FsBorderIsChecked(sp.DsrHolding ? 1 : 0, DsrBorder, DsrTextBlock);
                    SetPinValue(sp.DsrHolding, "DSR");
                });
                break;
            case SerialPinChange.CDChanged:
                DispatcherQueue.TryEnqueue(() =>
                {
                    FsBorderIsChecked(sp.CDHolding ? 1 : 0, CdhBorder, CdhTextBlock);
                    SetPinValue(sp.CDHolding, "DCD");
                });
                break;
            case SerialPinChange.Ring:
                DispatcherQueue.TryEnqueue(() =>
                {
                    FsBorderIsChecked(ringHold == 0 ? 1 : 0, RiBorder, RiTextBlock);
                    SetPinValue(ringHold == 0, "RI");
                    ringHold = ringHold == 0 ? 1 : 0;
                });
                break;
                // 根据需要添加其他情况
        }
    }
    private string OpenSerialPortInfo;

    public void SerialPortConnect()
    {
        ConCom = MainSerialPortLIstBox.SelectedPortSingle;

        var portName = MainSerialPortLIstBox.SelectedPortSingle;
        var bandRate = Convert.ToInt32(BandComboBox.SelectedItem);
        var parity = ToolControlSerialPortMenuBox.SerialPortParity;
        var stopBits = ToolControlSerialPortMenuBox.SerialPortStopBits;
        var dataBits = ToolControlSerialPortMenuBox.SerialPortDataBits;
        var encoding = ToolControlSerialPortMenuBox.SerialPortEncoding;
        /*
        var parity = ((ParityOption)ParComboBox.SelectedItem).Value;
        var stopBits = ((StopBitsOption)StopComboBox.SelectedItem).Value;
        var dataBits = Convert.ToInt32(DataComboBox.SelectedItem);
        var encoding = (string)EncodingComboBox.SelectedItem;
        */

        Page1.Current.SerialPortConnect(portName, bandRate, parity, stopBits, dataBits, 1500, encoding);

        Page1.Current._viewModel.AppendToRxTextinfo("BaudRate = " + Convert.ToInt32(BandComboBox.SelectedItem) + "\r\n");
        Page1.Current._viewModel.AppendToRxTextinfo("Parity = " + ToolControlSerialPortMenuBox.SerialPortParity + "\r\n");
        Page1.Current._viewModel.AppendToRxTextinfo("StopBits = " + ToolControlSerialPortMenuBox.SerialPortStopBits + "\r\n");
        Page1.Current._viewModel.AppendToRxTextinfo("DataBits = " + ToolControlSerialPortMenuBox.SerialPortDataBits + "\r\n");
        Page1.Current._viewModel.AppendToRxTextinfo("Encoding = " + ToolControlSerialPortMenuBox.SerialPortEncoding?.WebName + "\r\n");
        Page1.Current._viewModel.AppendToRxTextinfo(LanguageText("serialPortl") + " " + MainSerialPortLIstBox.SelectedPortSingle + LanguageText("spConnect") + "\r\n");

        // 同步引脚状态
        var sp = CommonRes.SerialPort;
        DispatcherQueue.TryEnqueue(() =>
        {
            try
            {
                FsBorderIsChecked(sp.CtsHolding ? 1 : 0, CtsBorder, CtsTextBlock);
                SetPinValue(sp.CtsHolding, "CTS");

                FsBorderIsChecked(sp.DsrHolding ? 1 : 0, DsrBorder, DsrTextBlock);
                SetPinValue(sp.DsrHolding, "DSR");

                FsBorderIsChecked(sp.CDHolding ? 1 : 0, CdhBorder, CdhTextBlock);
                SetPinValue(sp.CDHolding, "DCD");
            }
            catch (Exception ex)
            {
                
            }

            // RI 线一般没有直接属性，通常靠事件，初始可设为0或不变
            //FsBorderIsChecked(0, RiBorder, RiTextBlock);
            //SetPinValue(false, "RI");
        });

        RunTProgressBar.Value = 100;
        OpenSerialPortInfo = MainSerialPortLIstBox.SelectedPortSingle;
        PortIsConnect = 1;
    }

    public void SerialPortConnectCatch(Exception exception)
    {
        if (MainSerialPortLIstBox.SelectedPortSingle == null)
        {
            Page1.Current._viewModel.AppendToRxTextinfo("串口未选择" + "\r\n");
        }
        else 
        {
            //Page1.Current._viewModel.AppendToRxTextinfo(LanguageText("openSPErr") + "\r\n");
            string friendlyMessage = GetFriendlySerialPortErrorMessage(exception);
            Page1.Current._viewModel.AppendToRxTextinfo(friendlyMessage + "\r\n");
        }

        PortIsConnect = 0;
        //CONTButton.Content = "CONNECT";
    }

    private string GetFriendlySerialPortErrorMessage(Exception ex)
    {
        // 2. 权限问题 / 被占用（最常见）
        if (ex is UnauthorizedAccessException)
        {
            return "串口已被其他程序占用或无权限访问";
        }

        // 3. IO异常（最常见的一大类）
        if (ex is IOException ioEx)
        {
            string msg = ioEx.Message.ToLowerInvariant();

            if (msg.Contains("in use") || msg.Contains("占用") || msg.Contains("被另一个进程"))
            {
                return "串口正在被其他程序占用";
            }

            if (msg.Contains("不存在") || msg.Contains("not exist") || msg.Contains("not find") || msg.Contains("找不到") || msg.Contains("已移除"))
            {
                return "找不到该串口（设备可能已被拔掉或断开）";
            }

            if (msg.Contains("access denied"))
            {
                return "访问被拒绝（可能权限不足或被占用）";
            }

            if (msg.Contains("parameter") || msg.Contains("参数") || msg.Contains("无效"))
            {
                return "串口参数设置不合法（波特率、数据位、校验等）";
            }
            if (msg.Contains("超时") || msg.Contains("timed out"))
            {
                return "串口设备超时";
            }
            if (msg.Contains("没有发挥作用"))
            {
                return "串口设备未响应";
            }

            // 其他IO异常兜底
            return "打开串口失败(IO错误):" + ex.Message;
        }

        // 4. 参数非法（波特率、数据位等超出范围）
        if (ex is ArgumentException || ex is ArgumentOutOfRangeException)
        {
            if (ex.Message.Contains("port name") || ex.Message.Contains("端口名称"))
            {
                return "串口名称无效";
            }
            if (ex.Message.Contains("baud rate") || ex.Message.Contains("波特率"))
            {
                return "波特率设置不合法";
            }
            if (ex.Message.Contains("data bits") || ex.Message.Contains("数据位"))
            {
                return "数据位设置不合法（通常为5-8）";
            }
            if (ex.Message.Contains("stop bits") || ex.Message.Contains("停止位"))
            {
                return "停止位设置不合法";
            }
            return "参数设置错误：" + ex.Message;
        }

        // 5. 已经打开（理论上不应该发生，但以防万一）
        if (ex is InvalidOperationException && ex.Message.Contains("already open") || ex.Message.Contains("已经打开"))
        {
            return "串口已经处于打开状态";
        }

        // 6. 其他未知异常（保留原始信息方便调试）
        string friendly = "无法打开串口，请检查设备连接和设置";
        // 开发阶段可以临时加上这行，上线后再去掉
        // friendly += $"  [{ex.GetType().Name}] {ex.Message}";

        return friendly;
    }

    public void SerialPortClose()
    {
        var sp = CommonRes.SerialPort;
        DispatcherQueue.TryEnqueue(() =>
        {
            FsBorderIsChecked(0, CtsBorder, CtsTextBlock);
            SetPinValue(false, "CTS");

            FsBorderIsChecked(0, DsrBorder, DsrTextBlock);
            SetPinValue(false, "DSR");

            FsBorderIsChecked(0, CdhBorder, CdhTextBlock);
            SetPinValue(false, "DCD");

            FsBorderIsChecked(0, RiBorder, RiTextBlock);
            SetPinValue(false, "RI");
        });

        CommonRes.SerialPort.Close();                                                                              //关闭串口
        Page1.Current._viewModel.AppendToRxTextinfo("\n" + LanguageText("serialPortl") + " " + OpenSerialPortInfo + LanguageText("spClose") + "\r\n");
    }
    public void SerialPortDisconnect()
    {
        RunTProgressBar.Value = 0;

        PortIsConnect = 0;
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
                Page1.Current._viewModel.AppendToRxTextinfo("BaudRate = " + Convert.ToInt32(BandComboBox.SelectedItem) + "\r\n");
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
            Page1.Current._viewModel.AppendToRxTextinfo("Parity = " + (Parity)Enum.Parse(typeof(Parity), ((ParityOption)ParComboBox.SelectedItem).Value) + "\r\n");
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
            Page1.Current._viewModel.AppendToRxTextinfo("StopBits = " + (StopBits)Enum.Parse(typeof(StopBits), ((StopBitsOption)StopComboBox.SelectedItem).Value) + "\r\n");
        }
        if (StopComboBox.SelectedItem is StopBitsOption selectedOption)
        {
            if (float.TryParse(selectedOption.DisplayText, out var stopBits))
            {
                SetStopBorderWidth(stopBits);
            }
            else
            {
                SetStopBorderWidth(1);
            }
        }
    }
    
    public void SetStopBorderWidth(float stopBits)
    {
        StopBorder.Scale = new(stopBits, 1, 1);
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
            Page1.Current._viewModel.AppendToRxTextinfo("DataBits = " + Convert.ToInt32(DataComboBox.SelectedItem) + "\r\n");
        }
        DatainfoBadge.Value = Convert.ToInt32(DataComboBox.SelectedItem);
    }

    public static string EncodingSelectedItem = "utf-8";



    private void RXHEXButton_Click(object sender, RoutedEventArgs e)    //接收以十六进制数显示
    {
        RxHex = RxHex is 0 ? 1 : 0;
        RxhexButton.IsChecked = RxHex is 1;
        if (AutoSaveSet is 1)
        {
            ComboboxSaveSetting("SerialPortSettings", "DefaultRXHEX", Convert.ToString(RxHex));
        }
    }

    private void TXHEXButton_Click(object sender, RoutedEventArgs e)    //发送以十六进制数显示
    {
        TxHex = TxHex is 0 ? 1 : 0;
        TxhexButton.IsChecked = TxHex is 1;
        if (AutoSaveSet is 1)
        {
            ComboboxSaveSetting("SerialPortSettings", "DefaultTXHEX", Convert.ToString(TxHex));
        }
    }
    private void DTRButton_Click(object sender, RoutedEventArgs e)      //DTR信号使能
    {
        //FsButtonChecked(dtr, DTRButton);
        Dtr = Dtr is 0 ? 1 : 0;
        CommonRes.SerialPort.DtrEnable = Dtr is 1;
        if (AutoSaveSet is 1)
        {
            ComboboxSaveSetting("SerialPortSettings", "DefaultDTR", Convert.ToString(Dtr));
        }
    }
    private void RTSButton_Click(object sender, RoutedEventArgs e)      //RTS信号使能
    {
        //FsButtonChecked(rts, RTSButton);
        Rts = Rts is 0 ? 1 : 0;
        CommonRes.SerialPort.RtsEnable = Rts is 1;
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
    /*
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
    */
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

    private void MainSerialPortLIstBox_PortInserted(object sender, Controls.SerialPortEventArgs e)
    {
        Page1.Current._viewModel.AppendToRxTextinfo($"{e.PortName}: {e.PortDeviceDescription}{LanguageText("spPlogin")}\r\n");
        if(PortIsConnect == 0 && MainSerialPortLIstBox.AutoConnectSetting && MainSerialPortLIstBox.SelectedPortSingle == null)
        {
            Task.Run(() =>
            {
                Thread.Sleep(500);
                DispatcherQueue.TryEnqueue(() =>
                {
                    MainPage1.Current.SerialPortConnectToggleButton_Click(null, null);
                });
            });
        }
    }

    private void MainSerialPortLIstBox_PortRemoved(object sender, Controls.SerialPortEventArgs e)
    {
        Page1.Current._viewModel.AppendToRxTextinfo($"{e.PortName}: {e.PortDeviceDescription}{LanguageText("spPullout")}\r\n");
        if (PortIsConnect == 1 && e.PortName == OpenSerialPortInfo)
        {
            MainPage1.Current.SerialPortConnectToggleButton_Click(null, null);
        }
    }
}
