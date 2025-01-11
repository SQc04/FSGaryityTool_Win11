using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;

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

    public sealed partial class SerialPortFlowInfoBox : UserControl
    {
        private DispatcherTimer _timer;
        private List<(bool Value, DateTime Timestamp)> _logicalValues;
        private double _canvasHeight;
        private double _canvasWidth;
        private int LogicAnalyzerBoxTimems = 200;

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
            if((SerialPortFlowInfoLogicAnalyzerBoxHorizontal)e.NewValue == SerialPortFlowInfoLogicAnalyzerBoxHorizontal.Left)
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

        public SerialPortFlowInfoBox()
        {
            this.InitializeComponent();
            InfoNameTextBlock.DataContext = this;
            _logicalValues = new List<(bool, DateTime)>();
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
            control.UpdateOscilloscope(newValue); // 立即更新图形
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
            DateTime cutoffTime = now.AddSeconds(-LogicAnalyzerBoxMaxTime);

            // 清除过期的逻辑值
            _logicalValues.RemoveAll(value => value.Timestamp < cutoffTime);
        }

        private void UpdateOscilloscope(bool? newValue = null)
        {
            DateTime now = DateTime.Now;
            if (_logicalValues.Count > 0)
            {
                // 添加当前值
                _logicalValues.Add((newValue ?? _logicalValues[^1].Value, now));
            }
            else
            {
                // 添加初始值
                _logicalValues.Add((LogicalValue, now));
            }
            RedrawCanvas();
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
            RedrawCanvas();
        }

        private double startPront = 0;//_canvasWidth
        private double strokeThickness = 2;

        private void RedrawCanvas()
        {
            OscilloscopeCanvas.Children.Clear();
            if (_logicalValues.Count == 0)
            {
                return;
            }

            var path = new Path
            {
                Stroke = (Brush)Application.Current.Resources["AccentTextFillColorTertiaryBrush"],
                StrokeThickness = strokeThickness
            };
            var geometry = new PathGeometry();
            var figure = new PathFigure { StartPoint = new Windows.Foundation.Point(_canvasWidth, _logicalValues[0].Value ? 0 : _canvasHeight) };

            DateTime startTime = _logicalValues[0].Timestamp;
            for (int i = 1; i < _logicalValues.Count; i++)
            {
                double elapsedTime = (_logicalValues[i].Timestamp - startTime).TotalMilliseconds;
                double x = _canvasWidth - (elapsedTime / (LogicAnalyzerBoxMaxTime * 1000) * _canvasWidth);
                double y = _logicalValues[i].Value ? 0 : _canvasHeight;
                figure.Segments.Add(new LineSegment { Point = new Windows.Foundation.Point(x, y) });
            }

            geometry.Figures.Add(figure);
            path.Data = geometry;
            OscilloscopeCanvas.Children.Add(path);
        }

        private void FsBorderIsChecked(int isChecked, Border border, TextBlock textBlock)
        {
            SolidColorBrush foregroundColor = (SolidColorBrush)Application.Current.Resources["TextFillColorPrimaryBrush"];
            SolidColorBrush foreCheckColor = (SolidColorBrush)Application.Current.Resources["TextOnAccentFillColorPrimaryBrush"];

            SolidColorBrush backgroundColor = (SolidColorBrush)Application.Current.Resources["CardBackgroundFillColorDefaultBrush"];
            SolidColorBrush backCheckColor = (SolidColorBrush)Application.Current.Resources["SystemFillColorAttentionBrush"];

            border.Background = isChecked == 1 ? backCheckColor : backgroundColor;
            textBlock.Foreground = isChecked == 1 ? foreCheckColor : foregroundColor;
        }
    }
}
