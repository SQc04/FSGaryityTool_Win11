using FSGaryityTool_Win11.Views.Pages.SerialPortPage;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FSGaryityTool_Win11.Controls
{
    public class StopBitsEnumToDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value switch
            {
                StopBits.One => 1.0,
                StopBits.OnePointFive => 1.5,
                StopBits.Two => 2.0,
                _ => 1.0
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return (double)value switch
            {
                1.0 => StopBits.One,
                1.5 => StopBits.OnePointFive,
                2.0 => StopBits.Two,
                _ => StopBits.One
            };
        }
    }
    public class EncodingNameToEncodingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is Encoding encoding)
                return encoding.WebName;
            return "utf-8";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is string name)
            {
                try
                {
                    return Encoding.GetEncoding(name);
                }
                catch
                {
                    return Encoding.UTF8;
                }
            }
            return Encoding.UTF8;
        }
    }
    public class ParityToCharConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value switch
            {
                Parity.None => "N",
                Parity.Odd => "O",
                Parity.Even => "E",
                Parity.Mark => "M",
                Parity.Space => "S",
                _ => "N"
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value is string s ? s.ToUpper() switch
            {
                "N" => Parity.None,
                "O" => Parity.Odd,
                "E" => Parity.Even,
                "M" => Parity.Mark,
                "S" => Parity.Space,
                _ => Parity.None
            } : Parity.None;
        }
    }
    public sealed partial class SerialPortMenuBox : UserControl, INotifyPropertyChanged
    {
        private int _serialPortBaudRate;
        private int _serialPortDataBits;
        private Parity _serialPortParity;
        private StopBits _serialPortStopBits;
        private Encoding _serialPortEncoding;

        private string _BaudRateText;
        private string _dataBitsText;
        private string _parityText;
        private string _stopBitsText;
        private string _encodingText;

        private string _parityValueText;
        private string _encodingValueText;
        public List<string> EncodingItems { get; }

        public int SerialPortBaudRate
        {
            get => _serialPortBaudRate;
            set
            {
                if (_serialPortBaudRate != value)
                {
                    _serialPortBaudRate = value;
                    OnPropertyChanged(nameof(SerialPortBaudRate));
                    OnPropertyChanged(nameof(BitRate)); // 通知比特率变化
                    OnPropertyChanged(nameof(BitRateDisplay));
                    OnPropertyChanged(nameof(ByteRateDisplay));
                    DelayAddBaudRateComboboxItem(); // 延迟添加波特率到ComboBox
                }
            }
        }
        public double BitRate
        {
            get
            {
                // 如果Parity为None，校验位为0，否则为1
                int parityBit = SerialPortParity == Parity.None ? 0 : 1;
                // 计算总位数
                double totalBits = 1 + SerialPortDataBits + parityBit + (double)SerialPortStopBits;
                // 计算比特率并四舍五入
                double bitRate = SerialPortBaudRate * SerialPortDataBits / totalBits;
                return Math.Round(bitRate);
            }
        }
        public string BitRateDisplay
        {
            get
            {
                if (SerialPortBaudRate > 9600)
                    return $"{(BitRate / 1000.0):F2} Kbit/s";
                else
                    return $"{BitRate} bit/s";
            }
        }
        public string ByteRateDisplay
        {
            get
            {
                double byteRate = BitRate / 8.0; // Convert bits to bytes
                if (SerialPortBaudRate > 9600)
                    return $"{(byteRate / 1000.0):F2} KB/s"; // Use KB/s for kilobytes per second
                else
                    return $"{byteRate:F2} B/s"; // Use B/s for bytes per second
            }
        }

        public int SerialPortDataBits
        {
            get => _serialPortDataBits;
            set
            {
                if (_serialPortDataBits != value)
                {
                    _serialPortDataBits = value;
                    OnPropertyChanged(nameof(SerialPortDataBits));
                    OnPropertyChanged(nameof(BitRate)); // 通知比特率变化
                    OnPropertyChanged(nameof(BitRateDisplay));
                    OnPropertyChanged(nameof(ByteRateDisplay));
                }
            }
        }

        public Parity SerialPortParity
        {
            get => _serialPortParity;
            set
            {
                if (_serialPortParity != value)
                {
                    _serialPortParity = value;
                    OnPropertyChanged(nameof(SerialPortParity));
                    OnPropertyChanged(nameof(BitRate)); // 通知比特率变化
                    OnPropertyChanged(nameof(BitRateDisplay));
                    OnPropertyChanged(nameof(ByteRateDisplay));
                    // 同步到Segmented控件
                    if (ParitySegmented != null)
                        ParitySegmented.SelectedIndex = (int)value;
                }
            }
        }
        public StopBits SerialPortStopBits
        {
            get => _serialPortStopBits;
            set
            {
                if (_serialPortStopBits != value)
                {
                    _serialPortStopBits = value;
                    OnPropertyChanged(nameof(SerialPortStopBits));
                    OnPropertyChanged(nameof(BitRate)); // 通知比特率变化
                    OnPropertyChanged(nameof(BitRateDisplay));
                    OnPropertyChanged(nameof(ByteRateDisplay));
                }
            }
        }

        public Encoding SerialPortEncoding
        {
            get => _serialPortEncoding;
            set
            {
                if (_serialPortEncoding != value)
                {
                    _serialPortEncoding = value;
                    OnPropertyChanged(nameof(SerialPortEncoding));
                }
            }
        }

        public string BaudRateText
        {
            get => _BaudRateText;
            set
            {
                if (_BaudRateText != value)
                {
                    _BaudRateText = value;
                    OnPropertyChanged(nameof(BaudRateText));
                }
            }
        }
        public string DataBitsText
        {
            get => _dataBitsText;
            set
            {
                if (_dataBitsText != value)
                {
                    _dataBitsText = value;
                    OnPropertyChanged(nameof(DataBitsText));
                }
            }
        }

        public string ParityText
        {
            get => _parityText;
            set
            {
                if (_parityText != value)
                {
                    _parityText = value;
                    OnPropertyChanged(nameof(ParityText));
                }
            }
        }

        public string StopBitsText
        {
            get => _stopBitsText;
            set
            {
                if (_stopBitsText != value)
                {
                    _stopBitsText = value;
                    OnPropertyChanged(nameof(StopBitsText));
                }
            }
        }

        public string EncodingText
        {
            get => _encodingText;
            set
            {
                if (_encodingText != value)
                {
                    _encodingText = value;
                    OnPropertyChanged(nameof(EncodingText));
                }
            }
        }
        public string ParityValueText
        {
            get => _parityValueText;
            set
            {
                if (_parityValueText != value)
                {
                    _parityValueText = value;
                    OnPropertyChanged(nameof(ParityValueText));
                }
            }
        }
        public string EncodingValueText
        {
            get => _encodingValueText;
            set
            {
                if (_encodingValueText != value)
                {
                    _encodingValueText = value;
                    OnPropertyChanged(nameof(EncodingValueText));
                }
            }
        }

        public SerialPortMenuBox()
        {
            InitializeComponent();
            this.DataContext = this;

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            EncodingItems = Encoding.GetEncodings().Select(e => e.Name).OrderBy(x => x).ToList();
            ParitySegmented.Loaded += ParitySegmented_Loaded;

            
            if (SerialPortDataBits == 0 || SerialPortDataBits < 5 || SerialPortDataBits > 8)
            {
                SerialPortDataBits = 8;
                _serialPortDataBits = 8;
            }
            if (SerialPortStopBits == StopBits.None)
            {
                SerialPortStopBits = StopBits.One;
                _serialPortStopBits = StopBits.One;
            }
            if (SerialPortEncoding == null)
            {
                SerialPortEncoding = Encoding.UTF8;
                _serialPortEncoding = Encoding.UTF8;
            }

            if (SerialPortBaudRate == 0 || SerialPortBaudRate < 1 || SerialPortBaudRate > 10000000)
            {
                SerialPortBaudRate = 115200;
                _serialPortBaudRate = 115200;
            }

            //BaudRateComboBox.SelectedItem = SerialPortBaudRate.ToString();
            //AddBaudRateComboboxItem(BaudRateComboBox, SerialPortBaudRate);

        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private string _lastValidEncoding = "utf-8";

        private void EncodingComboBox_TextSubmitted(ComboBox sender, ComboBoxTextSubmittedEventArgs args)
        {
            var text = args.Text?.Trim() ?? "";
            var match = FindBestEncodingMatch(text);
            if (match != null)
            {
                EncodingValueText = match;
                _lastValidEncoding = match;
                sender.Text = match;
            }
            else
            {
                // 恢复上次有效项或默认utf-8
                EncodingValueText = string.IsNullOrEmpty(_lastValidEncoding) ? "utf-8" : _lastValidEncoding;
                sender.Text = EncodingValueText;
            }
        }

        private void EncodingComboBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is ComboBox comboBox)
            {
                var text = comboBox.Text?.Trim() ?? "";
                var match = FindBestEncodingMatch(text);
                if (match != null)
                {
                    EncodingValueText = match;
                    _lastValidEncoding = match;
                    comboBox.Text = match;
                }
                else
                {
                    EncodingValueText = string.IsNullOrEmpty(_lastValidEncoding) ? "utf-8" : _lastValidEncoding;
                    comboBox.Text = EncodingValueText;
                }
            }
        }
        private string FindBestEncodingMatch(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;

            // 统一小写并去除非字母数字
            string normInput = new string(input.ToLowerInvariant().Where(char.IsLetterOrDigit).ToArray());

            // 先精确查找
            var exact = EncodingItems.FirstOrDefault(e => e.Equals(input, StringComparison.OrdinalIgnoreCase));
            if (exact != null)
                return exact;

            // 再模糊查找
            foreach (var item in EncodingItems)
            {
                string normItem = new string(item.ToLowerInvariant().Where(char.IsLetterOrDigit).ToArray());
                if (normItem == normInput)
                    return item;
            }

            // 支持部分匹配（如输入utf8能匹配utf-8）
            foreach (var item in EncodingItems)
            {
                string normItem = new string(item.ToLowerInvariant().Where(char.IsLetterOrDigit).ToArray());
                if (normItem.Contains(normInput))
                    return item;
            }

            // 顺序缩写匹配（如ut8能匹配utf-8），最少3个字符
            if (normInput.Length >= 3)
            {
                foreach (var item in EncodingItems)
                {
                    string normItem = new string(item.ToLowerInvariant().Where(char.IsLetterOrDigit).ToArray());
                    int pos = 0;
                    foreach (char c in normInput)
                    {
                        pos = normItem.IndexOf(c, pos);
                        if (pos == -1)
                            break;
                        pos++;
                    }
                    if (pos != -1)
                        return item;
                }
            }

            return null;
        }

        private void ParitySegmented_Loaded(object sender, RoutedEventArgs e)
        {
            // 初始化时同步控件和属性
            ParitySegmented.SelectedIndex = (int)SerialPortParity;
        }

        private void ParitySegmented_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ParitySegmented.SelectedIndex >= 0)
            {
                SerialPortParity = (Parity)ParitySegmented.SelectedIndex;
            }
        }

        private void EncodingComboBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            if (comboBox != null && !comboBox.IsDropDownOpen)
            {
                comboBox.IsDropDownOpen = true;
                e.Handled = true;
            }
        }

        private void BaudRateComboBox_TextSubmitted(ComboBox sender, ComboBoxTextSubmittedEventArgs args)
        {
            ValidateBaudRateInput(sender, args.Text);
        }

        private void BaudRateComboBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is ComboBox comboBox)
            {
                ValidateBaudRateInput(comboBox, comboBox.Text);
            }
        }

        private CancellationTokenSource _BaudRateCts; // 用于取消之前的延迟

        private async void DelayAddBaudRateComboboxItem()
        {
            // Cancel any previous delay (if exists)
            _BaudRateCts?.Cancel();
            _BaudRateCts = new CancellationTokenSource();

            try
            {
                // Delay for 2 seconds, cancellable
                await Task.Delay(3500, _BaudRateCts.Token);

                // Enqueue the operation on the DispatcherQueue
                bool enqueued = BaudRateComboBox.DispatcherQueue.TryEnqueue(() =>
                {
                    AddBaudRateComboboxItem(BaudRateComboBox, SerialPortBaudRate);
                });

                if (!enqueued)
                {
                    // Handle the case where enqueueing failed (optional)
                    // For example, log an error or retry
                }
            }
            catch (TaskCanceledException)
            {
                // Delay was cancelled, no further action needed
            }
            finally
            {
                // Clean up CancellationTokenSource
                _BaudRateCts?.Dispose();
                _BaudRateCts = null;
            }
        }

        private async void AddBaudRateComboboxItem(ComboBox comboBox, int value)
        {
            // 检查是否需要将新值添加到 ComboBox 的 Items
            int min = 1, max = 10000000;
            if (!comboBox.Items.Contains(value) && value >= min && value <= max)
            {
                // 添加新值到 ComboBox 的 Items
                comboBox.Items.Add(value);

                await Task.Run(() =>
                {
                    Thread.Sleep(3000);
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        ///*
                        // 可选：对 Items 进行排序（保持波特率升序）
                        var sortedItems = comboBox.Items.Cast<int>().ToList();
                        sortedItems.Sort();
                        comboBox.Items.Clear();
                        foreach (int item in sortedItems)
                        {
                            comboBox.Items.Add(item);
                        }
                        //*/
                    });
                });
                

                // 设置当前选中项为新添加的值
                comboBox.SelectedItem = value;
            }
        }

        private void ValidateBaudRateInput(ComboBox comboBox, string text)
        {
            // 允许的波特率范围
            int min = 1, max = 10000000;

            // 验证输入是否为纯数字且在范围内
            if (!string.IsNullOrWhiteSpace(text) && text.All(char.IsDigit) && int.TryParse(text, out int value) && value >= min && value <= max)
            {
                // 更新绑定的属性
                SerialPortBaudRate = value;

                //AddBaudRateComboboxItem(comboBox, value);
            }
            else
            {
                // 处理无效输入
                if (text == "0")
                {
                    comboBox.Text = min.ToString();
                    SerialPortBaudRate = min; // 更新绑定的属性
                }
                else if(Convert.ToInt32(text) >= max)
                {
                    comboBox.Text = max.ToString();
                    SerialPortBaudRate = max;

                }
                else
                {
                    // 恢复为上次有效值或默认值
                    comboBox.Text = SerialPortBaudRate.ToString();
                }

                // 标记事件为已处理，防止 ComboBox 接受无效输入
            }
        }
    }
}

