using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FSGaryityTool_Win11.Controls
{
    public class LogarithmicSlider : Slider
    {
        private ToolTip customToolTip = new ToolTip();

        public LogarithmicSlider()
        {
            this.ValueChanged += LogarithmicSlider_ValueChanged;
            this.PointerPressed += LogarithmicSlider_PointerPressed;
            this.PointerReleased += LogarithmicSlider_PointerReleased;
            this.PointerMoved += LogarithmicSlider_PointerMoved;
        }

        public static readonly DependencyProperty UseLogarithmicTicksProperty =
            DependencyProperty.Register(
                "UseLogarithmicTicks",
                typeof(bool),
                typeof(LogarithmicSlider),
                new PropertyMetadata(false, OnUseLogarithmicTicksChanged));

        public bool UseLogarithmicTicks
        {
            get { return (bool)GetValue(UseLogarithmicTicksProperty); }
            set { SetValue(UseLogarithmicTicksProperty, value); }
        }

        private static void OnUseLogarithmicTicksChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var slider = d as LogarithmicSlider;
            slider?.UpdateTicks();
        }

        private void LogarithmicSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            double linearValue = e.NewValue;
            double minValue = this.Minimum > 0 ? Math.Log10(this.Minimum) : 0;
            double maxValue = Math.Log10(this.Maximum);
            double nonLinearValue = Math.Pow(10, linearValue * (maxValue - minValue) / 100 + minValue);

            // 更新自定义ToolTip的内容
            if (customToolTip != null)
            {
                customToolTip.Content = nonLinearValue.ToString("F2");
            }
        }

        private void LogarithmicSlider_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (customToolTip != null)
            {
                //customToolTip.IsOpen = true;
            }
        }

        private void LogarithmicSlider_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (customToolTip != null)
            {
                //customToolTip.IsOpen = false;
            }
        }

        private void LogarithmicSlider_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (customToolTip != null && customToolTip.IsOpen)
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
                double minValue = this.Minimum > 0 ? Math.Log10(this.Minimum) : 0;
                double maxValue = Math.Log10(this.Maximum);
                double tickFrequency = (maxValue - minValue) / 5;

                for (double i = minValue; i <= maxValue; i += tickFrequency)
                {
                    double tickValue = Math.Pow(10, i);
                    // 在这里添加刻度线的逻辑
                }
            }
            else
            {
                // 使用线性刻度的逻辑
            }
        }
    }
}
