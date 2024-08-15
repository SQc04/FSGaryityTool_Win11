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
using System.Text;
using static FSGaryityTool_Win11.Views.Pages.SerialPortPage.SerialPortCoreServicesPage.SerialPortConfigManager;
using Windows.Networking;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FSGaryityTool_Win11.Views.Pages.SerialPortPage
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SerialPortCoreServicesPage : Page
    {
        public SerialPortCoreServicesPage()
        {
            this.InitializeComponent();
        }

        public static class SerialPortConfigManager
        {
            public static Dictionary<string, SerialPortConfig> serialPortConfigs = new Dictionary<string, SerialPortConfig>();
            public class SerialPortConfig
            {
                //SerialPortConfig
                public int BaudRate { get; set; }
                public Parity Parity { get; set; }
                public StopBits StopBits { get; set; }
                public int DataBits { get; set; }
                public int Timeout { get; set; }
                public Encoding Encoding { get; set; }

                //SerialDeviceConfig
                public string SerialDeviceIcon { get; set; }
                public string SerialDeviceName { get; set; }
                public string SerialDeviceDescription { get; set; }
                public string SerialDeviceanufacturer { get; set; }
                public int SerialDeviceResetDefBaudRate {  get; set; }

            }
            public static SerialPortConfig GetSerialPortConfig(string portName)
            {
                if (serialPortConfigs.ContainsKey(portName))
                {
                    return serialPortConfigs[portName];
                }
                return null;
            }
            public static void UpdateSerialPortConfig(string portName, int? baudRate = null, string parity = null, string stopBits = null, int? dataBits = null, int? timeout = null, string encoding = null)
            {
                if (serialPortConfigs.ContainsKey(portName))
                {
                    SerialPortConfig config = serialPortConfigs[portName];

                    if (baudRate.HasValue)
                    {
                        config.BaudRate = baudRate.Value;
                    }
                    else config.BaudRate = 115200;

                    if (parity != null)
                    {
                        config.Parity = (Parity)Enum.Parse(typeof(Parity), parity, true);
                    }
                    else config.Parity = Parity.None;

                    if (stopBits != null)
                    {
                        config.StopBits = (StopBits)Enum.Parse(typeof(StopBits), stopBits, true);
                    }
                    else config.StopBits = StopBits.One;

                    if (dataBits.HasValue)
                    {
                        config.DataBits = dataBits.Value;
                    }
                    else config.DataBits = 8;

                    if (timeout.HasValue)
                    {
                        config.Timeout = timeout.Value;
                    }
                    else config.Timeout = 1500;

                    if (encoding != null)
                    {
                        config.Encoding = Encoding.GetEncoding(encoding);
                    }
                    else config.Encoding = Encoding.UTF8;

                }
            }

        }

        public static class SerialPortManager
        {
            public static Dictionary<string, SerialPort> serialPorts = new Dictionary<string, SerialPort>();

            public static SerialPort GetSerialPort(string portName, int baudRate, string parity, string stopBits, int dataBits, int timeout, string encoding)
            {
                if (!serialPorts.ContainsKey(portName))
                {
                    SerialPort serialPort = new SerialPort
                    {
                        PortName = portName,
                        BaudRate = baudRate,
                        Parity = (Parity)Enum.Parse(typeof(Parity), parity, true),
                        StopBits = (StopBits)Enum.Parse(typeof(StopBits), stopBits, true),
                        DataBits = dataBits,
                        ReadTimeout = timeout,
                        Encoding = Encoding.GetEncoding(encoding)
                    };

                    serialPorts[portName] = serialPort;
                }

                return serialPorts[portName];
            }

            public static void AddSerialPortFormConfig(string portName)
            {
                if (!serialPorts.ContainsKey(portName) && serialPortConfigs.ContainsKey(portName))
                {
                    SerialPortConfig config = serialPortConfigs[portName];
                    SerialPort serialPort = new SerialPort
                    {
                        PortName = portName,
                        BaudRate = config.BaudRate,
                        Parity = config.Parity,
                        StopBits = config.StopBits,
                        DataBits = config.DataBits,
                        ReadTimeout = config.Timeout,
                        Encoding = config.Encoding
                    };
                }
            }
            public static void OpenSerialPort(string portName)
            {
                if (serialPorts.ContainsKey(portName))
                {
                    serialPorts[portName].Open();
                }
            }
            public static void CloseSerialPort(string portName)
            {
                if (serialPorts.ContainsKey(portName))
                {
                    serialPorts[portName].Close();
                }
            }

            public static void CloseAllSerialPorts()
            {
                foreach (var port in serialPorts.Values)
                {
                    port.Close();
                }
            }
            public static void ClearSerialPort(string portName)
            {
                if (serialPorts.ContainsKey(portName))
                {
                    serialPorts.Remove(portName);
                }
            }
            public static void ClearAllSerialPort(string portName)
            {
                serialPorts.Clear();
            }
        }
        
    }
}
