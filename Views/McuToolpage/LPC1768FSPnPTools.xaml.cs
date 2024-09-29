using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FSGaryityTool_Win11.Views.McuToolpage
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LPC1768FSPnPTools : Page
    {
        public LPC1768FSPnPTools()
        {
            this.InitializeComponent();
            CustomSlider.ValueChanged += CustomSlider_ValueChanged;
        }
        private void CustomSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {

        }

        public double xMinNum = 0;
        public double xMaxNum = 300;
        public double yMinNum = 0;
        public double yMaxNum = 300;
        public double zMinNum = 0;
        public double zMaxNum = 30;

        private void XRangeSelectorMiniumNumberBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (!double.IsNaN(XRangeSelectorMiniumNumberBox.Value))
            {
                xMinNum = XRangeSelectorMiniumNumberBox.Value;
            }
            else
            {
                XRangeSelectorMiniumNumberBox.Value = xMinNum;
            }
            if (XRangeSelector != null) XRangeSelector.Minimum = xMinNum;
        }

        private void XRangeSelectorMaxiumNumberBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (!double.IsNaN(XRangeSelectorMaxiumNumberBox.Value))
            {
                xMaxNum = XRangeSelectorMaxiumNumberBox.Value;
            }
            else
            {
                XRangeSelectorMaxiumNumberBox.Value = xMaxNum;
            }
            if (XRangeSelector != null) XRangeSelector.Maximum = xMaxNum;
        }

        private void YRangeSelectorMiniumNumberBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {

        }

        private void YRangeSelectorMaxiumNumberBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {

        }

        private void ZRangeSelectorMiniumNumberBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {

        }

        private void ZRangeSelectorMaxiumNumberBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {

        }
    }
}
