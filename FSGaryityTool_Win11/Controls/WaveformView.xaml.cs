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
using Windows.Storage;
using Windows.Storage.Streams;
using System.Threading.Tasks;
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
            // Use instance helper to avoid allocating a closure per invalidation
            // See DrawIntoSession below

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
                DrawAllSourcesIntoSession(sender, ds, grid);
            }
            else
            {
                //绘制所有无效区域
                foreach (var region in args.InvalidatedRegions)
                {
                    using var ds = sender.CreateDrawingSession(region);
                    DrawAllSourcesIntoSession(sender, ds, grid);
                }
            }
            SetFPSUpdateTime();
        }

        // Non-capturing helper to avoid per-frame closure allocations for DrawIntoSession
        private void DrawAllSourcesIntoSession(CanvasVirtualControl sender, CanvasDrawingSession ds, GridBounds grid)
        {
            if (Data != null)
            {
                foreach (var source in Data)
                {
                    DrawPolyLines(sender, ds, source, grid, ViewMinX, ViewMaxX, ViewMinY, ViewMaxY, HorizontalZoomScale);
                    DrawFunction(sender, ds, source, grid, ViewMinX, ViewMaxX, ViewMinY, ViewMaxY);
                    DrawTickMarks(ds, source, grid, ViewMinX, ViewMaxX, ViewMinY, ViewMaxY, GetBrushColor(source.StrokeBrush));
                }
            }

            DrawPointerIndicatorLines(ds, grid);
        }

        /// <summary>
        /// Renders the current waveform view into the provided StorageFile as an image.
        /// The caller is responsible for obtaining a writable StorageFile (e.g. via KnownFolders or a FileSavePicker).
        /// </summary>
        public async Task SaveWaveformToFileAsync(StorageFile file, CanvasBitmapFileFormat format = CanvasBitmapFileFormat.Png, float dpi = 96f)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));
            if (WaveformDemonstratorCanvasControl == null) throw new InvalidOperationException("Canvas control is not initialized.");

            double width = WaveformDemonstratorCanvasControl.ActualWidth;
            double height = WaveformDemonstratorCanvasControl.ActualHeight;
            if (width <= 0 || height <= 0) throw new InvalidOperationException("Invalid control size.");

            var device = WaveformDemonstratorCanvasControl.Device;
            using (var render = new CanvasRenderTarget(device, (float)width, (float)height, dpi))
            {
                using (var ds = render.CreateDrawingSession())
                {
                    // clear explicitly then draw the same content we render to screen into the render target
                    ds.Clear(Microsoft.UI.Colors.Transparent);
                    var grid = GetGridBounds(width, height);
                    DrawAllSourcesIntoSession(WaveformDemonstratorCanvasControl, ds, grid);
                }

                // Win2D CanvasRenderTarget: save via stream with explicit format
                using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    await render.SaveAsync(stream, format);
                }
            }
        }

        /// <summary>
        /// Convenience: save a screenshot into the user's Pictures library (generates unique name).
        /// </summary>
        public async Task<StorageFile> SaveWaveformToPicturesAsync(string suggestedFileName = "waveform.png", CanvasBitmapFileFormat format = CanvasBitmapFileFormat.Png)
        {
            var pictures = KnownFolders.PicturesLibrary;
            var file = await pictures.CreateFileAsync(suggestedFileName, CreationCollisionOption.GenerateUniqueName);
            await SaveWaveformToFileAsync(file, format);
            return file;
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
        // Inertia accumulator for post-drag animation
        private readonly InertiaAccumulator _inertiaAccumulator = new() { MinVelocity = 30f };

        private Dictionary<uint, PointerPoint> _activePointers = new();
        private bool _isMultiTouch = false;
        private double _lastDistance = 0;
        private Point _lastCenter;


        private void OnWaveformPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            HandleWaveformPointerMoved(e);
        }

        private void UserControl_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            HandlePointerPressed(e);
        }

        private void UserControl_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            HandlePointerMoved(e);
        }

        private void UserControl_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            HandlePointerReleased(e);
        }

        private void UserControl_PointerCanceled(object sender, PointerRoutedEventArgs e)
        {
            HandlePointerCanceled(e);
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
