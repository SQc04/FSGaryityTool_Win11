using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FSGaryityTool_Win11.Controls;

public sealed partial class ScrollwaveformView : UserControl
{
    public static readonly DependencyProperty RowTickCountProperty =
        DependencyProperty.Register(nameof(RowTickCount), typeof(string), typeof(ScrollwaveformView), new PropertyMetadata("8", OnVisualPropertyChanged));

    public string RowTickCount
    {
        get => (string)GetValue(RowTickCountProperty);
        set => SetValue(RowTickCountProperty, value);
    }

    public static readonly DependencyProperty ColumnTickCountProperty =
        DependencyProperty.Register(nameof(ColumnTickCount), typeof(string), typeof(ScrollwaveformView), new PropertyMetadata("8", OnVisualPropertyChanged));

    public string ColumnTickCount
    {
        get => (string)GetValue(ColumnTickCountProperty);
        set => SetValue(ColumnTickCountProperty, value);
    }


    public static readonly DependencyProperty MinRowTickCountProperty =
            DependencyProperty.Register(nameof(MinRowTickCount), typeof(int), typeof(ScrollwaveformView), new PropertyMetadata(0, OnVisualPropertyChanged));

    public int MinRowTickCount
    {
        get => (int)GetValue(MinRowTickCountProperty);
        set => SetValue(MinRowTickCountProperty, value);
    }

    public static readonly DependencyProperty MinColumnTickCountProperty =
        DependencyProperty.Register(nameof(MinColumnTickCount), typeof(int), typeof(ScrollwaveformView), new PropertyMetadata(0, OnVisualPropertyChanged));

    public int MinColumnTickCount
    {
        get => (int)GetValue(MinColumnTickCountProperty);
        set => SetValue(MinColumnTickCountProperty, value);
    }

    public static readonly DependencyProperty MaxRowTickCountProperty =
        DependencyProperty.Register(nameof(MaxRowTickCount), typeof(int), typeof(ScrollwaveformView), new PropertyMetadata(16, OnVisualPropertyChanged));

    public int MaxRowTickCount
    {
        get => (int)GetValue(MaxRowTickCountProperty);
        set => SetValue(MaxRowTickCountProperty, value);
    }

    public static readonly DependencyProperty MaxColumnTickCountProperty =
        DependencyProperty.Register(nameof(MaxColumnTickCount), typeof(int), typeof(ScrollwaveformView), new PropertyMetadata(16, OnVisualPropertyChanged));

    public int MaxColumnTickCount
    {
        get => (int)GetValue(MaxColumnTickCountProperty);
        set => SetValue(MaxColumnTickCountProperty, value);
    }

    
    public static readonly DependencyProperty WaveGridBorderMarginProperty =
        DependencyProperty.Register(nameof(WaveGridBorderMargin), typeof(Thickness), typeof(ScrollwaveformView), new PropertyMetadata(default(Thickness), OnVisualPropertyChanged));

    public Thickness WaveGridBorderMargin
    {
        get => (Thickness)GetValue(WaveGridBorderMarginProperty);
        set => SetValue(WaveGridBorderMarginProperty, value);
    }

    public static readonly DependencyProperty ControlBorderMarginProperty =
        DependencyProperty.Register(nameof(ControlBorderMargin), typeof(Thickness), typeof(ScrollwaveformView), new PropertyMetadata(new Thickness(3), OnVisualPropertyChanged));

    public Thickness ControlBorderMargin
    {
        get => (Thickness)GetValue(ControlBorderMarginProperty);
        set => SetValue(ControlBorderMarginProperty, value);
    }


    public static readonly DependencyProperty WaveGridBorderThicknessProperty =
    DependencyProperty.Register(nameof(WaveGridBorderThickness), typeof(Thickness), typeof(ScrollwaveformView), new PropertyMetadata(new Thickness(2), OnVisualPropertyChanged));

    public Thickness WaveGridBorderThickness
    {
        get => (Thickness)GetValue(WaveGridBorderThicknessProperty);
        set => SetValue(WaveGridBorderThicknessProperty, value);
    }

    public static readonly DependencyProperty RowTickLineWidthProperty =
        DependencyProperty.Register(nameof(RowTickLineWidth), typeof(double), typeof(ScrollwaveformView), new PropertyMetadata(1.0, OnVisualPropertyChanged));

    public double RowTickLineWidth
    {
        get => (double)GetValue(RowTickLineWidthProperty);
        set => SetValue(RowTickLineWidthProperty, value);
    }

