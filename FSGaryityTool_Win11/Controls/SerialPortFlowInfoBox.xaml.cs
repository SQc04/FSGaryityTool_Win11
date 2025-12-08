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
        private DispatcherTimer _timer;
        private Queue<(bool Value, DateTime Timestamp)> _logicalValues;
        private int LogicAnalyzerBoxTimems = 1;

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
            _logicalValues = new Queue<(bool, DateTime)>();

            WaveformSources = new ObservableCollection<WaveformDataSource>
            {
                new WaveformDataSource
                {
                    Name = "SerialFlow",
                    StrokeBrush = (Brush)Application.Current.Resources["AccentFillColorSecondaryBrush"],
                    StrokeThickness = 2f
                }
            };

            this.ActualThemeChanged += OnActualThemeChanged;
            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(LogicAnalyzerBoxTimems) };
            _timer.Tick += Timer_Tick;
            WaveViewWidth = LogicAnalyzerBoxMaxTime * 1000.0;
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
            control.UpdateOscilloscope(newValue); // 更新示波器图像
        }

        private static void OnLogicAnalyzerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as SerialPortFlowInfoBox;
            bool newValue = (bool)e.NewValue;
            control.UpdateLogicAnalyzer(newValue);
        }

        private void UpdateMaxValues()
        {
            DateTime now = DateTime.Now;
            RemoveExpiredLogicalValues(now);
        }

        private void UpdateOscilloscope(bool? newValue = null)
        {
            DateTime now = DateTime.Now;
            _logicalValues.Enqueue((newValue ?? LogicalValue, now));
            RemoveExpiredLogicalValues(now);

            var points = new ObservableCollection<(float x, float y)>();
            double totalTimeSpan = LogicAnalyzerBoxMaxTime * 1000.0; // 毫秒范围

            foreach (var value in _logicalValues)
            {
                double elapsedTime = (now - value.Timestamp).TotalMilliseconds;
                // 横坐标直接用毫秒值（0 ~ totalTimeSpan）
                float x = (float)(totalTimeSpan - elapsedTime);
                // 纵坐标归一化到 0/1
                float y = value.Value ? 1f : 0f;
                points.Add((x, y));
            }

            WaveformSources[0].PolylinePointsData = points;
        }

        private void RemoveExpiredLogicalValues(DateTime now)
        {
            DateTime cutoffTime = now.AddSeconds(-LogicAnalyzerBoxMaxTime);

            // 只要队列中有两个及以上点，并且第2个点也超时，才移除第1个点
            while (_logicalValues.Count > 1)
            {
                var first = _logicalValues.Peek();
                var second = _logicalValues.ToArray()[1];
                if (second.Timestamp < cutoffTime)
                {
                    _logicalValues.Dequeue();
                }
                else
                {
                    break;
                }
            }
        }

        private void UpdateLogicAnalyzer(bool isEnabled)
        {
            if (isEnabled)
            {
                _timer.Start();
            }
            else
            {
                _timer.Stop();
            }
        }

        private void Timer_Tick(object sender, object e)
        {
            UpdateOscilloscope();
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
            WaveformSources[0].PolylinePointsData = new ObservableCollection<(float x, float y)>();
        }
    }
}
