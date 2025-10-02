using FSGaryityTool_Win11.Views.Pages.SerialPortPage;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
        private readonly List<string> _encodingItems;

        public EncodingNameToEncodingConverter()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            _encodingItems = Encoding.GetEncodings().Select(e => e.Name).OrderBy(x => x).ToList();
        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            if (value is Encoding encoding)
                return encoding.WebName;
            return "us-ascii";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is string name)
            {
                // 只允许 EncodingItems 中的编码
                var match = _encodingItems
                    .FirstOrDefault(e => e.Equals(name, StringComparison.OrdinalIgnoreCase));
                if (match != null)
                {
                    try
                    {
                        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                        return Encoding.GetEncoding(match);
                    }
                    catch
                    {
                        return Encoding.ASCII;
                    }
                }
            }
            return Encoding.ASCII;
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

        private string _portText;
        private string _BaudRateText;
        private string _dataBitsText;
        private string _parityText;
        private string _stopBitsText;
        private string _encodingText;

        private string _parityValueText;
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
                    return $"{byteRate:F2} Byte/s"; // Use B/s for bytes per second
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
        public string PortText
        {
            get => _portText;
            set
            {
                if (_portText != value)
                {
                    _portText = value;
                    OnPropertyChanged(nameof(PortText));
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

        public static readonly DependencyProperty SerialPortEncodingNameProperty =
            DependencyProperty.Register(
                nameof(SerialPortEncodingName),
                typeof(string),
                typeof(SerialPortMenuBox),
                new PropertyMetadata(null, OnSerialPortEncodingNameChanged));

        public string SerialPortEncodingName
        {
            get => (string)GetValue(SerialPortEncodingNameProperty);
            set => SetValue(SerialPortEncodingNameProperty, value);
        }

        private static void OnSerialPortEncodingNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as SerialPortMenuBox;
            var name = e.NewValue as string;
            if (!string.IsNullOrWhiteSpace(name))
            {
                var match = control.FindBestEncodingMatch(name);
                if (match != null)
                {
                    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                    control.SerialPortEncoding = Encoding.GetEncoding(match);
                }
                else
                {
                    control.SerialPortEncoding = Encoding.UTF8;
                }
            }
        }


        public SerialPortMenuBox()
        {
            InitializeComponent();
            this.DataContext = this;

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var allEncodings = Encoding.GetEncodings().Select(e => e.Name).ToList();
            EncodingItems = GetSortedEncodingItems(allEncodings);

            ParitySegmented.Loaded += ParitySegmented_Loaded;

            // 输出 EncodingItems 到 Debug 命令行
            Debug.WriteLine("EncodingItems 列表：");
            foreach (var item in EncodingItems)
            {
                Debug.WriteLine(item);
            }


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

            if (SerialPortBaudRate == 0 || SerialPortBaudRate < 1 || SerialPortBaudRate > 100000000)
            {
                SerialPortBaudRate = 115200;
                _serialPortBaudRate = 115200;
            }

            //BaudRateComboBox.SelectedItem = SerialPortBaudRate.ToString();
            //AddBaudRateComboboxItem(BaudRateComboBox, SerialPortBaudRate);

        }
        private List<string> GetSortedEncodingItems(List<string> allEncodings)
        {
            // 编码分类，按常见类别分组
            var encodingCategories = new Dictionary<string, List<string>>
            {
                ["ASCII"] = new List<string> { "us-ascii", "ascii" },
                ["Unicode"] = new List<string> { "utf-8", "utf-16", "utf-16BE", "utf-32", "utf-32BE", "unicode" },
                ["GB"] = new List<string> { "gb2312", "gbk", "gb18030" },
                ["ISO"] = allEncodings.Where(x => x.StartsWith("iso-", StringComparison.OrdinalIgnoreCase)).ToList(),
                ["Big5"] = new List<string> { "big5" },
                ["Japanese"] = new List<string> { "shift_jis", "EUC-JP", "x-mac-japanese" },
                ["Korean"] = new List<string> { "ks_c_5601-1987", "Johab", "x-ebcdic-koreanextended" },
                ["Russian"] = new List<string> { "koi8-r", "koi8-u" },
                ["Windows"] = allEncodings.Where(x => x.StartsWith("windows-", StringComparison.OrdinalIgnoreCase)).ToList(),
                ["IBM/CP/DOS"] = allEncodings.Where(x =>
                    x.StartsWith("IBM", StringComparison.OrdinalIgnoreCase) ||
                    x.StartsWith("ibm", StringComparison.OrdinalIgnoreCase) ||
                    x.StartsWith("cp", StringComparison.OrdinalIgnoreCase) ||
                    x.StartsWith("DOS", StringComparison.OrdinalIgnoreCase)
                ).ToList(),
                ["Mac"] = allEncodings.Where(x => x.StartsWith("mac", StringComparison.OrdinalIgnoreCase) || x.StartsWith("x-mac", StringComparison.OrdinalIgnoreCase)).ToList(),
                ["Other"] = allEncodings.ToList()
            };

            // 移除已分类的编码，避免重复
            var categorized = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var kv in encodingCategories)
            {
                categorized.UnionWith(kv.Value);
            }
            encodingCategories["Other"] = allEncodings
                .Where(x => !categorized.Contains(x))
                .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                .ToList();

            // 自然排序比较器：优先按字母，数字按数值排序
            int NaturalCompare(string a, string b)
            {
                int i = 0, j = 0;
                while (i < a.Length && j < b.Length)
                {
                    if (char.IsDigit(a[i]) && char.IsDigit(b[j]))
                    {
                        // 提取数字
                        int startI = i, startJ = j;
                        while (i < a.Length && char.IsDigit(a[i])) i++;
                        while (j < b.Length && char.IsDigit(b[j])) j++;
                        var numA = int.Parse(a.Substring(startI, i - startI));
                        var numB = int.Parse(b.Substring(startJ, j - startJ));
                        if (numA != numB)
                            return numA.CompareTo(numB);
                    }
                    else
                    {
                        char ca = char.ToLower(a[i]);
                        char cb = char.ToLower(b[j]);
                        if (ca != cb)
                            return ca.CompareTo(cb);
                        i++; j++;
                    }
                }
                return a.Length.CompareTo(b.Length);
            }

            // 按类别合并排序
            var sortedList = new List<string>();
            foreach (var category in new[] { "Unicode", "ASCII", "GB", "ISO", "Big5", "Japanese", "Korean", "Russian", "Windows", "IBM/CP/DOS", "Mac", "Other" })
            {
                var items = encodingCategories[category]
                    .Where(x => allEncodings.Contains(x, StringComparer.OrdinalIgnoreCase))
                    .OrderBy(x => x, Comparer<string>.Create(NaturalCompare))
                    .ToList();
                sortedList.AddRange(items);
            }
            return sortedList;
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
                _lastValidEncoding = match;
                sender.Text = match;
            }
            else
            {
                // 恢复上次有效项或默认utf-8
                sender.Text = string.IsNullOrEmpty(_lastValidEncoding) ? "utf-8" : _lastValidEncoding;
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
                    _lastValidEncoding = match;
                    comboBox.Text = match;
                }
                else
                {
                    comboBox.Text = string.IsNullOrEmpty(_lastValidEncoding) ? "utf-8" : _lastValidEncoding;
                }
            }
        }
        private static readonly Dictionary<string, string> EncodingAliasMap = new()
        {
            // 常见关联：用户习惯输入 => 实际编码名称
            { "gbk", "gb2312" }, // gbk 实际对应 gb2312
            { "unicode8", "utf-8" },
            { "unicode16", "utf-16" },
            { "latin1", "iso-8859-1" },
            { "ascii", "us-ascii" },
            // 可继续扩展更多别名
        };

        private string FindBestEncodingMatch(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;

            // 统一小写并去除非字母数字
            string normInput = new string(input.ToLowerInvariant().Where(char.IsLetterOrDigit).ToArray());

            // 1. 先查别名映射
            if (EncodingAliasMap.TryGetValue(normInput, out var alias))
            {
                var aliasExact = EncodingItems.FirstOrDefault(e => e.Equals(alias, StringComparison.OrdinalIgnoreCase));
                if (aliasExact != null)
                    return aliasExact;
            }

            // 2. 精确查找
            var exact = EncodingItems.FirstOrDefault(e => e.Equals(input, StringComparison.OrdinalIgnoreCase));
            if (exact != null)
                return exact;

            // 3. 模糊查找（长度>=2）
            if (normInput.Length >= 2)
            {
                foreach (var item in EncodingItems)
                {
                    string normItem = new string(item.ToLowerInvariant().Where(char.IsLetterOrDigit).ToArray());
                    if (normItem == normInput)
                        return item;
                }

                foreach (var item in EncodingItems)
                {
                    string normItem = new string(item.ToLowerInvariant().Where(char.IsLetterOrDigit).ToArray());
                    if (normItem.Contains(normInput))
                        return item;
                }

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
            int min = 1, max = 100000000;
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
            int min = 1, max = 100000000;

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

