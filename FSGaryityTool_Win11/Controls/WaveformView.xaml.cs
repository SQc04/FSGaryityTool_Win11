using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using static FSGaryityTool_Win11.Controls.WaveformView;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FSGaryityTool_Win11.Controls
{

    public sealed partial class WaveformView : UserControl, INotifyPropertyChanged
    {

        public double ViewMinX { get; set; }
        public double ViewMaxX { get; set; }
        public double ViewMinY { get; set; }
        public double ViewMaxY { get; set; }

        private bool _isDragging = false;
        private Point _lastDragPosition;
        private Vector2 _inertiaVelocity;
        private DispatcherTimer? _inertiaTimer;

        private const double DragSensitivity = 1.0;     // 拖动灵敏度，可调
        private const double InertiaFriction = 0.94;    // 惯性衰减（0.9~0.95 手感好）
        private const double MinInertiaSpeed = 1.0;

        private Visibility _resetViewVisibility = Visibility.Collapsed;
        public Visibility ResetViewVisibility
        {
            get => _resetViewVisibility;
            set
            {
                if (_resetViewVisibility != value)
                {
                    _resetViewVisibility = value;
                    OnPropertyChanged(nameof(ResetViewVisibility));
                }
            }
        }
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private Point RuntimeCenter
        {
            get => (Point)GetValue(RuntimeCenterProperty);
            set
            {
                SetValue(RuntimeCenterProperty, value);
                UpdateVisibleRange();
                InvalidateCanvas();
            }
        }
        private Point DefaultCenter => CalculateDefaultCenter();
        public ObservableCollection<WaveformDataSource> Data
        {
            get => (ObservableCollection<WaveformDataSource>)GetValue(DataProperty);
            set => SetValue(DataProperty, value);
        }

        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register(nameof(Data), typeof(ObservableCollection<WaveformDataSource>), typeof(WaveformView), new PropertyMetadata(null, OnDataChanged));

        private static void OnDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (WaveformView)d;

            if (e.OldValue is ObservableCollection<WaveformDataSource> oldCollection)
                oldCollection.CollectionChanged -= control.OnDataCollectionChanged;

            if (e.NewValue is ObservableCollection<WaveformDataSource> newCollection)
                newCollection.CollectionChanged += control.OnDataCollectionChanged;

            control.SubscribeToDataSourceChanges();
            control.InvalidateCanvas(); // 初次绑定时触发重绘
        }

        private void OnDataCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            SubscribeToDataSourceChanges();
            InvalidateCanvas(); // 集合变更时重绘
        }

        private void SubscribeToDataSourceChanges()
        {
            if (Data == null) return;

            foreach (var source in Data)
            {
                source.PropertyChanged -= OnDataSourceChanged;
                source.PropertyChanged += OnDataSourceChanged;
                source.Owner = this;
            }
        }

        private void OnDataSourceChanged(object sender, PropertyChangedEventArgs e)
        {
            // 可根据属性名过滤是否需要重绘
            if (e.PropertyName is nameof(WaveformDataSource.FunctionFormulaData) or
                nameof(WaveformDataSource.PolylinePointsData) or
                nameof(WaveformDataSource.Count) or
                nameof(WaveformDataSource.StrokeBrush))
            {
                InvalidateCanvas();
            }
        }

        public void WaveformView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            InvalidateVisual();
        }
        public void InvalidateCanvas()
        {
            WaveformDemonstratorCanvasControl?.Invalidate(); // 控件内部 CanvasControl 的重绘方法
        }

        private void InvalidateVisual()
        {
            LineDemonstratorCanvasControl?.Invalidate();
            WaveformDemonstratorCanvasControl?.Invalidate();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private static void OnVisualPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not WaveformView view) return;

            view.InvalidateVisual();

            // 处理默认中心逻辑
            if (e.Property == MinHorizontalValueProperty ||
                e.Property == MaxHorizontalValueProperty ||
                e.Property == MinVerticalValueProperty ||
                e.Property == MaxVerticalValueProperty ||
                e.Property == ViewCenterPointModeProperty ||
                e.Property == ViewCenterPointProperty)
            {
                // 实时计算的默认中心
                Point newDefaultCenter = view.DefaultCenter;

                // 判断用户当前是否处于“默认视图”（未手动缩放且未平移）
                bool isDefaultZoom =
                    Math.Abs(view.HorizontalZoomScale - 1.0) < 0.0001 &&
                    Math.Abs(view.VerticalZoomScale - 1.0) < 0.0001;

                bool isDefaultCenter = view.RuntimeCenter.Equals(newDefaultCenter);

                if (isDefaultZoom && isDefaultCenter)
                {
                    // 用户当前就是默认视图 → 自动跟随新的默认中心（带动画）
                    view.RuntimeCenter = newDefaultCenter;
                }

                // 更新 Reset 按钮的可见性
                view.UpdateResetViewVisibility();
            }
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



        private float? _cursorX = null;
        private float? _cursorY = null;

        private void OnWaveformPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            var point = e.GetCurrentPoint(WaveformDemonstratorCanvasControl).Position;
            _cursorX = (float)point.X;
            _cursorY = (float)point.Y;
            InvalidateCanvas();
        }

        private void OnActualThemeChanged(FrameworkElement sender, object args)
        {
            InvalidateVisual();

        }

        public WaveformView()
        {
            InitializeComponent();
            ActualThemeChanged += OnActualThemeChanged;
            WaveformDemonstratorCanvasControl.PointerMoved += OnWaveformPointerMoved;
            RootGrid.SizeChanged += WaveformView_SizeChanged;

            Loaded += WaveformView_Loaded;
        }
        private void WaveformView_Loaded(object sender, RoutedEventArgs e)
        {
            // 此时 XAML 属性已经全部应用完毕
            RuntimeCenter = DefaultCenter;  // 触发更新
            UpdateVisibleRange();
            InvalidateCanvas();
        }

        private Point CalculateDefaultCenter()
        {
            double minX = MinHorizontalValue;
            double maxX = MaxHorizontalValue;
            double minY = MinVerticalValue;
            double maxY = MaxVerticalValue;

            // 先用Auto方式计算中心
            Point autoCenter = new Point((minX + maxX) / 2, (minY + maxY) / 2);

            if (ViewCenterPointMode == ViewCenterMode.Auto)
            {
                // 只用范围中心
                return autoCenter;
            }

            // 其它模式先用autoCenter再根据模式调整
            return ViewCenterPointMode switch
            {
                ViewCenterMode.LeftTop => new Point(minX, maxY),
                ViewCenterMode.Top => new Point(autoCenter.X, maxY),
                ViewCenterMode.RightTop => new Point(maxX, maxY),
                ViewCenterMode.Left => new Point(minX, autoCenter.Y),
                ViewCenterMode.Center => autoCenter,
                ViewCenterMode.Right => new Point(maxX, autoCenter.Y),
                ViewCenterMode.LeftBottom => new Point(minX, minY),
                ViewCenterMode.Bottom => new Point(autoCenter.X, minY),
                ViewCenterMode.RightBottom => new Point(maxX, minY),
                ViewCenterMode.ManualPoint => ViewCenterPoint,
                _ => autoCenter,
            };
        }
        private int GetOptimalSampleCount(WaveformDataSource source)
        {
            // 1. 手动模式
            if (!source.Count.Equals("Auto", StringComparison.OrdinalIgnoreCase) &&
                int.TryParse(source.Count, out int manual))
            {
                return Math.Clamp(manual, 200, 100000);
            }

            // 2. Auto 模式：基础计算
            var margin = WaveGridBorderMargin;
            double drawableWidth = ActualWidth - margin.Left - margin.Right;
            if (drawableWidth <= 10) return 1000;

            double viewRangeX = ViewMaxX - ViewMinX;
            if (viewRangeX <= 0) return 1000;

            double pixelsPerUnit = drawableWidth / viewRangeX;
            int desired = (int)Math.Ceiling(pixelsPerUnit * 3.0);

            // 关键！LOD 上限控制（解决 2000 倍卡死）
            const int MaxReasonablePoints = 16000;   // 16000 是 Win2D 舒适上限
            const int UltraZoomThreshold = 2000;     // 放大倍数阈值

            double currentZoom = (MaxHorizontalValue - MinHorizontalValue) / viewRangeX;

            if (currentZoom > UltraZoomThreshold)
            {
                // 超大放大：强制降采样，但保证每像素至少 1 个点
                int minPoints = (int)Math.Ceiling(drawableWidth * 1.5); // 每像素 1.5 点，防锯齿
                desired = Math.Max(minPoints, desired);
                desired = Math.Min(desired, MaxReasonablePoints);
            }
            else
            {
                // 正常放大：允许高密度
                desired = Math.Min(desired, MaxReasonablePoints * 2);
            }

            return Math.Clamp(desired, 500, MaxReasonablePoints);
        }
        private void WaveformDemonstratorCanvasControl_RegionsInvalidated(CanvasVirtualControl sender, CanvasRegionsInvalidatedEventArgs args)
        {
            foreach (var region in args.InvalidatedRegions)
            {
                using var ds = sender.CreateDrawingSession(region);

                float width = (float)sender.ActualWidth;
                float height = (float)sender.ActualHeight;

                var margin = WaveGridBorderMargin;
                float gridLeft = (float)margin.Left;
                float gridTop = (float)margin.Top;
                float gridRight = width - (float)margin.Right;
                float gridBottom = height - (float)margin.Bottom;

                var grid = new GridBounds(gridLeft, gridTop, gridRight, gridBottom);

                // 计算逻辑中心点和缩放后的可视范围
                var center = CurrentLogicalCenter;

                double zoomX = ZoomMode is WaveformZoomMode.Horizontal or WaveformZoomMode.Both ? HorizontalZoomScale : 1.0;
                double zoomY = ZoomMode is WaveformZoomMode.Vertical or WaveformZoomMode.Both ? VerticalZoomScale : 1.0;

                double rangeX = (MaxHorizontalValue - MinHorizontalValue) / zoomX;
                double rangeY = (MaxVerticalValue - MinVerticalValue) / zoomY;

                ViewMinX = center.X - rangeX / 2;
                ViewMaxX = center.X + rangeX / 2;
                ViewMinY = center.Y - rangeY / 2;
                ViewMaxY = center.Y + rangeY / 2;

                // 绘制所有数据源
                if (Data != null)
                {
                    foreach (var source in Data)
                    {
                        var brush = source.StrokeBrush;
                        var thickness = source.StrokeThickness;
                        var style = source.LineStyle;
                        var color = GetBrushColor(brush);

                        CanvasPathBuilder builder = null;
                        bool hasValidPoints = false;

                        if (source.PolylinePointsData is { Count: > 1 } points)
                        {
                            builder = new CanvasPathBuilder(sender.Device);
                            bool first = true;

                            foreach (var rawPt in points)
                            {
                                // 原始数据点 + 偏移 = 最终逻辑坐标
                                var logicalPoint = ApplyOffset(new Point(rawPt.x, rawPt.y), source);

                                var canvasPt = MapToCanvas(logicalPoint, grid, ViewMinX, ViewMaxX, ViewMinY, ViewMaxY);

                                if (first)
                                {
                                    builder.BeginFigure(canvasPt);
                                    first = false;
                                }
                                else
                                {
                                    builder.AddLine(canvasPt);
                                }
                            }

                            if (!first)
                                builder.EndFigure(CanvasFigureLoop.Open);

                            hasValidPoints = true;
                        }
                        else if (source.FunctionFormulaData is not null)
                        {
                            int count = source.IsAutoCount ? GetOptimalSampleCount(source) : source.EffectiveCount;

                            double viewRangeX = ViewMaxX - ViewMinX;
                            double overdraw = viewRangeX * 0.3;
                            double drawMinX = ViewMinX - overdraw;
                            double drawMaxX = ViewMaxX + overdraw;

                            double fullRangeX = MaxHorizontalValue - MinHorizontalValue;
                            if (fullRangeX <= 0) fullRangeX = 1.0;

                            double originX = source.OffSetX;  // 函数原点

                            builder = new CanvasPathBuilder(sender.Device);
                            bool first = true;

                            for (int i = 0; i < count; i++)
                            {
                                // 1. 屏幕比例
                                float tScreen = i / (float)(count - 1);
                                float xScreen = (float)(drawMinX + tScreen * (drawMaxX - drawMinX));

                                // 2. 计算相对偏移 → 归一化参数
                                double offsetFromOrigin = xScreen - originX;
                                float tNorm = (float)(offsetFromOrigin / fullRangeX);

                                // 3. 计算函数值
                                float yRaw = source.FunctionFormulaData(tNorm);

                                // 关键！真实逻辑坐标（必须用这个！）
                                double trueLogicX = originX + tNorm * fullRangeX;   // 正确 X
                                double trueLogicY = yRaw + source.OffSetY;         // 正确 Y

                                var trueLogicalPoint = new Point(trueLogicX, trueLogicY);

                                // 4. 映射到画布（使用你完美的 MapToCanvas）
                                var canvasPt = MapToCanvas(trueLogicalPoint, grid, ViewMinX, ViewMaxX, ViewMinY, ViewMaxY);

                                // 防极端跳跃（可选）
                                if (float.IsInfinity(canvasPt.X) || float.IsInfinity(canvasPt.Y) ||
                                    canvasPt.X < -50000 || canvasPt.X > ActualWidth + 50000 ||
                                    canvasPt.Y < -50000 || canvasPt.Y > ActualHeight + 50000)
                                {
                                    if (!first) { builder.EndFigure(CanvasFigureLoop.Open); first = true; }
                                    continue;
                                }

                                if (first)
                                {
                                    builder.BeginFigure(canvasPt);
                                    first = false;
                                }
                                else
                                {
                                    builder.AddLine(canvasPt);
                                }
                            }

                            if (!first)
                                builder.EndFigure(CanvasFigureLoop.Open);

                            hasValidPoints = true;
                        }

                        if (hasValidPoints && builder != null)
                        {
                            using var geometry = CanvasGeometry.CreatePath(builder);
                            DrawGeometryStyle(ds, geometry, color, thickness, style);
                        }

                        // 十字标记（TickMode.Cross）
                        // TickMode 绘制（必须偏移！）
                        if (source.TickMode != TickModeStyle.None && source.PolylinePointsData is { Count: > 0 } pts)
                        {
                            foreach (var rawPt in pts)
                            {
                                var logicalPoint = ApplyOffset(new Point(rawPt.x, rawPt.y), source);
                                var canvasPt = MapToCanvas(logicalPoint, grid, ViewMinX, ViewMaxX, ViewMinY, ViewMaxY);

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
                            }
                        }
                    }
                }

                // 绘制指针指示线（十字光标）
                DrawPointerIndicatorLines(ds, grid);
            }
        }

        private Point ApplyOffset(Point logicalPoint, WaveformDataSource source)
        {
            return new Point(
                logicalPoint.X + source.OffSetX,
                logicalPoint.Y + source.OffSetY
            );
        }

        private Point CurrentLogicalCenter => RuntimeCenter;
        private void UpdateVisibleRange()
        {
            double zoomX = ZoomMode is WaveformZoomMode.Horizontal or WaveformZoomMode.Both ? HorizontalZoomScale : 1.0;
            double zoomY = ZoomMode is WaveformZoomMode.Vertical or WaveformZoomMode.Both ? VerticalZoomScale : 1.0;

            double rangeX = (MaxHorizontalValue - MinHorizontalValue) / zoomX;
            double rangeY = (MaxVerticalValue - MinVerticalValue) / zoomY;

            ViewMinX = RuntimeCenter.X - rangeX / 2;
            ViewMaxX = RuntimeCenter.X + rangeX / 2;
            ViewMinY = RuntimeCenter.Y - rangeY / 2;
            ViewMaxY = RuntimeCenter.Y + rangeY / 2;
        }

        private static Vector2 MapToCanvas(Point logical, GridBounds grid, double viewMinX, double viewMaxX, double viewMinY, double viewMaxY)
        {
            double viewW = viewMaxX - viewMinX;
            double viewH = viewMaxY - viewMinY;

            if (viewW <= 0 || viewH <= 0) return new Vector2(0, 0);

            float x = grid.Left + (float)((logical.X - viewMinX) / viewW * grid.Width);
            float y = grid.Bottom - (float)((logical.Y - viewMinY) / viewH * grid.Height);

            return new Vector2(x, y);
        }

        public static Point MapToLogical(Point pixel, GridBounds grid, double minX, double maxX, double minY, double maxY)
        {
            double logicalX = minX;
            double logicalY = minY;

            if (grid.Width > 0 && maxX != minX)
            {
                logicalX = minX + Math.Clamp((pixel.X - grid.Left) / grid.Width, 0, 1) * (maxX - minX);
            }

            if (grid.Height > 0 && maxY != minY)
            {
                logicalY = maxY - Math.Clamp((pixel.Y - grid.Top) / grid.Height, 0, 1) * (maxY - minY);
            }

            return new Point(logicalX, logicalY);
        }


        private DispatcherTimer _zoomTimer;

        private void StartZoomAnimation(double centerX, double centerY, double targetScaleX, double targetScaleY, TimeSpan duration)
        {
            var startCenter = RuntimeCenter;
            var startScaleX = HorizontalZoomScale;
            var startScaleY = VerticalZoomScale;

            targetScaleX = Math.Clamp(targetScaleX, 0.001, 80000);
            targetScaleY = Math.Clamp(targetScaleY, 0.001, 80000);

            _zoomTimer?.Stop();
            _zoomTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1) };
            var startTime = DateTime.Now;

            _zoomTimer.Tick += (_, _) =>
            {
                double elapsed = (DateTime.Now - startTime).TotalMilliseconds;
                double progress = Math.Min(elapsed / duration.TotalMilliseconds, 1.0);
                double eased = 1 - Math.Pow(1 - progress, 3); // EaseOutCubic

                // 只更新运行时中心和缩放
                RuntimeCenter = new Point(
                    startCenter.X + (centerX - startCenter.X) * eased,
                    startCenter.Y + (centerY - startCenter.Y) * eased);

                HorizontalZoomScale = startScaleX + (targetScaleX - startScaleX) * eased;
                VerticalZoomScale = startScaleY + (targetScaleY - startScaleY) * eased;
                UpdateResetViewVisibility();
                if (progress >= 1.0) _zoomTimer.Stop();
            };

            _zoomTimer.Start();
        }

        private void UpdateSampleCountFromSize()
        {
            int pixelWidth = (int)Math.Max(1, WaveformDemonstratorCanvasControl.ActualWidth);
            if (Data == null)
                return;

            foreach (var source in Data)
            {
                // 只更新函数模式，忽略已设置 PolylinePointsData 的源，且 Count 为 Auto
                if (source.FunctionFormulaData != null &&
                    source.PolylinePointsData == null &&
                    source.IsAutoCount)
                {
                    source.Count = pixelWidth.ToString();
                }
            }

            WaveformDemonstratorCanvasControl.Invalidate(); // 触发重绘
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
        private void DrawPointerIndicatorLines(CanvasDrawingSession ds, GridBounds grid)
        {
            if (PointerIndicatorMode == IndicatorMode.None ||
                PointerIndicatorCursorMode == CrosshairCursorMode.Disabled ||
                !_cursorX.HasValue || !_cursorY.HasValue)
                return;

            float cursorX = _cursorX.Value;
            float cursorY = _cursorY.Value;

            if (!grid.Contains(cursorX, cursorY))
                return;

            var lineBrush = PointerIndicatorBrush ?? new SolidColorBrush(Microsoft.UI.Colors.DeepSkyBlue);
            var lineColor = GetBrushColor(lineBrush, GetThemeColor("AccentTextFillColorPrimaryBrush", Microsoft.UI.Colors.DeepSkyBlue));
            float lineWidth = (float)PointerIndicatorLineWidth;

            // 默认使用鼠标位置（像素坐标）
            float verticalX = cursorX;
            float horizontalY = cursorY;

            // 吸附器（返回的是像素坐标）
            var snappingProvider = GetSnappingProvider(PointerIndicatorMode);
            if (snappingProvider != null && Data?.Count > 0)
            {
                var snapped = snappingProvider.GetSnappedPoint(
                    cursorX, cursorY, grid,
                    Data,
                    ViewMinX, ViewMaxX,
                    ViewMinY, ViewMaxY);

                if (snapped is (float px, float py))
                {
                    verticalX = px;
                    horizontalY = py;
                }
            }

            // 绘制十字线（像素坐标）
            if (PointerIndicatorCursorMode is CrosshairCursorMode.Full or CrosshairCursorMode.VerticalOnly)
                DrawLineStyle(ds, verticalX, grid.Top, verticalX, grid.Bottom, lineColor, lineWidth, PointerIndicatorLineStyle);

            if (PointerIndicatorCursorMode is CrosshairCursorMode.Full or CrosshairCursorMode.HorizontalOnly)
                DrawLineStyle(ds, grid.Left, horizontalY, grid.Right, horizontalY, lineColor, lineWidth, PointerIndicatorLineStyle);
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

        public struct GridBounds
        {
            public float Left { get; set; }
            public float Top { get; set; }
            public float Right { get; set; }
            public float Bottom { get; set; }

            public float Width => Right - Left;
            public float Height => Bottom - Top;

            public float CenterX => (Left + Right) / 2f;
            public float CenterY => (Top + Bottom) / 2f;

            public GridBounds(float left, float top, float right, float bottom)
            {
                Left = left;
                Top = top;
                Right = right;
                Bottom = bottom;
            }

            public bool Contains(float x, float y)
            {
                return x >= Left && x <= Right && y >= Top && y <= Bottom;
            }

            public override string ToString()
            {
                return $"GridBounds({Left}, {Top}, {Right}, {Bottom})";
            }
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
        private void DrawLineStyle(CanvasDrawingSession ds, float x1, float y1, float x2, float y2, Windows.UI.Color color, float width, LineStyle style)
        {
            switch (style)
            {
                case LineStyle.None:
                    return;
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

        private Windows.UI.Color GetBrushColor(Brush brush, Windows.UI.Color fallback)
        {
            if (brush is SolidColorBrush solid)
                return solid.Color;

            return fallback;
        }

        // 默认版本使用系统主题色
        private Windows.UI.Color GetBrushColor(Brush brush)
        {
            return GetBrushColor(brush, GetThemeColor("AccentTextFillColorTertiaryBrush", Microsoft.UI.Colors.DeepSkyBlue));
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

        private void UserControl_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            _cursorX = null;
            _cursorY = null;
            WaveformDemonstratorCanvasControl.Invalidate();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateSampleCountFromSize();
            InvalidateCanvas();
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateSampleCountFromSize();
        }

        private void ResetViewButton_Click(object sender, RoutedEventArgs e)
        {
            ZoomControlToReset();
        }

        private void UpdateResetViewVisibility()
        {
            bool isZoomed = Math.Abs(HorizontalZoomScale - 1.0) > 0.001 ||
                            Math.Abs(VerticalZoomScale - 1.0) > 0.001;
            bool isPanned = RuntimeCenter != DefaultCenter;  // 实时比较

            // 判断是否在原点 (0,0)
            bool isNotOrigin = !RuntimeCenter.Equals(DefaultCenter);

            ResetViewVisibility = (ZoomMode != WaveformZoomMode.Disabled && (isZoomed || isPanned || isNotOrigin))
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        public void ZoomControlToSet(Point point, double zoomSize)
        {
            if (zoomSize <= 0) zoomSize = 1.0;

            double targetScaleX = HorizontalZoomScale;
            double targetScaleY = VerticalZoomScale;

            if (ZoomMode is WaveformZoomMode.Horizontal or WaveformZoomMode.Both)
                targetScaleX = zoomSize;
            if (ZoomMode is WaveformZoomMode.Vertical or WaveformZoomMode.Both)
                targetScaleY = zoomSize;

            // 关键：调用 5 参数版本，传入 point.X 和 point.Y
            UpdateResetViewVisibility();
            StartZoomAnimation(
                centerX: point.X,
                centerY: point.Y,
                targetScaleX: targetScaleX,
                targetScaleY: targetScaleY,
                duration: TimeSpan.FromMilliseconds(280));
        }
        private void ZoomControlToReset()
        {
            StartZoomAnimation(
                centerX: DefaultCenter.X,   // 这里直接用计算属性
                centerY: DefaultCenter.Y,
                targetScaleX: 1.0,
                targetScaleY: 1.0,
                duration: TimeSpan.FromMilliseconds(350));
        }

        private void UserControl_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            if (ZoomMode == WaveformZoomMode.Disabled) return;

            var pointerPoint = e.GetCurrentPoint(WaveformDemonstratorCanvasControl);
            var position = pointerPoint.Position; // 鼠标像素坐标
            int delta = pointerPoint.Properties.MouseWheelDelta;
            if (delta == 0) return;

            double factor = delta > 0 ? 1.18 : 1.0 / 1.18;
            double targetScaleX = HorizontalZoomScale * (ZoomMode is WaveformZoomMode.Horizontal or WaveformZoomMode.Both ? factor : 1.0);
            double targetScaleY = VerticalZoomScale * (ZoomMode is WaveformZoomMode.Vertical or WaveformZoomMode.Both ? factor : 1.0);

            // 1. 获取当前画布的 grid 区域
            var grid = GetGridBounds(WaveformDemonstratorCanvasControl.ActualWidth, WaveformDemonstratorCanvasControl.ActualHeight);

            // 2. 记录缩放前鼠标像素坐标对应的逻辑坐标
            var logicalBefore = MapToLogical(position, grid, ViewMinX, ViewMaxX, ViewMinY, ViewMaxY);

            // 3. 计算缩放后的显示范围
            double zoomX = targetScaleX;
            double zoomY = targetScaleY;
            double rangeX = (MaxHorizontalValue - MinHorizontalValue) / zoomX;
            double rangeY = (MaxVerticalValue - MinVerticalValue) / zoomY;

            // 4. 计算新的中心点，使得缩放后鼠标像素坐标对应的逻辑坐标和缩放前一致
            double newCenterX = logicalBefore.X;
            double newCenterY = logicalBefore.Y;

            // 让鼠标位置在缩放后依然对应 logicalBefore
            // 反推新的中心点
            double percentX = (position.X - grid.Left) / grid.Width;
            double percentY = 1.0 - (position.Y - grid.Top) / grid.Height;

            newCenterX = logicalBefore.X - (percentX - 0.5) * rangeX;
            newCenterY = logicalBefore.Y - (percentY - 0.5) * rangeY;
            UpdateResetViewVisibility();
            StartZoomAnimation(newCenterX, newCenterY, targetScaleX, targetScaleY, TimeSpan.FromMilliseconds(160));
            e.Handled = true;
        }



        private void UserControl_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            // 只处理鼠标
            if (e.Pointer.PointerDeviceType != PointerDeviceType.Mouse)
                return;

            var props = e.GetCurrentPoint(this).Properties;

            if (props.IsLeftButtonPressed)
            {
                _isDragging = true;
                _lastDragPosition = e.GetCurrentPoint(this).Position;
                _inertiaTimer?.Stop();

                this.CapturePointer(e.Pointer);   // 防止鼠标移出控件后丢失事件
                e.Handled = true;
            }
        }

        private void UserControl_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (!_isDragging || ZoomMode == WaveformZoomMode.Disabled) return;

            var curPos = e.GetCurrentPoint(this).Position;

            // 手动计算像素偏移（Point 不支持 - 运算）
            double deltaX = curPos.X - _lastDragPosition.X;
            double deltaY = curPos.Y - _lastDragPosition.Y;

            if (Math.Abs(deltaX) < 1 && Math.Abs(deltaY) < 1) return;

            // 关键：计算实际绘图区域（排除 WaveGridBorderMargin）
            var margin = WaveGridBorderMargin;
            double drawableWidth = ActualWidth - margin.Left - margin.Right;
            double drawableHeight = ActualHeight - margin.Top - margin.Bottom;

            if (drawableWidth <= 0 || drawableHeight <= 0) return;

            // 像素偏移 → 逻辑偏移
            double viewW = ViewMaxX - ViewMinX;
            double viewH = ViewMaxY - ViewMinY;

            double logicDeltaX = deltaX * viewW / drawableWidth;
            double logicDeltaY = -deltaY * viewH / drawableHeight;  // Y 轴反向

            RuntimeCenter = new Point(
                RuntimeCenter.X - logicDeltaX * DragSensitivity,
                RuntimeCenter.Y - logicDeltaY * DragSensitivity);

            // 实时更新速度（用于惯性）
            _inertiaVelocity = new Vector2((float)deltaX, (float)deltaY) * 0.95f;

            _lastDragPosition = curPos;
            InvalidateCanvas();
            e.Handled = true;
        }

        private void UserControl_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (!_isDragging) return;

            _isDragging = false;
            this.ReleasePointerCapture(e.Pointer);

            // 速度够大才开惯性
            if (_inertiaVelocity.Length() > 0.1)
            {
                StartInertiaAnimation();
            }

            e.Handled = true;
        }

        private void StartInertiaAnimation()
        {
            _inertiaTimer?.Stop();
            _inertiaTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(0.5) };

            _inertiaTimer.Tick += (_, _) =>
            {
                if (_inertiaVelocity.Length() < MinInertiaSpeed)
                {
                    _inertiaTimer.Stop();
                    return;
                }

                var margin = WaveGridBorderMargin;
                double drawableWidth = ActualWidth - margin.Left - margin.Right;
                double drawableHeight = ActualHeight - margin.Top - margin.Bottom;

                if (drawableWidth <= 0 || drawableHeight <= 0)
                {
                    _inertiaTimer.Stop();
                    return;
                }

                double viewW = ViewMaxX - ViewMinX;
                double viewH = ViewMaxY - ViewMinY;

                double deltaX = _inertiaVelocity.X * viewW / drawableWidth;
                double deltaY = -_inertiaVelocity.Y * viewH / drawableHeight;

                RuntimeCenter = new Point(
                    RuntimeCenter.X - deltaX * DragSensitivity,
                    RuntimeCenter.Y - deltaY * DragSensitivity);

                _inertiaVelocity *= (float)InertiaFriction;

                InvalidateCanvas();
                UpdateResetViewVisibility();
            };

            _inertiaTimer.Start();
        }
    }
}
