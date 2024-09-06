using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FSGaryityTool_Win11.Core.SerialPorts.SerialPortCoreServices.SerialPortConfigManager;

namespace FSGaryityTool_Win11.Core.SerialPorts
{
    internal class SerialPortCoreServices
    {


        /// <summary>
        /// Manages serial port configurations.
        /// </summary>
        public static class SerialPortConfigManager
        {
            public static Dictionary<string, SerialPortConfig> serialPortConfigs = new Dictionary<string, SerialPortConfig>();

            /// <summary>
            /// Represents the configuration for a serial port.
            /// </summary>
            public class SerialPortConfig
            {
                //SerialPortConfig

                /// <summary>
                /// Gets or sets the baud rate.
                /// </summary>
                public int BaudRate { get; set; }

                /// <summary>
                /// Gets or sets the parity.
                /// </summary>
                public Parity Parity { get; set; }

                /// <summary>
                /// Gets or sets the stop bits.
                /// </summary>
                public StopBits StopBits { get; set; }

                /// <summary>
                /// Gets or sets the data bits.
                /// </summary>
                public int DataBits { get; set; }

                /// <summary>
                /// Gets or sets the timeout.
                /// </summary>
                public int Timeout { get; set; }

                /// <summary>
                /// Gets or sets the encoding.
                /// </summary>
                public Encoding Encoding { get; set; }

                //SerialDeviceConfig

                /// <summary>
                /// Gets or sets the serial device icon.
                /// </summary>
                public string SerialDeviceIcon { get; set; }

                /// <summary>
                /// Gets or sets the serial device name.
                /// </summary>
                public string SerialDeviceName { get; set; }

                /// <summary>
                /// Gets or sets the serial device description.
                /// </summary>
                public string SerialDeviceDescription { get; set; }

                /// <summary>
                /// Gets or sets the serial device manufacturer.
                /// </summary>
                public string SerialDeviceManufacturer { get; set; }

                /// <summary>
                /// Gets or sets the default baud rate for resetting the serial device.
                /// </summary>
                public int SerialDeviceResetDefBaudRate { get; set; }

            }

            /// <summary>
            /// Gets the serial port configuration for the specified port name.
            /// </summary>
            /// <param name="portName">The name of the port.</param>
            /// <returns>The serial port configuration if found; otherwise, null.</returns>
            public static SerialPortConfig GetSerialPortConfig(string portName)
            {
                if (serialPortConfigs.ContainsKey(portName))
                {
                    return serialPortConfigs[portName];
                }
                return null;
            }

            /// <summary>
            /// Gets the names of all serial ports in the dictionary.
            /// </summary>
            /// <returns>A list of all serial port names.</returns>
            public static List<string> GetAllSerialPortNames()
            {
                return serialPortConfigs.Keys.ToList();
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
                if (serialPortConfigs.ContainsKey(portName))
                {
                    SerialPortConfig config = serialPortConfigs[portName];

                    if (baudRate.HasValue)
                    {
                        config.BaudRate = baudRate.Value;
                    }
                    else if (config.BaudRate == 0)
                    {
                        config.BaudRate = 115200;
                    }

                    if (parity != null)
                    {
                        config.Parity = (Parity)Enum.Parse(typeof(Parity), parity, true);
                    }
                    else if (config.Parity == 0)
                    {
                        config.Parity = Parity.None;
                    }

                    if (stopBits != null)
                    {
                        config.StopBits = (StopBits)Enum.Parse(typeof(StopBits), stopBits, true);
                    }
                    else if (config.StopBits == 0)
                    {
                        config.StopBits = StopBits.One;
                    }

                    if (dataBits.HasValue)
                    {
                        config.DataBits = dataBits.Value;
                    }
                    else if (config.DataBits == 0)
                    {
                        config.DataBits = 8;
                    }

                    if (timeout.HasValue)
                    {
                        config.Timeout = timeout.Value;
                    }
                    else if (config.Timeout == 0)
                    {
                        config.Timeout = 1500;
                    }

                    if (encoding != null)
                    {
                        config.Encoding = Encoding.GetEncoding(encoding);
                    }
                    else if (config.Encoding == null)
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
                if (serialPortConfigs.ContainsKey(portName))
                {
                    SerialPortConfig config = serialPortConfigs[portName];

                    if (icon != null)
                    {
                        config.SerialDeviceIcon = icon;
                    }
                    else config.SerialDeviceIcon = "\uE964";

                    if (name != null)
                    {
                        config.SerialDeviceName = name;
                    }
                    else config.SerialDeviceName = portName;

                    if (description != null)
                    {
                        config.SerialDeviceDescription = description;
                    }
                    else config.SerialDeviceDescription = "Serial Device";

                    if (manufacturer != null)
                    {
                        config.SerialDeviceManufacturer = manufacturer;
                    }
                    else config.SerialDeviceManufacturer = "Microsoft";

                    if (resetBaudRate.HasValue)
                    {
                        config.SerialDeviceResetDefBaudRate = resetBaudRate.Value;
                    }
                    else config.SerialDeviceResetDefBaudRate = 115200;

                }
            }

        }

