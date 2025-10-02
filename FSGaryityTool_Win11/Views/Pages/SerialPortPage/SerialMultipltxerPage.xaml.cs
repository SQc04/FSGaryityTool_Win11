using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.ComponentModel;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;

namespace FSGaryityTool_Win11.Views.Pages.SerialPortPage
{
    public sealed partial class SerialMultipltxerPage : Page, INotifyPropertyChanged
    {
        private SerialPort serialPortUpper;
        private SerialPort serialPortLower;
        private SerialPort serialPortUpperInput;
        private SerialPort serialPortLowerInput;

        private bool _isConnected;
        public bool IsConnected
        {
            get => _isConnected;
            set
            {
                if (_isConnected != value)
                {
                    _isConnected = value;
                    OnPropertyChanged(nameof(IsConnected));
                }
            }
        }
        private string[] _ports;
        public string[] Ports
        {
            get => _ports;
            set
            {
                if (_ports != value)
                {
                    _ports = value;
                    OnPropertyChanged(nameof(Ports));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public SerialMultipltxerPage()
        {
            this.InitializeComponent();
            InitializeComboBoxes();
        }

        private void InitializeComboBoxes()
        {
            serialPortUpper = new SerialPort();
            serialPortLower = new SerialPort();
            serialPortUpperInput = new SerialPort();
            serialPortLowerInput = new SerialPort();

            Ports = SerialPort.GetPortNames();

            serialPortUpper.DataReceived += _serialPortUpper_DataReceived;
            serialPortLower.DataReceived += _serialPortLower_DataReceived;
            serialPortUpperInput.DataReceived += _serialPortUpperInput_DataReceived;
            serialPortLowerInput.DataReceived += _serialPortLowerInput_DataReceived;

        }

        private void ConnectPort(SerialPort port, string portName)
        {
            if (port != null && !port.IsOpen)
            {
                port.PortName = portName;
                port.BaudRate = 460800;
                port.Parity = Parity.None;
                port.DataBits = 8;
                port.StopBits = StopBits.One;
                port.Open();
            }
        }
        private void DisconnectPort(SerialPort port)
        {
            if (port != null && port.IsOpen)
            {
                port.Close();
            }
        }

        private void ConnectToggleButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsConnected)
            {
                ConnectPort(serialPortUpper, (string)SerialPortUpper.SelectedItem );
                ConnectPort(serialPortLower, (string)SerialPortLower.SelectedItem);
                ConnectPort(serialPortUpperInput, (string)SerialPortUpperInput.SelectedItem);
                ConnectPort(serialPortLowerInput, (string)SerialPortLowerInput.SelectedItem);
            }
            else
            {
                DisconnectPort(serialPortUpper);
                DisconnectPort(serialPortLower);
                DisconnectPort(serialPortUpperInput);
                DisconnectPort(serialPortLowerInput);
            }
        }

        private void _serialPortUpper_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                int bytesToRead = serialPortUpper.BytesToRead;
                if (bytesToRead <= 0) return;

                byte[] buffer = new byte[bytesToRead];
                serialPortUpper.Read(buffer, 0, bytesToRead);

                // 实时转发到 Lower
                ForwardData(serialPortLower, buffer);

                // 异步转发到 UpperInput（监听通道）
                Task.Run(() => ForwardData(serialPortUpperInput, buffer));
            }
            catch (Exception ex)
            {
                // TODO: 日志记录 ex.Message
            }
        }

        private void _serialPortLower_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                int bytesToRead = serialPortLower.BytesToRead;
                if (bytesToRead <= 0) return;

                byte[] buffer = new byte[bytesToRead];
                serialPortLower.Read(buffer, 0, bytesToRead);

                // 实时转发到 Upper
                ForwardData(serialPortUpper, buffer);

                // 异步转发到 LowerInput（监听通道）
                Task.Run(() => ForwardData(serialPortLowerInput, buffer));
            }
            catch (Exception ex)
            {
                // TODO: 日志记录 ex.Message
            }
        }
        private void ForwardData(SerialPort targetPort, byte[] data)
        {
            try
            {
                if (targetPort != null && targetPort.IsOpen)
                {
                    targetPort.Write(data, 0, data.Length);
                }
            }
            catch (Exception ex)
            {
                // TODO: 可选日志记录 targetPort.PortName + ": " + ex.Message
            }
        }
        private void _serialPortUpperInput_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {

        }
        private void _serialPortLowerInput_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {

        }
    }
}