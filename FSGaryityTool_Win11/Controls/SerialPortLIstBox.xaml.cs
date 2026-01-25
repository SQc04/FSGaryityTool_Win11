using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace FSGaryityTool_Win11.Controls;


public static class ObservableCollectionExtensions
{
    public static void AddRange<T>(this ObservableCollection<T> collection, IEnumerable<T> items)
    {
        foreach (var item in items)
            collection.Add(item);
    }
}


public class SerialPortInfo : INotifyPropertyChanged
{
    private string _portName = string.Empty;
    public string PortName
    {
        get => _portName;
        set { _portName = value; OnPropertyChanged(nameof(PortName)); }
    }

    private string _portDeviceDescription = string.Empty;
    public string PortDeviceDescription
    {
        get => _portDeviceDescription;
        set { _portDeviceDescription = value; OnPropertyChanged(nameof(PortDeviceDescription)); }
    }

    private string _portDeviceIcon = string.Empty;
    public string PortDeviceIcon
    {
        get => _portDeviceIcon;
        set { _portDeviceIcon = value; OnPropertyChanged(nameof(PortDeviceIcon)); }
    }

    private Visibility _newPortTag = Visibility.Collapsed;
    public Visibility NewPortTag
    {
        get => _newPortTag;
        set { _newPortTag = value; OnPropertyChanged(nameof(NewPortTag)); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private static readonly ConcurrentDictionary<string, SerialPortInfo> _portInfoDictionary = new(StringComparer.OrdinalIgnoreCase);

    public static SerialPortInfo FromPortName(string portName)
    {
        if (string.IsNullOrWhiteSpace(portName)) return null!;
        return _portInfoDictionary.GetOrAdd(portName.Trim(), key => new SerialPortInfo { PortName = key });
    }

    private static string GetIconFromDescription(ReadOnlySpan<char> description)
    {
        if (description.Contains("bluetooth", StringComparison.OrdinalIgnoreCase) ||
            description.Contains("蓝牙", StringComparison.OrdinalIgnoreCase))
            return "\uE702"; // Bluetooth

        if (description.Contains("usb", StringComparison.OrdinalIgnoreCase) ||
            description.Contains("cdc", StringComparison.OrdinalIgnoreCase)) // 加强 RP2040 CDC 识别
            return "\uECF0"; // USB-SERIAL

        if (description.Contains("com0com", StringComparison.OrdinalIgnoreCase))
            return "\uE8CE"; // Emulator

        return "\uE783"; // Default serial
    }

    private static string EscapeWmiString(string input)
    {
        return input?.Replace("'", "''") ?? string.Empty;
    }

    /// <summary>
    /// 刷新端口信息，支持全刷或单端口刷新
    /// </summary>
    public static async Task RefreshPortInfoAsync(string? portName = null)
    {
        await Task.Run(() =>
        {
            string query;
            if (string.IsNullOrWhiteSpace(portName))
            {
                // 全量查询所有 Ports 类设备
                query = "SELECT * FROM Win32_PnPEntity WHERE ClassGuid = '{4d36e978-e325-11ce-bfc1-08002be10318}'";
            }
            else
            {
                var safePort = EscapeWmiString(portName.Trim());
                // 针对单个端口，尽量精确匹配（避免误匹配）
                query = $"SELECT * FROM Win32_PnPEntity WHERE ClassGuid = '{{4d36e978-e325-11ce-bfc1-08002be10318}}' " +
                        $"AND (Name LIKE '%({safePort})%' OR Name LIKE '%{safePort}%')";
            }

            using var searcher = new ManagementObjectSearcher(query);
            using var results = searcher.Get();

            var currentPortNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (ManagementObject obj in results)
            {
                string? name = obj["Name"]?.ToString();
                if (string.IsNullOrWhiteSpace(name)) continue;

                // 更稳健的 COM 端口提取（兼容各种描述格式）
                string portExtracted = ExtractComPort(name);
                if (string.IsNullOrEmpty(portExtracted) ||
                    (!portExtracted.StartsWith("COM", StringComparison.OrdinalIgnoreCase) &&
                     !portExtracted.StartsWith("CNC", StringComparison.OrdinalIgnoreCase)))
                    continue;

                string description = obj["Description"]?.ToString() ?? obj["Caption"]?.ToString() ?? "Unknown";
                string icon = GetIconFromDescription(description.AsSpan());

                currentPortNames.Add(portExtracted);

                var info = _portInfoDictionary.AddOrUpdate(
                    portExtracted,
                    _ => new SerialPortInfo
                    {
                        PortName = portExtracted,
                        PortDeviceDescription = description,
                        PortDeviceIcon = icon
                    },
                    (_, existing) =>
                    {
                        if (existing.PortDeviceDescription != description)
                            existing.PortDeviceDescription = description;
                        if (existing.PortDeviceIcon != icon)
                            existing.PortDeviceIcon = icon;
                        return existing;
                    });
            }

            // 全量刷新时清理已消失的端口（防止内存累积）
            if (string.IsNullOrWhiteSpace(portName))
            {
                var toRemove = _portInfoDictionary.Keys
                    .Where(k => !currentPortNames.Contains(k))
                    .ToList();

                foreach (var key in toRemove)
                {
                    _portInfoDictionary.TryRemove(key, out _);
                }
            }
        });
    }

    private static string ExtractComPort(string name)
    {
        ReadOnlySpan<char> span = name.AsSpan();

        int openParenIndex = span.IndexOf('(');
        if (openParenIndex < 0)
        {
            return ExtractWithoutParen(span);
        }

        var afterOpen = span.Slice(openParenIndex + 1);

        int closeParenIndex = afterOpen.IndexOf(')');  
        if (closeParenIndex < 0)
        {
            return string.Empty;
        }

        var candidate = afterOpen.Slice(0, closeParenIndex).Trim();

        if (candidate.StartsWith("COM", StringComparison.OrdinalIgnoreCase) ||
            candidate.StartsWith("CNC", StringComparison.OrdinalIgnoreCase))
        {
            return candidate.ToString();
        }

        return ExtractWithoutParen(span);
    }

    private static string ExtractWithoutParen(ReadOnlySpan<char> span)
    {
        int comIndex = span.IndexOf("COM", StringComparison.OrdinalIgnoreCase);
        if (comIndex < 0)
        {
            comIndex = span.IndexOf("CNC", StringComparison.OrdinalIgnoreCase);
            if (comIndex < 0) return string.Empty;
        }

        var sub = span.Slice(comIndex);

        int endIndex = sub.IndexOfAny([' ', ')', '\t', '\r', '\n']);
        if (endIndex < 0)
        {
            endIndex = sub.Length;
        }

        return sub.Slice(0, endIndex).ToString();
    }

    public static async Task<SerialPortInfo?> GetPortAsync(string portName)
    {
        if (string.IsNullOrWhiteSpace(portName)) return null;

        portName = portName.Trim();
        if (_portInfoDictionary.TryGetValue(portName, out var info))
            return info;

        await RefreshPortInfoAsync(portName);
        _portInfoDictionary.TryGetValue(portName, out info);
        return info;
    }

    public static IReadOnlyDictionary<string, SerialPortInfo> GetAllPorts() => _portInfoDictionary;

    /// <summary>
    /// 获取当前有效的串口名称列表（替代 SerialPort.GetPortNames()）
    /// 使用 WMI Win32_PnPEntity 查询，更可靠地处理 USB CDC 端口移除
    /// </summary>
    /// <returns>排序后的串口名列表（如 "COM1", "COM3", "CNC5"）</returns>
    public static List<string> GetAvailablePortNames()
    {
        var ports = new HashSet<string>(StringComparer.OrdinalIgnoreCase); // 去重 + 忽略大小写

        try
        {
            using var searcher = new ManagementObjectSearcher(
                @"SELECT Name, DeviceID FROM Win32_PnPEntity 
              WHERE ClassGuid = '{4d36e978-e325-11ce-bfc1-08002be10318}'"); // Ports 类 GUID

            foreach (ManagementObject obj in searcher.Get())
            {
                // 优先尝试 DeviceID（有些驱动直接是 COMx）
                string? deviceId = obj["DeviceID"]?.ToString();
                if (!string.IsNullOrWhiteSpace(deviceId) &&
                    (deviceId.StartsWith("COM", StringComparison.OrdinalIgnoreCase) ||
                     deviceId.StartsWith("CNC", StringComparison.OrdinalIgnoreCase)))
                {
                    ports.Add(deviceId);
                    continue;
                }

                // fallback：从 Name 中提取（如 "USB Serial Port (COM5)"）
                string? name = obj["Name"]?.ToString();
                if (!string.IsNullOrWhiteSpace(name))
                {
                    string extracted = ExtractComPort(name); // 你类里已有的提取方法
                    if (!string.IsNullOrEmpty(extracted))
                    {
                        ports.Add(extracted);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // WMI 异常时降级（生产环境可记录日志）
            System.Diagnostics.Debug.WriteLine($"WMI 获取端口列表失败: {ex.Message}");
            ports.UnionWith(SerialPort.GetPortNames());
        }

        return ports.OrderBy(p => p).ToList();
    }
}


public class SerialPortEventArgs : EventArgs
{
    public string PortName { get; }
    public string PortDeviceDescription { get; }

    public SerialPortEventArgs(string portName, string portDeviceDescription)
    {
        PortName = portName;
        PortDeviceDescription = portDeviceDescription;
    }
}

// ========== SerialPortLIstBox ==========
public sealed partial class SerialPortLIstBox : UserControl, INotifyPropertyChanged
{
    private bool _autoConnectSetting;
    public bool AutoConnectSetting
    {
        get => _autoConnectSetting;
        set { if (_autoConnectSetting != value) { _autoConnectSetting = value; OnPropertyChanged(nameof(AutoConnectSetting)); } }
    }

    private bool _autoSearchSetting;
    public bool AutoSearchSetting
    {
        get => _autoSearchSetting;
        set { if (_autoSearchSetting != value) { _autoSearchSetting = value; OnPropertyChanged(nameof(AutoSearchSetting)); } }
    }

    private ListViewSelectionMode _selectionMode;
    public ListViewSelectionMode SelectionMode
    {
        get => _selectionMode;
        set { if (_selectionMode != value) { _selectionMode = value; OnPropertyChanged(nameof(SelectionMode)); } }
    }

    private ObservableCollection<SerialPortInfo> _portList = new();
    public ObservableCollection<SerialPortInfo> PortList
    {
        get => _portList;
        set { if (_portList != value) { _portList = value; OnPropertyChanged(nameof(PortList)); } }
    }

    private ObservableCollection<string> _selectedPorts = new();
    public ObservableCollection<string> SelectedPorts
    {
        get => _selectedPorts;
        set { if (_selectedPorts != value) { _selectedPorts = value; OnPropertyChanged(nameof(SelectedPorts)); } }
    }

    public static readonly DependencyProperty SelectedPortProperty =
        DependencyProperty.Register(nameof(SelectedPort), typeof(string), typeof(SerialPortLIstBox),
            new PropertyMetadata(null, OnSelectedPortChanged));

    public string? SelectedPort
    {
        get => (string?)GetValue(SelectedPortProperty);
        set => SetValue(SelectedPortProperty, value);
    }

    private static void OnSelectedPortChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (SerialPortLIstBox)d;
        control.OnSelectedPortChanged(e.OldValue as string, e.NewValue as string);
    }

    private void OnSelectedPortChanged(string? oldValue, string? newValue)
    {
        // 外部设置 SelectedPort 时，同步到内部选中项
        if (newValue != SelectedPortSingle)
        {
            var portInfo = PortList.FirstOrDefault(p => p.PortName == newValue);
            if (portInfo != null)
            {
                ComListview.SelectedItem = portInfo;
            }
            else
            {
                ComListview.SelectedItem = null;
            }
        }
    }

    public string SelectedPortSingle => SelectedPorts.FirstOrDefault();

    public event PropertyChangedEventHandler PropertyChanged;
    private void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private readonly HashSet<string> _lastPortNames = new();
    private readonly Dictionary<string, string> _lastPortDescriptions = new Dictionary<string, string>();
    private List<string> _lastPortSnapshot = new();


    private DispatcherTimer? _scrollTimer;
    private DispatcherTimer? _pauseTimer;
    private ScrollViewer? _activeScrollViewer;
    private const double _scrollStep = 1.0;
    private bool _scrollingRight = true;
    private bool _isPaused = false;

    private ManagementEventWatcher? _deviceWatcher;
    private CancellationTokenSource _watcherCancelToken = new();
    private bool _isInitializing = true;

    public SerialPortLIstBox()
    {
        InitializeComponent();

        AutoSearchSetting = true;

        StartDeviceWatcher();

        Task.Run(() =>
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                _ = RefreshSearchPortsAsync().ContinueWith(_ =>
                {
                    // 首次刷新完成后，标记初始化结束
                    DispatcherQueue.TryEnqueue(() => _isInitializing = false);
                });
            });
        });
        ClearSeledItemButton.IsEnabled = ComListview.SelectedItem != null;
        AutoConnectButton.IsEnabled = ComListview.SelectedItem == null;
    }


    private void ClearComCombobox_Click(object sender, RoutedEventArgs e)
    {
        ComListview.SelectedItem = null;
    }

    private void ComListview_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        SelectedPorts.Clear();
        foreach (var item in ComListview.SelectedItems)
        {
            if (item is SerialPortInfo port)
                SelectedPorts.Add(port.PortName);
        }
        OnPropertyChanged(nameof(SelectedPortSingle));
        ClearSeledItemButton.IsEnabled = ComListview.SelectedItem != null;
        AutoConnectButton.IsEnabled = ComListview.SelectedItem == null; 
        SelectedPort = SelectedPortSingle;
    }


    public event EventHandler<SerialPortEventArgs>? PortInserted;
    public event EventHandler<SerialPortEventArgs>? PortRemoved;

    private void StartDeviceWatcher()
    {
        try
        {
            var query = new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 2 OR EventType = 3");
            _deviceWatcher = new ManagementEventWatcher(query);

            _deviceWatcher.EventArrived += (s, e) =>
            {
                Task.Run(() =>
                {
                    var currentSnapshot = SerialPortInfo.GetAvailablePortNames();

                    // 比较快照是否有变化
                    bool hasChanged = !_lastPortSnapshot.SequenceEqual(currentSnapshot);
                    if (!hasChanged)
                    {
                        Debug.WriteLine("端口列表无变化，跳过刷新");
                        return;
                    }

                    _lastPortSnapshot = currentSnapshot;

                    DispatcherQueue.TryEnqueue(() =>
                    {
                        _ = RefreshSearchPortsAsync();
                    });

                    Debug.WriteLine("设备变更事件触发，刷新端口列表");
                });
            };
            _deviceWatcher.Start();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"设备监听启动失败: {ex.Message}");
        }
    }

