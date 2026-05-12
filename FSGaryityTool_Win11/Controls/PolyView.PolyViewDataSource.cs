using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FSGaryityTool_Win11.Controls.PolyView;

namespace FSGaryityTool_Win11.Controls
{
    /// <summary>
    /// 可编辑的折线数据源，逻辑坐标使用与控件绑定的 Min/MaxHorizontalValue 与 Min/MaxVerticalValue 范围。
    /// 支持双向绑定：外部可直接绑定到 PointsData 并在 UI 或代码中更新；数据更改会触发控件重绘。
    /// </summary>
    public class PolyViewDataSource : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// When true, the data source will not call Owner?.Invalidate() in response to internal changes.
        /// Used to avoid heavy UI rebuilding while the user is actively dragging.
        /// </summary>
        public bool SuppressInvalidate { get; set; }

        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        /// <summary>
        /// 所属控件（可选），用于在数据变更时请求重绘
        /// </summary>
        internal PolyView? Owner { get; set; }

        private string? _name;
        public string? Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }

        private ObservableCollection<(double x, double y)>? _pointsData;

        /// <summary>
        /// 折线的逻辑坐标点集合。X 使用 MinHorizontalValue..MaxHorizontalValue，Y 使用 MinVerticalValue..MaxVerticalValue。
        /// 外部可以双向绑定并直接操作集合；集合变更会触发重绘。
        /// </summary>
        public ObservableCollection<(double x, double y)>? PointsData
        {
            get => _pointsData;
            set
            {
                if (_pointsData != value)
                {
                    if (_pointsData != null)
                        _pointsData.CollectionChanged -= PointsData_CollectionChanged;

                    _pointsData = value;

                    if (_pointsData != null)
                        _pointsData.CollectionChanged += PointsData_CollectionChanged;

                    OnPropertyChanged(nameof(PointsData));
                }
            }
        }

        private void PointsData_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (!SuppressInvalidate)
                Owner?.Invalidate();
        }

        public bool IsEditable { get; set; } = true;

        public PolyViewDataSource()
        {
            PointsData = new ObservableCollection<(double x, double y)>();
        }

        /// <summary>
        /// 将逻辑坐标点映射为 Canvas 像素坐标用于绘制
        /// </summary>
        public List<(float x, float y)> GetCanvasPoints(GridBounds grid)
        {
            var list = new List<(float x, float y)>();
            if (PointsData == null || PointsData.Count == 0)
                return list;

            double minX = Owner?.MinHorizontalValue ?? 0.0;
            double maxX = Owner?.MaxHorizontalValue ?? 100.0;
            double minY = Owner?.MinVerticalValue ?? 0.0;
            double maxY = Owner?.MaxVerticalValue ?? 100.0;

            double dx = maxX - minX;
            double dy = maxY - minY;
            if (Math.Abs(dx) < 1e-9) dx = 1.0;
            if (Math.Abs(dy) < 1e-9) dy = 1.0;

            foreach (var p in PointsData)
            {
                float cx = grid.Left + (float)(((p.x - minX) / dx) * grid.Width);
                // logical Y increases upward; canvas Y increases downward -> invert
                float cy = grid.Bottom - (float)(((p.y - minY) / dy) * grid.Height);
                list.Add((cx, cy));
            }

            return list;
        }

        /// <summary>
        /// 将 Canvas 像素坐标转换为逻辑坐标
        /// </summary>
        public (double x, double y) CanvasToLogical(float canvasX, float canvasY, GridBounds grid)
        {
            double minX = Owner?.MinHorizontalValue ?? 0.0;
            double maxX = Owner?.MaxHorizontalValue ?? 100.0;
            double minY = Owner?.MinVerticalValue ?? 0.0;
            double maxY = Owner?.MaxVerticalValue ?? 100.0;

            double dx = maxX - minX;
            double dy = maxY - minY;
            if (Math.Abs(dx) < 1e-9) dx = 1.0;
            if (Math.Abs(dy) < 1e-9) dy = 1.0;

            double nx = (canvasX - grid.Left) / grid.Width;
            double ny = (grid.Bottom - canvasY) / grid.Height; // invert

            double lx = minX + nx * dx;
            double ly = minY + ny * dy;
            return (lx, ly);
        }

        /// <summary>
        /// 寻找距离给定画布坐标最近的点索引，超过 maxDistancePx 则返回 -1
        /// </summary>
        public int GetNearestPointIndex(float canvasX, float canvasY, GridBounds grid, float maxDistancePx = 12f)
        {
            if (PointsData == null || PointsData.Count == 0)
                return -1;

            var pts = GetCanvasPoints(grid);
            int best = -1;
            float bestDist2 = float.MaxValue;
            for (int i = 0; i < pts.Count; i++)
            {
                var p = pts[i];
                float dx = p.x - canvasX;
                float dy = p.y - canvasY;
                float d2 = dx * dx + dy * dy;
                if (d2 < bestDist2)
                {
                    bestDist2 = d2;
                    best = i;
                }
            }

            if (best < 0) return -1;
            if (bestDist2 <= maxDistancePx * maxDistancePx) return best;
            return -1;
        }

        /// <summary>
        /// 将点移动到新的画布坐标（用于拖动），会在逻辑坐标范围内限制并保持 X 单调性（除非移动的是首尾点）
        /// </summary>
        public void MovePointToCanvasPosition(int index, float canvasX, float canvasY, GridBounds grid)
        {
            if (!IsEditable || PointsData == null) return;
            if (index < 0 || index >= PointsData.Count) return;

            var logical = CanvasToLogical(canvasX, canvasY, grid);
            MovePointLogical(index, logical.x, logical.y);
        }

        /// <summary>
        /// 在逻辑坐标系中移动点，自动约束 X/Y 到控件范围并保持单调递增的 X 序列（除首尾）
        /// </summary>
        public void MovePointLogical(int index, double newX, double newY)
        {
            if (!IsEditable || PointsData == null) return;
            if (index < 0 || index >= PointsData.Count) return;

            double minX = Owner?.MinHorizontalValue ?? 0.0;
            double maxX = Owner?.MaxHorizontalValue ?? 100.0;
            double minY = Owner?.MinVerticalValue ?? 0.0;
            double maxY = Owner?.MaxVerticalValue ?? 100.0;

            newX = Math.Clamp(newX, minX, maxX);
            newY = Math.Clamp(newY, minY, maxY);

            // enforce monotonic X order for interior points
            const double eps = 1e-6;
            if (index > 0 && index < PointsData.Count - 1)
            {
                var prev = PointsData[index - 1];
                var next = PointsData[index + 1];
                double minAllowed = prev.x + eps;
                double maxAllowed = next.x - eps;
                if (minAllowed > maxAllowed)
                {
                    // degenerate: force squeeze into prev..next
                    minAllowed = prev.x;
                    maxAllowed = next.x;
                }
                newX = Math.Clamp(newX, minAllowed, maxAllowed);
            }

            // For endpoints allow any X within range but keep ordering with neighbor
            if (index == 0 && PointsData.Count > 1)
            {
                var next = PointsData[1];
                newX = Math.Clamp(newX, minX, next.x - eps);
            }
            if (index == PointsData.Count - 1 && PointsData.Count > 1)
            {
                var prev = PointsData[PointsData.Count - 2];
                newX = Math.Clamp(newX, prev.x + eps, maxX);
            }

            // If owner requests integer scale snapping, snap values to nearest integer while keeping constraints
            // but do NOT snap while SuppressInvalidate is enabled (used during active drag) to preserve drag feel.
            bool snapToInteger = (Owner?.IsIntegerScaleMode ?? false) && !this.SuppressInvalidate;
            if (snapToInteger)
            {
                // round to nearest integer
                double rx = Math.Round(newX);
                double ry = Math.Round(newY);

                // Ensure integer values respect overall min/max
                rx = Math.Clamp(rx, minX, maxX);
                ry = Math.Clamp(ry, minY, maxY);

                // Re-apply monotonic constraints for X after rounding
                if (index > 0 && index < PointsData.Count - 1)
                {
                    var prev = PointsData[index - 1];
                    var next = PointsData[index + 1];
                    // ensure rx stays between neighbors; if collision, try to nudge within integer grid
                    if (rx <= prev.x)
                        rx = Math.Min(prev.x + 1, next.x - 1);
                    if (rx >= next.x)
                        rx = Math.Max(next.x - 1, prev.x + 1);
                }
                else if (index == 0 && PointsData.Count > 1)
                {
                    var next = PointsData[1];
                    rx = Math.Clamp(rx, minX, next.x - 1);
                }
                else if (index == PointsData.Count - 1 && PointsData.Count > 1)
                {
                    var prev = PointsData[PointsData.Count - 2];
                    rx = Math.Clamp(rx, prev.x + 1, maxX);
                }

                newX = rx;
                newY = ry;
            }

            PointsData[index] = (newX, newY);
            OnPropertyChanged(nameof(PointsData));
            if (!SuppressInvalidate)
                Owner?.Invalidate();
        }

        /// <summary>
        /// 在给定逻辑位置插入点并保持集合按 X 排序
        /// </summary>
        public int InsertPointLogical((double x, double y) logicalPoint)
        {
            if (!IsEditable || PointsData == null) return -1;
            // clamp
            double minX = Owner?.MinHorizontalValue ?? 0.0;
            double maxX = Owner?.MaxHorizontalValue ?? 100.0;
            double minY = Owner?.MinVerticalValue ?? 0.0;
            double maxY = Owner?.MaxVerticalValue ?? 100.0;

            double x = Math.Clamp(logicalPoint.x, minX, maxX);
            double y = Math.Clamp(logicalPoint.y, minY, maxY);

            // Do not snap here to integer when inserting via pointer-drag workflows;
            // explicit callers can use CanvasPoint->GetSnappedLogicalForCanvasPoint when needed.

            int insertAt = 0;
            while (insertAt < PointsData.Count && PointsData[insertAt].x < x) insertAt++;
            PointsData.Insert(insertAt, (x, y));
            OnPropertyChanged(nameof(PointsData));
            Owner?.Invalidate();
            return insertAt;
        }

        /// <summary>
        /// 插入点到指定索引位置（不按 X 自动排序），返回插入索引或 -1
        /// </summary>
        public int InsertPointLogicalAt(int index, (double x, double y) logicalPoint)
        {
            if (!IsEditable || PointsData == null) return -1;
            if (index < 0) index = 0;
            if (index > PointsData.Count) index = PointsData.Count;

            double minX = Owner?.MinHorizontalValue ?? 0.0;
            double maxX = Owner?.MaxHorizontalValue ?? 100.0;
            double minY = Owner?.MinVerticalValue ?? 0.0;
            double maxY = Owner?.MaxVerticalValue ?? 100.0;

            double x = Math.Clamp(logicalPoint.x, minX, maxX);
            double y = Math.Clamp(logicalPoint.y, minY, maxY);

            // Snap to integer grid if requested by owner
            if (Owner?.IsIntegerScaleMode ?? false)
            {
                x = Math.Round(x);
                y = Math.Round(y);
                x = Math.Clamp(x, minX, maxX);
                y = Math.Clamp(y, minY, maxY);
            }

            // insert at requested index
            PointsData.Insert(index, (x, y));
            OnPropertyChanged(nameof(PointsData));
            Owner?.Invalidate();
            return index;
        }

        public void RemoveAt(int index)
        {
            if (!IsEditable || PointsData == null) return;
            if (index < 0 || index >= PointsData.Count) return;
            PointsData.RemoveAt(index);
            OnPropertyChanged(nameof(PointsData));
            Owner?.Invalidate();
        }
    }
}