    public static readonly DependencyProperty ColumnTickLineWidthProperty =
        DependencyProperty.Register(nameof(ColumnTickLineWidth), typeof(double), typeof(ScrollwaveformView), new PropertyMetadata(1.0, OnVisualPropertyChanged));

    public double ColumnTickLineWidth
    {
        get => (double)GetValue(ColumnTickLineWidthProperty);
        set => SetValue(ColumnTickLineWidthProperty, value);
    }

    public static readonly DependencyProperty WaveGridBorderBrushProperty =
        DependencyProperty.Register(nameof(WaveGridBorderBrush), typeof(Brush), typeof(ScrollwaveformView),
            new PropertyMetadata(null, OnVisualPropertyChanged));
    public Brush WaveGridBorderBrush
    {
        get => (Brush)GetValue(WaveGridBorderBrushProperty);
        set => SetValue(WaveGridBorderBrushProperty, value);
    }

    public static readonly DependencyProperty RowTickLineBrushProperty =
        DependencyProperty.Register(nameof(RowTickLineBrush), typeof(Brush), typeof(ScrollwaveformView),
            new PropertyMetadata(null, OnVisualPropertyChanged));
    public Brush RowTickLineBrush
    {
        get => (Brush)GetValue(RowTickLineBrushProperty);
        set => SetValue(RowTickLineBrushProperty, value);
    }

    public static readonly DependencyProperty ColumnTickLineBrushProperty =
        DependencyProperty.Register(nameof(ColumnTickLineBrush), typeof(Brush), typeof(ScrollwaveformView),
            new PropertyMetadata(null, OnVisualPropertyChanged));
    public Brush ColumnTickLineBrush
    {
        get => (Brush)GetValue(ColumnTickLineBrushProperty);
        set => SetValue(ColumnTickLineBrushProperty, value);
    }

    public static readonly DependencyProperty RowCenterTickLineBrushProperty =
        DependencyProperty.Register(nameof(RowCenterTickLineBrush), typeof(Brush), typeof(ScrollwaveformView),
            new PropertyMetadata(null, OnVisualPropertyChanged));
    public Brush RowCenterTickLineBrush
    {
        get => (Brush)GetValue(RowCenterTickLineBrushProperty);
        set => SetValue(RowCenterTickLineBrushProperty, value);
    }

    public static readonly DependencyProperty ColumnCenterTickLineBrushProperty =
        DependencyProperty.Register(nameof(ColumnCenterTickLineBrush), typeof(Brush), typeof(ScrollwaveformView),
            new PropertyMetadata(null, OnVisualPropertyChanged));
    public Brush ColumnCenterTickLineBrush
    {
        get => (Brush)GetValue(ColumnCenterTickLineBrushProperty);
        set => SetValue(ColumnCenterTickLineBrushProperty, value);
    }

    public static readonly DependencyProperty WaveGridBorderLineStyleProperty =
        DependencyProperty.Register(nameof(WaveGridBorderLineStyle), typeof(LineStyle), typeof(ScrollwaveformView), new PropertyMetadata(LineStyle.Solid, OnVisualPropertyChanged));

    public LineStyle WaveGridBorderLineStyle
    {
        get => (LineStyle)GetValue(WaveGridBorderLineStyleProperty);
        set => SetValue(WaveGridBorderLineStyleProperty, value);
    }

    public static readonly DependencyProperty GridTickLineStyleProperty =
        DependencyProperty.Register(nameof(GridTickLineStyle), typeof(LineStyle), typeof(ScrollwaveformView), new PropertyMetadata(LineStyle.Solid, OnVisualPropertyChanged));

    public LineStyle GridTickLineStyle
    {
        get => (LineStyle)GetValue(GridTickLineStyleProperty);
        set => SetValue(GridTickLineStyleProperty, value);
    }

    public static readonly DependencyProperty GridTickModeStyleProperty =
        DependencyProperty.Register(nameof(GridTickMode), typeof(TickModeStyle), typeof(ScrollwaveformView), new PropertyMetadata(TickModeStyle.None, OnVisualPropertyChanged));
    public TickModeStyle GridTickMode
    {
        get => (TickModeStyle)GetValue(GridTickModeStyleProperty);
        set => SetValue(GridTickModeStyleProperty, value);
    }
    public static readonly DependencyProperty WaveGridBorderTickThicknessProperty =
        DependencyProperty.Register(nameof(WaveGridBorderTickThickness), typeof(Thickness), typeof(ScrollwaveformView), new PropertyMetadata(new Thickness(4), OnVisualPropertyChanged));

