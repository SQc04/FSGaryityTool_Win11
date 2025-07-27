using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using System.ComponentModel;
using Microsoft.UI;
using Microsoft.UI.Xaml.Shapes;
using static System.Net.Mime.MediaTypeNames;

namespace FSGaryityTool_Win11.Controls
{
    public sealed partial class DataPlotBox : UserControl, INotifyPropertyChanged
    {
        public enum DataPlotBoxDisplayMode
        {
            DataVisualization,
            FrequencySpectrumVisualization,
            MelFilterBankVisualization,
            GammatoneFilterBankVisualization,
            CochlearFilterBankVisualization // 绘制
        }
        public DataPlotBox()
        {
            this.InitializeComponent();
            this.DataPlotCanvas.SizeChanged += DataPlotCanvas_SizeChanged;
        }

        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register("Data", typeof(object), typeof(DataPlotBox), new PropertyMetadata(null, OnDataChanged));

        public static readonly DependencyProperty DisplayModeProperty =
            DependencyProperty.Register("DisplayMode", typeof(DataPlotBoxDisplayMode), typeof(DataPlotBox), new PropertyMetadata(DataPlotBoxDisplayMode.DataVisualization, OnDisplayModeChanged));

        public static readonly DependencyProperty XAxisLabelProperty =
            DependencyProperty.Register("XAxisLabel", typeof(string), typeof(DataPlotBox), new PropertyMetadata("X"));

        public static readonly DependencyProperty YAxisLabelProperty =
            DependencyProperty.Register("YAxisLabel", typeof(string), typeof(DataPlotBox), new PropertyMetadata("Y"));

        public static readonly DependencyProperty XAxisTicksProperty =
            DependencyProperty.Register("XAxisTicks", typeof(int), typeof(DataPlotBox), new PropertyMetadata(10, OnXAxisTicksChanged));

        public static readonly DependencyProperty YAxisTicksProperty =
            DependencyProperty.Register("YAxisTicks", typeof(int), typeof(DataPlotBox), new PropertyMetadata(10, OnYAxisTicksChanged));

        public static readonly DependencyProperty SampleRateProperty =
            DependencyProperty.Register("SampleRate", typeof(int), typeof(DataPlotBox), new PropertyMetadata(16000));

        public static readonly DependencyProperty NumFiltersProperty =
            DependencyProperty.Register("NumFilters", typeof(int), typeof(DataPlotBox), new PropertyMetadata(26));

        public static readonly DependencyProperty FftSizeProperty =
            DependencyProperty.Register("FftSize", typeof(int), typeof(DataPlotBox), new PropertyMetadata(512));

        private object _data;
        public object Data
        {
            get => _data;
            set
            {
                if (_data != value)
                {
                    _data = value;
                    OnPropertyChanged(nameof(Data));
                    PlotData();
                }
            }
        }

        private DataPlotBoxDisplayMode _displayMode;
        public DataPlotBoxDisplayMode DisplayMode
        {
            get => _displayMode;
            set
            {
                if (_displayMode != value)
                {
                    _displayMode = value;
                    OnPropertyChanged(nameof(DisplayMode));
                    PlotData();
                }
            }
        }

        private string _xAxisLabel;
        public string XAxisLabel
        {
            get => _xAxisLabel;
            set
            {
                if (_xAxisLabel != value)
                {
                    _xAxisLabel = value;
                    OnPropertyChanged(nameof(XAxisLabel));
                }
            }
        }

        private string _yAxisLabel;
        public string YAxisLabel
        {
            get => _yAxisLabel;
            set
            {
                if (_yAxisLabel != value)
                {
                    _yAxisLabel = value;
                    OnPropertyChanged(nameof(YAxisLabel));
                }
            }
        }

        public int XAxisTicks
        {
            get => (int)GetValue(XAxisTicksProperty);
            set => SetValue(XAxisTicksProperty, value);
        }

        public int YAxisTicks
        {
            get => (int)GetValue(YAxisTicksProperty);
            set => SetValue(YAxisTicksProperty, value);
        }