        /// <summary>
        /// Manages serial port configurations and operations.
        /// </summary>
        public static class SerialPortManager
        {
            public static Dictionary<string, SerialPort> serialPorts = new Dictionary<string, SerialPort>();

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
            }

            /// <summary>
            /// Adds a serial port from the configuration.
            /// </summary>
            /// <param name="portName">The name of the port.</param>
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

                    serialPorts[portName] = serialPort;
                }
            }

            /// <summary>
            /// Updates a serial port from the configuration.
            /// </summary>
            /// <param name="portName">The name of the port.</param>
            public static void UpdateSerialPortFormConfig(string portName)
            {
                if (serialPorts.ContainsKey(portName) && serialPortConfigs.ContainsKey(portName))
                {
                    SerialPortConfig config = serialPortConfigs[portName];
                    SerialPort serialPort = serialPorts[portName];

                    serialPort.BaudRate = config.BaudRate;
                    serialPort.Parity = config.Parity;
                    serialPort.StopBits = config.StopBits;
                    serialPort.DataBits = config.DataBits;
                    serialPort.ReadTimeout = config.Timeout;
                    serialPort.Encoding = config.Encoding;
                }
            }

            /// <summary>
            /// Gets the serial port object for the specified port name.
            /// </summary>
            /// <param name="portName">The name of the port.</param>
            /// <returns>The SerialPort object if found; otherwise, null.</returns>
            public static SerialPort GetSerialPort(string portName)
            {
                if (serialPorts.ContainsKey(portName))
                {
                    return serialPorts[portName];
                }

                return null;
            }

            /// <summary>
            /// Gets all closed serial ports.
            /// </summary>
            /// <returns>A string containing the names of all closed serial ports.</returns>
            public static List<string> GetAllClosedPorts()
            {
                List<string> closedPorts = new List<string>();

                foreach (var port in serialPorts)
                {
                    if (!port.Value.IsOpen)
                    {
                        closedPorts.Add(port.Key);
                    }
                }

                return closedPorts;
            }

            /// <summary>
            /// Opens the specified serial port.
            /// </summary>
            /// <param name="portName">The name of the port.</param>
            public static void OpenSerialPort(string portName)
            {
                if (serialPorts.ContainsKey(portName))
                {
                    serialPorts[portName].Open();
                }
            }

            /// <summary>
            /// Closes the specified serial port.
            /// </summary>
            /// <param name="portName">The name of the port.</param>
            public static void CloseSerialPort(string portName)
            {
                if (serialPorts.ContainsKey(portName))
                {
                    serialPorts[portName].Close();
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
                if (serialPorts.ContainsKey(portName))
                {
                    SerialPort port = serialPorts[portName];
                    if (port.IsOpen)
                    {
                        port.Close();
                    }
                    port.Dispose();
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
}
