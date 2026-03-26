using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
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
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FSGaryityTool_Win11.Controls
{
    public sealed partial class PolyView : UserControl
    {
        


        private static void OnVisualPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PolyView view)
            {
                view.LineDemonstratorCanvasControl.Invalidate();
            }
        }
        

        public PolyView()
        {
            InitializeComponent();
        }

        private void LineDemonstratorCanvasControl_Draw(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs args)
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

        public void Invalidate()
        {
            LineDemonstratorCanvasControl.Invalidate();
        }
        private int GetActualRowTickCount(double height)
        {
            return GetActualTickCount(RowTickCount, height, MinRowTickHeight, MaxRowTickHeight,
                MinRowTickCount, MaxRowTickCount, RowAutoGridMode, RowAutoGridMultiple);
        }

        private int GetActualColumnTickCount(double width)
        {
            return GetActualTickCount(ColumnTickCount, width, MinColumnTickWidth, MaxColumnTickWidth,
                MinColumnTickCount, MaxColumnTickCount, ColumnAutoGridMode, ColumnAutoGridMultiple);
        }
        private int GetActualTickCount(string tickCountSetting, double length, double minTickSize, double maxTickSize, int minTickCount, int maxTickCount, AutoGridMode mode, int multiple)
        {
            if (string.Equals(tickCountSetting, "Auto", StringComparison.OrdinalIgnoreCase))
            {
                double clampedTickSize = Math.Clamp(minTickSize, 1.0, maxTickSize);
                int rawCount = (int)(length / clampedTickSize);
                int clampedCount = Math.Clamp(rawCount, minTickCount, maxTickCount);

                return mode switch
                {
                    AutoGridMode.Even => (clampedCount % 2 == 0) ? clampedCount : clampedCount + 1,
                    AutoGridMode.Odd => (clampedCount % 2 == 1) ? clampedCount : clampedCount + 1,
                    AutoGridMode.MultipleOfN => Math.Max(multiple, (clampedCount / multiple) * multiple),
                    AutoGridMode.PowerOfN => GetNearestPowerOfN(clampedCount, multiple),
                    _ => clampedCount
                };
            }

            if (int.TryParse(tickCountSetting, out int manualCount))
                return manualCount;

            return 8;
        }
        private int GetNearestPowerOfN(int target, int baseN)
        {
            if (baseN < 2) baseN = 2; // 最小底数为2
            int power = 1;
            while (power < target)
                power *= baseN;
            return power;
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
            if (WaveGridTickMode == TickModeStyle.None)
                return;

            float crossSize = (float)CrossSize;
            float lineWidth = 1.5f;
            float dotRadius = (float)DotRadius;

            foreach (var (y, rowIdx) in rowPositions.Select((v, i) => (v, i)))
            {
                foreach (var (x, colIdx) in colPositions.Select((v, i) => (v, i)))
                {
                    var isRowCenter = rowIdx == rowCenterIndex;
                    var isColCenter = colIdx == colCenterIndex;

                    Windows.UI.Color color = isRowCenter ? colors.RowCenterColor :
                                              isColCenter ? colors.ColCenterColor :
                                              colors.ColColor;

                    if (WaveGridTickMode == TickModeStyle.Intersection)
                    {
                        ds.FillCircle(x, y, dotRadius, color);
                    }
                    else if (WaveGridTickMode == TickModeStyle.Cross)
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

            // 主刻度线
            DrawTickSegments(ds, rowPositions, grid.Left, grid.Left + (float)WaveGridBorderTickThickness.Left, rowCenterIndex, colors.RowColor, colors.RowCenterColor, true);
            DrawTickSegments(ds, rowPositions, grid.Right, grid.Right - (float)WaveGridBorderTickThickness.Right, rowCenterIndex, colors.RowColor, colors.RowCenterColor, true);
            DrawTickSegments(ds, colPositions, grid.Top, grid.Top + (float)WaveGridBorderTickThickness.Top, colCenterIndex, colors.ColColor, colors.ColCenterColor, false);
            DrawTickSegments(ds, colPositions, grid.Bottom, grid.Bottom - (float)WaveGridBorderTickThickness.Bottom, colCenterIndex, colors.ColColor, colors.ColCenterColor, false);

            // 子刻度线
            DrawSubTicks(ds, grid, rowPositions, colPositions, colors);
        }
        private void DrawSubTicks(CanvasDrawingSession ds, GridBounds grid, List<float> rowPositions, List<float> colPositions, TickColors colors)
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
                    DrawLineStyle(ds, grid.Left, y, grid.Left + subTickLeft, y, colors.RowColor, (float)RowSubTickLineWidth, WaveGridBorderTickLineStyle);
                    DrawLineStyle(ds, grid.Right, y, grid.Right - subTickRight, y, colors.RowColor, (float)RowSubTickLineWidth, WaveGridBorderTickLineStyle);
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
                    DrawLineStyle(ds, x, grid.Top, x, grid.Top + subTickTop, colors.ColColor, (float)ColumnSubTickLineWidth, WaveGridBorderTickLineStyle);
                    DrawLineStyle(ds, x, grid.Bottom, x, grid.Bottom - subTickBottom, colors.ColColor, (float)ColumnSubTickLineWidth, WaveGridBorderTickLineStyle);
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
        private void DrawLineStyle(CanvasDrawingSession ds, float x1, float y1, float x2, float y2, Windows.UI.Color color, float width, LineStyle style)
        {
            if (style == LineStyle.None)
                return;

            if (ViewDrawMode == DrawMode.BranchPrediction)
            {
                switch (style)
                {
                    case LineStyle.Solid:
                        ds.DrawLine(x1, y1, x2, y2, color, width);
                        return;
                    case LineStyle.Dot:
                        DrawDottedLine(ds, x1, y1, x2, y2, color, width);
                        return;
                    case LineStyle.Dash:
                        DrawDashedLine(ds, x1, y1, x2, y2, color, width);
                        return;
                }
            }
            else
            {
                CanvasStrokeStyle? strokeStyle = style switch
                {
                    LineStyle.Solid => null,
                    LineStyle.Dash => new CanvasStrokeStyle { DashStyle = CanvasDashStyle.Dash },
                    LineStyle.Dot => new CanvasStrokeStyle
                    {
                        DashStyle = CanvasDashStyle.Solid,
                        CustomDashStyle = [0.1f, 3f],
                        DashCap = CanvasCapStyle.Round
                    },
                    _ => null
                };

                if (strokeStyle != null)
                    ds.DrawLine(x1, y1, x2, y2, color, width, strokeStyle);
                else
                    ds.DrawLine(x1, y1, x2, y2, color, width);
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
    }
}