    public Thickness WaveGridBorderTickThickness
    {
        get => (Thickness)GetValue(WaveGridBorderTickThicknessProperty);
        set => SetValue(WaveGridBorderTickThicknessProperty, value);
    }

    public static readonly DependencyProperty WaveGridBorderSubTickThicknessProperty =
        DependencyProperty.Register(nameof(WaveGridBorderSubTickThickness), typeof(Thickness), typeof(ScrollwaveformView), new PropertyMetadata(new Thickness(2), OnVisualPropertyChanged));
    public Thickness WaveGridBorderSubTickThickness
    {
        get => (Thickness)GetValue(WaveGridBorderSubTickThicknessProperty);
        set => SetValue(WaveGridBorderSubTickThicknessProperty, value);
    }
    public static readonly DependencyProperty RowGridBorderSubTickCountProperty =
        DependencyProperty.Register(nameof(RowGridBorderSubTickCount), typeof(int), typeof(ScrollwaveformView), new PropertyMetadata(0, OnVisualPropertyChanged));
    public int RowGridBorderSubTickCount
    {
        get => (int)GetValue(RowGridBorderSubTickCountProperty);
        set => SetValue(RowGridBorderSubTickCountProperty, value);
    }
    public static readonly DependencyProperty ColumnGridBorderSubTickCountProperty =
        DependencyProperty.Register(nameof(ColumnGridBorderSubTickCount), typeof(int), typeof(ScrollwaveformView), new PropertyMetadata(0, OnVisualPropertyChanged));
    public int ColumnGridBorderSubTickCount
    {
        get => (int)GetValue(ColumnGridBorderSubTickCountProperty);
        set => SetValue(ColumnGridBorderSubTickCountProperty, value);
    }

    public static readonly DependencyProperty CrossSizeProperty =
        DependencyProperty.Register(nameof(CrossSize), typeof(double), typeof(ScrollwaveformView), new PropertyMetadata(6.0, OnVisualPropertyChanged));

    public double CrossSize
    {
        get => (double)GetValue(CrossSizeProperty);
        set => SetValue(CrossSizeProperty, value);
    }

    public static readonly DependencyProperty WaveGridBorderTickLineStyleProperty =
        DependencyProperty.Register(nameof(WaveGridBorderTickLineStyle), typeof(LineStyle), typeof(ScrollwaveformView), new PropertyMetadata(LineStyle.Solid, OnVisualPropertyChanged));

    public LineStyle WaveGridBorderTickLineStyle
    {
        get => (LineStyle)GetValue(WaveGridBorderTickLineStyleProperty);
        set => SetValue(WaveGridBorderTickLineStyleProperty, value);
    }

    public event PropertyChangedEventHandler PropertyChanged;

    private static void OnVisualPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ScrollwaveformView view)
        {
            view.InvalidateVisual();
        }
    }
    private void InvalidateVisual()
    {
        LineDemonstratorCanvasControl?.Invalidate();
        WaveformDemonstratorCanvasControl?.Invalidate();
    }

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    private int GetActualRowTickCount(double height)
    {
        if (string.Equals(RowTickCount, "Auto", StringComparison.OrdinalIgnoreCase))
        {
            // 自动计算，示例：每40像素一格，最少2格
            return Math.Max(2, (int)(height / 40));
        }
        else if (int.TryParse(RowTickCount, out int val))
        {
            return val; // 不受min/max限制
        }
        else
        {
            return 8; // 默认
        }
    }
    private int GetActualColumnTickCount(double width)
    {
        if (string.Equals(ColumnTickCount, "Auto", StringComparison.OrdinalIgnoreCase))
        {
            return Math.Max(2, (int)(width / 40));
        }
        else if (int.TryParse(ColumnTickCount, out int val))
        {
            return val;
        }
        else
        {
            return 8;
        }
    }

    private void SetDefaultColors()
    {
    }
    private void OnActualThemeChanged(FrameworkElement sender, object args)
    {
        SetDefaultColors();
    }
    public ScrollwaveformView()
    {
        InitializeComponent();

        SetDefaultColors();
        ActualThemeChanged += OnActualThemeChanged;

    }

    private void LineDemonstratorCanvasControl_Draw(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs args)
    {

    }

    private void WaveformDemonstratorCanvasControl_Draw(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs args)
    {

    }
}
