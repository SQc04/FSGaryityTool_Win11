using FSGaryityTool_Win11.Core.SpeechReconition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.UI;
using Windows.Foundation;


namespace FSGaryityTool_Win11.Controls
{
    public sealed partial class WaveFormBox : UserControl, INotifyPropertyChanged
    {
        public enum WaveFormBoxDisplayMode
        {
            TimeDomain,
            FrequencySpectrum
        }
        public WaveFormBox()
        {
            this.InitializeComponent();
            this.SoundCanvas.SizeChanged += SoundCanvas_SizeChanged;
        }

        public static readonly DependencyProperty AudioDataProperty =
            DependencyProperty.Register("AudioData", typeof(double[]), typeof(WaveFormBox), new PropertyMetadata(null, OnAudioDataChanged));

        public static readonly DependencyProperty AmplitudeScaleProperty =
            DependencyProperty.Register("AmplitudeScale", typeof(double), typeof(WaveFormBox), new PropertyMetadata(0.75));

        public static readonly DependencyProperty DisplayModeProperty =
            DependencyProperty.Register("DisplayMode", typeof(WaveFormBoxDisplayMode), typeof(WaveFormBox), new PropertyMetadata(WaveFormBoxDisplayMode.TimeDomain, OnDisplayModeChanged));

        public static readonly DependencyProperty SpectrumDataProperty =
            DependencyProperty.Register("SpectrumData", typeof(double[][]), typeof(WaveFormBox), new PropertyMetadata(null, OnSpectrumDataChanged));

        public static readonly DependencyProperty SpectrumStartFrameProperty =
            DependencyProperty.Register("SpectrumStartFrame", typeof(int), typeof(WaveFormBox), new PropertyMetadata(0, OnSpectrumStartFrameChanged));

        public static readonly DependencyProperty SpectrumFrameCountProperty =
            DependencyProperty.Register("SpectrumFrameCount", typeof(int), typeof(WaveFormBox), new PropertyMetadata(0, OnSpectrumFrameCountChanged));

        public static readonly DependencyProperty SpectrumDisplayHeightBlocksProperty =
            DependencyProperty.Register("SpectrumDisplayHeightBlocks", typeof(int), typeof(WaveFormBox), new PropertyMetadata(0, OnSpectrumDisplayHeightBlocksChanged));

        public static readonly DependencyProperty SampleRateProperty =
            DependencyProperty.Register("SampleRate", typeof(int), typeof(WaveFormBox), new PropertyMetadata(1000, OnSampleRateChanged));

        public static readonly DependencyProperty XAxisTicksProperty =
            DependencyProperty.Register("XAxisTicks", typeof(int), typeof(WaveFormBox), new PropertyMetadata(10, OnXAxisTicksChanged));

        public static readonly DependencyProperty YAxisTicksProperty =
            DependencyProperty.Register("YAxisTicks", typeof(int), typeof(WaveFormBox), new PropertyMetadata(10, OnYAxisTicksChanged));

        public static readonly DependencyProperty XAxisLabelProperty =
            DependencyProperty.Register("XAxisLabel", typeof(string), typeof(WaveFormBox), new PropertyMetadata("Time (s)", OnXAxisLabelChanged));

        public static readonly DependencyProperty YAxisLabelProperty =
            DependencyProperty.Register("YAxisLabel", typeof(string), typeof(WaveFormBox), new PropertyMetadata("Amplitude", OnYAxisLabelChanged));

        private double[] _audioData;
        public double[] AudioData
        {
            get => _audioData;
            set
            {
                if (_audioData != value)
                {
                    _audioData = value;
                    OnPropertyChanged(nameof(AudioData));
                    DrawWaveform();
                }
            }
        }

        public double AmplitudeScale
        {
            get => (double)GetValue(AmplitudeScaleProperty);
            set => SetValue(AmplitudeScaleProperty, value);
        }

        public WaveFormBoxDisplayMode DisplayMode
        {
            get => (WaveFormBoxDisplayMode)GetValue(DisplayModeProperty);
            set => SetValue(DisplayModeProperty, value);
        }

