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
//using System.Management;
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

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FSGaryityTool_Win11
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Page1 : Page
    {
        public static int Con = 0;
        public static int txf = 0;
        public static int tx = 0; //TXHEX
        public static int rx = 0; //RXHEX
        public static int dtr = 0;//FTR
        public static int rts = 0;//RTS
        public static int shtime = 0;//ShowTime
        public static int autotr = 0;//AUTOScroll
        public static int rxs = 0;
        public static string rxpstr;
        public static StringBuilder datapwate = new StringBuilder(2048);

        public static int RunPBT = 0;
        public static int RunT = 0;

        public static string SYSAPLOCAL = Environment.GetFolderPath(folder: Environment.SpecialFolder.LocalApplicationData);
        public static string FSFolder = Path.Combine(SYSAPLOCAL, "FAIRINGSTUDIO");
        public static string FSGravif = Path.Combine(FSFolder, "FSGravityTool");
        public static string FSSetJson = Path.Combine(FSGravif, "Settings.json");
        public static string FSSetToml = Path.Combine(FSGravif, "Settings.toml");

        

        public Timer timer;
        private bool _isLoaded;
        public static string str;
        private DateTime current_time = new DateTime();

        public static class CommonRes
        {
            public static SerialPort _serialPort = new SerialPort();
            public static SerialPort serialPort2 = new SerialPort();

        }



        public Page1()
        {
            string DefaultBAUD;
            string DefaultPart;
            string DefaultSTOP;
            int DefaultDATA;

            using (StreamReader reader = File.OpenText(FSSetToml))
            {
                TomlTable SPsettingstomlr = TOML.Parse(reader);             //读取TOML
                //Debug.WriteLine("Print:" + SPsettingstomlr["FSGravitySettings"]["DefaultNvPage"]);
                //NvPage = int.Parse(settingstomlr["FSGravitySettings"]["DefaultNvPage"]);

                DefaultBAUD = SPsettingstomlr["SerialPortSettings"]["DefaultBAUD"];
                DefaultPart = SPsettingstomlr["SerialPortSettings"]["DefaultParity"];
                DefaultSTOP = SPsettingstomlr["SerialPortSettings"]["DefaultSTOP"];
                DefaultDATA = int.Parse(SPsettingstomlr["SerialPortSettings"]["DefaultDATA"]);

                tx = int.Parse(SPsettingstomlr["SerialPortSettings"]["DefaultTXHEX"]);
                rx = int.Parse(SPsettingstomlr["SerialPortSettings"]["DefaultRXHEX"]);
                dtr = int.Parse(SPsettingstomlr["SerialPortSettings"]["DefaultDTR"]);
                rts = int.Parse(SPsettingstomlr["SerialPortSettings"]["DefaultRTS"]);
                shtime = int.Parse(SPsettingstomlr["SerialPortSettings"]["DefaultSTime"]);
                autotr = int.Parse(SPsettingstomlr["SerialPortSettings"]["DefaultAUTOSco"]);

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

            this.InitializeComponent();

            this.Loaded += Page1_Loaded;

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
            var darkaccentColor = (Windows.UI.Color)Application.Current.Resources["SystemAccentColorLight2"];
            var ligtaccentColor = (Windows.UI.Color)Application.Current.Resources["SystemAccentColorDark1"];
            var theme = Application.Current.RequestedTheme;

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
            if (rx == 1)
            {
                if (theme == ApplicationTheme.Dark)
                {
                    // 当前处于深色模式
                    RXHEXButton.Background = new SolidColorBrush(darkaccentColor);
                    RXHEXButton.Foreground = new SolidColorBrush(Colors.Black);
                }
                else if (theme == ApplicationTheme.Light)
                {
                    // 当前处于浅色模式
                    RXHEXButton.Background = new SolidColorBrush(ligtaccentColor);
                    RXHEXButton.Foreground = new SolidColorBrush(Colors.White);
                }
            }
            else
            {
                RXHEXButton.Background = backgroundColor;
                RXHEXButton.Foreground = foregroundColor;
            }

            if (tx == 1)
            {
                if (theme == ApplicationTheme.Dark)
                {
                    // 当前处于深色模式
                    TXHEXButton.Background = new SolidColorBrush(darkaccentColor);
                    TXHEXButton.Foreground = new SolidColorBrush(Colors.Black);
                }
                else if (theme == ApplicationTheme.Light)
                {
                    // 当前处于浅色模式
                    TXHEXButton.Background = new SolidColorBrush(ligtaccentColor);
                    TXHEXButton.Foreground = new SolidColorBrush(Colors.White);
                }
            }
            else
            {
                TXHEXButton.Background = backgroundColor;
                TXHEXButton.Foreground = foregroundColor;
            }

            if (dtr == 1)
            {
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
            }
            else
            {
                DTRButton.Background = backgroundColor;
                DTRButton.Foreground = foregroundColor;

                CommonRes._serialPort.DtrEnable = false;
            }

            if (rts == 1)
            {
                if (theme == ApplicationTheme.Dark)
                {
                    // 当前处于深色模式
                    RTSButton.Background = new SolidColorBrush(darkaccentColor);
                    RTSButton.Foreground = new SolidColorBrush(Colors.Black);
                }
                else if (theme == ApplicationTheme.Light)
                {
                    // 当前处于浅色模式
                    RTSButton.Background = new SolidColorBrush(ligtaccentColor);
                    RTSButton.Foreground = new SolidColorBrush(Colors.White);
                }
                CommonRes._serialPort.RtsEnable = true;
            }
            else
            {
                RTSButton.Background = backgroundColor;
                RTSButton.Foreground = foregroundColor;

                CommonRes._serialPort.RtsEnable = false;
            }

            if (shtime == 1)
            {
                if (theme == ApplicationTheme.Dark)
                {
                    // 当前处于深色模式
                    ShowTimeButton.Background = new SolidColorBrush(darkaccentColor);
                    ShowTimeButton.Foreground = new SolidColorBrush(Colors.Black);
                }
                else if (theme == ApplicationTheme.Light)
                {
                    // 当前处于浅色模式
                    ShowTimeButton.Background = new SolidColorBrush(ligtaccentColor);
                    ShowTimeButton.Foreground = new SolidColorBrush(Colors.White);
                }

            }
            else
            {
                ShowTimeButton.Background = backgroundColor;
                ShowTimeButton.Foreground = foregroundColor;

            }

            if (autotr == 1)
            {
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
            }
            else
            {
                AUTOScrollButton.Background = backgroundColor;
                AUTOScrollButton.Foreground = foregroundColor;

            }

            RunProgressBar.Visibility = Visibility.Collapsed;

            BorderBack1.Background = backgroundColor;
            BorderBack2.Background = backgroundColor;
            BorderBack3.Background = backgroundColor;
            BorderBack4.Background = backgroundColor;
        }

        private void Page1_Loaded(object sender, RoutedEventArgs e)
        {
            if (!_isLoaded)
            {
                COMButton_Click(this, new RoutedEventArgs());
                _isLoaded = true;
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

            });
            
        }

        //public event SerialDataReceivedEventHandler DataReceived;

        private void COMButton_Click(object sender, RoutedEventArgs e)
        {
            //COMButton.Content = "Clicked";
            SearchAndAddSerialToComboBox(CommonRes._serialPort, COMComboBox);           //扫描并将串口添加至下拉列表

            void SearchAndAddSerialToComboBox(SerialPort MyPort, ComboBox MyBox)
            {
                RXTextBox.Text = RXTextBox.Text + "Start search SerialPort\r\n";
                string commme = (string)COMComboBox.SelectedItem;           //记忆串口名
                string[] ArryPort;                                          //定义字符串数组，数组名为 ArryPort
                ArryPort = SerialPort.GetPortNames();                       //SerialPort.GetPortNames()函数功能为获取计算机所有可用串口，以字符串数组形式输出
                string scom = String.Join("\r\n", ArryPort);
                RXTextBox.Text = RXTextBox.Text + scom + "\r\n";
                MyBox.Items.Clear();                                        //清除当前组合框下拉菜单内容                  
                for (int i = 0; i < ArryPort.Length; i++)
                {
                    MyBox.Items.Add(ArryPort[i]);                           //将所有的可用串口号添加到端口对应的组合框中
                }
                //MyBox.Items.Add("COM0");
                RXTextBox.Text = RXTextBox.Text + "Search SerialPort succeed!\r\n";
                COMComboBox.SelectedItem = commme;
            }
            //COMComboBox.SelectedItem = "COM0";
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

            if (Con == 0)
            {
                try
                {
                    CommonRes._serialPort.PortName = (string)COMComboBox.SelectedItem;                  //开启的串口名称为选择串口的ComboBox组件中的内容
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

                    RXTextBox.Text = RXTextBox.Text + "SerialPort " + COMComboBox.SelectedItem + " IS OPEN" + "\r\n";

                    CommonRes._serialPort.Open();                                                                               //打开串口
                    
                    timer = new Timer(TimerTick, null, 0, 125); // 每秒触发8次

                    CONTButton.Content = "DISCONNECT";
                    Con = 1;
                    RunProgressBar.ShowError = false;
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
                    RXTextBox.Text = RXTextBox.Text + "打开串口失败，请检查相关设置" + "\r\n";
                    //MessageBox.Show("打开串口失败，请检查相关设置", "错误");
                    Con = 0;
                    CONTButton.Content = "CONNECT";
                    CONTButton.Background = backgroundColor;
                    CONTButton.Foreground = foregroundColor;
                    RunProgressBar.IsIndeterminate = true;
                    RunProgressBar.ShowError=true;
                    RunProgressBar.Visibility = Visibility.Visible;
                }

            }
            else
            {
                //RXTextBox.Text = RXTextBox.Text + "SerialPort IS DISCONNECT\r\n";

                try
                {
                    CommonRes._serialPort.Close();                                                                              //关闭串口
                    RXTextBox.Text = RXTextBox.Text + "\n" + "SerialPort IS CLOSE" + "\r\n";
                }
                catch (Exception err)                                                                       //一般情况下关闭串口不会出错，所以不需要加处理程序
                {
                    RXTextBox.Text = RXTextBox.Text + err + "\r\n";
                }
                CONTButton.Content = "CONNECT";
                Con = 0;
                CONTButton.Background = backgroundColor;
                CONTButton.Foreground = foregroundColor;
                RunProgressBar.IsIndeterminate = false;
                RunProgressBar.ShowError = false;
                RunProgressBar.Visibility = Visibility.Collapsed;
                timer.Dispose();
            }
        }


        /*
        
        */
        

        private void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
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
            string Timesr = current_time.ToString("HH:mm:ss   ");//显示时间
            //StringBuilder datawate = new StringBuilder(1024);



            if (rx == 0)                                         // 如果以字符串形式读取
            {
                
                rxstr = CommonRes._serialPort.ReadExisting();                    // 读取串口接收缓冲区字符串
                if (shtime == 1)
                {
                    rxstr = string.Concat(Timesr, rxstr);
                }
                //datawate.Append(rxstr);

                
                DispatcherQueue.TryEnqueue(() =>
                {
                    datapwate.Append(rxstr);                          // 在接收文本框中进行显示
                });

                if (autotr == 1)
                {
                    //RXTextBox.ScrollToEnd();
                }
                
                
                

            }
            else                                                            // 以数值形式读取
            {
                int length = CommonRes._serialPort.BytesToRead;                       // 读取串口接收缓冲区字节数

                byte[] data = new byte[length];                             // 定义相同字节的数组

                CommonRes._serialPort.Read(data, 0, length);                          // 串口读取缓冲区数据到数组中

                for (int i = 0; i < length; i++)
                {
                    rxstr = Convert.ToString(data[i], 16).ToUpper();                                   // 将数据转换为字符串格式

                    DispatcherQueue.TryEnqueue(() =>
                    {
                        RXTextBox.Text = RXTextBox.Text + "0x" + (str.Length == 1 ? "0" + str + " " : str + " ");        // 添加到串口接收文本框中
                    });


                    
                }
                if (autotr == 1)
                {
                    //RXTextBox.ScrollToEnd();
                }

                DispatcherQueue.TryEnqueue(() =>
                {
                    RXTextBox.Text += "\r\n";
                });
            }

            /*
            ++rxs;                                          //接收自动清空
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


        private void COMComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void TXTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void TXButton_Click(object sender, RoutedEventArgs e)//发送数据
        {
            if (CommonRes._serialPort.IsOpen)            // 如果串口设备已经打开了
            {
                if (tx == 0)        // 如果是以字符的形式发送数据
                {
                    char[] str = new char[1];  // 定义一个字符数组，只有一位

                    try
                    {
                        if (shtime == 1)
                        {
                            //显示时间
                            current_time = System.DateTime.Now;     //获取当前时间
                            RXTextBox.Text = RXTextBox.Text + current_time.ToString("HH:mm:ss") + "  ";

                        }
                        for (int i = 0; i < TXTextBox.Text.Length; i++)
                        {
                            str[0] = Convert.ToChar(TXTextBox.Text.Substring(i, 1));  // 取待发送文本框中的第i个字符
                            CommonRes._serialPort.Write(str, 0, 1);                            // 写入串口设备进行发送
                        }
                        RXTextBox.Text = RXTextBox.Text + "TX: " + TXTextBox.Text + "\r\n";

                        if (autotr == 1)
                        {
                            //RXTextBox.ScrollToEnd();
                        }

                        else
                        {

                        }
                        txf = 1;
                    }
                    catch
                    {
                        //MessageBox.Show("串口字符写入错误!", "错误");   // 弹出发送错误对话框
                        RXTextBox.Text = RXTextBox.Text + "串口字符写入错误!" + "\r\n";
                        
                        CONTButton_Click(sender, e);
                    }
                }
                else                                                  // 如果以数值的形式发送
                {
                    byte[] Data = new byte[1];                        // 定义一个byte类型数据，相当于C语言的unsigned char类型
                    int flag = 0;                                     // 定义一个标志，标志这是第几位
                    try
                    {
                        if (shtime == 1)
                        {
                            //显示时间
                            current_time = System.DateTime.Now;     //获取当前时间
                            RXTextBox.Text = RXTextBox.Text + current_time.ToString("HH:mm:ss") + "  ";

                        }
                        for (int i = 0; i < TXTextBox.Text.Length; i++)
                        {
                            if (TXTextBox.Text.Substring(i, 1) == " " && flag == 0)                // 如果是第一位，并且为空字符
                            {
                                continue;
                            }

                            if (TXTextBox.Text.Substring(i, 1) != " " && flag == 0)                // 如果是第一位，但不为空字符
                            {
                                flag = 1;                                                         // 标志转到第二位数据去
                                if (i == TXTextBox.Text.Length - 1)                                // 如果这是文本框字符串的最后一个字符
                                {
                                    Data[0] = Convert.ToByte(TXTextBox.Text.Substring(i, 1), 16);  // 转化为byte类型数据，以16进制显示
                                    CommonRes._serialPort.Write(Data, 0, 1);                                // 通过串口发送
                                    RXTextBox.Text = RXTextBox.Text + Data + " ";
                                    flag = 0;                                                     // 标志回到第一位数据去
                                }
                                continue;
                            }
                            else if (TXTextBox.Text.Substring(i, 1) == " " && flag == 1)           // 如果是第二位，且第二位字符为空
                            {
                                Data[0] = Convert.ToByte(TXTextBox.Text.Substring(i - 1, 1), 16);  // 只将第一位字符转化为byte类型数据，以十六进制显示
                                CommonRes._serialPort.Write(Data, 0, 1);                                    // 通过串口发送
                                RXTextBox.Text = RXTextBox.Text + Data + " ";
                                flag = 0;                                                         // 标志回到第一位数据去
                                continue;
                            }
                            else if (TXTextBox.Text.Substring(i, 1) != " " && flag == 1)           // 如果是第二位字符，且第一位字符不为空
                            {
                                Data[0] = Convert.ToByte(TXTextBox.Text.Substring(i - 1, 2), 16);  // 将第一，二位字符转化为byte类型数据，以十六进制显示
                                CommonRes._serialPort.Write(Data, 0, 1);                                    // 通过串口发送
                                RXTextBox.Text = RXTextBox.Text + Data + " ";
                                flag = 0;                                                         // 标志回到第一位数据去
                                continue;
                            }

                        }
                        RXTextBox.Text += "\r\n";

                        if (autotr == 1)
                        {
                            //RXTextBox.ScrollToEnd();
                        }

                        else
                        {

                        }
                        txf = 1;
                    }
                    catch
                    {
                        //MessageBox.Show("串口数值写入错误!", "错误");
                        RXTextBox.Text = RXTextBox.Text + "串口字符写入错误!" + "\r\n";

                        CONTButton_Click(sender, e);
                    }
                }
                TXTextBox.Text = "";
            }
        }



        private void CLEARButton_Click(object sender, RoutedEventArgs e)
        {
            RXTextBox.Text = "";    //清除文本框内容
        }

        private void RXTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
        private void BANDComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        private void PARComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        private void STOPComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        private void DATAComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        

        private void RXHEXButton_Click(object sender, RoutedEventArgs e)    //接收以十六进制数显示
        {
            var foregroundColor = COMButton.Foreground as SolidColorBrush;
            var backgroundColor = COMButton.Background as SolidColorBrush;
            var darkaccentColor = (Windows.UI.Color)Application.Current.Resources["SystemAccentColorLight2"];
            var ligtaccentColor = (Windows.UI.Color)Application.Current.Resources["SystemAccentColorDark1"];
            var theme = Application.Current.RequestedTheme;
            if (rx == 0)
            {
                if (theme == ApplicationTheme.Dark)
                {
                    // 当前处于深色模式
                    RXHEXButton.Background = new SolidColorBrush(darkaccentColor);
                    RXHEXButton.Foreground = new SolidColorBrush(Colors.Black);
                }
                else if (theme == ApplicationTheme.Light)
                {
                    // 当前处于浅色模式
                    RXHEXButton.Background = new SolidColorBrush(ligtaccentColor);
                    RXHEXButton.Foreground = new SolidColorBrush(Colors.White);
                }
                rx = 1;
            }
            else
            {
                RXHEXButton.Background = backgroundColor;
                RXHEXButton.Foreground = foregroundColor;
                rx = 0;
            }
        }

        private void TXHEXButton_Click(object sender, RoutedEventArgs e)    //发送以十六进制数显示
        {
            var foregroundColor = COMButton.Foreground as SolidColorBrush;
            var backgroundColor = COMButton.Background as SolidColorBrush;
            var darkaccentColor = (Windows.UI.Color)Application.Current.Resources["SystemAccentColorLight2"];
            var ligtaccentColor = (Windows.UI.Color)Application.Current.Resources["SystemAccentColorDark1"];
            var theme = Application.Current.RequestedTheme;
            if (tx == 0)
            {
                if (theme == ApplicationTheme.Dark)
                {
                    // 当前处于深色模式
                    TXHEXButton.Background = new SolidColorBrush(darkaccentColor);
                    TXHEXButton.Foreground = new SolidColorBrush(Colors.Black);
                }
                else if (theme == ApplicationTheme.Light)
                {
                    // 当前处于浅色模式
                    TXHEXButton.Background = new SolidColorBrush(ligtaccentColor);
                    TXHEXButton.Foreground = new SolidColorBrush(Colors.White);
                }
                tx = 1;
            }
            else
            {
                TXHEXButton.Background = backgroundColor;
                TXHEXButton.Foreground = foregroundColor;
                tx = 0;
            }
        }


        private void RSTButton_Click(object sender, RoutedEventArgs e)      //自动重启
        {
            CommonRes._serialPort.RtsEnable = true;
            Thread.Sleep(10);
            CommonRes._serialPort.DtrEnable = true;
            Thread.Sleep(10);
            CommonRes._serialPort.DtrEnable = false;
            Thread.Sleep(10);
            CommonRes._serialPort.RtsEnable = false;
            if(dtr == 1)
            {
                DTRButton_Click(sender, e);
                Thread.Sleep(50);
                DTRButton_Click(sender, e);
            }
            else
            {
                Thread.Sleep(50);
                DTRButton_Click(sender, e);
            }
            //CommonRes._serialPort.DtrEnable = true;
        }

        private void DTRButton_Click(object sender, RoutedEventArgs e)      //DTR信号使能
        {
            var foregroundColor = COMButton.Foreground as SolidColorBrush;
            var backgroundColor = COMButton.Background as SolidColorBrush;
            var darkaccentColor = (Windows.UI.Color)Application.Current.Resources["SystemAccentColorLight2"];
            var ligtaccentColor = (Windows.UI.Color)Application.Current.Resources["SystemAccentColorDark1"];
            var theme = Application.Current.RequestedTheme;
            if (dtr == 0)
            {
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
                dtr = 1;
            }
            else
            {
                DTRButton.Background = backgroundColor;
                DTRButton.Foreground = foregroundColor;

                CommonRes._serialPort.DtrEnable = false;
                dtr = 0;
            }
        }

        private void RTSButton_Click(object sender, RoutedEventArgs e)      //RTS信号使能
        {
            var foregroundColor = COMButton.Foreground as SolidColorBrush;
            var backgroundColor = COMButton.Background as SolidColorBrush;
            var darkaccentColor = (Windows.UI.Color)Application.Current.Resources["SystemAccentColorLight2"];
            var ligtaccentColor = (Windows.UI.Color)Application.Current.Resources["SystemAccentColorDark1"];
            var theme = Application.Current.RequestedTheme;
            if (rts == 0)
            {
                if (theme == ApplicationTheme.Dark)
                {
                    // 当前处于深色模式
                    RTSButton.Background = new SolidColorBrush(darkaccentColor);
                    RTSButton.Foreground = new SolidColorBrush(Colors.Black);
                }
                else if (theme == ApplicationTheme.Light)
                {
                    // 当前处于浅色模式
                    RTSButton.Background = new SolidColorBrush(ligtaccentColor);
                    RTSButton.Foreground = new SolidColorBrush(Colors.White);
                }
                CommonRes._serialPort.RtsEnable = true;
                rts = 1;
            }
            else
            {
                RTSButton.Background = backgroundColor;
                RTSButton.Foreground = foregroundColor;

                CommonRes._serialPort.RtsEnable = false;
                rts = 0;
            }
        }

        private void ShowTimeButton_Click(object sender, RoutedEventArgs e)
        {
            var foregroundColor = COMButton.Foreground as SolidColorBrush;
            var backgroundColor = COMButton.Background as SolidColorBrush;
            var darkaccentColor = (Windows.UI.Color)Application.Current.Resources["SystemAccentColorLight2"];
            var ligtaccentColor = (Windows.UI.Color)Application.Current.Resources["SystemAccentColorDark1"];
            var theme = Application.Current.RequestedTheme;
            if (shtime == 0)
            {
                if (theme == ApplicationTheme.Dark)
                {
                    // 当前处于深色模式
                    ShowTimeButton.Background = new SolidColorBrush(darkaccentColor);
                    ShowTimeButton.Foreground = new SolidColorBrush(Colors.Black);
                }
                else if (theme == ApplicationTheme.Light)
                {
                    // 当前处于浅色模式
                    ShowTimeButton.Background = new SolidColorBrush(ligtaccentColor);
                    ShowTimeButton.Foreground = new SolidColorBrush(Colors.White);
                }
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
                ShowTimeButton.Background = backgroundColor;
                ShowTimeButton.Foreground = foregroundColor;

                shtime = 0;
            }
        }

        private void AUTOScrollButton_Click(object sender, RoutedEventArgs e)
        {
            var foregroundColor = COMButton.Foreground as SolidColorBrush;
            var backgroundColor = COMButton.Background as SolidColorBrush;
            var darkaccentColor = (Windows.UI.Color)Application.Current.Resources["SystemAccentColorLight2"];
            var ligtaccentColor = (Windows.UI.Color)Application.Current.Resources["SystemAccentColorDark1"];
            var theme = Application.Current.RequestedTheme;
            if (autotr == 0)
            {
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
                autotr = 1;
            }
            else
            {
                AUTOScrollButton.Background = backgroundColor;
                AUTOScrollButton.Foreground = foregroundColor;

                autotr = 0;
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
    }
}
