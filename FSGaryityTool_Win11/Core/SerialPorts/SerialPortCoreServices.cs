using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using System.Text.RegularExpressions;
using System.Threading;
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
            private SerialPort _serialPort;

            private string _portName;
            private int baudRate;                       // 波特率
            private Parity parity;                      // 校验位
            private StopBits stopBits;                  // 停止位
            private int dataBits;                       // 数据位
            private int timeout;                        // 超时时间
            private Encoding encoding;                  // 编码
            private Handshake handshake;                // 握手协议

            private bool isOpen;                        // 是否打开
            private bool _DCD;                           // 数据载波检测
            private bool _RI;                            // 振铃指示
            private bool _CTS;                           // 清除发送
            private bool _DSR;                           // 数据发送准备
            private bool _DTR;                           // 数据终端准备
            private bool _RTS;                           // 请求发送


            private string serialDeviceName;            // 设备名
            private string serialDeviceDescription;     // 设备描述
            private string serialDeviceManufacturer;    // 设备制造商
            private int serialDeviceResetDefBaudRate;   // 设备硬重置时的默认波特率

            private string serialDeviceVid;             // USB VID
            private string serialDevicePid;             // USB PID
            private string serialDeviceType;            // 设备类型（如 USB、Bluetooth、Other）

            private bool _deviceInfoLoaded;
            private bool _isFetchingDeviceInfo;

            public event PropertyChangedEventHandler PropertyChanged;

            protected void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            public SerialPort SerialPort
            {
                get => _serialPort;
                set
                {
                    if (_serialPort != value)
                    {
                        _serialPort = value;
                        OnPropertyChanged(nameof(SerialPort));
                    }
                }
            }

            public string PortName
            {
                get => _portName;
                set
                {
                    if (_portName != value)
                    {
                        _portName = value;
                        OnPropertyChanged(nameof(PortName));
                    }
                }
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
            public Handshake Handshake
            {
                get => handshake;
                set
                {
                    if (handshake != value)
                    {
                        handshake = value;
                        OnPropertyChanged(nameof(Handshake));
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
            public string SerialDeviceVid
            {
                get => serialDeviceVid;
                set
                {
                    if (serialDeviceVid != value)
                    {
                        serialDeviceVid = value;
                        OnPropertyChanged(nameof(SerialDeviceVid));
                    }
                }
            }

            public string SerialDevicePid
            {
                get => serialDevicePid;
                set
                {
                    if (serialDevicePid != value)
                    {
                        serialDevicePid = value;
                        OnPropertyChanged(nameof(SerialDevicePid));
                    }
                }
            }

            public string SerialDeviceType
            {
                get => serialDeviceType;
                set
                {
                    if (serialDeviceType != value)
                    {
                        serialDeviceType = value;
                        OnPropertyChanged(nameof(SerialDeviceType));
                    }
                }
            }
            /// <summary>
            /// True when device info (description/manufacturer/vid/pid) has been loaded.
            /// </summary>
            public bool DeviceInfoLoaded
            {
                get => _deviceInfoLoaded;
                private set
                {
                    if (_deviceInfoLoaded != value)
                    {
                        _deviceInfoLoaded = value;
                        OnPropertyChanged(nameof(DeviceInfoLoaded));
                    }
                }
            }

            /// <summary>
            /// True while background fetch of device info is in progress.
            /// </summary>
            public bool IsFetchingDeviceInfo
            {
                get => _isFetchingDeviceInfo;
                private set
                {
                    if (_isFetchingDeviceInfo != value)
                    {
                        _isFetchingDeviceInfo = value;
                        OnPropertyChanged(nameof(IsFetchingDeviceInfo));
                    }
                }
            }

            /// <summary>
            /// Manually fetches device information (description/manufacturer/VID/PID).
            /// This operation may be slow and should be triggered explicitly by callers.
            /// </summary>
            /// <param name="cancellationToken">Optional cancellation token.</param>
            public async Task RefreshDeviceInfoAsync(CancellationToken cancellationToken = default)
            {
                if (string.IsNullOrEmpty(PortName)) return;
                if (IsFetchingDeviceInfo) return;

                IsFetchingDeviceInfo = true;
                try
                {
                    await Task.Run(() =>
                    {
                        try
                        {
                            // Query PnP entities and match the one that contains the COM port name
                            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity");
                            foreach (ManagementObject mo in searcher.Get())
                            {
                                cancellationToken.ThrowIfCancellationRequested();

                                var nameObj = mo["Name"];
                                if (nameObj == null) continue;
                                var name = nameObj.ToString();
                                if (!name.Contains(PortName, StringComparison.OrdinalIgnoreCase)) continue;

                                // Found matching device entry
                                var caption = mo["Caption"]?.ToString();
                                var manufacturer = mo["Manufacturer"]?.ToString();
                                var pnpId = mo["PNPDeviceID"]?.ToString();

                                // Update properties on UI thread via OnPropertyChanged (caller may be UI thread)
                                SerialDeviceDescription = caption ?? name;
                                SerialDeviceManufacturer = manufacturer ?? SerialDeviceManufacturer;

                                if (!string.IsNullOrEmpty(pnpId))
                                {
                                    var vidMatch = Regex.Match(pnpId, "VID_([0-9A-Fa-f]{4})");
                                    var pidMatch = Regex.Match(pnpId, "PID_([0-9A-Fa-f]{4})");
                                    SerialDeviceVid = vidMatch.Success ? vidMatch.Groups[1].Value : SerialDeviceVid;
                                    SerialDevicePid = pidMatch.Success ? pidMatch.Groups[1].Value : SerialDevicePid;
                                }

                                // Attempt to infer device type from PNPClass or other fields
                                var pnpClass = mo["PNPClass"]?.ToString();
                                SerialDeviceType = pnpClass ?? SerialDeviceType;

                                DeviceInfoLoaded = true;
                                break;
                            }
                        }
                        catch
                        {
                            // swallow - leave existing values
                        }
                    }, cancellationToken).ConfigureAwait(false);
                }
                finally
                {
                    IsFetchingDeviceInfo = false;
                }
            }
            public bool IsOpen
            {
                get => isOpen;
                set
                {
                    if (isOpen != value)
                    {
                        isOpen = value;
                        OnPropertyChanged(nameof(IsOpen));
                    }
                }
            }
            public bool DCD
            {
                get => _DCD;
                set
                {
                    if (_DCD != value)
                    {
                        _DCD = value;
                        OnPropertyChanged(nameof(DCD));
                    }
                }
            }
            public bool RI
            {
                get => _RI;
                set
                {
                    if (_RI != value)
                    {
                        _RI = value;
                        OnPropertyChanged(nameof(RI));
                    }
                }
            }
            public bool CTS
            {
                get => _CTS;
                set
                {
                    if (_CTS != value)
                    {
                        _CTS = value;
                        OnPropertyChanged(nameof(CTS));
                    }
                }
            }
            public bool DSR
            {
                get => _DSR;
                set
                {
                    if (_DSR != value)
                    {
                        _DSR = value;
                        OnPropertyChanged(nameof(DSR));
                    }
                }
            }
            public bool DTR
            {
                get => _DTR;
                set
                {
                    if (_DTR != value)
                    {
                        _DTR = value;
                        OnPropertyChanged(nameof(DTR));
                    }
                }
            }
            public bool RTS
            {
                get => _RTS;
                set
                {
                    if (_RTS != value)
                    {
                        _RTS = value;
                        OnPropertyChanged(nameof(RTS));
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
        public static void UpdateSerialPortConfig(string portName, int? baudRate = null, Parity? parity = null, StopBits? stopBits = null, int? dataBits = null, int? timeout = null, Encoding encoding = null)
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

                if (parity.HasValue)
                {
                    config.Parity = parity.Value;
                }
                else if (config.Parity is 0)
                {
                    config.Parity = Parity.None;
                }

                if (stopBits.HasValue)
                {
                    config.StopBits = stopBits.Value;
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
                    config.Encoding = encoding;
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
        /// <param name="name">The serial device name.</param>
        /// <param name="description">The serial device description.</param>
        /// <param name="manufacturer">The serial device manufacturer.</param>
        /// <param name="resetBaudRate">The default baud rate for resetting the serial device.</param>
        public static void UpdateSerialDeviceConfig(string portName, string name = null, string description = null, string manufacturer = null, int? resetBaudRate = null)
        {
            if (SerialPortConfigs.TryGetValue(portName, out var config))
            {

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
        public static void AddSerialPort(string portName, int baudRate, Parity parity, StopBits stopBits, int dataBits, int timeout, Encoding encoding)
        {
            if (!serialPorts.ContainsKey(portName))
            {
                var serialPort = new SerialPort
                {
                    PortName = portName,
                    BaudRate = baudRate,
                    Parity = parity,
                    StopBits = stopBits,
                    DataBits = dataBits,
                    ReadTimeout = timeout,
                    Encoding = encoding
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
            // Keep synchronous wrapper for compatibility - internally uses the async implementation
            OpenSerialPortAsync(portName).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Opens the specified serial port asynchronously to avoid blocking caller threads.
        /// </summary>
        /// <param name="portName">The name of the port.</param>
        public static async Task OpenSerialPortAsync(string portName)
        {
            if (serialPorts.TryGetValue(portName, out var value))
            {
                await Task.Run(() => value.Open()).ConfigureAwait(false);
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