    public void DisposeWatcher()
    {
        _deviceWatcher?.Stop();
        _deviceWatcher?.Dispose();
        _watcherCancelToken.Cancel();
    }

    private async Task RefreshSearchPortsAsync()
    {
        var previouslySelected = new HashSet<string>(SelectedPorts);

        // 刷新端口信息
        await SerialPortInfo.RefreshPortInfoAsync();

        var currentPorts = SerialPortInfo.GetAllPorts();
        var currentPortNames = new HashSet<string>(currentPorts.Keys);

        var inserted = currentPortNames.Except(_lastPortNames).ToList();
        var removed = _lastPortNames.Except(currentPortNames).ToList();

        // 触发插入和移除事件，并更新描述缓存
        if (!_isInitializing)
        {
            foreach (var port in inserted)
            {
                var portInfo = currentPorts[port];
                // 更新缓存
                _lastPortDescriptions[port] = portInfo.PortDeviceDescription;
                PortInserted?.Invoke(this, new SerialPortEventArgs(port, portInfo.PortDeviceDescription));
            }
            foreach (var port in removed)
            {
                // 从缓存获取描述，默认为 "Unknown"
                string description = _lastPortDescriptions.TryGetValue(port, out var cachedDescription)
                    ? cachedDescription
                    : "Unknown";
                PortRemoved?.Invoke(this, new SerialPortEventArgs(port, description));
                // 可选：移除缓存条目以避免内存累积
                _lastPortDescriptions.Remove(port);
            }
        }

        // 重建 UI 列表
        var newPortList = currentPorts.Values
            .Select(info =>
            {
                info.NewPortTag = inserted.Contains(info.PortName)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
                return info;
            })
            .OrderBy(GetSortOrder)
            .ToList();

        PortList.Clear();
        PortList.AddRange(newPortList);

        // 更新状态
        _lastPortNames.Clear();
        _lastPortNames.UnionWith(currentPortNames);

        // 如果当前没有选中任何端口，且有新插入的端口，且不是初始化阶段，自动选中第一个新端口
        if (!_isInitializing && !previouslySelected.Any() && inserted.Any())
        {
            var firstNewPort = inserted.OrderBy(port => GetSortOrder(currentPorts[port])).FirstOrDefault();
            if (firstNewPort != null)
            {
                SelectedPorts.Add(firstNewPort);
            }
        }

        // 恢复选中状态（仅当有先前选中项时）
        if (previouslySelected.Any())
        {
            SelectedPorts.Clear();
            var portsToSelect = previouslySelected.Where(currentPortNames.Contains).ToList();
            foreach (var port in portsToSelect)
                SelectedPorts.Add(port);
        }

        // 同步 UI 选中状态
        DispatcherQueue.TryEnqueue(() =>
        {
            try
            {
                // 清空当前选择（适用于所有 SelectionMode）
                ComListview.SelectedIndex = -1;

                if (SelectionMode == ListViewSelectionMode.Single)
                {
                    // 单选模式：只选择 SelectedPorts 中的第一个有效端口
                    var firstPort = SelectedPorts
                        .Select(port => PortList.FirstOrDefault(p => p.PortName == port))
                        .Where(p => p != null)
                        .OrderBy(GetSortOrder)
                        .FirstOrDefault();

                    if (firstPort != null)
                    {
                        ComListview.SelectedItem = firstPort;
                        Debug.WriteLine($"Single mode: Selected port {firstPort.PortName}");
                    }
                }
                else // Extended 或 Multiple 模式
                {
                    // 多选模式：选择所有 SelectedPorts 中的端口
                    var itemsToSelect = SelectedPorts
                        .Select(port => PortList.FirstOrDefault(p => p.PortName == port))
                        .Where(p => p != null)
                        .ToList();

                    foreach (var portInfo in itemsToSelect)
                    {
                        if (!ComListview.SelectedItems.Contains(portInfo))
                        {
                            ComListview.SelectedItems.Add(portInfo);
                            Debug.WriteLine($"Extended/Multiple mode: Selected port {portInfo.PortName}");
                        }
                    }
                }

                OnPropertyChanged(nameof(SelectedPortSingle));
                ClearSeledItemButton.IsEnabled = ComListview.SelectedItem != null;
                AutoConnectButton.IsEnabled = ComListview.SelectedItem == null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating ComListview selection: {ex.Message}, StackTrace: {ex.StackTrace}");
            }
        }); 
        _lastPortSnapshot = SerialPortInfo.GetAvailablePortNames();
    }

