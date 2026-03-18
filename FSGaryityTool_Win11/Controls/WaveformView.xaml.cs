using MathNet.Numerics.Distributions;
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
using System.Diagnostics;
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
using Windows.System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FSGaryityTool_Win11.Controls
{

    public sealed partial class WaveformView : UserControl, INotifyPropertyChanged
    {

        private Point _lastDragPosition;
        private bool _isDragging = false;

        public string ViewFPS
        {
            get => (string)GetValue(ViewFPSProperty);
            set => SetValue(ViewFPSProperty, value);
        }
        public static readonly DependencyProperty ViewFPSProperty =
            DependencyProperty.Register(nameof(ViewFPS), typeof(string), typeof(WaveformView), new PropertyMetadata("60"));

        private DispatcherTimer _fpsTimer;


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

        
        

        public void WaveformView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Update exposed Actual size properties so x:Bind shows real-time values (helps XAML Hot Reload and debug display).
            ActualRootWidth = RootGrid?.ActualWidth ?? 0;
            ActualRootHeight = RootGrid?.ActualHeight ?? 0;

            InvalidateVisual();
        }

        private double _actualRootWidth;
        public double ActualRootWidth
        {
            get => _actualRootWidth;
            set
            {
                if (Math.Abs(_actualRootWidth - value) < 0.0001) return;
                _actualRootWidth = value;
                OnPropertyChanged(nameof(ActualRootWidth));
            }
        }

        private double _actualRootHeight;
        public double ActualRootHeight
        {
            get => _actualRootHeight;
            set
            {
                if (Math.Abs(_actualRootHeight - value) < 0.0001) return;
                _actualRootHeight = value;
                OnPropertyChanged(nameof(ActualRootHeight));
            }
        }
        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateSampleCountFromSize();
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
            // 有时 SizeChanged/Loaded 可能未能覆盖所有布局时机，订阅 LayoutUpdated 以确保最终尺寸被采集
            RootGrid.LayoutUpdated += RootGrid_LayoutUpdated;
            Loaded += WaveformView_Loaded;

            _fpsTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(250)  // 1000ms / 4 = 250ms
            };
            _fpsTimer.Tick += (s, e) => UpdateFPS();
            _fpsTimer.Start();
        }

        private void RootGrid_LayoutUpdated(object? sender, object? e)
        {
            if (RootGrid is null) return;

            double w = RootGrid.ActualWidth;
            double h = RootGrid.ActualHeight;

            // 只有在尺寸发生变化时才触发属性变更，避免频繁通知
            if (Math.Abs(ActualRootWidth - w) > 0.5 || Math.Abs(ActualRootHeight - h) > 0.5)
            {
                ActualRootWidth = w;
                ActualRootHeight = h;
            }
        }
        private void WaveformView_Loaded(object sender, RoutedEventArgs e)
        {
            // 此时 XAML 属性已经全部应用完毕
            RuntimeCenter = DefaultCenter;  // 触发更新
            UpdateVisibleRange();
            // 确保在 Loaded 时也采集一次最终尺寸，避免某些情况下 SizeChanged 未触发或已错过第一次事件
            ActualRootWidth = RootGrid?.ActualWidth ?? 0;
            ActualRootHeight = RootGrid?.ActualHeight ?? 0;

            InvalidateCanvas();
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateSampleCountFromSize();
            // 同步一次尺寸信息（某些场景下控件已布局完成但 RootGrid.SizeChanged 未再次触发）
            ActualRootWidth = RootGrid?.ActualWidth ?? 0;
            ActualRootHeight = RootGrid?.ActualHeight ?? 0;
            InvalidateCanvas();
        }

        //绘制波形主逻辑
        private void WaveformDemonstratorCanvasControl_RegionsInvalidated(CanvasVirtualControl sender, CanvasRegionsInvalidatedEventArgs args)
        {
            //SetLastFrameTime();
            //更新逻辑边界
            UpdateVisibleRange();

            //计算像素绘制区域
            float width = (float)sender.ActualWidth;
            float height = (float)sender.ActualHeight;
            var margin = WaveGridBorderMargin;
            float gridLeft = (float)margin.Left;
            float gridTop = (float)margin.Top;
            float gridRight = width - (float)margin.Right;
            float gridBottom = height - (float)margin.Bottom;
            var grid = new GridBounds(gridLeft, gridTop, gridRight, gridBottom);
            // Helper to draw the full content into a given drawing session
            void DrawIntoSession(CanvasDrawingSession ds)
            {
                //绘制所有数据源
                if (Data != null)
                {
                    foreach (var source in Data)
                    {
                        DrawPolyLines(sender, ds, source, grid, ViewMinX, ViewMaxX, ViewMinY, ViewMaxY, HorizontalZoomScale);
                        DrawFunction(sender, ds, source, grid, ViewMinX, ViewMaxX, ViewMinY, ViewMaxY);
                        DrawTickMarks(ds, source, grid, ViewMinX, ViewMaxX, ViewMinY, ViewMaxY, GetBrushColor(source.StrokeBrush));
                    }
                }
                //绘制指针指示线（十字光标）
                DrawPointerIndicatorLines(ds, grid);
            }

            // Some GPUs/devices or Win2D tiling behaviours can cause region-based invalidation to
            // miss tiles. Prefer full-draw when control is large OR when the invalidated regions
            // do not cover the whole control area (fallback coverage check).
            const double FullDrawThreshold = 2500.0;
            bool doFullDraw = sender.ActualWidth > FullDrawThreshold || sender.ActualHeight > FullDrawThreshold;

            if (!doFullDraw)
            {
                // Estimate coverage of invalidated regions. If they don't cover nearly the whole
                // control, fall back to a full draw to avoid missing tiles (observed on some GPUs).
                double totalArea = 0.0;
                foreach (var region in args.InvalidatedRegions)
                {
                    // clamp region to control bounds
                    double w = Math.Max(0.0, Math.Min(region.Width, sender.ActualWidth));
                    double h = Math.Max(0.0, Math.Min(region.Height, sender.ActualHeight));
                    totalArea += w * h;
                }

                double controlArea = Math.Max(1.0, sender.ActualWidth * sender.ActualHeight);
                // If less than 98% of the control area is covered, do a full draw to be safe.
                if (totalArea < controlArea * 0.98)
                    doFullDraw = true;
            }

            if (doFullDraw)
            {
                using var ds = sender.CreateDrawingSession(new Windows.Foundation.Rect(0, 0, sender.ActualWidth, sender.ActualHeight));
                DrawIntoSession(ds);
            }
            else
            {
                //绘制所有无效区域
                foreach (var region in args.InvalidatedRegions)
                {
                    using var ds = sender.CreateDrawingSession(region);
                    DrawIntoSession(ds);
                }
            }
            SetFPSUpdateTime();
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

        
        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            //if(LineDemonstratorCanvasControl != null) LineDemonstratorCanvasControl.RemoveFromVisualTree();
            //if(WaveformDemonstratorCanvasControl != null) WaveformDemonstratorCanvasControl.RemoveFromVisualTree();
        }



        private void ResetViewButton_Click(object sender, RoutedEventArgs e)
        {
            ZoomControlToReset();
        }

        private float? _cursorX = null;
        private float? _cursorY = null;
        // store recent drag samples for velocity estimation
        private readonly Queue<(Point pos, long time)> _dragSamples = new();

        private Dictionary<uint, PointerPoint> _activePointers = new();
        private bool _isMultiTouch = false;
        private double _lastDistance = 0;
        private Point _lastCenter;


        private void OnWaveformPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            var point = e.GetCurrentPoint(WaveformDemonstratorCanvasControl);
            _cursorX = (float)point.Position.X;
            _cursorY = (float)point.Position.Y;
            InvalidateCanvas();
        }

        private void UserControl_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var point = e.GetCurrentPoint(WaveformDemonstratorCanvasControl);
            _activePointers[point.PointerId] = point;

            // 多指触摸支持
            if (_activePointers.Count == 2)
            {
                _isMultiTouch = true;
                var pts = _activePointers.Values.ToList();
                _lastDistance = GetDistance(pts[0].Position, pts[1].Position);
                _lastCenter = GetCenter(pts[0].Position, pts[1].Position);
                return;
            }

            // 单指拖动（支持鼠标和触摸）
            if (_activePointers.Count == 1)
            {
                _isDragging = true;
                _lastDragPosition = point.Position;
                this.CapturePointer(e.Pointer);
                // cancel any running inertia when starting a new drag
                StopInertia();
                // clear any previous drag samples
                _dragSamples.Clear();
                // record initial sample (high-resolution timestamp)
                _dragSamples.Enqueue((point.Position, Stopwatch.GetTimestamp()));
                e.Handled = true;
            }
        }

        private void UserControl_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            var point = e.GetCurrentPoint(WaveformDemonstratorCanvasControl);
            if (_activePointers.ContainsKey(point.PointerId))
                _activePointers[point.PointerId] = point;

            // 多指缩放与拖动
            if (_isMultiTouch && _activePointers.Count == 2)
            {
                var pts = _activePointers.Values.ToList();
                Point p1 = pts[0].Position;
                Point p2 = pts[1].Position;

                Point newCenter = GetCenter(p1, p2);
                double newDistance = GetDistance(p1, p2);
                double scale = newDistance / _lastDistance;

                var grid = GetGridBounds(WaveformDemonstratorCanvasControl.ActualWidth, WaveformDemonstratorCanvasControl.ActualHeight);

                // 逻辑锚点
                var logicalAnchor = MapToLogical(_lastCenter, grid, ViewMinX, ViewMaxX, ViewMinY, ViewMaxY);

                // 计算缩放
                double targetScaleX = HorizontalZoomScale * scale;
                double targetScaleY = VerticalZoomScale * scale;
                double rangeX = (MaxHorizontalValue - MinHorizontalValue) / targetScaleX;
                double rangeY = (MaxVerticalValue - MinVerticalValue) / targetScaleY;

                // 反推新的逻辑中心点
                double percentX = (_lastCenter.X - grid.Left) / grid.Width;
                double percentY = 1.0 - (_lastCenter.Y - grid.Top) / grid.Height;
                double newCenterX = logicalAnchor.X - (percentX - 0.5) * rangeX;
                double newCenterY = logicalAnchor.Y - (percentY - 0.5) * rangeY;

                // 平移补偿
                double dx = newCenter.X - _lastCenter.X;
                double dy = newCenter.Y - _lastCenter.Y;
                double logicDeltaX = -dx * rangeX / grid.Width;
                double logicDeltaY = dy * rangeY / grid.Height;

                // 直接同步更新（无动画，提升流畅度） — 立即反馈
                HorizontalZoomScale = targetScaleX;
                VerticalZoomScale = targetScaleY;
                RuntimeCenter = new Point(newCenterX + logicDeltaX, newCenterY + logicDeltaY);
                InvalidateCanvas();
                UpdateResetViewVisibility();
                StartAnimationLoop();

                _lastDistance = newDistance;
                _lastCenter = newCenter;
                e.Handled = true;
                return;
            }

            // 单指拖动（支持鼠标和触摸）
            if (_isDragging && ZoomMode != WaveformZoomMode.Disabled)
            {
                var curPos = point.Position;
                var delta = new Vector2(
                    (float)(curPos.X - _lastDragPosition.X),
                    (float)(curPos.Y - _lastDragPosition.Y)
                );

                if (delta.Length() < 0.5f) return;

                var margin = WaveGridBorderMargin;
                double drawableWidth = ActualWidth - margin.Left - margin.Right;
                double drawableHeight = ActualHeight - margin.Top - margin.Bottom;
                if (drawableWidth <= 0 || drawableHeight <= 0) return;

                double viewW = ViewMaxX - ViewMinX;
                double viewH = ViewMaxY - ViewMinY;

                double logicDeltaX = delta.X * viewW / drawableWidth;
                double logicDeltaY = -delta.Y * viewH / drawableHeight;

                // Immediate update for drag (direct feedback)
                RuntimeCenter = new Point(
                    RuntimeCenter.X - logicDeltaX * DragSensitivity,
                    RuntimeCenter.Y - logicDeltaY * DragSensitivity);
                InvalidateCanvas();
                UpdateResetViewVisibility();

                _lastDragPosition = curPos;
                // record sample for velocity estimation (high-resolution timestamp)
                _dragSamples.Enqueue((curPos, Stopwatch.GetTimestamp()));
                while (_dragSamples.Count > 8) _dragSamples.Dequeue();

                e.Handled = true;
            }
        }

        private void UserControl_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            var point = e.GetCurrentPoint(WaveformDemonstratorCanvasControl);
            _activePointers.Remove(point.PointerId);

            if (_activePointers.Count < 2)
                _isMultiTouch = false;

                if (_isDragging)
            {
                _isDragging = false;
                this.ReleasePointerCapture(e.Pointer);

                    // 建议阈值调整为 20~50 像素/秒（根据实际手感调）
                    // compute velocity from samples
                    if (_dragSamples.Count >= 2)
                    {
                    var first = _dragSamples.Peek();
                    var last = _dragSamples.Last();
                    double dt = (last.time - first.time) / (double)Stopwatch.Frequency; // seconds
                    if (dt > 0.001) // ignore too-short intervals
                    {
                        var dx = last.pos.X - first.pos.X;
                        var dy = last.pos.Y - first.pos.Y;
                        var vel = new Vector2((float)(dx / dt), (float)(dy / dt)); // pixels/sec
                        // clamp extreme velocities to avoid 'fly' due to noise
                        const float maxVel = 8000f;
                        if (vel.Length() > maxVel)
                            vel = Vector2.Normalize(vel) * maxVel;

                        // set inertia velocity (pixel/sec)
                        _inertiaVelocity = vel;
                        if (_inertiaVelocity.Length() > 30f)
                            StartInertiaAnimation();
                    }
                        _dragSamples.Clear();
                    }

                e.Handled = true;
            }
        }

        private void UserControl_PointerCanceled(object sender, PointerRoutedEventArgs e)
        {
            var point = e.GetCurrentPoint(WaveformDemonstratorCanvasControl);
            _activePointers.Remove(point.PointerId);
            if (_activePointers.Count < 2)
                _isMultiTouch = false;
        }
        private void UserControl_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            // 仅在未拖动时清除指针相关状态（适配触摸和鼠标）
            if (!_isDragging)
            {
                _cursorX = null;
                _cursorY = null;
                _activePointers.Clear();
                _isMultiTouch = false;
            }
            WaveformDemonstratorCanvasControl.Invalidate();
        }

        private DateTime _lastWheelTime = DateTime.MinValue;

        private void UserControl_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            if (ZoomMode == WaveformZoomMode.Disabled) return;

            var pointerPoint = e.GetCurrentPoint(WaveformDemonstratorCanvasControl);
            var position = pointerPoint.Position;
            int delta = pointerPoint.Properties.MouseWheelDelta;
            if (delta == 0) return;

            // 计算滚动间隔
            var now = DateTime.Now;
            var intervalMs = (now - _lastWheelTime).TotalMilliseconds;
            _lastWheelTime = now;

            // 计算加速系数（越快滚动，系数越小，动画越短）
            double speedFactor = Math.Clamp(intervalMs / 120.0, 0.3, 1.0);
            var duration = TimeSpan.FromMilliseconds(160 * speedFactor);

            var (targetScaleX, targetScaleY) = CalculateZoomTarget(delta);
            // Modifier keys override axis behavior: Ctrl -> horizontal only, Alt (Menu) -> vertical only
            var mods = e.KeyModifiers;
            bool ctrl = mods.HasFlag(VirtualKeyModifiers.Control);
            bool alt = mods.HasFlag(VirtualKeyModifiers.Menu);

            if (ctrl && !alt)
            {
                // horizontal only
                targetScaleY = VerticalZoomScale;
            }
            else if (alt && !ctrl)
            {
                // vertical only
                targetScaleX = HorizontalZoomScale;
            }
            else if (ctrl && alt)
            {
                // both modifiers -> apply to both axes (no change)
            }
            var grid = GetGridBounds(WaveformDemonstratorCanvasControl.ActualWidth, WaveformDemonstratorCanvasControl.ActualHeight);
            var logicalBefore = MapToLogical(position, grid, ViewMinX, ViewMaxX, ViewMinY, ViewMaxY);
            var (newCenterX, newCenterY) = CalculateNewCenter(position, grid, logicalBefore, targetScaleX, targetScaleY);

            UpdateResetViewVisibility();
            StartZoomAnimation(newCenterX, newCenterY, targetScaleX, targetScaleY, duration);
            e.Handled = true;
        }

        private (double targetScaleX, double targetScaleY) CalculateZoomTarget(int delta)
        {
            double factor = delta > 0 ? 1.18 : 1.0 / 1.18;
            double targetScaleX = HorizontalZoomScale * (ZoomMode is WaveformZoomMode.Horizontal or WaveformZoomMode.Both ? factor : 1.0);
            double targetScaleY = VerticalZoomScale * (ZoomMode is WaveformZoomMode.Vertical or WaveformZoomMode.Both ? factor : 1.0);
            return (targetScaleX, targetScaleY);
        }

        private (double newCenterX, double newCenterY) CalculateNewCenter(Point position, GridBounds grid, Point logicalBefore, double targetScaleX, double targetScaleY)
        {
            double zoomX = targetScaleX;
            double zoomY = targetScaleY;
            double rangeX = (MaxHorizontalValue - MinHorizontalValue) / zoomX;
            double rangeY = (MaxVerticalValue - MinVerticalValue) / zoomY;
            double percentX = (position.X - grid.Left) / grid.Width;
            double percentY = 1.0 - (position.Y - grid.Top) / grid.Height;
            double newCenterX = logicalBefore.X - (percentX - 0.5) * rangeX;
            double newCenterY = logicalBefore.Y - (percentY - 0.5) * rangeY;
            return (newCenterX, newCenterY);
        }

        // 辅助方法
        private static double GetDistance(Point p1, Point p2)
        {
            double dx = p1.X - p2.X;
            double dy = p1.Y - p2.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        private static Point GetCenter(Point p1, Point p2)
        {
            return new Point((p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2);
        }

    }
}
