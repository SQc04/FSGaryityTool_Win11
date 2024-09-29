using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FSGaryityTool_Win11.Controls
{
    public sealed partial class FanAUserControl : UserControl
    {
        public FanAUserControl()
        {
            this.InitializeComponent();

            TempText.Text = "N/A℃";
            FanName = "N/A FAN";
        }

        // 公共属性
        public string FanName
        {
            get { return (string)GetValue(FanNameProperty); }
            set
            {
                SetValue(FanNameProperty, value);
                FanNameTextblock.Text = value;  // 更新TextBlock的Text属性
            }
        }

        // 使用DependencyProperty作为后备存储字段
        public static readonly DependencyProperty FanNameProperty =
            DependencyProperty.Register("FanName", typeof(string), typeof(FanAUserControl), new PropertyMetadata("N/A FAN"));

        public double FanRpmMaximum
        {
            get { return (double)GetValue(FanRpmMaximumProperty); }
            set
            {
                SetValue(FanRpmMaximumProperty, value);
                FanRpmRadialGauge.Maximum = value;  // 更新RadialGauge的Maximum属性
            }
        }

        // 使用DependencyProperty作为后备存储字段
        public static readonly DependencyProperty FanRpmMaximumProperty =
            DependencyProperty.Register("FanRpmMaximum", typeof(double), typeof(FanAUserControl), new PropertyMetadata(8000.0));

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
                TempText.Text = temperature.Value.ToString() + "℃";
            }
            else
            {
                // 如果没有获取到温度，设置为"N/A℃"
                TempText.Text = "N/A℃";
            }
        }
    }
}
