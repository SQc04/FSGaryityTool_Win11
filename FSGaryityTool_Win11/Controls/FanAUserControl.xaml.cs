using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace FSGaryityTool_Win11.Controls;

public sealed partial class FanAUserControl : UserControl
{
    public FanAUserControl()
    {
        InitializeComponent();

        TempText.Text = "N/A℃";
        FanName = "N/A FAN";
    }

    // 公共属性
    public string FanName
    {
        get => (string)GetValue(FanNameProperty);
        set
        {
            SetValue(FanNameProperty, value);
            FanNameTextBlock.Text = value;  // 更新TextBlock的Text属性
        }
    }

    // 使用DependencyProperty作为后备存储字段
    public static readonly DependencyProperty FanNameProperty =
        DependencyProperty.Register(nameof(FanName), typeof(string), typeof(FanAUserControl), new("N/A FAN"));

    public double FanRpmMaximum
    {
        get => (double)GetValue(FanRpmMaximumProperty);
        set
        {
            SetValue(FanRpmMaximumProperty, value);
            FanRpmRadialGauge.Maximum = value;  // 更新RadialGauge的Maximum属性
        }
    }

    // 使用DependencyProperty作为后备存储字段
    public static readonly DependencyProperty FanRpmMaximumProperty =
        DependencyProperty.Register(nameof(FanRpmMaximum), typeof(double), typeof(FanAUserControl), new(8000.0));

    // 公共方法
    public void SetFanRpm(int rpm)
    {
        // 设置风扇转速
        FanRpmRadialGauge.Value = rpm;
    }

    public void SetFanControlPercentage(double percentage)
    {
        // 设置风扇控制百分比
        FanRadialGauge.Value = percentage;
    }

    public void SetTemperature(int? temperature)
    {
        if (temperature.HasValue)
        {
            // 如果获取到了温度，设置温度
            Temp.Value = temperature.Value;
            TempText.Text = temperature.Value + "℃";
        }
        else
        {
            // 如果没有获取到温度，设置为"N/A℃"
            TempText.Text = "N/A℃";
        }
    }
}
