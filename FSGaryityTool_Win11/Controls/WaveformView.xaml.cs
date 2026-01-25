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
            InvalidateVisual();
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

            //WaveformDemonstratorCanvasControl.PointerMoved += OnWaveformPointerMoved;


            RootGrid.SizeChanged += WaveformView_SizeChanged;

            Loaded += WaveformView_Loaded;

            _fpsTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(250)  // 1000ms / 4 = 250ms
            };
            _fpsTimer.Tick += (s, e) => UpdateFPS();
            _fpsTimer.Start();
        }
        private void WaveformView_Loaded(object sender, RoutedEventArgs e)
        {
            // 此时 XAML 属性已经全部应用完毕
            RuntimeCenter = DefaultCenter;  // 触发更新
            UpdateVisibleRange();
            InvalidateCanvas();
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateSampleCountFromSize();
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

            //绘制所有无效区域
            foreach (var region in args.InvalidatedRegions)
            {
                using var ds = sender.CreateDrawingSession(region);

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
                _inertiaTimer?.Stop();
                this.CapturePointer(e.Pointer);
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

                // 直接同步更新（无动画，提升流畅度）
                HorizontalZoomScale = targetScaleX;
                VerticalZoomScale = targetScaleY;
                RuntimeCenter = new Point(newCenterX + logicDeltaX, newCenterY + logicDeltaY);

                _lastDistance = newDistance;
                _lastCenter = newCenter;
                e.Handled = true;
                return;
            }

            // 单指拖动（支持鼠标和触摸）
            if (_isDragging && ZoomMode != WaveformZoomMode.Disabled)
            {
                var curPos = point.Position;
                double deltaX = curPos.X - _lastDragPosition.X;
                double deltaY = curPos.Y - _lastDragPosition.Y;

                if (Math.Abs(deltaX) < 1 && Math.Abs(deltaY) < 1) return;

                var margin = WaveGridBorderMargin;
                double drawableWidth = ActualWidth - margin.Left - margin.Right;
                double drawableHeight = ActualHeight - margin.Top - margin.Bottom;
                if (drawableWidth <= 0 || drawableHeight <= 0) return;

                double viewW = ViewMaxX - ViewMinX;
                double viewH = ViewMaxY - ViewMinY;
                double logicDeltaX = deltaX * viewW / drawableWidth;
                double logicDeltaY = -deltaY * viewH / drawableHeight;

                RuntimeCenter = new Point(
                    RuntimeCenter.X - logicDeltaX * DragSensitivity,
                    RuntimeCenter.Y - logicDeltaY * DragSensitivity);

                _inertiaVelocity = new Vector2((float)deltaX, (float)deltaY) * 0.95f;
                _lastDragPosition = curPos;
                InvalidateCanvas();
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

                if (_inertiaVelocity.Length() > 0.1)
                {
                    StartInertiaAnimation();
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
            // 清除指针相关状态（适配触摸和鼠标）
            _cursorX = null;
            _cursorY = null;
            _isDragging = false;
            _activePointers.Clear();
            _isMultiTouch = false;
            WaveformDemonstratorCanvasControl.Invalidate();
        }

        private void UserControl_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            if (ZoomMode == WaveformZoomMode.Disabled) return;

            var pointerPoint = e.GetCurrentPoint(WaveformDemonstratorCanvasControl);
            var position = pointerPoint.Position; // 鼠标或触摸点像素坐标
            int delta = pointerPoint.Properties.MouseWheelDelta;
            if (delta == 0) return;

            double factor = delta > 0 ? 1.18 : 1.0 / 1.18;
            double targetScaleX = HorizontalZoomScale * (ZoomMode is WaveformZoomMode.Horizontal or WaveformZoomMode.Both ? factor : 1.0);
            double targetScaleY = VerticalZoomScale * (ZoomMode is WaveformZoomMode.Vertical or WaveformZoomMode.Both ? factor : 1.0);

            // 获取当前画布的 grid 区域
            var grid = GetGridBounds(WaveformDemonstratorCanvasControl.ActualWidth, WaveformDemonstratorCanvasControl.ActualHeight);

            // 记录缩放前指针像素坐标对应的逻辑坐标
            var logicalBefore = MapToLogical(position, grid, ViewMinX, ViewMaxX, ViewMinY, ViewMaxY);

            // 计算缩放后的显示范围
            double zoomX = targetScaleX;
            double zoomY = targetScaleY;
            double rangeX = (MaxHorizontalValue - MinHorizontalValue) / zoomX;
            double rangeY = (MaxVerticalValue - MinVerticalValue) / zoomY;

            // 计算新的中心点，使得缩放后指针像素坐标对应的逻辑坐标和缩放前一致
            double percentX = (position.X - grid.Left) / grid.Width;
            double percentY = 1.0 - (position.Y - grid.Top) / grid.Height;

            double newCenterX = logicalBefore.X - (percentX - 0.5) * rangeX;
            double newCenterY = logicalBefore.Y - (percentY - 0.5) * rangeY;

            UpdateResetViewVisibility();
            StartZoomAnimation(newCenterX, newCenterY, targetScaleX, targetScaleY, TimeSpan.FromMilliseconds(160));
            e.Handled = true;
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