        private double[][] _spectrumData;
        public double[][] SpectrumData
        {
            get => _spectrumData;
            set
            {
                if (_spectrumData != value)
                {
                    _spectrumData = value;
                    OnPropertyChanged(nameof(SpectrumData));
                    DrawWaveform();
                }
            }
        }
        public int SpectrumStartFrame
        {
            get => (int)GetValue(SpectrumStartFrameProperty);
            set => SetValue(SpectrumStartFrameProperty, value);
        }

        public int SpectrumFrameCount
        {
            get => (int)GetValue(SpectrumFrameCountProperty);
            set => SetValue(SpectrumFrameCountProperty, value);
        }
        public int SpectrumDisplayHeightBlocks
        {
            get => (int)GetValue(SpectrumDisplayHeightBlocksProperty);
            set => SetValue(SpectrumDisplayHeightBlocksProperty, value);
        }

        private int _sampleRate;
        public int SampleRate
        {
            get => _sampleRate;
            set
            {
                if (_sampleRate != value)
                {
                    _sampleRate = value;
                    OnPropertyChanged(nameof(SampleRate));
                    DrawWaveform();
                }
            }
        }

        private int _xAxisTicks;
        public int XAxisTicks
        {
            get => _xAxisTicks;
            set
            {
                if (_xAxisTicks != value)
                {
                    _xAxisTicks = value;
                    OnPropertyChanged(nameof(XAxisTicks));
                    DrawWaveform();
                }
            }
        }

