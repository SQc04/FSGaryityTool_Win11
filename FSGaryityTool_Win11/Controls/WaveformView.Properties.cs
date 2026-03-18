using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
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
        public static readonly DependencyProperty WaveGridBorderOpacityProperty = DependencyProperty.Register(
            nameof(WaveGridBorderOpacity), typeof(double), typeof(WaveformView), new PropertyMetadata(1.0, OnVisualPropertyChanged));
        public double WaveGridBorderOpacity
        {
            get => (double)GetValue(WaveGridBorderOpacityProperty);
            set => SetValue(WaveGridBorderOpacityProperty, value);
        }
        public static readonly DependencyProperty WaveDemonstratorOpacityProperty = DependencyProperty.Register(
            nameof(WaveDemonstratorOpacity), typeof(double), typeof(WaveformView), new PropertyMetadata(1.0, OnVisualPropertyChanged));
        public double WaveDemonstratorOpacity
        {
            get => (double)GetValue(WaveDemonstratorOpacityProperty);
            set => SetValue(WaveDemonstratorOpacityProperty, value);
        }

        public static readonly DependencyProperty MinRowTickCountProperty =
            DependencyProperty.Register(nameof(MinRowTickCount), typeof(int), typeof(WaveformView), new PropertyMetadata(0, OnVisualPropertyChanged));

        public int MinRowTickCount
        {
            get => (int)GetValue(MinRowTickCountProperty);
            set => SetValue(MinRowTickCountProperty, value);
        }

        public static readonly DependencyProperty MinColumnTickCountProperty =
            DependencyProperty.Register(nameof(MinColumnTickCount), typeof(int), typeof(WaveformView), new PropertyMetadata(0, OnVisualPropertyChanged));

        public int MinColumnTickCount
        {
            get => (int)GetValue(MinColumnTickCountProperty);
            set => SetValue(MinColumnTickCountProperty, value);
        }

        public static readonly DependencyProperty MaxRowTickCountProperty =
            DependencyProperty.Register(nameof(MaxRowTickCount), typeof(int), typeof(WaveformView), new PropertyMetadata(16, OnVisualPropertyChanged));

        public int MaxRowTickCount
        {
            get => (int)GetValue(MaxRowTickCountProperty);
            set => SetValue(MaxRowTickCountProperty, value);
        }

        public static readonly DependencyProperty MaxColumnTickCountProperty =
            DependencyProperty.Register(nameof(MaxColumnTickCount), typeof(int), typeof(WaveformView), new PropertyMetadata(16, OnVisualPropertyChanged));

        public int MaxColumnTickCount
        {
            get => (int)GetValue(MaxColumnTickCountProperty);
            set => SetValue(MaxColumnTickCountProperty, value);
        }

        public static readonly DependencyProperty MinColumnTickWidthProperty =
            DependencyProperty.Register(nameof(MinColumnTickWidth), typeof(double), typeof(WaveformView), new PropertyMetadata(20.0, OnVisualPropertyChanged));
        public double MinColumnTickWidth
        {
            get => (double)GetValue(MinColumnTickWidthProperty);
            set => SetValue(MinColumnTickWidthProperty, value);
        }
        public static readonly DependencyProperty MaxColumnTickWidthProperty =
            DependencyProperty.Register(nameof(MaxColumnTickWidth), typeof(double), typeof(WaveformView), new PropertyMetadata(1000.0, OnVisualPropertyChanged));
        public double MaxColumnTickWidth
        {
            get => (double)GetValue(MaxColumnTickWidthProperty);
            set => SetValue(MaxColumnTickWidthProperty, value);
        }

        public static readonly DependencyProperty MinRowTickHeightProperty =
            DependencyProperty.Register(nameof(MinRowTickHeight), typeof(double), typeof(WaveformView), new PropertyMetadata(20.0, OnVisualPropertyChanged));
        public double MinRowTickHeight
        {
            get => (double)GetValue(MinRowTickHeightProperty);
            set => SetValue(MinRowTickHeightProperty, value);
        }
        public static readonly DependencyProperty MaxRowTickHeightProperty =
            DependencyProperty.Register(nameof(MaxRowTickHeight), typeof(double), typeof(WaveformView), new PropertyMetadata(1000.0, OnVisualPropertyChanged));
        public double MaxRowTickHeight
        {
            get => (double)GetValue(MaxRowTickHeightProperty);
            set => SetValue(MaxRowTickHeightProperty, value);
        }

        public static readonly DependencyProperty RowTickCountProperty =
            DependencyProperty.Register(nameof(RowTickCount), typeof(string), typeof(WaveformView), new PropertyMetadata("8", OnVisualPropertyChanged));

        public string RowTickCount
        {
            get => (string)GetValue(RowTickCountProperty);
            set => SetValue(RowTickCountProperty, value);
        }

        public static readonly DependencyProperty ColumnTickCountProperty =
            DependencyProperty.Register(nameof(ColumnTickCount), typeof(string), typeof(WaveformView), new PropertyMetadata("8", OnVisualPropertyChanged));

        public string ColumnTickCount
        {
            get => (string)GetValue(ColumnTickCountProperty);
            set => SetValue(ColumnTickCountProperty, value);
        }
        public static readonly DependencyProperty RowAutoGridModeProperty =
            DependencyProperty.Register(nameof(RowAutoGridMode), typeof(AutoGridMode), typeof(WaveformView),
        new PropertyMetadata(AutoGridMode.Integer, OnVisualPropertyChanged));

        public AutoGridMode RowAutoGridMode
        {
            get => (AutoGridMode)GetValue(RowAutoGridModeProperty);
            set => SetValue(RowAutoGridModeProperty, value);
        }

        public static readonly DependencyProperty ColumnAutoGridModeProperty =
            DependencyProperty.Register(nameof(ColumnAutoGridMode), typeof(AutoGridMode), typeof(WaveformView),
                new PropertyMetadata(AutoGridMode.Integer, OnVisualPropertyChanged));

        public AutoGridMode ColumnAutoGridMode
        {
            get => (AutoGridMode)GetValue(ColumnAutoGridModeProperty);
            set => SetValue(ColumnAutoGridModeProperty, value);
        }

        public static readonly DependencyProperty RowAutoGridMultipleProperty =
            DependencyProperty.Register(nameof(RowAutoGridMultiple), typeof(int), typeof(WaveformView),
                new PropertyMetadata(1, OnVisualPropertyChanged));

        public int RowAutoGridMultiple
        {
            get => (int)GetValue(RowAutoGridMultipleProperty);
            set => SetValue(RowAutoGridMultipleProperty, value);
        }

        public static readonly DependencyProperty ColumnAutoGridMultipleProperty =
            DependencyProperty.Register(nameof(ColumnAutoGridMultiple), typeof(int), typeof(WaveformView),
                new PropertyMetadata(1, OnVisualPropertyChanged));

        public int ColumnAutoGridMultiple
        {
            get => (int)GetValue(ColumnAutoGridMultipleProperty);
            set => SetValue(ColumnAutoGridMultipleProperty, value);
        }



        public static readonly DependencyProperty WaveGridBorderMarginProperty =
            DependencyProperty.Register(nameof(WaveGridBorderMargin), typeof(Thickness), typeof(WaveformView), new PropertyMetadata(default(Thickness), OnVisualPropertyChanged));

        public Thickness WaveGridBorderMargin
        {
            get => (Thickness)GetValue(WaveGridBorderMarginProperty);
            set => SetValue(WaveGridBorderMarginProperty, value);
        }

        public static readonly DependencyProperty ControlBorderMarginProperty =
            DependencyProperty.Register(nameof(ControlBorderMargin), typeof(Thickness), typeof(WaveformView), new PropertyMetadata(new Thickness(3), OnVisualPropertyChanged));

        public Thickness ControlBorderMargin
        {
            get => (Thickness)GetValue(ControlBorderMarginProperty);
            set => SetValue(ControlBorderMarginProperty, value);
        }

        public static readonly DependencyProperty WaveGridBorderThicknessProperty =
            DependencyProperty.Register(nameof(WaveGridBorderThickness), typeof(Thickness), typeof(WaveformView), new PropertyMetadata(new Thickness(2), OnVisualPropertyChanged));

        public Thickness WaveGridBorderThickness
        {
            get => (Thickness)GetValue(WaveGridBorderThicknessProperty);
            set => SetValue(WaveGridBorderThicknessProperty, value);
        }

        public static readonly DependencyProperty RowTickLineWidthProperty =
            DependencyProperty.Register(nameof(RowTickLineWidth), typeof(double), typeof(WaveformView), new PropertyMetadata(1.5, OnVisualPropertyChanged));

        public double RowTickLineWidth
        {
            get => (double)GetValue(RowTickLineWidthProperty);
            set => SetValue(RowTickLineWidthProperty, value);
        }

        public static readonly DependencyProperty ColumnTickLineWidthProperty =
            DependencyProperty.Register(nameof(ColumnTickLineWidth), typeof(double), typeof(WaveformView), new PropertyMetadata(1.5, OnVisualPropertyChanged));

        public double ColumnTickLineWidth
        {
            get => (double)GetValue(ColumnTickLineWidthProperty);
            set => SetValue(ColumnTickLineWidthProperty, value);
        }

        public static readonly DependencyProperty RowSubTickLineWidthProperty =
            DependencyProperty.Register(nameof(RowSubTickLineWidth), typeof(double), typeof(WaveformView), new PropertyMetadata(1.0, OnVisualPropertyChanged));
        public double RowSubTickLineWidth
        {
            get => (double)GetValue(RowSubTickLineWidthProperty);
            set => SetValue(RowSubTickLineWidthProperty, value);
        }
        public static readonly DependencyProperty ColumnSubTickLineWidthProperty =
            DependencyProperty.Register(nameof(ColumnSubTickLineWidth), typeof(double), typeof(WaveformView), new PropertyMetadata(1.0, OnVisualPropertyChanged));
        public double ColumnSubTickLineWidth
        {
            get => (double)GetValue(ColumnSubTickLineWidthProperty);
            set => SetValue(ColumnSubTickLineWidthProperty, value);
        }

        public static readonly DependencyProperty WaveGridBorderBrushProperty =
            DependencyProperty.Register(nameof(WaveGridBorderBrush), typeof(Brush), typeof(WaveformView),
                new PropertyMetadata(null, OnVisualPropertyChanged));
        public Brush WaveGridBorderBrush
        {
            get => (Brush)GetValue(WaveGridBorderBrushProperty);
            set => SetValue(WaveGridBorderBrushProperty, value);
        }

        public static readonly DependencyProperty RowTickLineBrushProperty =
            DependencyProperty.Register(nameof(RowTickLineBrush), typeof(Brush), typeof(WaveformView),
                new PropertyMetadata(null, OnVisualPropertyChanged));
        public Brush RowTickLineBrush
        {
            get => (Brush)GetValue(RowTickLineBrushProperty);
            set => SetValue(RowTickLineBrushProperty, value);
        }

        public static readonly DependencyProperty ColumnTickLineBrushProperty =
            DependencyProperty.Register(nameof(ColumnTickLineBrush), typeof(Brush), typeof(WaveformView),
                new PropertyMetadata(null, OnVisualPropertyChanged));
        public Brush ColumnTickLineBrush
        {
            get => (Brush)GetValue(ColumnTickLineBrushProperty);
            set => SetValue(ColumnTickLineBrushProperty, value);
        }

        public static readonly DependencyProperty RowCenterTickLineBrushProperty =
            DependencyProperty.Register(nameof(RowCenterTickLineBrush), typeof(Brush), typeof(WaveformView),
                new PropertyMetadata(null, OnVisualPropertyChanged));
        public Brush RowCenterTickLineBrush
        {
            get => (Brush)GetValue(RowCenterTickLineBrushProperty);
            set => SetValue(RowCenterTickLineBrushProperty, value);
        }

        public static readonly DependencyProperty ColumnCenterTickLineBrushProperty =
            DependencyProperty.Register(nameof(ColumnCenterTickLineBrush), typeof(Brush), typeof(WaveformView),
                new PropertyMetadata(null, OnVisualPropertyChanged));
        public Brush ColumnCenterTickLineBrush
        {
            get => (Brush)GetValue(ColumnCenterTickLineBrushProperty);
            set => SetValue(ColumnCenterTickLineBrushProperty, value);
        }

        public static readonly DependencyProperty WaveGridBorderLineStyleProperty =
            DependencyProperty.Register(nameof(WaveGridBorderLineStyle), typeof(LineStyle), typeof(WaveformView), new PropertyMetadata(LineStyle.Solid, OnVisualPropertyChanged));

        public LineStyle WaveGridBorderLineStyle
        {
            get => (LineStyle)GetValue(WaveGridBorderLineStyleProperty);
            set => SetValue(WaveGridBorderLineStyleProperty, value);
        }

        public static readonly DependencyProperty WaveGridTickLineStyleProperty =
            DependencyProperty.Register(nameof(WaveGridTickLineStyle), typeof(LineStyle), typeof(WaveformView), new PropertyMetadata(LineStyle.Solid, OnVisualPropertyChanged));

        public LineStyle WaveGridTickLineStyle
        {
            get => (LineStyle)GetValue(WaveGridTickLineStyleProperty);
            set => SetValue(WaveGridTickLineStyleProperty, value);
        }

        public static readonly DependencyProperty WaveGridTickModeStyleProperty =
            DependencyProperty.Register(nameof(WaveGridTickMode), typeof(TickModeStyle), typeof(WaveformView), new PropertyMetadata(TickModeStyle.None, OnVisualPropertyChanged));
        public TickModeStyle WaveGridTickMode
        {
            get => (TickModeStyle)GetValue(WaveGridTickModeStyleProperty);
            set => SetValue(WaveGridTickModeStyleProperty, value);
        }
        public static readonly DependencyProperty WaveGridBorderTickThicknessProperty =
            DependencyProperty.Register(nameof(WaveGridBorderTickThickness), typeof(Thickness), typeof(WaveformView), new PropertyMetadata(new Thickness(4), OnVisualPropertyChanged));

        public Thickness WaveGridBorderTickThickness
        {
            get => (Thickness)GetValue(WaveGridBorderTickThicknessProperty);
            set => SetValue(WaveGridBorderTickThicknessProperty, value);
        }

        public static readonly DependencyProperty WaveGridBorderSubTickThicknessProperty =
            DependencyProperty.Register(nameof(WaveGridBorderSubTickThickness), typeof(Thickness), typeof(WaveformView), new PropertyMetadata(new Thickness(2), OnVisualPropertyChanged));
        public Thickness WaveGridBorderSubTickThickness
        {
            get => (Thickness)GetValue(WaveGridBorderSubTickThicknessProperty);
            set => SetValue(WaveGridBorderSubTickThicknessProperty, value);
        }
        public static readonly DependencyProperty RowGridBorderSubTickCountProperty =
            DependencyProperty.Register(nameof(RowGridBorderSubTickCount), typeof(int), typeof(WaveformView), new PropertyMetadata(0, OnVisualPropertyChanged));
        public int RowGridBorderSubTickCount
        {
            get => (int)GetValue(RowGridBorderSubTickCountProperty);
            set => SetValue(RowGridBorderSubTickCountProperty, value);
        }
        public static readonly DependencyProperty ColumnGridBorderSubTickCountProperty =
            DependencyProperty.Register(nameof(ColumnGridBorderSubTickCount), typeof(int), typeof(WaveformView), new PropertyMetadata(0, OnVisualPropertyChanged));
        public int ColumnGridBorderSubTickCount
        {
            get => (int)GetValue(ColumnGridBorderSubTickCountProperty);
            set => SetValue(ColumnGridBorderSubTickCountProperty, value);
        }

        public static readonly DependencyProperty CrossSizeProperty =
            DependencyProperty.Register(nameof(CrossSize), typeof(double), typeof(WaveformView), new PropertyMetadata(6.0, OnVisualPropertyChanged));

        public double CrossSize
        {
            get => (double)GetValue(CrossSizeProperty);
            set => SetValue(CrossSizeProperty, value);
        }

        public static readonly DependencyProperty DotRadiusProperty =
            DependencyProperty.Register(nameof(DotRadius), typeof(double), typeof(WaveformView), new PropertyMetadata(3.0, OnVisualPropertyChanged));
        public double DotRadius
        {
            get => (double)GetValue(DotRadiusProperty);
            set => SetValue(DotRadiusProperty, value);
        }

        public static readonly DependencyProperty WaveGridBorderTickLineStyleProperty =
            DependencyProperty.Register(nameof(WaveGridBorderTickLineStyle), typeof(LineStyle), typeof(WaveformView), new PropertyMetadata(LineStyle.Solid, OnVisualPropertyChanged));

        public LineStyle WaveGridBorderTickLineStyle
        {
            get => (LineStyle)GetValue(WaveGridBorderTickLineStyleProperty);
            set => SetValue(WaveGridBorderTickLineStyleProperty, value);
        }

        public static readonly DependencyProperty PointerIndicatorModeProperty =
            DependencyProperty.Register(nameof(PointerIndicatorMode), typeof(IndicatorMode), typeof(WaveformView), new PropertyMetadata(IndicatorMode.None, OnVisualPropertyChanged));
        public IndicatorMode PointerIndicatorMode
        {
            get => (IndicatorMode)GetValue(PointerIndicatorModeProperty);
            set => SetValue(PointerIndicatorModeProperty, value);
        }

        public static readonly DependencyProperty PointerIndicatorLineStyleProperty =
            DependencyProperty.Register(nameof(PointerIndicatorLineStyle), typeof(LineStyle), typeof(WaveformView), new PropertyMetadata(LineStyle.Dash, OnVisualPropertyChanged));
        public LineStyle PointerIndicatorLineStyle
        {
            get => (LineStyle)GetValue(PointerIndicatorLineStyleProperty);
            set => SetValue(PointerIndicatorLineStyleProperty, value);
        }

        public static readonly DependencyProperty PointerIndicatorBrushProperty =
            DependencyProperty.Register(nameof(PointerIndicatorBrush), typeof(Brush), typeof(WaveformView), new PropertyMetadata(null, OnVisualPropertyChanged));
        public Brush PointerIndicatorBrush
        {
            get => (Brush)GetValue(PointerIndicatorBrushProperty);
            set => SetValue(PointerIndicatorBrushProperty, value);
        }
        public static readonly DependencyProperty PointerIndicatorLineWidthProperty =
            DependencyProperty.Register(nameof(PointerIndicatorLineWidth), typeof(double), typeof(WaveformView), new PropertyMetadata(1.5, OnVisualPropertyChanged));
        public double PointerIndicatorLineWidth
        {
            get => (double)GetValue(PointerIndicatorLineWidthProperty);
            set => SetValue(PointerIndicatorLineWidthProperty, value);
        }
        public static readonly DependencyProperty PointerIndicatorCursorModeProperty =
            DependencyProperty.Register(nameof(PointerIndicatorLineWidth), typeof(CrosshairCursorMode), typeof(WaveformView), new PropertyMetadata(CrosshairCursorMode.Full, OnVisualPropertyChanged));

        public CrosshairCursorMode PointerIndicatorCursorMode
        {
            get => (CrosshairCursorMode)GetValue(PointerIndicatorCursorModeProperty);
            set => SetValue(PointerIndicatorCursorModeProperty, value);
        }

        public static readonly DependencyProperty ZoomModeProperty =
            DependencyProperty.Register(nameof(ZoomMode), typeof(WaveformZoomMode), typeof(WaveformView), new PropertyMetadata(WaveformZoomMode.Disabled, OnVisualPropertyChanged));
        public WaveformZoomMode ZoomMode
        {
            get => (WaveformZoomMode)GetValue(ZoomModeProperty);
            set => SetValue(ZoomModeProperty, value);
        }
        public static readonly DependencyProperty MoveModeProperty =
            DependencyProperty.Register(nameof(MoveMode), typeof(WaveformZoomMode), typeof(WaveformView), new PropertyMetadata(WaveformZoomMode.Disabled, OnVisualPropertyChanged));
        public WaveformZoomMode MoveMode
        {
            get => (WaveformZoomMode)GetValue(MoveModeProperty);
            set => SetValue(MoveModeProperty, value);
        }

        public static readonly DependencyProperty HorizontalScrollBarVisibilityProperty =
            DependencyProperty.Register(nameof(HorizontalScrollBarVisibility), typeof(ViewScrollBarVisibility), typeof(WaveformView), new PropertyMetadata(ViewScrollBarVisibility.Auto, OnVisualPropertyChanged));
        public ViewScrollBarVisibility HorizontalScrollBarVisibility
        {
            get => (ViewScrollBarVisibility)GetValue(HorizontalScrollBarVisibilityProperty);
            set => SetValue(HorizontalScrollBarVisibilityProperty, value);
        }
        public static readonly DependencyProperty VerticalScrollBarVisibilityProperty =
            DependencyProperty.Register(nameof(VerticalScrollBarVisibility), typeof(ViewScrollBarVisibility), typeof(WaveformView), new PropertyMetadata(ViewScrollBarVisibility.Auto, OnVisualPropertyChanged));
        public ViewScrollBarVisibility VerticalScrollBarVisibility
        {
            get => (ViewScrollBarVisibility)GetValue(VerticalScrollBarVisibilityProperty);
            set => SetValue(VerticalScrollBarVisibilityProperty, value);
        }

        public static readonly DependencyProperty ViewCenterPointModeProperty =
            DependencyProperty.Register(nameof(ViewCenterPointMode), typeof(ViewCenterMode), typeof(WaveformView), new PropertyMetadata(ViewCenterMode.Center, OnVisualPropertyChanged));
        public ViewCenterMode ViewCenterPointMode
        {
            get => (ViewCenterMode)GetValue(ViewCenterPointModeProperty);
            set => SetValue(ViewCenterPointModeProperty, value);
        }
        public static readonly DependencyProperty ViewCenterPointProperty =
            DependencyProperty.Register(nameof(ViewCenterPoint), typeof(Point), typeof(WaveformView), new PropertyMetadata(new Point(0, 0), OnVisualPropertyChanged));
        public Point ViewCenterPoint
        {
            get => (Point)GetValue(ViewCenterPointProperty);
            set => SetValue(ViewCenterPointProperty, value);
        }

        public static readonly DependencyProperty MinHorizontalValueProperty =
            DependencyProperty.Register(nameof(MinHorizontalValue), typeof(double), typeof(WaveformView), new PropertyMetadata(-100.0, OnVisualPropertyChanged));
        public double MinHorizontalValue
        {
            get => (double)GetValue(MinHorizontalValueProperty);
            set => SetValue(MinHorizontalValueProperty, value);
        }
        public static readonly DependencyProperty MaxHorizontalValueProperty =
            DependencyProperty.Register(nameof(MaxHorizontalValue), typeof(double), typeof(WaveformView), new PropertyMetadata(100.0, OnVisualPropertyChanged));
        public double MaxHorizontalValue
        {
            get => (double)GetValue(MaxHorizontalValueProperty);
            set => SetValue(MaxHorizontalValueProperty, value);
        }
        public static readonly DependencyProperty MinVerticalValueProperty =
            DependencyProperty.Register(nameof(MinVerticalValue), typeof(double), typeof(WaveformView), new PropertyMetadata(-100.0, OnVisualPropertyChanged));
        public double MinVerticalValue
        {
            get => (double)GetValue(MinVerticalValueProperty);
            set => SetValue(MinVerticalValueProperty, value);
        }
        public static readonly DependencyProperty MaxVerticalValueProperty =
            DependencyProperty.Register(nameof(MaxVerticalValue), typeof(double), typeof(WaveformView), new PropertyMetadata(100.0, OnVisualPropertyChanged));
        public double MaxVerticalValue
        {
            get => (double)GetValue(MaxVerticalValueProperty);
            set => SetValue(MaxVerticalValueProperty, value);
        }

        public static readonly DependencyProperty HorizontalZoomScaleProperty =
            DependencyProperty.Register(nameof(HorizontalZoomScale), typeof(double), typeof(WaveformView), new PropertyMetadata(1.0, OnVisualPropertyChanged));

        public double HorizontalZoomScale
        {
            get => (double)GetValue(HorizontalZoomScaleProperty);
            set => SetValue(HorizontalZoomScaleProperty, value);
        }

        public static readonly DependencyProperty VerticalZoomScaleProperty =
            DependencyProperty.Register(nameof(VerticalZoomScale), typeof(double), typeof(WaveformView), new PropertyMetadata(1.0, OnVisualPropertyChanged));

        public double VerticalZoomScale
        {
            get => (double)GetValue(VerticalZoomScaleProperty);
            set => SetValue(VerticalZoomScaleProperty, value);
        }


        // Expose RuntimeCenter as a public DependencyProperty so it can be set by XAML (including XAML Hot Reload).
        // Provide a specific property changed callback to ensure runtime updates refresh the view.
        public static readonly DependencyProperty RuntimeCenterProperty =
            DependencyProperty.Register(nameof(RuntimeCenter), typeof(Point), typeof(WaveformView), new PropertyMetadata(new Point(0, 0), OnRuntimeCenterChanged));

        private static void OnRuntimeCenterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not WaveformView view) return;

            // When RuntimeCenter is updated (for example via XAML hot reload), make sure
            // the visible range and canvas are refreshed and reset-button visibility updated.
            view.UpdateVisibleRange();
            view.InvalidateCanvas();
            view.UpdateResetViewVisibility();
        }

        public DrawMode ViewDrawMode
        {
            get => (DrawMode)GetValue(ViewDrawModeProperty);
            set => SetValue(ViewDrawModeProperty, value);
        }
        private static readonly DependencyProperty ViewDrawModeProperty
            = DependencyProperty.Register(nameof(RuntimeCenter), typeof(DrawMode), typeof(WaveformView), new PropertyMetadata(DrawMode.WinCanvas, OnVisualPropertyChanged));


        /// <summary>
        /// 获取或设置应用程序是否处于调试模式。
        /// </summary>
        /// <remarks>
        /// 当设置为 <see langword="true"/> 时，WaveformView控件将包含额外的调试信息，并启用用于开发和故障排查的相关功能。
        /// 此属性通常用于在开发过程中切换调试行为，生产环境下应设置为 <see langword="false"/>。
        /// </remarks>
        public bool IsDebugMode
        {
            get => (bool)GetValue(IsDebugModeProperty);
            set => SetValue(IsDebugModeProperty, value);
        }
        private static readonly DependencyProperty IsDebugModeProperty =
            DependencyProperty.Register(nameof(IsDebugMode), typeof(bool), typeof(WaveformView), new PropertyMetadata(false, OnVisualPropertyChanged));

        /// <summary>
        /// 获取或设置调试模块在用户界面中的可见状态。
        /// </summary>
        /// <remarks>
        /// 更改此属性会影响调试模块在 UI 中的显示或隐藏。可根据应用状态或用户偏好控制调试功能的呈现。
        /// </remarks>
        private Visibility DebugModuleVisibitly
        {
            get => (Visibility)GetValue(DebugModuleVisibitlyProperty);
            set => SetValue(DebugModuleVisibitlyProperty, value);
        }
        private static readonly DependencyProperty DebugModuleVisibitlyProperty =
            DependencyProperty.Register(nameof(DebugModuleVisibitly), typeof(Visibility), typeof(WaveformView), new PropertyMetadata(Visibility.Collapsed, OnVisualPropertyChanged));








    }
}
