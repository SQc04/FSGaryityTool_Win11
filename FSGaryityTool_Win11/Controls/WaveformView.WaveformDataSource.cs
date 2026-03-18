using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static FSGaryityTool_Win11.Controls.WaveformView;

namespace FSGaryityTool_Win11.Controls
{
    public interface ISampledWaveform
    {
        int Count { get; }
        (float x, float y) GetSample(int index, GridBounds grid);
    }
    public class WaveformDataSource : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _name;
        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }

        private Brush _strokeBrush = new SolidColorBrush(Microsoft.UI.Colors.DeepSkyBlue);
        public Brush StrokeBrush
        {
            get => _strokeBrush;
            set { _strokeBrush = value; OnPropertyChanged(nameof(StrokeBrush)); }
        }

        private float _strokeThickness = 2f;
        public float StrokeThickness
        {
            get => _strokeThickness;
            set { _strokeThickness = value; OnPropertyChanged(nameof(StrokeThickness)); }
        }

        private LineStyle _lineStyle = LineStyle.Solid;
        public LineStyle LineStyle
        {
            get => _lineStyle;
            set { _lineStyle = value; OnPropertyChanged(nameof(LineStyle)); }
        }

        private TickModeStyle _tickMode = TickModeStyle.None;
        public TickModeStyle TickMode
        {
            get => _tickMode;
            set { _tickMode = value; OnPropertyChanged(nameof(TickMode)); }
        }

        private double _offSetX = 0.0;
        public double OffSetX
        {
            get => _offSetX;
            set { _offSetX = value; OnPropertyChanged(nameof(OffSetX)); }
        }
        private double _offSetY = 0.0;
        public double OffSetY
        {
            get => _offSetY;
            set { _offSetY = value; OnPropertyChanged(nameof(OffSetY)); }
        }

        private ObservableCollection<(float x, float y)>? _polylinePointsData;
        private Func<float, float>? _functionFormulaData;
        // Parametric function: given parameter t (0..1) returns logical (x,y) coordinates
        private Func<float, (double x, double y)>? _parametricFunctionData;
        internal WaveformView Owner { get; set; }

        /// <summary>
        /// 顺序点数据（像素坐标），用于 Polyline 绘制
        /// </summary>
        public ObservableCollection<(float x, float y)>? PolylinePointsData
        {
            get => _polylinePointsData;
            set
            {
                if (PolylinePointsData != value)
                {
                    if (_polylinePointsData != null)
                        _polylinePointsData.CollectionChanged -= PolylinePointsData_CollectionChanged;

                    _polylinePointsData = value;

                    // 订阅新集合
                    if (_polylinePointsData != null)
                        _polylinePointsData.CollectionChanged += PolylinePointsData_CollectionChanged;
                    OnPropertyChanged(nameof(PolylinePointsData));
                    OnPropertyChanged(nameof(IsPolylineMode));
                }
            }
        }

        /// <summary>
        /// 函数表达式，输入为归一化 x（0~1），输出为 y 偏移
        /// </summary>
        public Func<float, float>? FunctionFormulaData
        {
            get => _functionFormulaData;
            set
            {
                if (FunctionFormulaData != value)
                {
                    _functionFormulaData = value;
                    OnPropertyChanged(nameof(FunctionFormulaData));
                    OnPropertyChanged(nameof(IsFunctionMode));
                    Owner?.InvalidateCanvas();
                }
            }
        }

        /// <summary>
        /// 参数化矢量函数，输入为参数 t（0..1），输出为逻辑坐标系中的 (x,y)。
        /// 当提供此函数时，控件会使用自适应细分采样以获得更准确的曲线表示。
        /// </summary>
        public Func<float, (double x, double y)>? ParametricFunctionData
        {
            get => _parametricFunctionData;
            set
            {
                if (_parametricFunctionData != value)
                {
                    _parametricFunctionData = value;
                    OnPropertyChanged(nameof(ParametricFunctionData));
                    OnPropertyChanged(nameof(IsParametricMode));
                    Owner?.InvalidateCanvas();
                }
            }
        }
        private void PolylinePointsData_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // 集合内部 Add/Remove 时触发重绘
            Owner?.InvalidateCanvas();
        }
        private bool IsFunctionMode => FunctionFormulaData != null;
        private bool IsParametricMode => ParametricFunctionData != null;
        private bool IsPolylineMode => PolylinePointsData != null && PolylinePointsData.Count > 1;

        private string _count = "Auto";
        public string Count
        {
            get => _count;
            set
            {
                if (_count != value)
                {
                    _count = value;
                    OnPropertyChanged(nameof(Count));
                }
            }
        }

        public bool IsAutoCount => Count?.Trim().Equals("Auto", StringComparison.OrdinalIgnoreCase) == true;

        public int EffectiveCount => IsAutoCount ? 1000 : int.TryParse(Count, out var val) && val > 0 ? val : 1000;


        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        /// <summary>
        /// 获取函数采样点（仅函数模式）
        /// </summary>
        public (float x, float y) GetSample(int index, GridBounds grid)
        {
            if (!IsFunctionMode || FunctionFormulaData == null)
                throw new InvalidOperationException("当前数据源不是函数模式");

            float t = index / (float)(EffectiveCount - 1);
            float x = grid.Left + t * grid.Width;
            float y = grid.CenterY - FunctionFormulaData(t) + (float)OffSetY;
            return (x, y);
        }

        /// <summary>
        /// 获取函数矢量路径（仅函数模式）
        /// </summary>
        public CanvasGeometry? GetFunctionGeometry(CanvasDevice device, GridBounds grid)
        {
            // Support both legacy function mode and new parametric mode
            if (IsParametricMode && ParametricFunctionData != null)
            {
                var builder = new CanvasPathBuilder(device);

                // adaptive subdivision parameters
                const double tolerancePx = 0.75; // pixel tolerance
                const int maxDepth = 12;

                // We'll sample parametric function in logical coordinates here; caller may map as needed later.
                // Use simple linear sampling to build an initial list then create geometry.
                var pts = new List<(double x, double y)>();

                // iterative adaptive subdivision stack
                var stack = new Stack<(float t0, (double x, double y) p0, float t1, (double x, double y) p1, int depth)>();

                float t0 = 0f;
                float t1 = 1f;
                var p0 = ParametricFunctionData(0f);
                var p1 = ParametricFunctionData(1f);

                // Determine view logical bounds (from Owner if available) once and reuse.
                // If Owner is not available, estimate bounds by coarse sampling the parametric function.
                double viewMinX, viewMaxX, viewMinY, viewMaxY;
                if (Owner != null)
                {
                    viewMinX = Owner.ViewMinX;
                    viewMaxX = Owner.ViewMaxX;
                    viewMinY = Owner.ViewMinY;
                    viewMaxY = Owner.ViewMaxY;
                }
                else
                {
                    // coarse sample to estimate bounds
                    viewMinX = Math.Min(p0.x, p1.x);
                    viewMaxX = Math.Max(p0.x, p1.x);
                    viewMinY = Math.Min(p0.y, p1.y);
                    viewMaxY = Math.Max(p0.y, p1.y);
                    const int estimateSamples = 64;
                    for (int si = 1; si < estimateSamples; si++)
                    {
                        float tt = si / (float)(estimateSamples - 1);
                        var pp = ParametricFunctionData(tt);
                        if (pp.x < viewMinX) viewMinX = pp.x;
                        if (pp.x > viewMaxX) viewMaxX = pp.x;
                        if (pp.y < viewMinY) viewMinY = pp.y;
                        if (pp.y > viewMaxY) viewMaxY = pp.y;
                    }
                }

                double denomX = viewMaxX - viewMinX;
                double denomY = viewMaxY - viewMinY;
                if (Math.Abs(denomX) < 1e-9) denomX = 1.0;
                if (Math.Abs(denomY) < 1e-9) denomY = 1.0;

                // Determine view logical bounds (for mapping) and deltas
                double vMinX = viewMinX;
                double vMaxX = viewMaxX;
                double vMinY = viewMinY;
                double vMaxY = viewMaxY;
                // viewMinX/viewMaxX may already come from Owner; keep as-is
                double dX = vMaxX - vMinX;
                double dY = vMaxY - vMinY;
                if (Math.Abs(dX) < 1e-9) dX = 1.0;
                if (Math.Abs(dY) < 1e-9) dY = 1.0;

                stack.Push((t0, p0, t1, p1, 0));

                while (stack.Count > 0)
                {
                    var (ta, pa, tb, pb, depth) = stack.Pop();
                    float tm = (ta + tb) * 0.5f;
                    var pm = ParametricFunctionData(tm);

                    // map logical coordinates (pa,pb,pm) to canvas pixels using precomputed view bounds
                    // apply per-source offsets so parametric shapes respect OffSetX/OffSetY
                    double paX = pa.x + OffSetX;
                    double paY = pa.y + OffSetY;
                    double pbX = pb.x + OffSetX;
                    double pbY = pb.y + OffSetY;
                    double pmX = pm.x + OffSetX;
                    double pmY = pm.y + OffSetY;

                    float caX = grid.Left + (float)((paX - viewMinX) / denomX * grid.Width);
                    float caY = grid.Bottom - (float)((paY - viewMinY) / denomY * grid.Height);
                    float cbX = grid.Left + (float)((pbX - viewMinX) / denomX * grid.Width);
                    float cbY = grid.Bottom - (float)((pbY - viewMinY) / denomY * grid.Height);
                    float cmX = grid.Left + (float)((pmX - viewMinX) / denomX * grid.Width);
                    float cmY = grid.Bottom - (float)((pmY - viewMinY) / denomY * grid.Height);

                    // linear midpoint
                    float lx = (caX + cbX) * 0.5f;
                    float ly = (caY + cbY) * 0.5f;
                    double err = Math.Sqrt((cmX - lx) * (cmX - lx) + (cmY - ly) * (cmY - ly));

                    if (err > tolerancePx && depth < maxDepth)
                    {
                        // subdivide
                        stack.Push((tm, pm, tb, pb, depth + 1));
                        stack.Push((ta, pa, tm, pm, depth + 1));
                    }
                    else
                    {
                        // accept segment end point pb
                        if (pts.Count == 0)
                            pts.Add(pa);
                        pts.Add(pb);
                    }
                }

                if (pts.Count == 0) return null;
                // Quick check: if mapped pts have near-zero vertical span (degenerate),
                // fallback to a dense uniform sampling to avoid collapsed horizontal line.
                bool degenerate = false;
                {
                    double minCy = double.MaxValue, maxCy = double.MinValue;
                    for (int i = 0; i < pts.Count; i++)
                    {
                        var lp = pts[i];
                        double cx = grid.Left + ((lp.x + OffSetX - viewMinX) / dX * grid.Width);
                        double cy = grid.Bottom - ((lp.y + OffSetY - viewMinY) / dY * grid.Height);
                        if (cx < -1e8 || cy < -1e8 || double.IsNaN(cx) || double.IsNaN(cy)) continue;
                        if (cy < minCy) minCy = cy;
                        if (cy > maxCy) maxCy = cy;
                    }
                    if (minCy < double.MaxValue && maxCy - minCy < 1.0) // less than 1 pixel high
                        degenerate = true;
                }

                if (degenerate)
                {
                    // rebuild with uniform dense sampling
                    pts.Clear();
                    int fallbackSamples = 1024;
                    for (int si = 0; si < fallbackSamples; si++)
                    {
                        float tt = si / (float)(fallbackSamples - 1);
                        var pp = ParametricFunctionData(tt);
                        pts.Add(pp);
                    }
                }

                // build geometry mapping logical -> canvas
                for (int i = 0; i < pts.Count; i++)
                {
                    var lp = pts[i];
                    // apply per-source offsets and map logical to canvas
                    float cx = grid.Left + (float)(((lp.x + OffSetX) - vMinX) / dX * grid.Width);
                    float cy = grid.Bottom - (float)(((lp.y + OffSetY) - vMinY) / dY * grid.Height);
                    if (i == 0)
                        builder.BeginFigure(cx, cy);
                    else
                        builder.AddLine(cx, cy);
                }

                builder.EndFigure(CanvasFigureLoop.Open);
                return CanvasGeometry.CreatePath(builder);
            }

            if (!IsFunctionMode || FunctionFormulaData == null)
                return null;

            var builder2 = new CanvasPathBuilder(device);
            for (int i = 0; i < EffectiveCount; i++)
            {
                var pt = GetSample(i, grid);
                if (i == 0)
                    builder2.BeginFigure(pt.x, pt.y);
                else
                    builder2.AddLine(pt.x, pt.y);
            }
            builder2.EndFigure(CanvasFigureLoop.Open);
            return CanvasGeometry.CreatePath(builder2);
        }
    }
}
