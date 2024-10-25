using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using static FSGaryityTool_Win11.Core.SerialPorts.SerialPortCoreServices.SerialPortConfigManager;

namespace FSGaryityTool_Win11.Core.SerialPorts;

internal class SerialPortCoreServices
{
    /// <summary>
    /// Manages serial port configurations.
    /// </summary>
    public static class SerialPortConfigManager
    {
        public static Dictionary<string, SerialPortConfig> SerialPortConfigs { get; } = new();

        /// <summary>
        /// Represents the configuration for a serial port.
        /// </summary>
        public class SerialPortConfig : INotifyPropertyChanged
        {
            private int baudRate;                       //波特率
            private Parity parity;                      //校验位
            private StopBits stopBits;                  //停止位
            private int dataBits;                       //数据位
            private int timeout;                        //超时时间
            private Encoding encoding;                  //编码

            private string serialDeviceIcon;                //设备图标
            private string serialDeviceName;                //设备名
            private string serialDeviceDescription;         //设备介绍
            private string serialDeviceManufacturer;        //设备制造商
            private int serialDeviceResetDefBaudRate;       //设备硬重启时时上电打印的默认波特率

            public event PropertyChangedEventHandler PropertyChanged;

            protected void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            public int BaudRate
            {
                get => baudRate;
                set
                {
                    if (baudRate != value)
                    {
                        baudRate = value;
                        OnPropertyChanged(nameof(BaudRate));
                    }
                }
            }

            public Parity Parity
            {
                get => parity;
                set
                {
                    if (parity != value)
                    {
                        parity = value;
                        OnPropertyChanged(nameof(Parity));
                    }
                }
            }

            public StopBits StopBits
            {
                get => stopBits;
                set
                {
                    if (stopBits != value)
                    {
                        stopBits = value;
                        OnPropertyChanged(nameof(StopBits));
                    }
                }
            }

            public int DataBits
            {
                get => dataBits;
                set
                {
                    if (dataBits != value)
                    {
                        dataBits = value;
                        OnPropertyChanged(nameof(DataBits));
                    }
                }
            }

            public int Timeout
            {
                get => timeout;
                set
                {
                    if (timeout != value)
                    {
                        timeout = value;
                        OnPropertyChanged(nameof(Timeout));
                    }
                }
            }

            public Encoding Encoding
            {
                get => encoding;
                set
                {
                    if (encoding != value)
                    {
                        encoding = value;
                        OnPropertyChanged(nameof(Encoding));
                    }
                }
            }

            public string SerialDeviceIcon
            {
                get => serialDeviceIcon;
                set
                {
                    if (serialDeviceIcon != value)
                    {
                        serialDeviceIcon = value;
                        OnPropertyChanged(nameof(SerialDeviceIcon));
                    }
                }
            }

            public string SerialDeviceName
            {
                get => serialDeviceName;
                set
                {
                    if (serialDeviceName != value)
                    {
                        serialDeviceName = value;
                        OnPropertyChanged(nameof(SerialDeviceName));
                    }
                }
            }

            public string SerialDeviceDescription
            {
                get => serialDeviceDescription;
                set
                {
                    if (serialDeviceDescription != value)
                    {
                        serialDeviceDescription = value;
                        OnPropertyChanged(nameof(SerialDeviceDescription));
                    }
                }
            }

            public string SerialDeviceManufacturer
            {
                get => serialDeviceManufacturer;
                set
                {
                    if (serialDeviceManufacturer != value)
                    {
                        serialDeviceManufacturer = value;
                        OnPropertyChanged(nameof(SerialDeviceManufacturer));
                    }
                }
            }

            public int SerialDeviceResetDefBaudRate
            {
                get => serialDeviceResetDefBaudRate;
                set
                {
                    if (serialDeviceResetDefBaudRate != value)
                    {
                        serialDeviceResetDefBaudRate = value;
                        OnPropertyChanged(nameof(SerialDeviceResetDefBaudRate));
                    }
                }
            }
        }

