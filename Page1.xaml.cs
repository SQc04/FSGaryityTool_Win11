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
using WinRT.Interop;
using Windows.UI;          // Needed for XAML/HWND interop.
using Windows.ApplicationModel.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Core;

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
        public static int tx = 0;
        public static int rx = 0;
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
            this.InitializeComponent();

            this.Loaded += Page1_Loaded;

            DTRCheckBox.IsChecked = true;
            //RTSCheckBox.IsChecked = true;

            // 在你的代码后台，定义一个List<string>作为数据源
            List<string> BaudRates = new List<string>()
            {
                "75", "110", "134", "150", "300", "600", "1200", "1800", "2400", "4800", "7200", "9600", "14400", "19200", "38400", "57600", "74880","115200", "128000", "230400", "250000", "500000", "1000000", "2000000"
            };
            // 将ComboBox的ItemsSource属性绑定到这个数据源
            BANDComboBox.ItemsSource = BaudRates;
            // 设置默认选项
            BANDComboBox.SelectedItem = "115200"; // 将"9600"设置为默认选项

            List<string> ParRates = new List<string>()
            {
                "None", "Odd", "Even", "Mark", "Space"
            };
            PARComboBox.ItemsSource = ParRates;
            PARComboBox.SelectedItem = "None";

            List<string> StopRates = new List<string>()
            {
                "None", "One", "OnePointFive", "Two"
            };
            STOPComboBox.ItemsSource = StopRates;
            STOPComboBox.SelectedItem = "One";
            
            for (int j = 5; j < 10; ++j)
            {
                DATAComboBox.Items.Add(j);
            }
            DATAComboBox.SelectedItem = 8;

        }

        private void Page1_Loaded(object sender, RoutedEventArgs e)
        {
            if (!_isLoaded)
            {
                COMButton_Click(this, new RoutedEventArgs());
                _isLoaded = true;
            }
                
        }

        private void COMButton_Click(object sender, RoutedEventArgs e)
        {
            //COMButton.Content = "Clicked";
            SearchAndAddSerialToComboBox(CommonRes._serialPort, COMComboBox);           //扫描并将串口添加至下拉列表

            void SearchAndAddSerialToComboBox(SerialPort MyPort, ComboBox MyBox)
            {
                RXTextBox.Text = RXTextBox.Text + "Start search SerialPort\r\n";
                string[] ArryPort;                                          // 定义字符串数组，数组名为 Buffer
                ArryPort = SerialPort.GetPortNames();                       // SerialPort.GetPortNames()函数功能为获取计算机所有可用串口，以字符串数组形式输出
                string scom = String.Join("\r\n", ArryPort);
                RXTextBox.Text = RXTextBox.Text + scom + "\r\n";
                MyBox.Items.Clear();                                        // 清除当前组合框下拉菜单内容                  
                for (int i = 0; i < ArryPort.Length; i++)
                {
                    MyBox.Items.Add(ArryPort[i]);                           // 将所有的可用串口号添加到端口对应的组合框中
                }
                RXTextBox.Text = RXTextBox.Text + "Search SerialPort succeed!\r\n";

            }
        }

        private void CONTButton_Click(object sender, RoutedEventArgs e)
        {
            if (Con == 0)
            {
                try
                {
                    CommonRes._serialPort.PortName = (string)COMComboBox.SelectedItem;                      //开启的串口名称为选择串口的ComboBox组件中的内容
                    CommonRes._serialPort.BaudRate = Convert.ToInt32(BANDComboBox.SelectedItem);     //将选择波特率ComboBox组件中的数据转为Int型，并且进行波特率的设置

                    RXTextBox.Text = RXTextBox.Text + "BaudRate = " + Convert.ToInt32(BANDComboBox.SelectedItem) + "\r\n";

                    CommonRes._serialPort.Parity = (Parity)Enum.Parse(typeof(Parity), (string)PARComboBox.SelectedItem);                       //校验位
                    CommonRes._serialPort.StopBits = (StopBits)Enum.Parse(typeof(StopBits), (string)STOPComboBox.SelectedItem);                 //停止位

                    RXTextBox.Text = RXTextBox.Text + "Parity = " + (Parity)Enum.Parse(typeof(Parity), (string)PARComboBox.SelectedItem) + "\r\n";
                    RXTextBox.Text = RXTextBox.Text + "StopBits = " + (StopBits)Enum.Parse(typeof(StopBits), (string)STOPComboBox.SelectedItem) + "\r\n";

                    CommonRes._serialPort.DataBits = Convert.ToInt32(DATAComboBox.SelectedItem);     //数据位8

                    RXTextBox.Text = RXTextBox.Text + "DataBits = " + Convert.ToInt32(DATAComboBox.SelectedItem) + "\r\n";

                    CommonRes._serialPort.ReadTimeout = 1500;
                    //_SerialPort.DtrEnable = true;                               //启用数据终端就绪信息
                    CommonRes._serialPort.Encoding = Encoding.UTF8;
                    CommonRes._serialPort.ReceivedBytesThreshold = 1;                     //DataReceived触发前内部输入缓冲器的字节数

                    RXTextBox.Text = RXTextBox.Text + "SerialPort " + COMComboBox.SelectedItem + " IS OPEN" + "\r\n";

                    CommonRes._serialPort.Open();                                         //打开串口
                    CONTButton.Content = "DISCONNECT";
                    Con = 1;
                }
                catch                                                           //如果打开串口失败 需要做如下警示
                {
                    RXTextBox.Text = RXTextBox.Text + "打开串口失败，请检查相关设置" + "\r\n";
                    //MessageBox.Show("打开串口失败，请检查相关设置", "错误");
                    Con = 0;
                }

            }
            else
            {
                //RXTextBox.Text = RXTextBox.Text + "SerialPort IS DISCONNECT\r\n";

                try
                {
                    CommonRes._serialPort.Close();                                        //关闭串口
                    RXTextBox.Text = RXTextBox.Text + "SerialPort IS CLOSE" + "\r\n";
                }
                catch (Exception err)//一般情况下关闭串口不会出错，所以不需要加处理程序
                {
                    RXTextBox.Text = RXTextBox.Text + err + "\r\n";
                }
                CONTButton.Content = "CONNECT";
                Con = 0;
            }
        }


        /*
        private async Task InitializeSerialPort()
        {
            var devices = await DeviceInformation.FindAllAsync(SerialDevice.GetDeviceSelector());

            // 如果没有找到任何设备，提示用户
            if (devices.Count == 0)
            {
                RXTextBox.Text = "没有找到任何串口设备，请检查连接";
                return;
            }

            var device = devices[0];

            // 从设备信息中创建一个 SerialDevice 对象
            var serialDevice = await SerialDevice.FromIdAsync(device.Id);

            // 设置串口的参数，例如波特率，数据位，停止位等
            serialDevice.BaudRate = 9600;
            serialDevice.DataBits = 8;
            serialDevice.StopBits = SerialStopBitCount.One;
            serialDevice.Parity = SerialParity.None;

            // 将 SerialDevice 对象赋值给 SerialPort 对象的 Device 属性
            _SerialPort.Device = serialDevice;

            // 创建一个 DataReader 对象，用于从串口读取数据
            _SerialPort.DataReader = new DataReader(_SerialPort.Device.InputStream);

            // 创建一个 DataWriter 对象，用于向串口写入数据
            _SerialPort.DataWriter = new DataWriter(_SerialPort.Device.OutputStream);

            // 注册 DataReceived 事件的处理程序
            _SerialPort.Device.DataReceived += SerialPort_DataReceived;
        }


        private async void SerialPort_DataReceived(SerialDevice sender, object args)
        {
            // 获取当前时间
            var current_time = System.DateTime.Now;

            // 读取串口接收缓冲区的字节数
            var length = await _SerialPort.DataReader.LoadAsync(1024);

            // 判断是否以字符串形式读取
            if (rx == 0)
            {
                // 读取串口接收缓冲区字符串
                var str = _SerialPort.DataReader.ReadString(length);

                // 使用 DispatcherQueue.TryEnqueue 方法来更新 UI
                DispatcherQueue.TryEnqueue(() =>
                {
                    // 判断是否显示时间
                    if (ShowTimeCheckBox.IsChecked == true)
                    {
                        // 在接收文本框中显示时间
                        RXTextBox.Text += current_time.ToString("HH:mm:ss") + "  ";
                    }

                    // 在接收文本框中显示字符串
                    RXTextBox.Text += str + "\r\n";
                });
            }
            else // 以数值形式读取
            {
                // 定义一个字节数组，用于存储接收到的数据
                byte[] data = new byte[length];

                // 从 DataReader 中读取数据到字节数组中
                _SerialPort.DataReader.ReadBytes(data);

                // 使用 DispatcherQueue.TryEnqueue 方法来更新 UI
                DispatcherQueue.TryEnqueue(() =>
                {
                    // 遍历字节数组
                    for (int i = 0; i < length; i++)
                    {
                        // 将数据转换为字符串格式
                        string str = Convert.ToString(data[i], 16).ToUpper();

                        // 在接收文本框中显示数据
                        RXTextBox.Text += "0x" + (str.Length == 1 ? "0" + str + " " : str + " ");
                    }

                    // 在接收文本框中换行
                    RXTextBox.Text += "\r\n";
                });
            }
        }
        */
        private void COMComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void TXTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void TXButton_Click(object sender, RoutedEventArgs e)
        {

        }
        private void CLEARButton_Click(object sender, RoutedEventArgs e)
        {
            RXTextBox.Text = "";
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
        private void RXHEXCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (RXHEXCheckBox.IsChecked == true)
            {
                rx = 1;
            }
            else
            {
                rx = 0;
            }
        }
        private void TXHEXCheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }
        private void DTRCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (DTRCheckBox.IsChecked == true)
            {
                CommonRes._serialPort.DtrEnable = true;
            }
            else
            {
                CommonRes._serialPort.DtrEnable = false;
            }
        }
        private void RTSCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (RTSCheckBox.IsChecked == true)
            {
                CommonRes._serialPort.RtsEnable = true;
            }
            else
            {
                CommonRes._serialPort.RtsEnable = false;
            }
        }
        private void AUTOScrollCheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }
        private void ShowTimeCheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }
        private void RXTCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (txf == 1)
            {
                RXTextBox.Text += "\r\n";
                txf = 0;
            }

            if (rx == 0)                                         // 如果以字符串形式读取
            {
                string str = CommonRes._serialPort.ReadExisting();                    // 读取串口接收缓冲区字符串
                if (ShowTimeCheckBox.IsChecked == true)
                {
                    //显示时间
                    current_time = System.DateTime.Now;     //获取当前时间
                    RXTextBox.Text = RXTextBox.Text + current_time.ToString("HH:mm:ss") + "  ";

                }
                RXTextBox.Text = RXTextBox.Text + str + "";                          // 在接收文本框中进行显示
                /*
                if (AUTOScrollCheckBox.IsChecked == true)
                {
                    //RXTextBox.ScrollToCaret();
                }
                else
                {

                }
                */

            }
            else                                                            // 以数值形式读取
            {
                int length = CommonRes._serialPort.ReadByte();                       // 读取串口接收缓冲区字节数

                byte[] data = new byte[length];                             // 定义相同字节的数组

                CommonRes._serialPort.Read(data, 0, length);                          // 串口读取缓冲区数据到数组中

                for (int i = 0; i < length; i++)
                {
                    string str = Convert.ToString(data[i], 16).ToUpper();                                   // 将数据转换为字符串格式
                    RXTextBox.Text = RXTextBox.Text + "0x" + (str.Length == 1 ? "0" + str + " " : str + " ");        // 添加到串口接收文本框中
                    /*
                    if (AUTOScrollCheckBox.IsChecked == true)
                    {
                        //RXTextBox.ScrollToCaret();
                    }
                    else
                    {

                    }
                    */
                }
            }
        }
    }
}