    private static int GetSortOrder(SerialPortInfo port)
    {
        var name = port.PortName.AsSpan();

        if (name.StartsWith("COM"))
        {
            if (int.TryParse(name.Slice(3), out int comNum))
                return comNum;
        }
        else if (name.StartsWith("CNC"))
        {
            int num = 0;
            bool hasDigit = false;
            foreach (char c in name.Slice(3))
            {
                if (c is >= '0' and <= '9')
                {
                    hasDigit = true;
                    num = num * 10 + (c - '0');
                }
                else
                {
                    break;
                }
            }
            if (hasDigit)
                return 1000 + num;
        }
        return int.MaxValue;
    }
    private void SortPortList()
    {
        var sorted = PortList.OrderBy(GetSortOrder).ToArray();
        PortList.Clear();
        foreach (var item in sorted)
            PortList.Add(item);
    }

    // ========== 滚动动画 ==========
    private void PortItemGrid_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is Grid grid)
        {
            var scrollViewer = FindChild<ScrollViewer>(grid, "PortDeviceDescriptionScrollViewer");
            grid.Tag = scrollViewer;
        }
    }

    private void PortItemGrid_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if (sender is Grid grid && grid.Tag is ScrollViewer scrollViewer && scrollViewer.ScrollableWidth > 0)
        {
            _activeScrollViewer = scrollViewer;
            _scrollingRight = true;
            _isPaused = false;
            _scrollTimer?.Stop();
            _scrollTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(20) };
            _scrollTimer.Tick += ScrollTimer_Tick;
            _scrollTimer.Start();
        }
    }

    private void ScrollTimer_Tick(object? sender, object e)
    {
        if (_isPaused || _activeScrollViewer == null) return;

        if (_scrollingRight)
        {
            double next = _activeScrollViewer.HorizontalOffset + _scrollStep;
            if (next >= _activeScrollViewer.ScrollableWidth)
            {
                _activeScrollViewer.ChangeView(_activeScrollViewer.ScrollableWidth, null, null);
                StartPauseTimer(100);
                _scrollingRight = false;
            }
            else
            {
                _activeScrollViewer.ChangeView(next, null, null);
            }
        }
        else
        {
            double next = _activeScrollViewer.HorizontalOffset - _scrollStep;
            if (next <= 0)
            {
                _activeScrollViewer.ChangeView(0, null, null);
                StartPauseTimer(300);
                _scrollingRight = true;
            }
            else
            {
                _activeScrollViewer.ChangeView(next, null, null);
            }
        }
    }

    private void StartPauseTimer(int milliseconds)
    {
        _isPaused = true;
        _scrollTimer?.Stop();

        _pauseTimer?.Stop();
        _pauseTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(milliseconds) };
        _pauseTimer.Tick += (s, e) =>
        {
            _isPaused = false;
            _pauseTimer?.Stop();
            _scrollTimer?.Start();
        };
        _pauseTimer.Start();
    }

    private void PortItemGrid_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        _scrollTimer?.Stop();
        _pauseTimer?.Stop();
        if (_activeScrollViewer != null)
        {
            _activeScrollViewer.ChangeView(0, null, null);
            _activeScrollViewer = null;
        }
    }

    private void PortDeviceDescriptionScrollViewer_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
    {
        e.Handled = true;
    }

    // ========== 工具方法 ==========
    public static T? FindChild<T>(DependencyObject parent, string childName) where T : DependencyObject
    {
        if (parent == null) return null;
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is FrameworkElement fe && fe.Name == childName)
                return child as T;
            var result = FindChild<T>(child, childName);
            if (result != null) return result;
        }
        return null;
    }
}