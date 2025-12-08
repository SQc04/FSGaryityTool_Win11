using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.ComponentModel;
using Windows.Foundation;
using System.Diagnostics;

namespace FSGaryityTool_Win11.Controls
{
    public enum ScrollViewBarOrientation
    {
        Horizontal,
        Vertical
    }

    public sealed partial class ScrollViewBar : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty WaveGridBorderOpacityProperty = DependencyProperty.Register(
            nameof(WaveGridBorderOpacity),
            typeof(double),
            typeof(ScrollViewBar),
            new PropertyMetadata(1.0, OnVisualPropertyChanged));

        public double WaveGridBorderOpacity
        {
            get => (double)GetValue(WaveGridBorderOpacityProperty);
            set => SetValue(WaveGridBorderOpacityProperty, value);
        }

        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(
            nameof(Orientation),
            typeof(ScrollViewBarOrientation),
            typeof(ScrollViewBar),
            new PropertyMetadata(ScrollViewBarOrientation.Horizontal, OnOrientationChanged));

        public ScrollViewBarOrientation Orientation
        {
            get => (ScrollViewBarOrientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        public static readonly DependencyProperty ScrollOffsetProperty = DependencyProperty.Register(
            nameof(ScrollOffset),
            typeof(double),
            typeof(ScrollViewBar),
            new PropertyMetadata(0.0, OnScrollOffsetChanged));

        public double ScrollOffset
        {
            get => (double)GetValue(ScrollOffsetProperty);
            set => SetValue(ScrollOffsetProperty, value);
        }

        public static readonly DependencyProperty ViewportSizeProperty = DependencyProperty.Register(
            nameof(ViewportSize),
            typeof(double),
            typeof(ScrollViewBar),
            new PropertyMetadata(20.0, OnViewportOrExtentChanged));

        public double ViewportSize
        {
            get => (double)GetValue(ViewportSizeProperty);
            set => SetValue(ViewportSizeProperty, value);
        }

        public static readonly DependencyProperty ExtentSizeProperty = DependencyProperty.Register(
            nameof(ExtentSize),
            typeof(double),
            typeof(ScrollViewBar),
            new PropertyMetadata(100.0, OnViewportOrExtentChanged));

        public double ExtentSize
        {
            get => (double)GetValue(ExtentSizeProperty);
            set => SetValue(ExtentSizeProperty, value);
        }

        public static readonly DependencyProperty WaveformDataProperty = DependencyProperty.Register(
            nameof(WaveformData),
            typeof(float[]),
            typeof(ScrollViewBar),
            new PropertyMetadata(null, OnWaveformDataChanged));

        public float[] WaveformData
        {
            get => (float[])GetValue(WaveformDataProperty);
            set => SetValue(WaveformDataProperty, value);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<double> ScrollOffsetChanged;

        private bool isDragging;
        private Point startPosition;
        private double startOffset;
        private const double ScrollIncrement = 10.0;

        public ScrollViewBar()
        {
            InitializeComponent();
            Loaded += ScrollViewBar_Loaded;
            SizeChanged += ScrollViewBar_SizeChanged;
        }

        private void ScrollViewBar_Loaded(object sender, RoutedEventArgs e)
        {
            VerticalThumb.AddHandler(UIElement.PointerPressedEvent, new PointerEventHandler(Thumb_PointerPressed), true);
            HorizontalThumb.AddHandler(UIElement.PointerPressedEvent, new PointerEventHandler(Thumb_PointerPressed), true);

            VerticalThumbTop.AddHandler(UIElement.PointerPressedEvent, new PointerEventHandler(ThumbTop_PointerPressed), true);
            VerticalThumbBottom.AddHandler(UIElement.PointerPressedEvent, new PointerEventHandler(ThumbBottom_PointerPressed), true);

            // 重复按钮事件处理
            if (HorizontalRoot?.FindName("RepeatButtonLeft") is RepeatButton leftButton)
                leftButton.Click += RepeatButtonLeft_Click;
            if (HorizontalRoot?.FindName("RepeatButtonRight") is RepeatButton rightButton)
                rightButton.Click += RepeatButtonRight_Click;
            if (VerticalRoot?.FindName("RepeatButtonUp") is RepeatButton upButton)
                upButton.Click += RepeatButtonUp_Click;
            if (VerticalRoot?.FindName("RepeatButtonDown") is RepeatButton downButton)
                downButton.Click += RepeatButtonDown_Click;

        }

        private void ScrollViewBar_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateThumbSize();
            UpdateScrollPosition();
        }

        private static void OnVisualPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ScrollViewBar view)
            {
                view.InvalidateCanvas();
            }
        }

