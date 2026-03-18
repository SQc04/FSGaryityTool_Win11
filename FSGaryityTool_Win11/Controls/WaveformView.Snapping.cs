using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace FSGaryityTool_Win11.Controls
{
    public partial class WaveformView
    {
        public interface IDataSnappingProvider
        {
            /// <summary>
            /// 根据鼠标位置和绘图区域，返回吸附后的点坐标（像素坐标）。
            /// </summary>
            (float x, float y)? GetSnappedPoint(
            float cursorX,
            float cursorY,
            GridBounds grid,
            IEnumerable<WaveformDataSource> sources,
            double minX,
            double maxX,
            double minY,
            double maxY);
        }

        public class UnifiedSnappingProvider : IDataSnappingProvider
        {
            private readonly SnappingAxis axis;

            public UnifiedSnappingProvider(SnappingAxis axis)
            {
                this.axis = axis;
            }

            public (float x, float y)? GetSnappedPoint(
                float cursorX,
                float cursorY,
                GridBounds grid,
                IEnumerable<WaveformDataSource> sources,
                double minX,
                double maxX,
                double minY,
                double maxY)
            {
                // 将鼠标像素坐标转换为逻辑坐标
                var logicalCursor = WaveformView.MapToLogical(new Point(cursorX, cursorY), grid, minX, maxX, minY, maxY);
                (double x, double y)? bestLogical = null;
                double minDist = double.MaxValue;

                double rangeX = Math.Max(1e-9, maxX - minX);
                double rangeY = Math.Max(1e-9, maxY - minY);

                foreach (var source in sources)
                {
                    IEnumerable<(double x, double y)> logicalPoints = null;

                    if (source.PolylinePointsData != null && source.PolylinePointsData.Count > 1)
                    {
                        // Polyline points are in logical coordinates (apply offsets)
                        logicalPoints = source.PolylinePointsData.Select(p => (x: p.x + source.OffSetX, y: p.y + source.OffSetY));
                    }
                    else if (source.ParametricFunctionData != null)
                    {
                        // Sample parametric function. Increase sample density when horizontally zoomed.
                        double hZoom = source.Owner?.HorizontalZoomScale ?? 1.0;
                        double vZoom = source.Owner?.VerticalZoomScale ?? 1.0;
                        double zoomFactor = Math.Max(1.0, Math.Max(hZoom, vZoom));
                        int sampleCount = Math.Clamp((int)(source.EffectiveCount * zoomFactor), 64, 4000);
                        var list = new List<(double x, double y)>(sampleCount);
                        for (int i = 0; i < sampleCount; i++)
                        {
                            float t = i / (float)(sampleCount - 1);
                            var p = source.ParametricFunctionData(t);
                            list.Add((p.x + source.OffSetX, p.y + source.OffSetY));
                        }
                        logicalPoints = list;
                    }
                    else if (source.FunctionFormulaData != null)
                    {
                        // Legacy function mode: sample across current view (do not limit by Owner min/max)
                        // Increase sampling when zoomed in so snapping can find closer points.
                        int baseCount = source.EffectiveCount;
                        double hZoom = source.Owner?.HorizontalZoomScale ?? 1.0;
                        double vZoom = source.Owner?.VerticalZoomScale ?? 1.0;
                        double zoomFactor = Math.Max(1.0, Math.Max(hZoom, vZoom));
                        int sampleCount = Math.Clamp((int)(baseCount * zoomFactor), 64, 4000);
                        // Match DrawFunction's sampling: include overdraw so snapping aligns with rendering
                        double viewRangeX = maxX - minX;
                        double overdraw = viewRangeX * 0.3;
                        double drawMinX = minX - overdraw;
                        double drawMaxX = maxX + overdraw;
                        double fullRangeX = (source.Owner != null) ? (source.Owner.MaxHorizontalValue - source.Owner.MinHorizontalValue) : Math.Max(1e-12, viewRangeX);
                        double originX = source.OffSetX;
                        logicalPoints = Enumerable.Range(0, sampleCount).Select(i =>
                        {
                            float t = i / (float)(sampleCount - 1);
                            double x = drawMinX + t * (drawMaxX - drawMinX);

                            double tNormalized = Math.Abs(fullRangeX) > 1e-12 ? (x - originX) / fullRangeX : t;
                            double y = source.FunctionFormulaData((float)tNormalized) + source.OffSetY;
                            return (x, y);
                        });
                    }
                    else
                    {
                        continue;
                    }

                    foreach (var pt in logicalPoints)
                    {
                        // Accept all points (including those outside current view) so snapping works
                        // even when data lies outside the default/view bounds.
                        // Map logical sample to canvas pixel coordinates and compute pixel delta
                        // against the raw cursor position — this avoids subtle inconsistencies
                        // with range-to-pixel conversion when the view is panned/scaled.
                        var canvasPt = WaveformView.MapToCanvas(new Point(pt.x, pt.y), grid, minX, maxX, minY, maxY);
                        double dxPixels = canvasPt.X - cursorX;
                        double dyPixels = canvasPt.Y - cursorY;

                        double dist = axis switch
                        {
                            SnappingAxis.Horizontal => Math.Abs(dxPixels),
                            SnappingAxis.Vertical => Math.Abs(dyPixels),
                            SnappingAxis.Closest => Math.Sqrt(dxPixels * dxPixels + dyPixels * dyPixels),
                            _ => double.MaxValue
                        };

                        if (dist < minDist)
                        {
                            minDist = dist;
                            bestLogical = pt;
                        }
                    }
                }

                if (bestLogical.HasValue)
                {
                    var mapped = WaveformView.MapToCanvas(
                        new Point(bestLogical.Value.x, bestLogical.Value.y),
                        grid, minX, maxX, minY, maxY);
                    return ((float)mapped.X, (float)mapped.Y);
                }

                return null;
            }
        }

        public class MouseSnappingProvider : IDataSnappingProvider
        {
            public (float x, float y)? GetSnappedPoint(float cursorX, float cursorY, GridBounds grid, IEnumerable<WaveformDataSource> sources, double minX, double maxX, double minY, double maxY)
            {
                // Simply return the cursor position (pixel coordinates)
                return (cursorX, cursorY);
            }
        }

        private IDataSnappingProvider? GetSnappingProvider(IndicatorMode mode)
        {
            return mode switch
            {
                IndicatorMode.ClosestData => new UnifiedSnappingProvider(SnappingAxis.Closest),
                IndicatorMode.HorizontalData => new UnifiedSnappingProvider(SnappingAxis.Horizontal),
                IndicatorMode.VerticalData => new UnifiedSnappingProvider(SnappingAxis.Vertical),
                IndicatorMode.Mouse => new MouseSnappingProvider(),
                _ => null
            };
        }
    }
}
