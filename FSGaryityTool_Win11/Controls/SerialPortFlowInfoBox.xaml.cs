using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;

namespace FSGaryityTool_Win11.Controls
{
    public enum SerialPortInfoName
    {
        DCD,
        RI,
        DSR,
        CTS,
        DTR,
        RTS
    }

    public enum SerialPortFlowInfoLogicAnalyzerBoxHorizontal
    {
        Left,
        Right
    }

    public sealed partial class SerialPortFlowInfoBox : UserControl, INotifyPropertyChanged
    {
        // 使用 CompositionTarget.Rendering 替代 DispatcherTimer
        private bool _isRenderingActive = false;
        private System.Diagnostics.Stopwatch _stopwatch = new System.Diagnostics.Stopwatch();
        private Queue<(bool Value, double TimestampMs)> _logicalValues;
        private Queue<SerialPortStateSample> _dataCache = new Queue<SerialPortStateSample>();

        // 串口状态采样结构
        private class SerialPortStateSample
        {
            public bool Value { get; set; }
            public double TimestampMs { get; set; } // 毫秒
        }

        private double _waveViewWidth;
        public double WaveViewWidth
        {
            get => _waveViewWidth;
            set
            {
                if (_waveViewWidth != value)
                {
                    _waveViewWidth = value;
                    OnPropertyChanged(nameof(WaveViewWidth));
                }
            }
        }

        public ObservableCollection<WaveformDataSource> WaveformSources { get; private set; }
        // 缓存并重用的点集合，避免每帧分配
        private ObservableCollection<(float x, float y)> _polylinePoints;

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


        public SerialPortInfoName InfoName
        {
            get { return (SerialPortInfoName)GetValue(InfoNameProperty); }
            set { SetValue(InfoNameProperty, value); }
        }

        public SerialPortFlowInfoLogicAnalyzerBoxHorizontal LogicAnalyzerBoxHorizontal
        {
            get { return (SerialPortFlowInfoLogicAnalyzerBoxHorizontal)GetValue(LogicAnalyzerBoxHorizontalProperty); }
            set { SetValue(LogicAnalyzerBoxHorizontalProperty, value); }
        }

        public static readonly DependencyProperty InfoNameProperty =
            DependencyProperty.Register("InfoName", typeof(SerialPortInfoName), typeof(SerialPortFlowInfoBox), new PropertyMetadata(SerialPortInfoName.DCD, OnInfoNameChanged));

        public static readonly DependencyProperty LogicAnalyzerBoxHorizontalProperty =
            DependencyProperty.Register("LogicAnalyzerBoxHorizontal", typeof(SerialPortFlowInfoLogicAnalyzerBoxHorizontal), typeof(SerialPortFlowInfoBox), new PropertyMetadata(SerialPortFlowInfoLogicAnalyzerBoxHorizontal.Left, OnLogicAnalyzerBoxHorizontalChanged));

        private static void OnInfoNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (SerialPortFlowInfoBox)d;
            control.InfoNameTextBlock.Text = e.NewValue.ToString();
        }

