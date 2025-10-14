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
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FSGaryityTool_Win11.Controls
{
    public sealed partial class WaveformView : UserControl, INotifyPropertyChanged
    {
        public enum GridLineStyle
        {
            Dot,
            Dash,
            Solid
        }

        public static readonly DependencyProperty MinRowTickCountProperty =
            DependencyProperty.Register(nameof(MinRowTickCount), typeof(int), typeof(WaveformView), new PropertyMetadata(0, OnVisualPropertyChanged));

        public int MinRowTickCount
        {
            get => (int)GetValue(MinRowTickCountProperty);
            set => SetValue(MinRowTickCountProperty, value);
        }

        public static readonly DependencyProperty MinColumnTickCountProperty =
            DependencyProperty.Register(nameof(MinColumnTickCount), typeof(int), typeof(WaveformView), new PropertyMetadata(0, OnVisualPropertyChanged));

        public int MinColumnTickCount
        {
            get => (int)GetValue(MinColumnTickCountProperty);
            set => SetValue(MinColumnTickCountProperty, value);
        }

        public static readonly DependencyProperty MaxRowTickCountProperty =
            DependencyProperty.Register(nameof(MaxRowTickCount), typeof(int), typeof(WaveformView), new PropertyMetadata(10, OnVisualPropertyChanged));

        public int MaxRowTickCount
        {
            get => (int)GetValue(MaxRowTickCountProperty);
            set => SetValue(MaxRowTickCountProperty, value);
        }

        public static readonly DependencyProperty MaxColumnTickCountProperty =
            DependencyProperty.Register(nameof(MaxColumnTickCount), typeof(int), typeof(WaveformView), new PropertyMetadata(10, OnVisualPropertyChanged));

        public int MaxColumnTickCount
        {
            get => (int)GetValue(MaxColumnTickCountProperty);
            set => SetValue(MaxColumnTickCountProperty, value);
        }

        public static readonly DependencyProperty RowTickCountProperty =
            DependencyProperty.Register(nameof(RowTickCount), typeof(int), typeof(WaveformView), new PropertyMetadata(8, OnVisualPropertyChanged));

        public int RowTickCount
        {
            get => (int)GetValue(RowTickCountProperty);
            set => SetValue(RowTickCountProperty, value);
        }

        public static readonly DependencyProperty ColumnTickCountProperty =
            DependencyProperty.Register(nameof(ColumnTickCount), typeof(int), typeof(WaveformView), new PropertyMetadata(8, OnVisualPropertyChanged));

        public int ColumnTickCount
        {
            get => (int)GetValue(ColumnTickCountProperty);
            set => SetValue(ColumnTickCountProperty, value);
        }

        public static readonly DependencyProperty GridBorderMarginProperty =
            DependencyProperty.Register(nameof(GridBorderMargin), typeof(Thickness), typeof(WaveformView), new PropertyMetadata(default(Thickness), OnVisualPropertyChanged));

        public Thickness GridBorderMargin
        {
            get => (Thickness)GetValue(GridBorderMarginProperty);
            set => SetValue(GridBorderMarginProperty, value);
        }

        public static readonly DependencyProperty ControlBorderMarginProperty =
            DependencyProperty.Register(nameof(ControlBorderMargin), typeof(Thickness), typeof(WaveformView), new PropertyMetadata(default(Thickness), OnVisualPropertyChanged));

        public Thickness ControlBorderMargin
        {
            get => (Thickness)GetValue(ControlBorderMarginProperty);
            set => SetValue(ControlBorderMarginProperty, value);
        }

        public static readonly DependencyProperty WaveGridBorderThicknessProperty =
        DependencyProperty.Register(nameof(WaveGridBorderThickness), typeof(Thickness), typeof(WaveformView), new PropertyMetadata(new Thickness(2), OnVisualPropertyChanged));

        public Thickness WaveGridBorderThickness
        {
            get => (Thickness)GetValue(WaveGridBorderThicknessProperty);
            set => SetValue(WaveGridBorderThicknessProperty, value);
        }

        public static readonly DependencyProperty RowTickLineWidthProperty =
            DependencyProperty.Register(nameof(RowTickLineWidth), typeof(double), typeof(WaveformView), new PropertyMetadata(1.0, OnVisualPropertyChanged));

        public double RowTickLineWidth
        {
            get => (double)GetValue(RowTickLineWidthProperty);
            set => SetValue(RowTickLineWidthProperty, value);
        }

        public static readonly DependencyProperty ColumnTickLineWidthProperty =
            DependencyProperty.Register(nameof(ColumnTickLineWidth), typeof(double), typeof(WaveformView), new PropertyMetadata(1.0, OnVisualPropertyChanged));

        public double ColumnTickLineWidth
        {
            get => (double)GetValue(ColumnTickLineWidthProperty);
            set => SetValue(ColumnTickLineWidthProperty, value);
        }

        public static readonly DependencyProperty GridBorderBrushProperty =
            DependencyProperty.Register(nameof(GridBorderBrush), typeof(Brush), typeof(WaveformView),
                new PropertyMetadata(null, OnVisualPropertyChanged));
        public Brush GridBorderBrush
        {
            get => (Brush)GetValue(GridBorderBrushProperty);
            set => SetValue(GridBorderBrushProperty, value);
        }

        public static readonly DependencyProperty RowTickLineBrushProperty =
            DependencyProperty.Register(nameof(RowTickLineBrush), typeof(Brush), typeof(WaveformView),
                new PropertyMetadata(null, OnVisualPropertyChanged));
        public Brush RowTickLineBrush
        {
            get => (Brush)GetValue(RowTickLineBrushProperty);
            set => SetValue(RowTickLineBrushProperty, value);
        }

        public static readonly DependencyProperty ColumnTickLineBrushProperty =
            DependencyProperty.Register(nameof(ColumnTickLineBrush), typeof(Brush), typeof(WaveformView),
                new PropertyMetadata(null, OnVisualPropertyChanged));
        public Brush ColumnTickLineBrush
        {
            get => (Brush)GetValue(ColumnTickLineBrushProperty);
            set => SetValue(ColumnTickLineBrushProperty, value);
        }

        public static readonly DependencyProperty RowCenterTickLineBrushProperty =
            DependencyProperty.Register(nameof(RowCenterTickLineBrush), typeof(Brush), typeof(WaveformView),
                new PropertyMetadata(null, OnVisualPropertyChanged));
        public Brush RowCenterTickLineBrush
        {
            get => (Brush)GetValue(RowCenterTickLineBrushProperty);
            set => SetValue(RowCenterTickLineBrushProperty, value);
        }

        public static readonly DependencyProperty ColumnCenterTickLineBrushProperty =
            DependencyProperty.Register(nameof(ColumnCenterTickLineBrush), typeof(Brush), typeof(WaveformView),
                new PropertyMetadata(null, OnVisualPropertyChanged));
        public Brush ColumnCenterTickLineBrush
        {
            get => (Brush)GetValue(ColumnCenterTickLineBrushProperty);
            set => SetValue(ColumnCenterTickLineBrushProperty, value);
        }

        public static readonly DependencyProperty GridBorderLineStyleProperty =
            DependencyProperty.Register(nameof(GridBorderLineStyle), typeof(GridLineStyle), typeof(WaveformView), new PropertyMetadata(GridLineStyle.Solid, OnVisualPropertyChanged));

        public GridLineStyle GridBorderLineStyle
        {
            get => (GridLineStyle)GetValue(GridBorderLineStyleProperty);
            set => SetValue(GridBorderLineStyleProperty, value);
        }

        public static readonly DependencyProperty GridTickLineStyleProperty =
            DependencyProperty.Register(nameof(GridTickLineStyle), typeof(GridLineStyle), typeof(WaveformView), new PropertyMetadata(GridLineStyle.Dot, OnVisualPropertyChanged));

        public GridLineStyle GridTickLineStyle
        {
            get => (GridLineStyle)GetValue(GridTickLineStyleProperty);
            set => SetValue(GridTickLineStyleProperty, value);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private static void OnVisualPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WaveformView view)
            {
                view.InvalidateVisual();
            }
        }
        private void InvalidateVisual()
        {
            LineDemonstratorCanvasControl?.Invalidate();
            WaveformDemonstratorCanvasControl?.Invalidate();
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private void SetDefaultColors()
        {
            // 获取当前主题下的TextFillColorTertiaryBrush
            if (Application.Current.Resources.TryGetValue("TextFillColorTertiaryBrush", out var brushObj) && brushObj is SolidColorBrush brush)
            {
                // 直接赋值为Brush，支持主题切换和XAML资源
                GridBorderBrush = brush;
                RowTickLineBrush = brush;
                ColumnTickLineBrush = brush;
                RowCenterTickLineBrush = brush;
                ColumnCenterTickLineBrush = brush;
            }
        }
        private void OnActualThemeChanged(FrameworkElement sender, object args)
        {
            SetDefaultColors();
        }

        public WaveformView()
        {
            InitializeComponent(); 
            SetDefaultColors();
            ActualThemeChanged += OnActualThemeChanged;
        }

        private void WaveformDemonstratorCanvasControl_Draw(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs args)
        {
            float width = (float)sender.ActualWidth;
            float height = (float)sender.ActualHeight;

            // 使用 GridBorderMargin 计算绘图区域
            var margin = GridBorderMargin;
            float left = (float)margin.Left;
            float top = (float)margin.Top;
            float right = (float)margin.Right;
            float bottom = (float)margin.Bottom;

            float gridLeft = left;
            float gridTop = top;
            float gridRight = width - right;
            float gridBottom = height - bottom;
            float gridWidth = gridRight - gridLeft;
            float gridHeight = gridBottom - gridTop;

            // 获取画笔颜色（AccentTextFillColorTertiaryBrush）
            var brushObj = Application.Current.Resources.TryGetValue("AccentTextFillColorTertiaryBrush", out var accentBrushObj) ? accentBrushObj : null;
            var accentColor = (brushObj as SolidColorBrush)?.Color ?? Microsoft.UI.Colors.DeepSkyBlue;

            var ds = args.DrawingSession;

            // 随机生成10000个采样点
            int sampleCount = 200;
            float[] samples = new float[sampleCount];
            var rand = new Random();
            for (int i = 0; i < sampleCount; i++)
            {
                // 采样范围为[-1, 1]
                samples[i] = (float)(rand.NextDouble() * 2.0 - 1.0);
            }

            // 绘制曲线
            float prevX = gridLeft;
            float prevY = gridTop + gridHeight / 2f - samples[0] * (gridHeight / 2f);

            for (int i = 1; i < sampleCount; i++)
            {
                float x = gridLeft + (i / (float)(sampleCount - 1)) * gridWidth;
                float y = gridTop + gridHeight / 2f - samples[i] * (gridHeight / 2f);
                ds.DrawLine(prevX, prevY, x, y, accentColor, 2f);
                prevX = x;
                prevY = y;
            }
        }

        // 2. 绘制时获取颜色
        private void LineDemonstratorCanvasControl_Draw(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs args)
        {
            float width = (float)sender.ActualWidth;
            float height = (float)sender.ActualHeight;

            var margin = GridBorderMargin;
            float left = (float)margin.Left;
            float top = (float)margin.Top;
            float right = (float)margin.Right;
            float bottom = (float)margin.Bottom;

            float gridLeft = left;
            float gridTop = top;
            float gridRight = width - right;
            float gridBottom = height - bottom;
            float gridWidth = gridRight - gridLeft;
            float gridHeight = gridBottom - gridTop;

            // 获取颜色
            var borderColor = (GridBorderBrush as SolidColorBrush)?.Color ?? Microsoft.UI.Colors.Black;
            var rowColor = (RowTickLineBrush as SolidColorBrush)?.Color ?? Microsoft.UI.Colors.Gray;
            var colColor = (ColumnTickLineBrush as SolidColorBrush)?.Color ?? Microsoft.UI.Colors.Gray;
            var rowCenterColor = (RowCenterTickLineBrush as SolidColorBrush)?.Color ?? Microsoft.UI.Colors.Gray;
            var colCenterColor = (ColumnCenterTickLineBrush as SolidColorBrush)?.Color ?? Microsoft.UI.Colors.Gray;

            float leftBorderWidth = (float)WaveGridBorderThickness.Left;
            float topBorderWidth = (float)WaveGridBorderThickness.Top;
            float rightBorderWidth = (float)WaveGridBorderThickness.Right;
            float bottomBorderWidth = (float)WaveGridBorderThickness.Bottom;
            DrawLineStyle(args, gridLeft, gridTop, gridRight, gridTop, borderColor, topBorderWidth, GridBorderLineStyle); // 上
            DrawLineStyle(args, gridRight, gridTop, gridRight, gridBottom, borderColor, rightBorderWidth, GridBorderLineStyle); // 右
            DrawLineStyle(args, gridRight, gridBottom, gridLeft, gridBottom, borderColor, bottomBorderWidth, GridBorderLineStyle); // 下
            DrawLineStyle(args, gridLeft, gridBottom, gridLeft, gridTop, borderColor, leftBorderWidth, GridBorderLineStyle); // 左

            int rowCount = Math.Max(MinRowTickCount, RowTickCount > 0 ? RowTickCount : MinRowTickCount);
            rowCount = Math.Min(rowCount, MaxRowTickCount);
            int colCount = Math.Max(MinColumnTickCount, ColumnTickCount > 0 ? ColumnTickCount : MinColumnTickCount);
            colCount = Math.Min(colCount, MaxColumnTickCount);

            for (int i = 1; i < rowCount; i++)
            {
                float y = gridTop + gridHeight * i / rowCount;
                var color = (i == rowCount / 2 && rowCount % 2 == 0) ? rowCenterColor : rowColor;
                float lineWidth = (float)RowTickLineWidth;
                DrawLineStyle(args, gridLeft, y, gridRight, y, color, lineWidth, GridTickLineStyle);
            }

            for (int i = 1; i < colCount; i++)
            {
                float x = gridLeft + gridWidth * i / colCount;
                var color = (i == colCount / 2 && colCount % 2 == 0) ? colCenterColor : colColor;
                float lineWidth = (float)ColumnTickLineWidth;
                DrawLineStyle(args, x, gridTop, x, gridBottom, color, lineWidth, GridTickLineStyle);
            }
        }

        // 辅助方法：根据线型绘制线
        private void DrawLineStyle(
            Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs args,
            float x1, float y1, float x2, float y2,
            Windows.UI.Color color, float width, GridLineStyle style)
        {
            var ds = args.DrawingSession;
            switch (style)
            {
                case GridLineStyle.Dot:
                    {
                        float length = (float)Math.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1));
                        float dotSpacing = 4f;
                        int dotCount = (int)(length / dotSpacing);
                        for (int i = 0; i < dotCount; i++)
                        {
                            float t1 = (float)i / dotCount;
                            float t2 = (float)(i + 0.5) / dotCount;
                            float sx = x1 + (x2 - x1) * t1;
                            float sy = y1 + (y2 - y1) * t1;
                            float ex = x1 + (x2 - x1) * t2;
                            float ey = y1 + (y2 - y1) * t2;
                            ds.DrawLine(sx, sy, ex, ey, color, width);
                        }
                    }
                    break;
                case GridLineStyle.Dash:
                    {
                        float length = (float)Math.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1));
                        float dashLength = 8f, gapLength = 4f;
                        float drawn = 0;
                        while (drawn < length)
                        {
                            float t1 = drawn / length;
                            float t2 = Math.Min((drawn + dashLength) / length, 1f);
                            float sx = x1 + (x2 - x1) * t1;
                            float sy = y1 + (y2 - y1) * t1;
                            float ex = x1 + (x2 - x1) * t2;
                            float ey = y1 + (y2 - y1) * t2;
                            ds.DrawLine(sx, sy, ex, ey, color, width);
                            drawn += dashLength + gapLength;
                        }
                    }
                    break;
                case GridLineStyle.Solid:
                default:
                    ds.DrawLine(x1, y1, x2, y2, color, width);
                    break;
            }
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            //if(LineDemonstratorCanvasControl != null) LineDemonstratorCanvasControl.RemoveFromVisualTree();
            //if(WaveformDemonstratorCanvasControl != null) WaveformDemonstratorCanvasControl.RemoveFromVisualTree();
        }
    }
}
