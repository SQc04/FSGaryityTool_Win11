using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;

namespace FSGaryityTool_Win11.Controls;

public class LogarithmicSlider : Slider
{
    private ToolTip customToolTip = new();

    public LogarithmicSlider()
    {
        ValueChanged += LogarithmicSlider_ValueChanged;
        PointerPressed += LogarithmicSlider_PointerPressed;
        PointerReleased += LogarithmicSlider_PointerReleased;
        PointerMoved += LogarithmicSlider_PointerMoved;
    }

    public static readonly DependencyProperty UseLogarithmicTicksProperty =
        DependencyProperty.Register(
            nameof(UseLogarithmicTicks),
            typeof(bool),
            typeof(LogarithmicSlider),
            new(false, OnUseLogarithmicTicksChanged));

    public bool UseLogarithmicTicks
    {
        get => (bool)GetValue(UseLogarithmicTicksProperty);
        set => SetValue(UseLogarithmicTicksProperty, value);
    }

    private static void OnUseLogarithmicTicksChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var slider = d as LogarithmicSlider;
        slider?.UpdateTicks();
    }

    private void LogarithmicSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
        var linearValue = e.NewValue;
        var minValue = Minimum > 0 ? Math.Log10(Minimum) : 0;
        var maxValue = Math.Log10(Maximum);
        var nonLinearValue = Math.Pow(10, linearValue * (maxValue - minValue) / 100 + minValue);

        // 更新自定义ToolTip的内容
        if (customToolTip is not null)
        {
            customToolTip.Content = nonLinearValue.ToString("F2");
        }
    }

    private void LogarithmicSlider_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (customToolTip is not null)
        {
            //customToolTip.IsOpen = true;
        }
    }

    private void LogarithmicSlider_PointerReleased(object sender, PointerRoutedEventArgs e)
    {
        if (customToolTip is not null)
        {
            //customToolTip.IsOpen = false;
        }
    }

    private void LogarithmicSlider_PointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if (customToolTip is not null && customToolTip.IsOpen)
        {
            //customToolTip.IsOpen = true;
            var position = e.GetCurrentPoint(this).Position;
            customToolTip.HorizontalOffset = position.X;
            customToolTip.VerticalOffset = position.Y - 30; // 调整ToolTip的位置
        }
    }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        UpdateTicks();

        ToolTipService.SetToolTip(this, customToolTip);
    }

    private void UpdateTicks()
    {
        if (UseLogarithmicTicks)
        {
            var minValue = Minimum > 0 ? Math.Log10(Minimum) : 0;
            var maxValue = Math.Log10(Maximum);
            var tickFrequency = (maxValue - minValue) / 5;

            for (var i = minValue; i <= maxValue; i += tickFrequency)
            {
                var tickValue = Math.Pow(10, i);
                // 在这里添加刻度线的逻辑
            }
        }
        else
        {
            // 使用线性刻度的逻辑
        }
    }
}
