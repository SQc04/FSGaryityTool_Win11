using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
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
    public enum GridLineStyle
    {
        None,
        Dot,
        Dash,
        Solid,
    }
    public enum GridTickModeStyle
    {
        None,
        Intersection,
        Cross
    }
    public sealed partial class WaveformView : UserControl, INotifyPropertyChanged
    {
        

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
            DependencyProperty.Register(nameof(MaxRowTickCount), typeof(int), typeof(WaveformView), new PropertyMetadata(16, OnVisualPropertyChanged));

        public int MaxRowTickCount
        {
            get => (int)GetValue(MaxRowTickCountProperty);
            set => SetValue(MaxRowTickCountProperty, value);
        }

        public static readonly DependencyProperty MaxColumnTickCountProperty =
            DependencyProperty.Register(nameof(MaxColumnTickCount), typeof(int), typeof(WaveformView), new PropertyMetadata(16, OnVisualPropertyChanged));

        public int MaxColumnTickCount
        {
            get => (int)GetValue(MaxColumnTickCountProperty);
            set => SetValue(MaxColumnTickCountProperty, value);
        }

        public static readonly DependencyProperty RowTickCountProperty =
            DependencyProperty.Register(nameof(RowTickCount), typeof(string), typeof(WaveformView), new PropertyMetadata("8", OnVisualPropertyChanged));

        public string RowTickCount
        {
            get => (string)GetValue(RowTickCountProperty);
            set => SetValue(RowTickCountProperty, value);
        }

        public static readonly DependencyProperty ColumnTickCountProperty =
            DependencyProperty.Register(nameof(ColumnTickCount), typeof(string), typeof(WaveformView), new PropertyMetadata("8", OnVisualPropertyChanged));

        public string ColumnTickCount
        {
            get => (string)GetValue(ColumnTickCountProperty);
            set => SetValue(ColumnTickCountProperty, value);
        }

        public static readonly DependencyProperty WaveGridBorderMarginProperty =
            DependencyProperty.Register(nameof(WaveGridBorderMargin), typeof(Thickness), typeof(WaveformView), new PropertyMetadata(default(Thickness), OnVisualPropertyChanged));

        public Thickness WaveGridBorderMargin
        {
            get => (Thickness)GetValue(WaveGridBorderMarginProperty);
            set => SetValue(WaveGridBorderMarginProperty, value);
        }

        public static readonly DependencyProperty ControlBorderMarginProperty =
            DependencyProperty.Register(nameof(ControlBorderMargin), typeof(Thickness), typeof(WaveformView), new PropertyMetadata(new Thickness(3), OnVisualPropertyChanged));

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

        public static readonly DependencyProperty WaveGridBorderBrushProperty =
            DependencyProperty.Register(nameof(WaveGridBorderBrush), typeof(Brush), typeof(WaveformView),
                new PropertyMetadata(null, OnVisualPropertyChanged));
        public Brush WaveGridBorderBrush
        {
            get => (Brush)GetValue(WaveGridBorderBrushProperty);
            set => SetValue(WaveGridBorderBrushProperty, value);
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

        public static readonly DependencyProperty WaveGridBorderLineStyleProperty =
            DependencyProperty.Register(nameof(WaveGridBorderLineStyle), typeof(GridLineStyle), typeof(WaveformView), new PropertyMetadata(GridLineStyle.Solid, OnVisualPropertyChanged));

        public GridLineStyle WaveGridBorderLineStyle
        {
            get => (GridLineStyle)GetValue(WaveGridBorderLineStyleProperty);
            set => SetValue(WaveGridBorderLineStyleProperty, value);
        }

        public static readonly DependencyProperty WaveGridTickLineStyleProperty =
            DependencyProperty.Register(nameof(WaveGridTickLineStyle), typeof(GridLineStyle), typeof(WaveformView), new PropertyMetadata(GridLineStyle.Solid, OnVisualPropertyChanged));

        public GridLineStyle WaveGridTickLineStyle
        {
            get => (GridLineStyle)GetValue(WaveGridTickLineStyleProperty);
            set => SetValue(WaveGridTickLineStyleProperty, value);
        }

        public static readonly DependencyProperty WaveGridTickModeStyleProperty =
            DependencyProperty.Register(nameof(WaveGridTickMode), typeof(GridTickModeStyle), typeof(WaveformView), new PropertyMetadata(GridTickModeStyle.None, OnVisualPropertyChanged));
        public GridTickModeStyle WaveGridTickMode
        {
            get => (GridTickModeStyle)GetValue(WaveGridTickModeStyleProperty);
            set => SetValue(WaveGridTickModeStyleProperty, value);
        }
        public static readonly DependencyProperty WaveGridBorderTickThicknessProperty =
            DependencyProperty.Register(nameof(WaveGridBorderTickThickness), typeof(Thickness), typeof(WaveformView), new PropertyMetadata(new Thickness(4), OnVisualPropertyChanged));

        public Thickness WaveGridBorderTickThickness
        {
            get => (Thickness)GetValue(WaveGridBorderTickThicknessProperty);
            set => SetValue(WaveGridBorderTickThicknessProperty, value);
        }

        public static readonly DependencyProperty WaveGridBorderSubTickThicknessProperty =
            DependencyProperty.Register(nameof(WaveGridBorderSubTickThickness), typeof(Thickness), typeof(WaveformView), new PropertyMetadata(new Thickness(2), OnVisualPropertyChanged));
        public Thickness WaveGridBorderSubTickThickness
        {
            get => (Thickness)GetValue(WaveGridBorderSubTickThicknessProperty);
            set => SetValue(WaveGridBorderSubTickThicknessProperty, value);
        }
        public static readonly DependencyProperty RowGridBorderSubTickCountProperty =
            DependencyProperty.Register(nameof(RowGridBorderSubTickCount), typeof(int), typeof(WaveformView), new PropertyMetadata(0, OnVisualPropertyChanged));
        public int RowGridBorderSubTickCount
        {
            get => (int)GetValue(RowGridBorderSubTickCountProperty);
            set => SetValue(RowGridBorderSubTickCountProperty, value);
        }
        public static readonly DependencyProperty ColumnGridBorderSubTickCountProperty =
            DependencyProperty.Register(nameof(ColumnGridBorderSubTickCount), typeof(int), typeof(WaveformView), new PropertyMetadata(0, OnVisualPropertyChanged));
        public int ColumnGridBorderSubTickCount
            {
            get => (int)GetValue(ColumnGridBorderSubTickCountProperty);
            set => SetValue(ColumnGridBorderSubTickCountProperty, value);
        }

        public static readonly DependencyProperty CrossSizeProperty =
            DependencyProperty.Register(nameof(CrossSize), typeof(double), typeof(WaveformView), new PropertyMetadata(6.0, OnVisualPropertyChanged));

        public double CrossSize
        {
            get => (double)GetValue(CrossSizeProperty);
            set => SetValue(CrossSizeProperty, value);
        }

        public static readonly DependencyProperty WaveGridBorderTickLineStyleProperty =
            DependencyProperty.Register(nameof(WaveGridBorderTickLineStyle), typeof(GridLineStyle), typeof(WaveformView), new PropertyMetadata(GridLineStyle.Solid, OnVisualPropertyChanged));

        public GridLineStyle WaveGridBorderTickLineStyle
        {
            get => (GridLineStyle)GetValue(WaveGridBorderTickLineStyleProperty);
            set => SetValue(WaveGridBorderTickLineStyleProperty, value);
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
        private int GetActualRowTickCount(double height)
        {
            if (string.Equals(RowTickCount, "Auto", StringComparison.OrdinalIgnoreCase))
            {
                // 自动计算，示例：每40像素一格，最少2格
                return Math.Max(2, (int)(height / 40));
            }
            else if (int.TryParse(RowTickCount, out int val))
            {
                return val; // 不受min/max限制
            }
            else
            {
                return 8; // 默认
            }
        }
        private int GetActualColumnTickCount(double width)
        {
            if (string.Equals(ColumnTickCount, "Auto", StringComparison.OrdinalIgnoreCase))
            {
                return Math.Max(2, (int)(width / 40));
            }
            else if (int.TryParse(ColumnTickCount, out int val))
            {
                return val;
            }
            else
            {
                return 8;
            }
        }

        private float? _cursorX = null;

        private void OnWaveformPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            var point = e.GetCurrentPoint(WaveformDemonstratorCanvasControl).Position;
            _cursorX = (float)point.X;
            WaveformDemonstratorCanvasControl.Invalidate(); // 触发重绘
        }


        private void SetDefaultColors()
        {
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
            WaveformDemonstratorCanvasControl.PointerMoved += OnWaveformPointerMoved;

        }

        private void WaveformDemonstratorCanvasControl_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            var ds = args.DrawingSession;
            float width = (float)sender.ActualWidth;
            float height = (float)sender.ActualHeight;

            var margin = WaveGridBorderMargin;
            float gridLeft = (float)margin.Left;
            float gridTop = (float)margin.Top;
            float gridRight = width - (float)margin.Right;
            float gridBottom = height - (float)margin.Bottom;
            float gridWidth = gridRight - gridLeft;
            float gridHeight = gridBottom - gridTop;

            var accentColor = GetThemeColor("AccentTextFillColorTertiaryBrush", Microsoft.UI.Colors.DeepSkyBlue);
            var cautionColor = GetThemeColor("SystemFillColorSuccessBrush", Microsoft.UI.Colors.Orange);

            // 绘制正弦波
            int sampleCount = 1000;
            float amplitude = gridHeight / 2f;
            float centerY = gridTop + amplitude;

            float prevX = gridLeft;
            float prevY = centerY;

            for (int i = 0; i < sampleCount; i++)
            {
                float x = gridLeft + i * gridWidth / (sampleCount - 1);
                float phase = (i / (float)(sampleCount - 1)) * 2 * MathF.PI * 2; // 两周期
                float y = centerY - MathF.Sin(phase) * amplitude;

                if (i > 0)
                    ds.DrawLine(prevX, prevY, x, y, accentColor, 2f);

                prevX = x;
                prevY = y;
            }

            // 绘制贯穿十字虚线（如果鼠标位置有效）
            if (_cursorX.HasValue && _cursorX.Value >= gridLeft && _cursorX.Value <= gridRight)
            {
                float normalizedX = (_cursorX.Value - gridLeft) / gridWidth;
                float phase = normalizedX * 2 * MathF.PI * 2;
                float y = centerY - MathF.Sin(phase) * amplitude;

                float dashWidth = 1.5f;

                // 横向虚线
                DrawDottedLine(ds, gridLeft, y, gridRight, y, cautionColor, dashWidth);

                // 纵向虚线
                DrawDottedLine(ds, _cursorX.Value, gridTop, _cursorX.Value, gridBottom, cautionColor, dashWidth);
            }
        }



        private void LineDemonstratorCanvasControl_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            var ds = args.DrawingSession;
            var grid = GetGridBounds(sender.ActualWidth, sender.ActualHeight);

            int rowCount = GetActualRowTickCount(grid.Height);
            int colCount = GetActualColumnTickCount(grid.Width);

            var rowPositions = GetTickPositions(grid.Top, grid.Height, rowCount);
            var colPositions = GetTickPositions(grid.Left, grid.Width, colCount);

            int rowCenterIndex = GetCenterIndex(rowCount);
            int colCenterIndex = GetCenterIndex(colCount);

            var colors = GetTickColors();

            DrawGridBorder(ds, grid, colors.BorderColor);
            DrawTickLines(ds, grid, rowPositions, colPositions, rowCenterIndex, colCenterIndex, colors);
            DrawBorderTicks(args, grid, rowPositions, colPositions, rowCenterIndex, colCenterIndex, colors);
            DrawIntersections(ds, rowPositions, colPositions, rowCenterIndex, colCenterIndex, colors);
        }

        private struct GridBounds
        {
            public float Left, Top, Right, Bottom;
            public float Width => Right - Left;
            public float Height => Bottom - Top;
        }

        private struct TickColors
        {
            public Windows.UI.Color BorderColor, RowColor, ColColor, RowCenterColor, ColCenterColor;
        }

        private GridBounds GetGridBounds(double width, double height)
        {
            var margin = WaveGridBorderMargin;
            return new GridBounds
            {
                Left = (float)margin.Left,
                Top = (float)margin.Top,
                Right = (float)(width - margin.Right),
                Bottom = (float)(height - margin.Bottom)
            };
        }

        private List<float> GetTickPositions(float start, float length, int count)
        {
            var positions = new List<float>();
            for (int i = 1; i < count; i++)
                positions.Add(start + length * i / count);
            return positions;
        }

        private int GetCenterIndex(int count) => (count % 2 == 0) ? (count / 2 - 1) : -1;

        private TickColors GetTickColors()
        {
            return new TickColors
            {
                BorderColor = GetBrushColor(WaveGridBorderBrush, GetThemeColor("TextFillColorTertiaryBrush", Microsoft.UI.Colors.Gray)),
                RowColor = GetBrushColor(RowTickLineBrush, GetThemeColor("TextFillColorTertiaryBrush", Microsoft.UI.Colors.Gray)),
                ColColor = GetBrushColor(ColumnTickLineBrush, GetThemeColor("TextFillColorTertiaryBrush", Microsoft.UI.Colors.Gray)),
                RowCenterColor = GetBrushColor(RowCenterTickLineBrush, GetThemeColor("AccentTextFillColorPrimaryBrush", Microsoft.UI.Colors.DeepSkyBlue)),
                ColCenterColor = GetBrushColor(ColumnCenterTickLineBrush, GetThemeColor("AccentTextFillColorPrimaryBrush", Microsoft.UI.Colors.DeepSkyBlue))
            };
        }

        // 根据线型绘制线
        private void DrawLineStyle(CanvasDrawingSession ds, float x1, float y1, float x2, float y2, Windows.UI.Color color, float width, GridLineStyle style)
        {
            switch (style)
            {
                case GridLineStyle.None:
                    return;
                case GridLineStyle.Solid:
                    ds.DrawLine(x1, y1, x2, y2, color, width);
                    return;
                case GridLineStyle.Dot:
                    DrawDottedLine(ds, x1, y1, x2, y2, color, width);
                    return;
                case GridLineStyle.Dash:
                    DrawDashedLine(ds, x1, y1, x2, y2, color, width);
                    return;
            }
        }


        private void DrawDottedLine(CanvasDrawingSession ds, float x1, float y1, float x2, float y2, Windows.UI.Color color, float width)
        {
            // 预计算方向向量和长度
            float dx = x2 - x1;
            float dy = y2 - y1;
            float length = (float)Math.Sqrt(dx * dx + dy * dy);
            float dotSpacing = 4f;
            int dotCount = Math.Max(1, (int)(length / dotSpacing));
            float step = 1f / dotCount;
            float halfStep = step * 0.5f;

            // 批量计算所有点
            float dotSize = halfStep * length; // 每个点的实际长度

            for (int i = 0; i < dotCount; i++)
            {
                float t = i * step;
                float sx = x1 + dx * (t);
                float sy = y1 + dy * t;
                float ex = x1 + dx * (t + halfStep);
                float ey = y1 + dy * (t + halfStep);
                ds.DrawLine(sx, sy, ex, ey, color, width);
            }
        }

        private void DrawDashedLine(CanvasDrawingSession ds, float x1, float y1, float x2, float y2, Windows.UI.Color color, float width)
        {
            // 预计算方向向量和长度
            float dx = x2 - x1;
            float dy = y2 - y1;
            float length = (float)Math.Sqrt(dx * dx + dy * dy);
            float dashLength = 8f;
            float gapLength = 4f;
            float segmentLength = dashLength + gapLength;

            float drawn = 0f;
            while (drawn < length)
            {
                float t1 = drawn / length;
                float t2 = Math.Min((drawn + dashLength) / length, 1f);

                float sx = x1 + dx * t1;
                float sy = y1 + dy * t1;
                float ex = x1 + dx * t2;
                float ey = y1 + dy * t2;

                ds.DrawLine(sx, sy, ex, ey, color, width);
                drawn += segmentLength;
            }
        }
        private void DrawGridBorder(CanvasDrawingSession ds, GridBounds grid, Windows.UI.Color color)
        {
            float left = grid.Left;
            float top = grid.Top;
            float right = grid.Right;
            float bottom = grid.Bottom;

            float leftWidth = (float)WaveGridBorderThickness.Left;
            float topWidth = (float)WaveGridBorderThickness.Top;
            float rightWidth = (float)WaveGridBorderThickness.Right;
            float bottomWidth = (float)WaveGridBorderThickness.Bottom;

            DrawLineStyle(ds, left, top, right, top, color, topWidth, WaveGridBorderLineStyle);       // Top
            DrawLineStyle(ds, right, top, right, bottom, color, rightWidth, WaveGridBorderLineStyle); // Right
            DrawLineStyle(ds, right, bottom, left, bottom, color, bottomWidth, WaveGridBorderLineStyle); // Bottom
            DrawLineStyle(ds, left, bottom, left, top, color, leftWidth, WaveGridBorderLineStyle);    // Left
        }
        private void DrawIntersections(CanvasDrawingSession ds, List<float> rowPositions, List<float> colPositions, int rowCenterIndex, int colCenterIndex, TickColors colors)
        {
            if (WaveGridTickMode == GridTickModeStyle.None)
                return;

            float crossSize = (float)CrossSize;
            float lineWidth = 1.5f;
            float dotRadius = 1f;

            foreach (var (y, rowIdx) in rowPositions.Select((v, i) => (v, i)))
            {
                foreach (var (x, colIdx) in colPositions.Select((v, i) => (v, i)))
                {
                    var isRowCenter = rowIdx == rowCenterIndex;
                    var isColCenter = colIdx == colCenterIndex;

                    Windows.UI.Color color = isRowCenter ? colors.RowCenterColor :
                                              isColCenter ? colors.ColCenterColor :
                                              colors.ColColor;

                    if (WaveGridTickMode == GridTickModeStyle.Intersection)
                    {
                        ds.FillCircle(x, y, dotRadius, color);
                    }
                    else if (WaveGridTickMode == GridTickModeStyle.Cross)
                    {
                        float half = crossSize / 2;
                        ds.DrawLine(x - half, y, x + half, y, color, lineWidth);
                        ds.DrawLine(x, y - half, x, y + half, color, lineWidth);
                    }
                }
            }
        }

        private void DrawTickLines(CanvasDrawingSession ds, GridBounds grid, List<float> rowPositions, List<float> colPositions, int rowCenterIndex, int colCenterIndex, TickColors colors)
        {
            foreach (var (y, i) in rowPositions.Select((v, idx) => (v, idx)))
            {
                var color = (i == rowCenterIndex) ? colors.RowCenterColor : colors.RowColor;
                DrawLineStyle(ds, grid.Left, y, grid.Right, y, color, (float)RowTickLineWidth, WaveGridTickLineStyle);
            }

            foreach (var (x, i) in colPositions.Select((v, idx) => (v, idx)))
            {
                var color = (i == colCenterIndex) ? colors.ColCenterColor : colors.ColColor;
                DrawLineStyle(ds, x, grid.Top, x, grid.Bottom, color, (float)ColumnTickLineWidth, WaveGridTickLineStyle);
            }
        }

        private void DrawBorderTicks(CanvasDrawEventArgs args, GridBounds grid, List<float> rowPositions, List<float> colPositions, int rowCenterIndex, int colCenterIndex, TickColors colors)
        {
            var ds = args.DrawingSession;
            float tickLineWidth = 1.5f;
            float subTickLineWidth = Math.Max(0.5f, tickLineWidth * 0.75f);

            // 主刻度线
            DrawTickSegments(ds, rowPositions, grid.Left, grid.Left + (float)WaveGridBorderTickThickness.Left, rowCenterIndex, colors.RowColor, colors.RowCenterColor, true);
            DrawTickSegments(ds, rowPositions, grid.Right, grid.Right - (float)WaveGridBorderTickThickness.Right, rowCenterIndex, colors.RowColor, colors.RowCenterColor, true);
            DrawTickSegments(ds, colPositions, grid.Top, grid.Top + (float)WaveGridBorderTickThickness.Top, colCenterIndex, colors.ColColor, colors.ColCenterColor, false);
            DrawTickSegments(ds, colPositions, grid.Bottom, grid.Bottom - (float)WaveGridBorderTickThickness.Bottom, colCenterIndex, colors.ColColor, colors.ColCenterColor, false);

            // 子刻度线
            DrawSubTicks(ds, grid, rowPositions, colPositions, subTickLineWidth, colors);
        }
        private void DrawSubTicks(CanvasDrawingSession ds, GridBounds grid, List<float> rowPositions, List<float> colPositions, float subTickLineWidth, TickColors colors)
        {
            int rowSubCells = Math.Max(0, RowGridBorderSubTickCount);
            int colSubCells = Math.Max(0, ColumnGridBorderSubTickCount);
            int rowSubTicksPerSegment = Math.Max(0, rowSubCells - 1);
            int colSubTicksPerSegment = Math.Max(0, colSubCells - 1);

            float subTickLeft = (float)WaveGridBorderSubTickThickness.Left;
            float subTickTop = (float)WaveGridBorderSubTickThickness.Top;
            float subTickRight = (float)WaveGridBorderSubTickThickness.Right;
            float subTickBottom = (float)WaveGridBorderSubTickThickness.Bottom;

            // 行方向子刻度（垂直线）
            var rowSegments = new List<(float start, float end)>();
            rowSegments.Add((grid.Top, rowPositions.First())); // 边框到第一个刻度
            for (int i = 0; i < rowPositions.Count - 1; i++)
                rowSegments.Add((rowPositions[i], rowPositions[i + 1]));
            rowSegments.Add((rowPositions.Last(), grid.Bottom)); // 最后一个刻度到边框

            foreach (var (start, end) in rowSegments)
            {
                for (int j = 1; j <= rowSubTicksPerSegment; j++)
                {
                    float y = start + (end - start) * j / rowSubCells;
                    DrawLineStyle(ds, grid.Left, y, grid.Left + subTickLeft, y, colors.RowColor, subTickLineWidth, WaveGridBorderTickLineStyle);
                    DrawLineStyle(ds, grid.Right, y, grid.Right - subTickRight, y, colors.RowColor, subTickLineWidth, WaveGridBorderTickLineStyle);
                }
            }

            // 列方向子刻度（水平线）
            var colSegments = new List<(float start, float end)>();
            colSegments.Add((grid.Left, colPositions.First()));
            for (int i = 0; i < colPositions.Count - 1; i++)
                colSegments.Add((colPositions[i], colPositions[i + 1]));
            colSegments.Add((colPositions.Last(), grid.Right));

            foreach (var (start, end) in colSegments)
            {
                for (int j = 1; j <= colSubTicksPerSegment; j++)
                {
                    float x = start + (end - start) * j / colSubCells;
                    DrawLineStyle(ds, x, grid.Top, x, grid.Top + subTickTop, colors.ColColor, subTickLineWidth, WaveGridBorderTickLineStyle);
                    DrawLineStyle(ds, x, grid.Bottom, x, grid.Bottom - subTickBottom, colors.ColColor, subTickLineWidth, WaveGridBorderTickLineStyle);
                }
            }
        }


        private void DrawTickSegments(CanvasDrawingSession ds, List<float> positions, float start, float end, int centerIndex, Windows.UI.Color normalColor, Windows.UI.Color centerColor, bool horizontal)
        {
            foreach (var (pos, i) in positions.Select((p, idx) => (p, idx)))
            {
                var color = (i == centerIndex) ? centerColor : normalColor;
                if (horizontal)
                    DrawLineStyle(ds, start, pos, end, pos, color, 1.5f, WaveGridBorderTickLineStyle);
                else
                    DrawLineStyle(ds, pos, start, pos, end, color, 1.5f, WaveGridBorderTickLineStyle);
            }
        }

        private Windows.UI.Color GetBrushColor(Brush brush, Windows.UI.Color fallback)
        {
            if (brush is SolidColorBrush solid)
                return solid.Color;
            return fallback;
        }
        private Windows.UI.Color GetThemeColor(string resourceKey, Windows.UI.Color fallback)
        {
            if (Application.Current.Resources.TryGetValue(resourceKey, out var brushObj) && brushObj is SolidColorBrush brush)
                return brush.Color;
            return fallback;
        }
        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            //if(LineDemonstratorCanvasControl != null) LineDemonstratorCanvasControl.RemoveFromVisualTree();
            //if(WaveformDemonstratorCanvasControl != null) WaveformDemonstratorCanvasControl.RemoveFromVisualTree();
        }
    }
}