        public int SampleRate
        {
            get => (int)GetValue(SampleRateProperty);
            set => SetValue(SampleRateProperty, value);
        }

        public int NumFilters
        {
            get => (int)GetValue(NumFiltersProperty);
            set => SetValue(NumFiltersProperty, value);
        }

        public int FftSize
        {
            get => (int)GetValue(FftSizeProperty);
            set => SetValue(FftSizeProperty, value);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private static void OnDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as DataPlotBox;
            if (control != null)
            {
                control.Data = e.NewValue;
            }
        }

        private static void OnDisplayModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as DataPlotBox;
            if (control != null)
            {
                control.DisplayMode = (DataPlotBoxDisplayMode)e.NewValue;
            }
        }

        private static void OnXAxisTicksChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as DataPlotBox;
            if (control != null)
            {
                control.PlotData();
            }
        }

        private static void OnYAxisTicksChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as DataPlotBox;
            if (control != null)
            {
                control.PlotData();
            }
        }

        private void DataPlotCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            PlotData();
            SetDataPlotXtextMargin();
            SetDataPlotYtextMargin();
        }

        private void PlotData()
        {
            if (Data == null || DataPlotCanvas.ActualWidth == 0 || DataPlotCanvas.ActualHeight == 0)
                return;

            DataPlotCanvas.Children.Clear();

            if (DisplayMode == DataPlotBoxDisplayMode.DataVisualization)
            {
                DrawDataVisualization();
                DrawAxes();
            }
            else if (DisplayMode == DataPlotBoxDisplayMode.FrequencySpectrumVisualization && Data is double[][])
            {
                DrawFrequencySpectrumVisualization();
                DrawAxes();
            }
            else if (DisplayMode == DataPlotBoxDisplayMode.MelFilterBankVisualization && Data is List<double[]>)
            {
                DrawMelFilterBankResponse();
                DrawAxes();
            }
            else if (DisplayMode == DataPlotBoxDisplayMode.GammatoneFilterBankVisualization && Data is List<double[]>)
            {
                DrawGammatoneFilterBankResponse();
                DrawAxes();
            }
            else if (DisplayMode == DataPlotBoxDisplayMode.CochlearFilterBankVisualization && Data is List<double[]>)
            {
                DrawCochlearFilterBankResponse(); 
                DrawAxes();
            }
        }

        private void DrawDataVisualization()
        {
            if (Data is double[][] dataArray)
            {
                double maxDataValue = dataArray.Max(array => array.Max());
                double minDataValue = dataArray.Min(array => array.Min());

                for (int i = 0; i < dataArray.Length; i++)
                {
                    double[] data = dataArray[i];
                    Polyline polyline = new Polyline
                    {
                        Stroke = (SolidColorBrush)Microsoft.UI.Xaml.Application.Current.Resources["AccentTextFillColorTertiaryBrush"],
                        StrokeThickness = 1
                    };

                    for (int j = 0; j < data.Length; j++)
                    {
                        double x = j * DataPlotCanvas.ActualWidth / (data.Length - 1);
                        double normalizedY = (data[j] - minDataValue) / (maxDataValue - minDataValue); // 归一化数据
                        double y = DataPlotCanvas.ActualHeight - (normalizedY * DataPlotCanvas.ActualHeight);
                        polyline.Points.Add(new Point(x, y));
                    }

                    DataPlotCanvas.Children.Add(polyline);
                }
            }
            else if (Data is double[] singleDataArray)
            {
                double maxDataValue = singleDataArray.Max();
                double minDataValue = singleDataArray.Min();

                Polyline polyline = new Polyline
                {
                    Stroke = (SolidColorBrush)Microsoft.UI.Xaml.Application.Current.Resources["AccentTextFillColorTertiaryBrush"],
                    StrokeThickness = 1
                };

                for (int j = 0; j < singleDataArray.Length; j++)
                {
                    double x = j * DataPlotCanvas.ActualWidth / (singleDataArray.Length - 1);
                    double normalizedY = (singleDataArray[j] - minDataValue) / (maxDataValue - minDataValue); // 归一化数据
                    double y = DataPlotCanvas.ActualHeight - (normalizedY * DataPlotCanvas.ActualHeight);
                    polyline.Points.Add(new Point(x, y));
                }

                DataPlotCanvas.Children.Add(polyline);
            }
        }

        private void DrawFrequencySpectrumVisualization()
        {
            if (Data == null || !(Data is double[][] frequencySpectrumData) || DataPlotCanvas.ActualWidth == 0 || DataPlotCanvas.ActualHeight == 0)
                return;

            DataPlotCanvas.Children.Clear();

            double maxDataValue = frequencySpectrumData.Max(array => array.Max());
            double minDataValue = frequencySpectrumData.Min(array => array.Min());

            for (int i = 0; i < frequencySpectrumData.Length; i++)
            {
                double[] data = frequencySpectrumData[i];
                Polyline polyline = new Polyline
                {
                    Stroke = (SolidColorBrush)Microsoft.UI.Xaml.Application.Current.Resources["AccentTextFillColorTertiaryBrush"],
                    StrokeThickness = 1
                };

                for (int j = 0; j < data.Length; j++)
                {
                    double x = j * DataPlotCanvas.ActualWidth / (data.Length - 1);
                    double normalizedY = (data[j] - minDataValue) / (maxDataValue - minDataValue); // 归一化数据
                    double y = DataPlotCanvas.ActualHeight - (normalizedY * DataPlotCanvas.ActualHeight);
                    polyline.Points.Add(new Point(x, y));
                }

                DataPlotCanvas.Children.Add(polyline);
            }
        }

        private void DrawMelFilterBankResponse()
        {
            if (Data == null || !(Data is List<double[]> melFilters) || DataPlotCanvas.ActualWidth == 0 || DataPlotCanvas.ActualHeight == 0)
                return;

            DataPlotCanvas.Children.Clear();

            for (int i = 0; i < melFilters.Count; i++)
            {
                double[] filter = melFilters[i];
                Polyline polyline = new Polyline
                {
                    Stroke = (SolidColorBrush)Microsoft.UI.Xaml.Application.Current.Resources["AccentTextFillColorTertiaryBrush"],
                    StrokeThickness = 1,
                    StrokeDashArray = new DoubleCollection { 6, 6 }
                };

                for (int j = 0; j < filter.Length; j++)
                {
                    double x = j * DataPlotCanvas.ActualWidth / (filter.Length - 1);
                    double y = DataPlotCanvas.ActualHeight - (filter[j] * DataPlotCanvas.ActualHeight);
                    polyline.Points.Add(new Point(x, y));
                }

                DataPlotCanvas.Children.Add(polyline);
            }
        }

        private void DrawGammatoneFilterBankResponse()
        {
            if (Data == null || !(Data is List<double[]> gammatoneFilters) || DataPlotCanvas.ActualWidth == 0 || DataPlotCanvas.ActualHeight == 0)
                return;

            DataPlotCanvas.Children.Clear();

            for (int i = 0; i < gammatoneFilters.Count; i++)
            {
                double[] filter = gammatoneFilters[i];
                Polyline polyline = new Polyline
                {
                    Stroke = (SolidColorBrush)Microsoft.UI.Xaml.Application.Current.Resources["AccentTextFillColorTertiaryBrush"],
                    StrokeThickness = 1,
                    StrokeDashArray = new DoubleCollection { 6, 6 }
                };

                for (int j = 0; j < filter.Length; j++)
                {
                    double x = j * DataPlotCanvas.ActualWidth / (filter.Length - 1);
                    double y = DataPlotCanvas.ActualHeight - (filter[j] * DataPlotCanvas.ActualHeight);
                    polyline.Points.Add(new Point(x, y));
                }

                DataPlotCanvas.Children.Add(polyline);
            }
        }
        private void DrawCochlearFilterBankResponse()
        {
            if (Data == null || !(Data is List<double[]> cochlearFilters) || DataPlotCanvas.ActualWidth == 0 || DataPlotCanvas.ActualHeight == 0)
                return;

            DataPlotCanvas.Children.Clear();

            double maxFrequency = SampleRate / 2.0;
            double frequencyStep = maxFrequency / cochlearFilters[0].Length;

            for (int i = 0; i < cochlearFilters.Count; i++)
            {
                double[] filter = cochlearFilters[i];
                Polyline polyline = new Polyline
                {
                    Stroke = (SolidColorBrush)Microsoft.UI.Xaml.Application.Current.Resources["AccentTextFillColorTertiaryBrush"],
                    StrokeThickness = 1,
                    StrokeDashArray = new DoubleCollection { 6, 6 }
                };

                for (int j = 0; j < filter.Length; j++)
                {
                    double x = j * DataPlotCanvas.ActualWidth / (filter.Length - 1);
                    double y = DataPlotCanvas.ActualHeight - (filter[j] * DataPlotCanvas.ActualHeight);
                    polyline.Points.Add(new Point(x, y));
                }

                DataPlotCanvas.Children.Add(polyline);
            }
        }


        private void SetDataPlotXtextMargin()
        {
            // 设置X轴刻度的Margin
            double xTickMargin = (-DataPlotCanvas.ActualWidth / (2 * XAxisTicks) + 12);
            if(DisplayMode == DataPlotBoxDisplayMode.FrequencySpectrumVisualization)
            {
                DataPlotXtext.Margin = new Thickness(xTickMargin, 0, xTickMargin + 24, 0);
            }
            else DataPlotXtext.Margin = new Thickness(xTickMargin, 0, xTickMargin + 12, 0);
        }

        private void SetDataPlotYtextMargin()
        {
            // 设置Y轴刻度的Margin
            double yTickMargin = (-DataPlotCanvas.ActualHeight / (2 * YAxisTicks) + 6);
            DataPlotYtext.Margin = new Thickness(6, yTickMargin, 0, yTickMargin);
        }

        private void DrawAxes()
        {
            // 绘制坐标轴
            DataPlotXtext.Children.Clear();
            DataPlotXtext.ColumnDefinitions.Clear();
            DataPlotYtext.Children.Clear();
            DataPlotYtext.RowDefinitions.Clear();

            // 设置 X 轴和 Y 轴的边距
            SetDataPlotXtextMargin();
            SetDataPlotYtextMargin();

            // 根据显示模式选择不同的刻度绘制逻辑
            if (DisplayMode == DataPlotBoxDisplayMode.FrequencySpectrumVisualization && Data is double[][] frequencySpectrumData)
            {
                // 频谱图模式：根据采样率和 FFT 大小计算频率分辨率
                double frequencyResolution = (double)SampleRate / FftSize;
                for (int i = 0; i <= XAxisTicks; i++)
                {
                    DataPlotXtext.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    double frequency = i * frequencyResolution * FftSize / XAxisTicks;
                    TextBlock textBlock = new TextBlock
                    {
                        Text = $"{frequency:F0} Hz",
                        HorizontalAlignment = HorizontalAlignment.Center
                    };
                    Grid.SetColumn(textBlock, i);
                    DataPlotXtext.Children.Add(textBlock);
                }

                // Y 轴数据范围
                double maxYValue = frequencySpectrumData.Max(array => array.Max());
                double minYValue = frequencySpectrumData.Min(array => array.Min());
                for (int i = 0; i <= YAxisTicks; i++)
                {
                    DataPlotYtext.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                    TextBlock textBlock = new TextBlock
                    {
                        Text = $"{minYValue + (maxYValue - minYValue) * (YAxisTicks - i) / YAxisTicks:F1}",
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    Grid.SetRow(textBlock, i);
                    DataPlotYtext.Children.Add(textBlock);
                }
            }
            else if (DisplayMode == DataPlotBoxDisplayMode.MelFilterBankVisualization && Data is List<double[]>)
            {
                // Mel 滤波器组：绘制 Mel 频率刻度
                double maxFrequency = SampleRate / 2.0;
                for (int i = 0; i <= XAxisTicks; i++)
                {
                    DataPlotXtext.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    double frequency = i * maxFrequency / XAxisTicks;
                    TextBlock textBlock = new TextBlock
                    {
                        Text = $"{i * (FftSize / 2) / XAxisTicks * 10} Hz",
                        HorizontalAlignment = HorizontalAlignment.Center
                    };
                    Grid.SetColumn(textBlock, i);
                    DataPlotXtext.Children.Add(textBlock);
                }

                // Y 轴归一化到 [0,1]
                for (int i = 0; i <= YAxisTicks; i++)
                {
                    DataPlotYtext.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                    TextBlock textBlock = new TextBlock
                    {
                        Text = $"{(1.0 - (double)i / YAxisTicks):F2}",
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    Grid.SetRow(textBlock, i);
                    DataPlotYtext.Children.Add(textBlock);
                }
            }
            else if (DisplayMode == DataPlotBoxDisplayMode.CochlearFilterBankVisualization && Data is List<double[]> filters)
            {
                // Cochlear 滤波器组：绘制滤波器中心频率和起始频率的频率刻度
                double maxFrequency = SampleRate / 2.0;
                double frequencyStep = maxFrequency / FftSize;

                for (int i = 0; i <= XAxisTicks; i++)
                {
                    DataPlotXtext.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    double frequency = i * maxFrequency / XAxisTicks;
                    TextBlock textBlock = new TextBlock
                    {
                        Text = $"{frequency:F0} Hz",
                        HorizontalAlignment = HorizontalAlignment.Center
                    };
                    Grid.SetColumn(textBlock, i);
                    DataPlotXtext.Children.Add(textBlock);
                }

                // Y 轴
                for (int i = 0; i <= YAxisTicks; i++)
                {
                    DataPlotYtext.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                    TextBlock textBlock = new TextBlock
                    {
                        Text = $"{(1.0 - (double)i / YAxisTicks):F2}",
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    Grid.SetRow(textBlock, i);
                    DataPlotYtext.Children.Add(textBlock);
                }
            }
            else if (DisplayMode == DataPlotBoxDisplayMode.GammatoneFilterBankVisualization && Data is List<double[]> filteres)
            {
                // Gammatone 滤波器组：绘制滤波器中心频率的频率刻度
                double maxFrequency = SampleRate / 2.0;
                int numFilters = filteres.Count;

                for (int i = 0; i <= XAxisTicks; i++)
                {
                    DataPlotXtext.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                    double centerFrequency = (i + 1) * maxFrequency / (XAxisTicks + 1);
                    TextBlock textBlock = new TextBlock
                    {
                        Text = $"{centerFrequency:F0} Hz",
                        HorizontalAlignment = HorizontalAlignment.Center
                    };
                    Grid.SetColumn(textBlock, i);
                    DataPlotXtext.Children.Add(textBlock);
                }

                // Y 轴归一化
                for (int i = 0; i <= YAxisTicks; i++)
                {
                    DataPlotYtext.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                    TextBlock textBlock = new TextBlock
                    {
                        Text = $"{(1.0 - (double)i / YAxisTicks):F2}",
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    Grid.SetRow(textBlock, i);
                    DataPlotYtext.Children.Add(textBlock);
                }
            }
            else if (Data is double[][] dataArray)
            {
                // 默认模式：绘制数据长度的刻度
                for (int i = 0; i <= XAxisTicks; i++)
                {
                    DataPlotXtext.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    TextBlock textBlock = new TextBlock
                    {
                        Text = $"{i * dataArray.Length / XAxisTicks:F0}",
                        HorizontalAlignment = HorizontalAlignment.Center
                    };
                    Grid.SetColumn(textBlock, i);
                    DataPlotXtext.Children.Add(textBlock);
                }

                // Y 轴数据范围
                double maxYValue = dataArray.Max(row => row.Max());
                for (int i = 0; i <= YAxisTicks; i++)
                {
                    DataPlotYtext.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                    TextBlock textBlock = new TextBlock
                    {
                        Text = $"{(maxYValue - maxYValue * i / YAxisTicks):F1}",
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    Grid.SetRow(textBlock, i);
                    DataPlotYtext.Children.Add(textBlock);
                }
            }
        }
    }
}