        /// <summary>
        /// Gets the serial port configuration for the specified port name.
        /// </summary>
        /// <param name="portName">The name of the port.</param>
        /// <returns>The serial port configuration if found; otherwise, null.</returns>
        public static SerialPortConfig GetSerialPortConfig(string portName)
        {
            return SerialPortConfigs.GetValueOrDefault(portName);
        }

        /// <summary>
        /// Gets the names of all serial ports in the dictionary.
        /// </summary>
        /// <returns>A list of all serial port names.</returns>
        public static List<string> GetAllSerialPortNames()
        {
            return SerialPortConfigs.Keys.ToList();
        }

        /// <summary>
        /// Updates the serial port configuration for the specified port name.
        /// </summary>
        /// <param name="portName">The name of the port.</param>
        /// <param name="baudRate">The baud rate.</param>
        /// <param name="parity">The parity.</param>
        /// <param name="stopBits">The stop bits.</param>
        /// <param name="dataBits">The data bits.</param>
        /// <param name="timeout">The timeout.</param>
        /// <param name="encoding">The encoding.</param>
        public static void UpdateSerialPortConfig(string portName, int? baudRate = null, string parity = null, string stopBits = null, int? dataBits = null, int? timeout = null, string encoding = null)
        {
            if (SerialPortConfigs.TryGetValue(portName, out var config))
            {
                if (baudRate.HasValue)
                {
                    config.BaudRate = baudRate.Value;
                }
                else if (config.BaudRate is 0)
                {
                    config.BaudRate = 115200;
                }

                if (parity is not null)
                {
                    config.Parity = (Parity)Enum.Parse(typeof(Parity), parity, true);
                }
                else if (config.Parity is 0)
                {
                    config.Parity = Parity.None;
                }

                if (stopBits is not null)
                {
                    config.StopBits = (StopBits)Enum.Parse(typeof(StopBits), stopBits, true);
                }
                else if (config.StopBits is 0)
                {
                    config.StopBits = StopBits.One;
                }

                if (dataBits.HasValue)
                {
                    config.DataBits = dataBits.Value;
                }
                else if (config.DataBits is 0)
                {
                    config.DataBits = 8;
                }

                if (timeout.HasValue)
                {
                    config.Timeout = timeout.Value;
                }
                else if (config.Timeout is 0)
                {
                    config.Timeout = 1500;
                }

                if (encoding is not null)
                {
                    config.Encoding = Encoding.GetEncoding(encoding);
                }
                else if (config.Encoding is null)
                {
                    config.Encoding = Encoding.UTF8;
                }
            }
        }

        /// <summary>
        /// Updates the serial device configuration for the specified port name.
        /// </summary>
        /// <param name="portName">The name of the port.</param>
        /// <param name="icon">The serial device icon.</param>
        /// <param name="name">The serial device name.</param>
        /// <param name="description">The serial device description.</param>
        /// <param name="manufacturer">The serial device manufacturer.</param>
        /// <param name="resetBaudRate">The default baud rate for resetting the serial device.</param>
        public static void UpdateSerialDeviceConfig(string portName, string icon = null, string name = null, string description = null, string manufacturer = null, int? resetBaudRate = null)
        {
            if (SerialPortConfigs.TryGetValue(portName, out var config))
            {
                config.SerialDeviceIcon = icon ?? "\uE964";

                config.SerialDeviceName = name ?? portName;

                config.SerialDeviceDescription = description ?? "Serial Device";

                config.SerialDeviceManufacturer = manufacturer ?? "Microsoft";

                config.SerialDeviceResetDefBaudRate = resetBaudRate ?? 115200;
            }
        }
    }

    /// <summary>
    /// Manages serial port configurations and operations.
    /// </summary>
    public static class SerialPortManager
    {
        public static Dictionary<string, SerialPort> serialPorts { get; } = new();