        private static void OnOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ScrollViewBar control)
            {
                control.UpdateVisualState();
            }
        }

        private static void OnScrollOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ScrollViewBar view)
            {
                view.ScrollOffsetChanged?.Invoke(view, (double)e.NewValue);
                view.UpdateThumbSize(); 
                view.UpdateScrollPosition();
            }
        }

        private static void OnViewportOrExtentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ScrollViewBar view)
            {
                view.UpdateThumbSize();
            }
        }

        private static void OnWaveformDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ScrollViewBar view)
            {
                view.InvalidateCanvas();
            }
        }

        private void UpdateVisualState()
        {
            VisualStateManager.GoToState(this, Orientation.ToString(), true);

            if (HorizontalRoot != null && VerticalRoot != null)
            {
                HorizontalRoot.Visibility = Orientation == ScrollViewBarOrientation.Horizontal
                    ? Visibility.Visible
                    : Visibility.Collapsed;

                VerticalRoot.Visibility = Orientation == ScrollViewBarOrientation.Vertical
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }

        private void InvalidateCanvas()
        {
            if (HorizontalRoot?.FindName("CanvasControl1") is CanvasControl horizontalCanvas1)
                horizontalCanvas1.Invalidate();
            if (HorizontalRoot?.FindName("CanvasControl2") is CanvasControl horizontalCanvas2)
                horizontalCanvas2.Invalidate();
            if (VerticalRoot?.FindName("CanvasControl1") is CanvasControl verticalCanvas1)
                verticalCanvas1.Invalidate();
            if (VerticalRoot?.FindName("CanvasControl2") is CanvasControl verticalCanvas2)
                verticalCanvas2.Invalidate();
        }

        private void Thumb_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (ExtentSize <= ViewportSize) return; // 无需拖动

            Debug.WriteLine("Pressed");
            isDragging = true;
            startPosition = e.GetCurrentPoint(RootGrid).Position;
            startOffset = ScrollOffset;

            if (sender is UIElement thumb)
                thumb.CapturePointer(e.Pointer);

            e.Handled = true;
        }

        private void Thumb_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (!isDragging) return;

            var currentPosition = e.GetCurrentPoint(RootGrid).Position;

            if (Orientation == ScrollViewBarOrientation.Horizontal)
            {
                var deltaX = currentPosition.X - startPosition.X;
                var trackLength = HorizontalScrollViewBarBorder.ActualWidth - AllHorizontalThumb.ActualWidth;

                if (trackLength > 0)
                {
                    var newOffset = startOffset + deltaX;
                    ScrollOffset = Math.Clamp(newOffset, 0, trackLength);
                }
            }
            else // Vertical
            {
                var deltaY = currentPosition.Y - startPosition.Y;
                var trackLength = VerticalScrollViewBarBorder.ActualHeight - AllVerticalThumb.ActualHeight;

                if (trackLength > 0)
                {
                    var newOffset = startOffset + deltaY;
                    ScrollOffset = Math.Clamp(newOffset, 0, trackLength);
                }
            }

            Debug.WriteLine($"Move {ScrollOffset}");
            e.Handled = true;
        }



        private void Thumb_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            Debug.WriteLine("Released");
            isDragging = false;

            if (sender is UIElement thumb)
                thumb.ReleasePointerCapture(e.Pointer);

            e.Handled = true;
        }


        private Point topStartPosition;
        private double topStartViewport;

        private void ThumbTop_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            topStartPosition = e.GetCurrentPoint(RootGrid).Position;
            topStartViewport = ViewportSize;
            (sender as UIElement)?.CapturePointer(e.Pointer);
            e.Handled = true;
        }

        private void ThumbTop_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            var currentPosition = e.GetCurrentPoint(RootGrid).Position;
            var deltaY = currentPosition.Y - topStartPosition.Y;

            var trackHeight = AllVerticalThumb.ActualHeight;
            if (trackHeight <= 0 || ExtentSize <= 0) return;

            // 将像素拖动距离映射为逻辑 ViewportSize 减少量
            var deltaViewport = (deltaY / trackHeight) * ExtentSize;
            var newViewport = Math.Clamp(topStartViewport - deltaViewport, GetMinViewportSize(), ExtentSize);

            ViewportSize = newViewport;
            UpdateThumbSize();
            UpdateScrollPosition();
            e.Handled = true;
        }

        private void ThumbTop_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            (sender as UIElement)?.ReleasePointerCapture(e.Pointer);
            e.Handled = true;
        }

        private Point bottomStartPosition;
        private double bottomStartViewport;

        private void ThumbBottom_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            bottomStartPosition = e.GetCurrentPoint(RootGrid).Position;
            bottomStartViewport = ViewportSize;

            if (sender is UIElement thumb)
                thumb.CapturePointer(e.Pointer);

            e.Handled = true;
        }

        private void ThumbBottom_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            var currentPosition = e.GetCurrentPoint(RootGrid).Position;
            var deltaY = currentPosition.Y - bottomStartPosition.Y;

            var trackHeight = AllVerticalThumb.ActualHeight;
            if (trackHeight <= 0 || ExtentSize <= 0) return;

            // 将像素拖动距离映射为逻辑 ViewportSize 增加量
            var deltaViewport = (deltaY / trackHeight) * ExtentSize;
            var newViewport = Math.Clamp(bottomStartViewport + deltaViewport, GetMinViewportSize(), ExtentSize);

            ViewportSize = newViewport;
            UpdateThumbSize();
            UpdateScrollPosition();
            e.Handled = true;
        }

        private void ThumbBottom_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (sender is UIElement thumb)
                thumb.ReleasePointerCapture(e.Pointer);

            e.Handled = true;
        }

        // ✅ 建议添加一个最小视口保护方法
        private double GetMinViewportSize()
        {
            // 将最小滑块高度转换为逻辑 ViewportSize
            double minThumbHeight = (double)Resources["ScrollViewBarVerticalThumbMinHeight"];
            double trackHeight = AllVerticalThumb.ActualHeight;
            return trackHeight > 0 ? (minThumbHeight / trackHeight) * ExtentSize : 10;
        }






        private void RepeatButtonLeft_Click(object sender, RoutedEventArgs e)
        {
            if (Orientation == ScrollViewBarOrientation.Horizontal)
            {
                ScrollOffset = Math.Max(0, ScrollOffset - ScrollIncrement);
            }
        }

        private void RepeatButtonRight_Click(object sender, RoutedEventArgs e)
        {
            if (Orientation == ScrollViewBarOrientation.Horizontal)
            {
                var maxScrollOffset = HorizontalScrollViewBarBorder.ActualWidth - AllHorizontalThumb.ActualWidth;
                ScrollOffset = Math.Min(maxScrollOffset, ScrollOffset + ScrollIncrement);
            }
        }

        private void RepeatButtonUp_Click(object sender, RoutedEventArgs e)
        {
            if (Orientation == ScrollViewBarOrientation.Vertical)
            {
                ScrollOffset = Math.Max(0, ScrollOffset - ScrollIncrement);
            }
        }

        private void RepeatButtonDown_Click(object sender, RoutedEventArgs e)
        {
            if (Orientation == ScrollViewBarOrientation.Vertical)
            {
                var maxScrollOffset = VerticalScrollViewBarBorder.ActualHeight - AllVerticalThumb.ActualHeight;
                ScrollOffset = Math.Min(maxScrollOffset, ScrollOffset + ScrollIncrement);
            }
        }

        private void UpdateThumbSize()
        {
            if (ExtentSize <= 0 || ViewportSize <= 0) return;

            if (Orientation == ScrollViewBarOrientation.Horizontal)
            {
                var trackWidth = HorizontalScrollViewBarBorder.ActualWidth;
                double thumbWidth;

                if (ExtentSize <= ViewportSize)
                {
                    thumbWidth = trackWidth; // 全部显示，无需滑动
                }
                else
                {
                    thumbWidth = (ViewportSize / ExtentSize) * trackWidth;
                    thumbWidth = Math.Max(thumbWidth, (double)Resources["ScrollViewBarHorizontalThumbMinHeight"]);
                }

                HorizontalThumb.Width = thumbWidth;
            }
            else
            {
                var trackHeight = VerticalScrollViewBarBorder.ActualHeight;
                double thumbHeight;

                if (ExtentSize <= ViewportSize)
                {
                    thumbHeight = trackHeight;
                }
                else
                {
                    thumbHeight = (ViewportSize / ExtentSize) * trackHeight;
                    thumbHeight = Math.Max(thumbHeight, (double)Resources["ScrollViewBarVerticalThumbMinHeight"]);
                }

                VerticalThumb.Height = thumbHeight;
            }
        }


        private void UpdateScrollPosition()
        {
            if (ExtentSize <= 0 || ViewportSize <= 0) return;

            double maxScrollOffset = Math.Max(ExtentSize - ViewportSize, 0);

            if (Orientation == ScrollViewBarOrientation.Vertical)
            {
                double trackLength = VerticalScrollViewBarBorder.ActualHeight - VerticalThumb.ActualHeight;
                VerticalThumbTransform.Y = maxScrollOffset == 0 ? 0 : (ScrollOffset / maxScrollOffset) * trackLength;
            }
            else
            {
                double trackLength = HorizontalScrollViewBarBorder.ActualWidth - HorizontalThumb.ActualWidth;
                HorizontalThumbTransform.X = maxScrollOffset == 0 ? 0 : (ScrollOffset / maxScrollOffset) * trackLength;
            }
        }




        private void CanvasControl_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {

        }

        public void SetScrollOffset(double offset)
        {
            ScrollOffset = offset;
        }
    }
}