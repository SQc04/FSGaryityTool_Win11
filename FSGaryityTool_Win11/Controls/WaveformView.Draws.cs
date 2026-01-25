using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace FSGaryityTool_Win11.Controls
{
    public partial class WaveformView
    {
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



        private void DrawGeometryStyle(CanvasDrawingSession ds, CanvasGeometry geometry, Windows.UI.Color color, float thickness, LineStyle style)
        {
            if (style == LineStyle.None) return;

            var strokeStyle = style switch
            {
                LineStyle.Solid => new CanvasStrokeStyle(),
                LineStyle.Dash => new CanvasStrokeStyle { DashStyle = CanvasDashStyle.Dash },
                LineStyle.Dot => new CanvasStrokeStyle
                {
                    DashStyle = CanvasDashStyle.Solid,
                    CustomDashStyle = new float[] { 0.1f, 4f }, // 点线
                    DashCap = CanvasCapStyle.Round
                },
                _ => new CanvasStrokeStyle()
            };

            ds.DrawGeometry(geometry, color, thickness, strokeStyle);
        }

        /// <summary>
        /// 绘制波形的折线数据（Polyline），将数据点从逻辑坐标映射到画布像素坐标，并按指定样式绘制路径。
        /// 支持降采样（最小像素间隔），根据缩放比例自动调整采样密度，提升性能。
        /// </summary>
        /// <param name="sender">CanvasVirtualControl 实例，用于获取设备和绘制相关信息。</param>
        /// <param name="ds">CanvasDrawingSession 绘图会话，用于实际绘制。</param>
        /// <param name="source">波形数据源，包含折线点、样式、颜色等信息。</param>
        /// <param name="grid">像素空间的绘制区域边界（GridBounds）。</param>
        /// <param name="viewMinX">当前逻辑坐标系的最小 X 边界。</param>
        /// <param name="viewMaxX">当前逻辑坐标系的最大 X 边界。</param>
        /// <param name="viewMinY">当前逻辑坐标系的最小 Y 边界。</param>
        /// <param name="viewMaxY">当前逻辑坐标系的最大 Y 边界。</param>
        /// <param name="zoomX">X 轴缩放比例，用于调整采样密度和像素间隔。</param>
        private void DrawPolyLines(CanvasVirtualControl sender, CanvasDrawingSession ds, WaveformDataSource source, GridBounds grid, double viewMinX, double viewMaxX, double viewMinY, double viewMaxY, double zoomX)
        {
            if (source.PolylinePointsData is not { Count: > 1 } points)
                return;

            var brush = source.StrokeBrush;
            var thickness = source.StrokeThickness;
            var style = source.LineStyle;
            var color = GetBrushColor(brush);

            var builder = new CanvasPathBuilder(sender.Device);
            bool first = true;

            // 计算最小像素间隔，缩小时增大间隔
            float minPixelDist = 1.5f;
            if (zoomX < 1.0)
                minPixelDist = (float)(1.5 / zoomX);

            float lastX = float.MinValue, lastY = float.MinValue;

            foreach (var rawPt in points)
            {
                var logicalPoint = ApplyOffset(new Point(rawPt.x, rawPt.y), source);
                var canvasPt = MapToCanvas(logicalPoint, grid, viewMinX, viewMaxX, viewMinY, viewMaxY);

                if (!first && Math.Abs(canvasPt.X - lastX) < minPixelDist && Math.Abs(canvasPt.Y - lastY) < minPixelDist)
                    continue;

                if (first)
                {
                    builder.BeginFigure(canvasPt);
                    first = false;
                }
                else
                {
                    builder.AddLine(canvasPt);
                }

                lastX = canvasPt.X;
                lastY = canvasPt.Y;
            }

            if (!first)
            {
                builder.EndFigure(CanvasFigureLoop.Open);
                using var geometry = CanvasGeometry.CreatePath(builder);
                DrawGeometryStyle(ds, geometry, color, thickness, style);
            }
        }

        /// <summary>
        /// 绘制函数波形曲线，将函数采样点从逻辑坐标映射到画布像素坐标，并按指定样式绘制路径。
        /// 支持自动采样点数、超范围预绘制（overdraw），并对异常点和可视区外点进行裁剪处理，提升性能和显示效果。
        /// </summary>
        /// <param name="sender">CanvasVirtualControl 实例，用于获取设备和绘制相关信息。</param>
        /// <param name="ds">CanvasDrawingSession 绘图会话，用于实际绘制。</param>
        /// <param name="source">波形数据源，包含函数公式、样式、颜色等信息。</param>
        /// <param name="grid">像素空间的绘制区域边界（GridBounds）。</param>
        /// <param name="viewMinX">当前逻辑坐标系的最小 X 边界。</param>
        /// <param name="viewMaxX">当前逻辑坐标系的最大 X 边界。</param>
        /// <param name="viewMinY">当前逻辑坐标系的最小 Y 边界。</param>
        /// <param name="viewMaxY">当前逻辑坐标系的最大 Y 边界。</param>
        private void DrawFunction(CanvasVirtualControl sender, CanvasDrawingSession ds, WaveformDataSource source,
                                GridBounds grid, double viewMinX, double viewMaxX, double viewMinY, double viewMaxY)
        {
            if (source.FunctionFormulaData is null)
                return;

            var brush = source.StrokeBrush;
            var thickness = source.StrokeThickness;
            var style = source.LineStyle;
            var color = GetBrushColor(brush);

            int count = source.IsAutoCount ? GetOptimalSampleCount(source) : source.EffectiveCount;



            double viewRangeX = viewMaxX - viewMinX;
            double overdraw = viewRangeX * 0.3;
            double viewRangeY = viewMaxY - viewMinY;
            double overdrawY = viewRangeY * 0.3;

            double drawMinX = viewMinX - overdraw;
            double drawMaxX = viewMaxX + overdraw;
            double drawMinY = viewMinY - overdrawY;
            double drawMaxY = viewMaxY + overdrawY;

            double fullRangeX = MaxHorizontalValue - MinHorizontalValue;
            if (fullRangeX <= 0) fullRangeX = 1.0;
            double fullRangeY = MaxVerticalValue - MinVerticalValue;
            if (fullRangeY <= 0) fullRangeY = 1.0;

            double originX = source.OffSetX;
            double originY = source.OffSetY;

            var builder = new CanvasPathBuilder(sender.Device);
            bool hasAnyFigure = false;
            bool inFigure = false;

            // 上一个点
            double? lastX = null, lastY = null;
            Vector2? lastCanvasPt = null;

            for (int i = 0; i < count; i++)
            {
                float tScreen = i / (float)(count - 1);
                float xScreen = (float)(drawMinX + tScreen * (drawMaxX - drawMinX));

                double offsetFromOrigin = xScreen - originX;
                float tNorm = (float)(offsetFromOrigin / fullRangeX);

                float yRaw = source.FunctionFormulaData(tNorm);

                double trueLogicX = originX + tNorm * fullRangeX;
                double trueLogicY = yRaw + source.OffSetY;

                var trueLogicalPoint = new Point(trueLogicX, trueLogicY);
                var canvasPt = MapToCanvas(trueLogicalPoint, grid, viewMinX, viewMaxX, viewMinY, viewMaxY);

                // 跳过极端异常点
                if (float.IsInfinity(canvasPt.X) || float.IsInfinity(canvasPt.Y) ||
                    canvasPt.X < -50000 || canvasPt.X > ActualWidth + 50000 ||
                    canvasPt.Y < -50000 || canvasPt.Y > ActualHeight + 50000)
                {
                    if (inFigure)
                    {
                        builder.EndFigure(CanvasFigureLoop.Open);
                        inFigure = false;
                    }
                    lastX = null;
                    lastY = null;
                    lastCanvasPt = null;
                    continue;
                }

                if (lastX.HasValue && lastY.HasValue && lastCanvasPt.HasValue)
                {
                    bool bothLeft = trueLogicX < viewMinX && lastX.Value < viewMinX;
                    bool bothRight = trueLogicX > viewMaxX && lastX.Value > viewMaxX;
                    bool bothBelow = trueLogicY < viewMinY && lastY.Value < viewMinY;
                    bool bothAbove = trueLogicY > viewMaxY && lastY.Value > viewMaxY;

                    if (bothLeft || bothRight || bothBelow || bothAbove)
                    {
                        if (inFigure)
                        {
                            builder.EndFigure(CanvasFigureLoop.Open);
                            inFigure = false;
                        }
                        lastX = trueLogicX;
                        lastY = trueLogicY;
                        lastCanvasPt = canvasPt;
                        continue;
                    }
                }

                if (!inFigure)
                {
                    builder.BeginFigure(canvasPt);
                    inFigure = true;
                    hasAnyFigure = true;
                }
                else
                {
                    builder.AddLine(canvasPt);
                }

                lastX = trueLogicX;
                lastY = trueLogicY;
                lastCanvasPt = canvasPt;
            }

            if (inFigure)
            {
                builder.EndFigure(CanvasFigureLoop.Open);
            }

            if (hasAnyFigure)
            {
                using var geometry = CanvasGeometry.CreatePath(builder);
                DrawGeometryStyle(ds, geometry, color, thickness, style);
            }
        }


        // 绘制刻度标记
        private void DrawTickMarks(CanvasDrawingSession ds, WaveformDataSource source, GridBounds grid, double viewMinX, double viewMaxX, double viewMinY, double viewMaxY, Windows.UI.Color color)
        {
            if (source.TickMode == TickModeStyle.None || source.PolylinePointsData is not { Count: > 0 } pts)
                return;

            // 降采样：最小像素距离，缩小时自动减少绘制
            const float minPixelDist = 3f; // 可根据实际需求调整
            int maxTicks = 200000;           // 最大绘制数量，防止极端卡顿
            float lastX = float.MinValue, lastY = float.MinValue;
            int drawn = 0;

            foreach (var rawPt in pts)
            {
                var logicalPoint = ApplyOffset(new Point(rawPt.x, rawPt.y), source);
                var canvasPt = MapToCanvas(logicalPoint, grid, viewMinX, viewMaxX, viewMinY, viewMaxY);

                // 距离过近则跳过
                if (Math.Abs(canvasPt.X - lastX) < minPixelDist && Math.Abs(canvasPt.Y - lastY) < minPixelDist)
                    continue;
                if (++drawn > maxTicks)
                    break;

                if (source.TickMode == TickModeStyle.Cross)
                {
                    float s = (float)CrossSize;
                    ds.DrawLine(canvasPt.X - s, canvasPt.Y, canvasPt.X + s, canvasPt.Y, color, 1.5f);
                    ds.DrawLine(canvasPt.X, canvasPt.Y - s, canvasPt.X, canvasPt.Y + s, color, 1.5f);
                }
                else if (source.TickMode == TickModeStyle.Intersection)
                {
                    ds.FillCircle(canvasPt.X, canvasPt.Y, 4f, color);
                }

                lastX = canvasPt.X;
                lastY = canvasPt.Y;
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

        private long _frameCount;
        private long _totalElapsedMs;           // 用 long 避免精度损失
        private DateTime _fpsUpdateTime = DateTime.Now;
        private DateTime _lastFpsRecordTime = DateTime.Now;
        private double _fps = 0;

        private void SetFPSUpdateTime()
        {
            _frameCount++;

            _fpsUpdateTime = DateTime.Now;
            _totalElapsedMs += (long)(_fpsUpdateTime - _lastFpsRecordTime).TotalMilliseconds;
            _fps = 1000.0 / (_fpsUpdateTime - _lastFpsRecordTime).TotalMilliseconds;
            _lastFpsRecordTime = _fpsUpdateTime;
        }

        private void UpdateFPS()
        {
            if (_frameCount < 8)
            {
                ViewFPS = "—";
                return;
            }

            double avgMsPerFrame = (double)_totalElapsedMs / _frameCount;
            double fps = 1000.0 / avgMsPerFrame;


            ViewFPS = _fps.ToString("F1") + " | " + fps.ToString("F1");  // 或 "F2" 看需求

            // 可选：每 N 秒重置一次，防止 long 溢出或长期运行漂移
            if (_frameCount > 3600)  // 例如跑了1分钟左右
            {
                _frameCount = 0;
                _totalElapsedMs = 0;
                _lastFpsRecordTime = DateTime.Now;
            }

        }
    }
}
