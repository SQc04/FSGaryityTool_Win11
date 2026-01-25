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

                foreach (var source in sources)
                {
                    IEnumerable<(double x, double y)> logicalPoints;

                    if (source.PolylinePointsData != null && source.PolylinePointsData.Count > 1)
                    {
                        logicalPoints = source.PolylinePointsData.Select(p =>
                            (x: p.x + source.OffSetX, y: p.y + source.OffSetY));
                    }
                    else if (source.FunctionFormulaData != null)
                    {
                        double fullRangeX = source.Owner.MaxHorizontalValue - source.Owner.MinHorizontalValue;
                        logicalPoints = Enumerable.Range(0, source.EffectiveCount).Select(i =>
                        {
                            float t = i / (float)(source.EffectiveCount - 1);
                            double x = minX + t * (maxX - minX) + source.OffSetX; // 修正：加 OffSetX
                            double tNormalized = fullRangeX != 0 ? (x - source.Owner.MinHorizontalValue - source.OffSetX) / fullRangeX : t;
                            tNormalized = Math.Clamp(tNormalized, 0, 1);
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
                        // 过滤超界点
                        if (pt.x < minX || pt.x > maxX || pt.y < minY || pt.y > maxY)
                            continue;

                        double dx = (pt.x - logicalCursor.X) * (maxX - minX);
                        double dy = (pt.y - logicalCursor.Y) * (maxY - minY);
                        double dist = axis switch
                        {
                            SnappingAxis.Horizontal => Math.Abs(dx),
                            SnappingAxis.Vertical => Math.Abs(dy),
                            SnappingAxis.Closest => Math.Sqrt(dx * dx + dy * dy),
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

        private IDataSnappingProvider? GetSnappingProvider(IndicatorMode mode)
        {
            return mode switch
            {
                IndicatorMode.ClosestData => new UnifiedSnappingProvider(SnappingAxis.Closest),
                IndicatorMode.HorizontalData => new UnifiedSnappingProvider(SnappingAxis.Horizontal),
                IndicatorMode.VerticalData => new UnifiedSnappingProvider(SnappingAxis.Vertical),
                _ => null
            };
        }
    }
}