        private static void OnLogicAnalyzerBoxHorizontalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (SerialPortFlowInfoBox)d;
            if ((SerialPortFlowInfoLogicAnalyzerBoxHorizontal)e.NewValue == SerialPortFlowInfoLogicAnalyzerBoxHorizontal.Left)
            {
                //control.InfoBorder.HorizontalAlignment = HorizontalAlignment.Right;
            }
            else
            {
                //control.InfoBorder.HorizontalAlignment = HorizontalAlignment.Left;
            }
        }

        public static readonly DependencyProperty LogicAnalyzerBoxMaxTimeProperty =
            DependencyProperty.Register("LogicAnalyzerBoxMaxTime", typeof(int), typeof(SerialPortFlowInfoBox), new PropertyMetadata(0, OnLogicAnalyzerBoxMaxTimeChanged));

        public static readonly DependencyProperty LogicalValueProperty =
            DependencyProperty.Register("LogicalValue", typeof(bool), typeof(SerialPortFlowInfoBox), new PropertyMetadata(false, OnLogicalValueChanged));

        public static readonly DependencyProperty LogicAnalyzerProperty =
            DependencyProperty.Register("LogicAnalyzer", typeof(bool), typeof(SerialPortFlowInfoBox), new PropertyMetadata(false, OnLogicAnalyzerChanged));

        public int LogicAnalyzerBoxMaxTime
        {
            get => (int)GetValue(LogicAnalyzerBoxMaxTimeProperty);
            set
            {
                SetValue(LogicAnalyzerBoxMaxTimeProperty, value);
                WaveViewWidth = LogicAnalyzerBoxMaxTime * 1000.0;
            }
        }

        public bool LogicalValue
        {
            get => (bool)GetValue(LogicalValueProperty);
            set => SetValue(LogicalValueProperty, value);
        }

        public bool LogicAnalyzer
        {
            get => (bool)GetValue(LogicAnalyzerProperty);
            set => SetValue(LogicAnalyzerProperty, value);
        }
        private void UserControl_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "PointerEntered", true);
        }

        private void UserControl_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "PointerExited", true);
        }

        public SerialPortFlowInfoBox()
        {
            this.InitializeComponent();
            _logicalValues = new Queue<(bool, double)>();

            WaveformSources = new ObservableCollection<WaveformDataSource>
            {
                new WaveformDataSource
                {
                    Name = "SerialFlow",
                    StrokeBrush = (Brush)Application.Current.Resources["AccentFillColorSecondaryBrush"],
                    StrokeThickness = 2f
                }
            };

            // 初始化并重用点集合
            _polylinePoints = new ObservableCollection<(float x, float y)>();
            WaveformSources[0].PolylinePointsData = _polylinePoints;

            this.ActualThemeChanged += OnActualThemeChanged;
            WaveViewWidth = LogicAnalyzerBoxMaxTime * 1000.0;

            this.Loaded += SerialPortFlowInfoBox_Loaded;
            this.Unloaded += SerialPortFlowInfoBox_Unloaded;
        }

        private void SerialPortFlowInfoBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (!_isRenderingActive)
            {
                _stopwatch.Restart();
                Microsoft.UI.Xaml.Media.CompositionTarget.Rendering += CompositionTarget_Rendering;
                _isRenderingActive = true;
                // 初始化时确保至少有一个采样点，避免无图形渲染
                if (_logicalValues.Count == 0)
                {
                    double timestampMs = _stopwatch.IsRunning ? _stopwatch.Elapsed.TotalMilliseconds : 0;
                    _logicalValues.Enqueue((LogicalValue, timestampMs));
                    UpdateOscilloscope();
                }
            }
        }

        private void SerialPortFlowInfoBox_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_isRenderingActive)
            {
                Microsoft.UI.Xaml.Media.CompositionTarget.Rendering -= CompositionTarget_Rendering;
                _isRenderingActive = false;
                _stopwatch.Stop();
            }
        }

        private void CompositionTarget_Rendering(object sender, object e)
        {
            // 每次 CompositionTarget.Rendering 触发都刷新，自动适应屏幕刷新率
            if (LogicAnalyzer)
            {
                // 批量处理数据缓存
                while (_dataCache.Count > 0)
                {
                    var sample = _dataCache.Dequeue();
                    _logicalValues.Enqueue((sample.Value, sample.TimestampMs));
                }
                UpdateOscilloscope();
            }
        }


        private static void OnLogicAnalyzerBoxMaxTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as SerialPortFlowInfoBox;
            control.UpdateMaxValues();
        }

        private static void OnLogicalValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as SerialPortFlowInfoBox;
            bool newValue = (bool)e.NewValue;
            control.FsBorderIsChecked(newValue ? 1 : 0, control.InfoBorder, control.InfoNameTextBlock);
            control.AddSerialPortStateSample(newValue); // 只采集数据，绘制交给渲染事件
        }

        private static void OnLogicAnalyzerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as SerialPortFlowInfoBox;
            bool newValue = (bool)e.NewValue;
            control.UpdateLogicAnalyzer(newValue);
        }

        private void UpdateMaxValues()
        {
            double nowMs = _stopwatch.IsRunning ? _stopwatch.Elapsed.TotalMilliseconds : 0;
            RemoveExpiredLogicalValues(nowMs);
        }

        private void UpdateOscilloscope(bool? newValue = null)
        {
            // 使用高精度计时器，绘制时统一处理队列
            double nowMs = _stopwatch.IsRunning ? _stopwatch.Elapsed.TotalMilliseconds : 0;
            RemoveExpiredLogicalValues(nowMs);
            double totalTimeSpan = LogicAnalyzerBoxMaxTime * 1000.0; // 毫秒范围

            // 将队列转换为数组便于索引
            var samples = _logicalValues.ToArray();

            // 生成阶梯波点（从左到右）：先在 x=0 处填充当前最早的值
            var newPoints = new List<(float x, float y)>();
            if (samples.Length == 0)
            {
                // 没有采样则清空点
                _polylinePoints.Clear();
                return;
            }

            float valueToY(bool v) => v ? 1f : 0f;

            // 左端使用最早样本的值
            bool prevValue = samples[0].Value;
            newPoints.Add((0f, valueToY(prevValue)));

            for (int i = 0; i < samples.Length; i++)
            {
                var s = samples[i];
                double elapsed = nowMs - s.TimestampMs;
                float x = (float)(totalTimeSpan - elapsed);

                // 横到达此时间点，保持之前的值
                newPoints.Add((x, valueToY(prevValue)));
                // 在此时间点发生跳变（如果有变化）
                if (s.Value != prevValue)
                {
                    newPoints.Add((x, valueToY(s.Value)));
                    prevValue = s.Value;
                }
            }

            // 将当前值一直拉伸到右边缘
            newPoints.Add(((float)totalTimeSpan, valueToY(prevValue)));

            // 同步更新到重用集合，避免每帧分配
            int iExisting = 0;
            for (; iExisting < newPoints.Count && iExisting < _polylinePoints.Count; iExisting++)
            {
                _polylinePoints[iExisting] = newPoints[iExisting];
            }
            // 添加额外的点
            for (int j = iExisting; j < newPoints.Count; j++)
            {
                _polylinePoints.Add(newPoints[j]);
            }
            // 移除多余的点
            while (_polylinePoints.Count > newPoints.Count)
            {
                _polylinePoints.RemoveAt(_polylinePoints.Count - 1);
            }
        }

        private void RemoveExpiredLogicalValues(double nowMs)
        {
            // 重载为高精度毫秒
            double cutoffMs = nowMs - LogicAnalyzerBoxMaxTime * 1000.0;
            while (_logicalValues.Count > 1)
            {
                var first = _logicalValues.Peek();
                var second = _logicalValues.ToArray()[1];
                if (second.TimestampMs < cutoffMs)
                {
                    _logicalValues.Dequeue();
                }
                else
                {
                    break;
                }
            }
        }

        // 采集数据时调用，入队缓存
        private void AddSerialPortStateSample(bool value)
        {
            double timestampMs = _stopwatch.IsRunning ? _stopwatch.Elapsed.TotalMilliseconds : 0;
            _dataCache.Enqueue(new SerialPortStateSample { Value = value, TimestampMs = timestampMs });
        }

        private void UpdateLogicAnalyzer(bool isEnabled)
        {
            // CompositionTarget.Rendering 由 Loaded/Unloaded 控制，无需在此启动/停止
        }

        private void Timer_Tick(object sender, object e)
        {
            // 已被 CompositionTarget_Rendering 替代，无需实现
        }

        private double startPront = 0;//_canvasWidth
        private double strokeThickness = 2;

        
        
        private void FsBorderIsChecked(int isChecked, Border border, TextBlock textBlock)
        {
            SolidColorBrush foregroundColor = (SolidColorBrush)Application.Current.Resources["TextFillColorPrimaryBrush"];
            SolidColorBrush foreCheckColor = (SolidColorBrush)Application.Current.Resources["TextOnAccentFillColorPrimaryBrush"];

            SolidColorBrush backgroundColor = (SolidColorBrush)Application.Current.Resources["CardBackgroundFillColorDefaultBrush"];
            SolidColorBrush backCheckColor = (SolidColorBrush)Application.Current.Resources["AccentFillColorDefaultBrush"];

            border.Background = isChecked == 1 ? backCheckColor : backgroundColor;
            textBlock.Foreground = isChecked == 1 ? foreCheckColor : foregroundColor;
        }

        private void OnActualThemeChanged(FrameworkElement sender, object args)
        {
            FsBorderIsChecked(LogicalValue ? 1 : 0, InfoBorder, InfoNameTextBlock);
        }

        private void ClearSerialInfoButton_Click(object sender, RoutedEventArgs e)
        {
            _logicalValues.Clear();
            _dataCache.Clear();
            // 在清空后添加一个初始点以保证控件有可渲染的数据
            double timestampMs = _stopwatch.IsRunning ? _stopwatch.Elapsed.TotalMilliseconds : 0;
            bool initial = LogicalValue; // 使用当前逻辑值
            _logicalValues.Enqueue((initial, timestampMs));
            _polylinePoints.Clear();
            UpdateOscilloscope();
        }
    }
}
