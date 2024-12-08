using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSGaryityTool_Win11.Controls
{
    public class WidthStateTrigger : StateTriggerBase
    {
        public static readonly DependencyProperty MinWidthProperty = DependencyProperty.Register(
            nameof(MinWidth), typeof(double), typeof(WidthStateTrigger), new PropertyMetadata(0.0, OnMinWidthPropertyChanged));

        public double MinWidth
        {
            get => (double)GetValue(MinWidthProperty);
            set => SetValue(MinWidthProperty, value);
        }

        private static void OnMinWidthPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var trigger = (WidthStateTrigger)d;
            var minWidth = (double)e.NewValue;
            trigger.UpdateTrigger(trigger.CurrentWidth);
        }

        private double CurrentWidth { get; set; }

        public void UpdateTrigger(double width)
        {
            CurrentWidth = width;
            SetActive(CurrentWidth >= MinWidth);
        }
    }

}
