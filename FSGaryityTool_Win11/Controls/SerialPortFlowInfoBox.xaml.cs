using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
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
        private double _canvasHeight;
        private double _canvasWidth;
        private int LogicAnalyzerBoxTimems = 6;

        private PointCollection _oscilloscopePoints = new PointCollection();
        public PointCollection OscilloscopePoints
        {
            get => _oscilloscopePoints;
            set
            {
                if (_oscilloscopePoints != value)
                {
                    _oscilloscopePoints = value;
                    OnPropertyChanged(nameof(OscilloscopePoints));
                }
            }
        }

        private PointCollection _oscilloscopePolygonPoints = new PointCollection();
        public PointCollection OscilloscopePolygonPoints
        {
            get => _oscilloscopePolygonPoints;
            set
            {
                if (_oscilloscopePolygonPoints != value)
                {
                    _oscilloscopePolygonPoints = value;
                    OnPropertyChanged(nameof(OscilloscopePolygonPoints));
                }
            }
        }

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
            set => SetValue(LogicAnalyzerBoxMaxTimeProperty, value);
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
            InfoNameTextBlock.DataContext = this;
            _logicalValues = new Queue<(bool, DateTime)>();

            this.ActualThemeChanged += OnActualThemeChanged;

            UpdateMaxValues();
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(LogicAnalyzerBoxTimems)
            };
            _timer.Tick += Timer_Tick;
            OscilloscopeCanvas.SizeChanged += OscilloscopeCanvas_SizeChanged;
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
            // 添加新值
            _logicalValues.Enqueue((newValue ?? LogicalValue, now));

            RemoveExpiredLogicalValues(now);
            RedrawCanvas(now);
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

        private void OscilloscopeCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _canvasHeight = e.NewSize.Height;
            _canvasWidth = e.NewSize.Width;
            DateTime now = DateTime.Now;
            RedrawCanvas(now);
        }

        private double startPront = 0;//_canvasWidth
        private double strokeThickness = 2;


        private void RedrawCanvas(DateTime now)
        {
            double totalTimeSpan = LogicAnalyzerBoxMaxTime * 1000; // 转换为毫秒

            Task.Run(() =>
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    if (_logicalValues.Count == 0)
                    {

                        OscilloscopePoints?.Clear();
                        OscilloscopePolygonPoints?.Clear();
                        return;
                    }
                    RemoveExpiredLogicalValues(now);
                    if (_logicalValues.Count == 0)
                    {
                        return;
                    }
                });

                var logicalValuesSnapshot = _logicalValues.ToArray();

                DispatcherQueue.TryEnqueue(() =>
                {
                    OscilloscopePoints.Clear();
                    OscilloscopePolygonPoints.Clear();
                    OscilloscopePolygonPoints.Add(new Windows.Foundation.Point(_canvasWidth, _canvasHeight));
                    OscilloscopePolygonPoints.Add(new Windows.Foundation.Point(0, _canvasHeight));
                });

                foreach (var value in logicalValuesSnapshot)
                {
                    double elapsedTime = (now - value.Timestamp).TotalMilliseconds;
                    double x = _canvasWidth - (elapsedTime / totalTimeSpan * _canvasWidth);
                    double y = value.Value ? 0 : _canvasHeight;

                    if (x >= 0 && x <= _canvasWidth)
                    {
                        DispatcherQueue.TryEnqueue(() =>
                        {
                            OscilloscopePoints.Add(new Windows.Foundation.Point(x, y));
                            OscilloscopePolygonPoints.Add(new Windows.Foundation.Point(x, y));
                        });
                    }
                }
            });
        }
        
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
        }
    }
}