        private int _yAxisTicks;
        public int YAxisTicks
        {
            get => _yAxisTicks;
            set
            {
                if (_yAxisTicks != value)
                {
                    _yAxisTicks = value;
                    OnPropertyChanged(nameof(YAxisTicks));
                    DrawWaveform();
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

        private static void OnAudioDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as WaveFormBox;
            if (control != null)
            {
                control.AudioData = (double[])e.NewValue;
            }
        }
        private static void OnSpectrumDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as WaveFormBox;
            if (control != null)
            {
                control.SpectrumData = (double[][])e.NewValue;
            }
        }

        private static void OnDisplayModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as WaveFormBox;
            if (control != null)
            {
                control.DrawWaveform();
            }
        }
        private static void OnSpectrumStartFrameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as WaveFormBox;
            if (control != null)
            {
                control.DrawWaveform();
            }
        }
        private static void OnSpectrumFrameCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as WaveFormBox;
            if (control != null)
            {
                control.DrawWaveform();
            }
        }
        private static void OnSpectrumDisplayHeightBlocksChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as WaveFormBox;
            if (control != null)
            {
                control.DrawWaveform();
            }
        }

        private static void OnSampleRateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as WaveFormBox;
            if (control != null)
            {
                control.SampleRate = (int)e.NewValue;
            }
        }

        private static void OnXAxisTicksChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as WaveFormBox;
            if (control != null)
            {
                control.XAxisTicks = (int)e.NewValue;
            }
        }

        private static void OnYAxisTicksChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as WaveFormBox;
            if (control != null)
            {
                control.YAxisTicks = (int)e.NewValue;
            }
        }

        private static void OnXAxisLabelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as WaveFormBox;
            if (control != null)
            {
                control.XAxisLabel = (string)e.NewValue;
            }
        }

        private static void OnYAxisLabelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as WaveFormBox;
            if (control != null)
            {
                control.YAxisLabel = (string)e.NewValue;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SoundCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DrawWaveform();
            SetDataPlotXtextMargin();
            SetDataPlotYtextMargin();
        }

        private void DrawWaveform()
        {
            if (DisplayMode == WaveFormBoxDisplayMode.TimeDomain)
            {
                if (AudioData == null || AudioData.Length == 0 || WaveFormGrid.ActualWidth == 0 || WaveFormGrid.ActualHeight == 0)
                    return;

                SoundCanvas.Children.Clear();
                DrawTimeDomainWaveform();
                DrawAxes();
            }
            else if (DisplayMode == WaveFormBoxDisplayMode.FrequencySpectrum)
            {
                if (SpectrumData == null || SpectrumData.Length == 0 || WaveFormGrid.ActualWidth == 0 || WaveFormGrid.ActualHeight == 0)
                    return;

                SoundCanvas.Children.Clear();
                GenerateColorScale();
                DrawFrequencySpectrum();
                DrawAxes();
            }
        }

        private void DrawTimeDomainWaveform()
        {
            if (AudioData == null || AudioData.Length == 0 || WaveFormGrid.ActualWidth == 0 || WaveFormGrid.ActualHeight == 0)
                return;

            SoundCanvas.Children.Clear();

            Polyline waveformPolyline = new Polyline
            {
                Stroke = (SolidColorBrush)Application.Current.Resources["AccentTextFillColorTertiaryBrush"],
                Opacity = 0.9,
                StrokeThickness = 1
            };

            double xScale = WaveFormGrid.ActualWidth / (double)AudioData.Length;
            double yScale = WaveFormGrid.ActualHeight / 2.0 * AmplitudeScale;

            for (int i = 0; i < AudioData.Length; i++)
            {
                double x = i * xScale;
                double y = (WaveFormGrid.ActualHeight / 2) - (AudioData[i] * yScale);
                waveformPolyline.Points.Add(new Point(x, y));
            }

            SoundCanvas.Children.Add(waveformPolyline);
        }

        private static double MIN_DB = -80;   // 最小显示分贝值
        private static double MAX_DB = 0;     // 最大显示分贝值
        private void DrawFrequencySpectrum()
        {
            if (SpectrumData == null || SpectrumData.Length == 0) return;

            // 获取帧数和频率数
            int frameCount = SpectrumData.Length;
            int frequencyCount = SpectrumData[0].Length;

            // 显示范围限制
            int startFrame = Math.Clamp(SpectrumStartFrame, 0, frameCount - 1);
            int endFrame = SpectrumFrameCount > 0
                ? Math.Min(startFrame + SpectrumFrameCount, frameCount)
                : frameCount;
            int displayFrameCount = endFrame - startFrame;

            // 创建位图
            int bitmapWidth = SpectrumData.Length;
            int bitmapHeight = SpectrumData[0].Length / 1;
            WriteableBitmap bitmap = new WriteableBitmap(bitmapWidth, bitmapHeight);

            using (var buffer = bitmap.PixelBuffer.AsStream())
            {
                byte[] pixels = new byte[bitmapWidth * bitmapHeight * 4];

                // 第一步，计算动态范围
                double maxLogValue = MIN_DB; // 初始化为最小值

                // 计算每个频率的最小值
                Parallel.For(startFrame, endFrame, x =>
                {
                    var spectrumEnergy = SpectrumData[x];
                    for (int y = 0; y < frequencyCount / 1; y++)
                    {
                        double blockEnergy = spectrumEnergy[y];

                        // 转换为分贝值
                        double dbValue = 10 * Math.Log10(blockEnergy + 1e-10);
                        lock (this)
                        {
                            if (dbValue > maxLogValue) maxLogValue = dbValue;
                        }
                    }
                });

                // 调整动态范围
                maxLogValue = Math.Min(maxLogValue, MAX_DB);
                double dynamicRange = maxLogValue - MIN_DB;

                // 第二步，绘制图像元素
                for (int x = 0; x < displayFrameCount; x++)
                {
                    int frameIndex = startFrame + x;
                    double[] spectrumEnergy = SpectrumData[frameIndex];

                    for (int y = 0; y < frequencyCount / 1; y++)
                    {
                        // 获取能量值
                        double blockEnergy = spectrumEnergy[y];

                        // 转换为分贝值
                        double dbValue = 10 * Math.Log10(blockEnergy + 1e-10);
                        dbValue = Math.Clamp(dbValue, MIN_DB, MAX_DB);
                        double normalized = (dbValue - MIN_DB) / dynamicRange;

                        // 获取颜色
                        Color color = GetColorFromMagnitude(normalized);

                        // 设置像素颜色
                        int pixelX = x;
                        int pixelY = bitmapHeight - y - 1;
                        int pixelIndex = (pixelY * bitmapWidth + pixelX) * 4;
                        pixels[pixelIndex] = color.B;
                        pixels[pixelIndex + 1] = color.G;
                        pixels[pixelIndex + 2] = color.R;
                        pixels[pixelIndex + 3] = color.A;
                    }
                }

                // 将像素数据写入位图
                buffer.Write(pixels, 0, pixels.Length);
            }

            // 设置位图源
            WaveFormImage.Source = bitmap;
        }

        private Color GetColorFromMagnitude(double normalizedMagnitude)
        {
            // 确保值在[0,1]范围内
            double clamped = Math.Clamp(normalizedMagnitude, 0.0, 1.0);

            // 颜色映射逻辑
            const double BLUE_HUE = 240;   // 蓝色对应的色调
            const double RED_HUE = 0;      // 红色对应的色调
            const double SATURATION = 0.8; // 饱和度
            const double VALUE = 0.8;      // 亮度

            // 计算色调（线性插值）
            double hue = BLUE_HUE - (BLUE_HUE - RED_HUE) * clamped;

            // 增强色调强度
            double enhancedHue = Math.Pow(hue / BLUE_HUE, 0.8) * BLUE_HUE;

            return HsvToRgb(enhancedHue, SATURATION, VALUE);
        }

        private Color HsvToRgb(double hue, double saturation, double value)
        {
            hue = Math.Clamp(hue, 0.0, 360.0);
            saturation = Math.Clamp(saturation, 0.0, 1.0);
            value = Math.Clamp(value, 0.0, 1.0);

            int hi = (int)(hue / 60) % 6;
            double f = hue / 60 - hi;
            double p = value * (1 - saturation);
            double q = value * (1 - f * saturation);
            double t = value * (1 - (1 - f) * saturation);

            (double r, double g, double b) = hi switch
            {
                0 => (value, t, p),
                1 => (q, value, p),
                2 => (p, value, t),
                3 => (p, q, value),
                4 => (t, p, value),
                _ => (value, p, q)
            };

            return Color.FromArgb(255,
                (byte)(r * 255),
                (byte)(g * 255),
                (byte)(b * 255));
        }
        private void GenerateColorScale()
        {
            // 清空现有刻度
            EnergyScaleGrid.Children.Clear();
            EnergyScaleGrid.RowDefinitions.Clear();

            GradientStopCollection gradientStops = new GradientStopCollection();
            for (int i = 0; i <= 10; i++)
            {
                double normalized = i / 10.0;
                Color color = GetColorFromMagnitude(normalized);
                gradientStops.Add(new GradientStop { Color = color, Offset = 1.0 - normalized });
            }

            LinearGradientBrush gradientBrush = new LinearGradientBrush
            {
                GradientStops = gradientStops,
                StartPoint = new Point(0, 0),
                EndPoint = new Point(0, 1)
            };

            SetEnergyScaleGridMargin();
            ColorScaleRectangle.Fill = gradientBrush;

            // 添加刻度值标签
            for (int i = 0; i <= 10; i++)
            {
                EnergyScaleGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                double dbValue = MIN_DB + (MAX_DB - MIN_DB) * ((10 - i) / 10.0);
                TextBlock textBlock = new TextBlock
                {
                    Text = $"{dbValue:F0} dB",
                    VerticalAlignment = VerticalAlignment.Center,
                };
                Grid.SetRow(textBlock, i);
                EnergyScaleGrid.Children.Add(textBlock);
            }
        }

        private void SetDataPlotXtextMargin()
        {
            // 设置X轴刻度的Margin
            double xTickMargin = (-SoundCanvas.ActualWidth / (2 * XAxisTicks) + 12);
            DataPlotXtext.Margin = new Thickness(xTickMargin, 0, xTickMargin, 0);
        }
        private void SetDataPlotYtextMargin()
        {
            // 设置Y轴刻度的Margin
            double yTickMargin = (-SoundCanvas.ActualHeight / (2 * YAxisTicks) + 6);
            DataPlotYtext.Margin = new Thickness(6, yTickMargin, 0, yTickMargin);
        }

        private void SetEnergyScaleGridMargin()
        {
            double energyScaleGridMargin = (-ColorScaleRectangle.ActualHeight / (2 * 10) + 6);
            EnergyScaleGrid.Margin = new Thickness(0, energyScaleGridMargin, 0, energyScaleGridMargin);
        }

        private void DrawAxes()
        {
            // 清空现有刻度
            DataPlotXtext.Children.Clear();
            DataPlotXtext.ColumnDefinitions.Clear();
            DataPlotYtext.Children.Clear();
            DataPlotYtext.RowDefinitions.Clear();

            // 设置X轴和Y轴刻度的Margin
            SetDataPlotXtextMargin();
            SetDataPlotYtextMargin();

            // 绘制X轴刻度
            if (DisplayMode == WaveFormBoxDisplayMode.TimeDomain && AudioData != null)
            {
                for (int i = 0; i <= XAxisTicks; i++)
                {
                    DataPlotXtext.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    double position = i * WaveFormGrid.ActualWidth / XAxisTicks;
                    TextBlock textBlock = new TextBlock
                    {
                        Text = $"{i * (AudioData.Length / XAxisTicks) / (double)SampleRate:F1}s", // 使用SampleRate计算
                        HorizontalAlignment = HorizontalAlignment.Center
                    };
                    Grid.SetColumn(textBlock, i);
                    DataPlotXtext.Children.Add(textBlock);
                }

                // 绘制Y轴刻度
                for (int i = 0; i <= YAxisTicks; i++)
                {
                    DataPlotYtext.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                    double position = i * WaveFormGrid.ActualHeight / YAxisTicks;
                    TextBlock textBlock = new TextBlock
                    {
                        Text = $"{(1 - (double)i / YAxisTicks) * 2 - 1:F1}", // 假设Y轴范围为[-1, 1]
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(12, 0, 12, 0)
                    };
                    Grid.SetRow(textBlock, i);
                    DataPlotYtext.Children.Add(textBlock);
                }
            }
            else if (DisplayMode == WaveFormBoxDisplayMode.FrequencySpectrum && SpectrumData != null)
            {
                int frameStrideMs = 48; // 帧移为32ms
                int frameSizeMs = 64; // 帧长为32ms
                for (int i = 0; i <= XAxisTicks; i++)
                {
                    DataPlotXtext.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    double position = i * WaveFormGrid.ActualWidth / XAxisTicks;
                    TextBlock textBlock = new TextBlock
                    {
                        Text = $"{i * (SpectrumData.Length / XAxisTicks) * frameStrideMs / 1000.0:F1}s", // 使用帧移计算时间
                        HorizontalAlignment = HorizontalAlignment.Center
                    };
                    Grid.SetColumn(textBlock, i);
                    DataPlotXtext.Children.Add(textBlock);
                }

                // 计算频率分辨率
                int frameSize = (int)(SampleRate * frameSizeMs / 1000.0); // 每帧样本数
                double frequencyResolution = (double)SampleRate / frameSize;

                // 绘制Y轴刻度
                for (int i = 0; i <= YAxisTicks; i++)
                {
                    DataPlotYtext.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                    double position = i * WaveFormGrid.ActualHeight / YAxisTicks;
                    TextBlock textBlock = new TextBlock
                    {
                        Text = $"{(YAxisTicks - i) * (frequencyResolution * frameSize / YAxisTicks):F1} Hz", // 使用频率分辨率计算频率刻度
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    Grid.SetRow(textBlock, i);
                    DataPlotYtext.Children.Add(textBlock);
                }
            }
            else
            {

            }
        }
    }

}
