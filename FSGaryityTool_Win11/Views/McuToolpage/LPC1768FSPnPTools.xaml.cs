using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;

namespace FSGaryityTool_Win11.Views.McuToolpage;

public sealed partial class Lpc1768FsPnPTools : Page
{
    public Lpc1768FsPnPTools()
    {
        InitializeComponent();
        CustomSlider.ValueChanged += CustomSlider_ValueChanged;
    }

    private void CustomSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
    }

    public double XMinNum { get; set; }

    public double XMaxNum { get; set; }= 300;

    public double YMinNum { get; set; } = 0;

    public double YMaxNum { get; set; } = 300;

    public double ZMinNum { get; set; } = 0;

    public double ZMaxNum { get; set; } = 30;

    private void XRangeSelectorMinimumNumberBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        if (!double.IsNaN(XRangeSelectorMinimumNumberBox.Value))
        {
            XMinNum = XRangeSelectorMinimumNumberBox.Value;
        }
        else
        {
            XRangeSelectorMinimumNumberBox.Value = XMinNum;
        }
        if (XRangeSelector is not null)
            XRangeSelector.Minimum = XMinNum;
    }

    private void XRangeSelectorMaximumNumberBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        if (!double.IsNaN(XRangeSelectorMaximumNumberBox.Value))
        {
            XMaxNum = XRangeSelectorMaximumNumberBox.Value;
        }
        else
        {
            XRangeSelectorMaximumNumberBox.Value = XMaxNum;
        }
        if (XRangeSelector is not null)
            XRangeSelector.Maximum = XMaxNum;
    }

    private void YRangeSelectorMinimumNumberBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
    }

    private void YRangeSelectorMaximumNumberBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
    }

    private void ZRangeSelectorMinimumNumberBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
    }

    private void ZRangeSelectorMaximumNumberBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
    }
}
