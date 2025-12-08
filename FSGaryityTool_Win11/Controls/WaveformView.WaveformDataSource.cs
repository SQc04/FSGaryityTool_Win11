using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
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
        private void PolylinePointsData_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // 集合内部 Add/Remove 时触发重绘
            Owner?.InvalidateCanvas();
        }
        private bool IsFunctionMode => FunctionFormulaData != null;
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
            if (!IsFunctionMode || FunctionFormulaData == null)
                return null;

            var builder = new CanvasPathBuilder(device);
            for (int i = 0; i < EffectiveCount; i++)
            {
                var pt = GetSample(i, grid);
                if (i == 0)
                    builder.BeginFigure(pt.x, pt.y);
                else
                    builder.AddLine(pt.x, pt.y);
            }
            builder.EndFigure(CanvasFigureLoop.Open);
            return CanvasGeometry.CreatePath(builder);
        }
    }
}