        /// <summary>
        /// Sets or creates a serial port with the specified configuration.
        /// </summary>
        /// <param name="portName">The name of the port.</param>
        /// <param name="baudRate">The baud rate.</param>
        /// <param name="parity">The parity.</param>
        /// <param name="stopBits">The stop bits.</param>
        /// <param name="dataBits">The data bits.</param>
        /// <param name="timeout">The read timeout.</param>
        /// <param name="encoding">The encoding.</param>
        /// <returns>The configured serial port.</returns>
        public static void AddSerialPort(string portName, int baudRate, string parity, string stopBits, int dataBits, int timeout, string encoding)
        {
            if (!serialPorts.ContainsKey(portName))
            {
                var serialPort = new SerialPort
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
        }

        /// <summary>
        /// Adds a serial port from the configuration.
        /// </summary>
        /// <param name="portName">The name of the port.</param>
        public static void AddSerialPortFormConfig(string portName)
        {
            if (!serialPorts.ContainsKey(portName) && SerialPortConfigs.TryGetValue(portName, out var config))
            {
                var serialPort = new SerialPort
                {
                    PortName = portName,
                    BaudRate = config.BaudRate,
                    Parity = config.Parity,
                    StopBits = config.StopBits,
                    DataBits = config.DataBits,
                    ReadTimeout = config.Timeout,
                    Encoding = config.Encoding
                };

                serialPorts[portName] = serialPort;
            }
        }

        /// <summary>
        /// Updates a serial port from the configuration.
        /// </summary>
        /// <param name="portName">The name of the port.</param>
        public static void UpdateSerialPortFormConfig(string portName)
        {
            if (serialPorts.TryGetValue(portName, out var value) && SerialPortConfigs.TryGetValue(portName, out var config))
            {
                value.BaudRate = config.BaudRate;
                value.Parity = config.Parity;
                value.StopBits = config.StopBits;
                value.DataBits = config.DataBits;
                value.ReadTimeout = config.Timeout;
                value.Encoding = config.Encoding;
            }
        }

        /// <summary>
        /// Gets the serial port object for the specified port name.
        /// </summary>
        /// <param name="portName">The name of the port.</param>
        /// <returns>The SerialPort object if found; otherwise, null.</returns>
        public static SerialPort GetSerialPort(string portName)
            => serialPorts.GetValueOrDefault(portName);

        /// <summary>
        /// Gets all closed serial ports.
        /// </summary>
        /// <returns>A string containing the names of all closed serial ports.</returns>
        public static List<string> GetAllClosedPorts() => 
            serialPorts.Where(port => !port.Value.IsOpen).Select(port => port.Key).ToList();

        /// <summary>
        /// Opens the specified serial port.
        /// </summary>
        /// <param name="portName">The name of the port.</param>
        public static void OpenSerialPort(string portName)
        {
            if (serialPorts.TryGetValue(portName, out var value))
            {
                value.Open();
            }
        }

        /// <summary>
        /// Closes the specified serial port.
        /// </summary>
        /// <param name="portName">The name of the port.</param>
        public static void CloseSerialPort(string portName)
        {
            if (serialPorts.TryGetValue(portName, out var value))
            {
                value.Close();
            }
        }

        /// <summary>
        /// Closes all serial ports.
        /// </summary>
        public static void CloseAllSerialPorts()
        {
            foreach (var port in serialPorts.Values)
            {
                port.Close();
            }
        }

        /// <summary>
        /// Clears the specified serial port from the dictionary and releases its resources.
        /// </summary>
        /// <param name="portName">The name of the port.</param>
        public static void ClearSerialPort(string portName)
        {
            if (serialPorts.TryGetValue(portName, out var value))
            {
                if (value.IsOpen)
                {
                    value.Close();
                }
                value.Dispose();
                serialPorts.Remove(portName);
            }
        }

        /// <summary>
        /// Clears all serial ports from the dictionary and releases their resources.
        /// </summary>
        public static void ClearAllSerialPorts()
        {
            foreach (var port in serialPorts.Values)
            {
                if (port.IsOpen)
                {
                    port.Close();
                }
                port.Dispose();
            }
            serialPorts.Clear();
        }
    }
}
