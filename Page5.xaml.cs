using FSGaryityTool_Win11.Reliance;
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
using ClevoEcControlinfo;
using Windows.UI.Core;
using System.Threading;
using System.ComponentModel;
using Microsoft.UI.Xaml.Media.Animation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FSGaryityTool_Win11
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Page5 : Page
    {
        public System.Threading.Timer tempTimer;
        private System.Threading.Timer cpuDelayTimer;
        private System.Threading.Timer gpuDelayTimer;

        public string CpuTempDisplay => $"{CpuTemp.Value}℃";
        public string GpuTempDisplay => $"{GpuTemp.Value}℃";

        public static int oldCpuDutySet = 500, oldGpuDutySet = 500, cpuDutySet = 166, gpuDutySet = 166;

        public Page5()
        {
            this.InitializeComponent();

            Clevoinfo_Click(null, null);

            bool isConnect = ClevoEcControl.IsServerStarted();
            if (isConnect)
            {
                bool info = ClevoEcControl.InitIo();
                int fan_id = 1;
                ClevoEcControl.ECData data = ClevoEcControl.GetTempFanDuty(fan_id);
                CPUFanRadialGauge.Value = (int)Math.Round((data.FanDuty / 255.0) * 100);
                fan_id = 2;
                data = ClevoEcControl.GetTempFanDuty(fan_id);
                GPUFanRadialGauge.Value = (int)Math.Round((data.FanDuty / 255.0) * 100);
                CpuTempText.Text = "50℃";
                GpuTempText.Text = "50℃";
            }
            else
            {
                CPUFanRadialGauge.Value = 60;
                GPUFanRadialGauge.Value = 60;
                CpuTempText.Text = "N/A℃";
                GpuTempText.Text = "N/A℃";
            }
            CPUFanRpmRadialGauge.Value = 500;
            GPUFanRpmRadialGauge.Value = 500;

            tempTimer = new System.Threading.Timer(TempTimerTick, null, 0, 1000);
            CPUFanRadialGauge.ValueChanged += CPUFanRadialGauge_ValueChanged;
            GPUFanRadialGauge.ValueChanged += GPUFanRadialGauge_ValueChanged;
            // 初始化定时器，但不启动
            cpuDelayTimer = new System.Threading.Timer(CpuOnTimer, null, Timeout.Infinite, Timeout.Infinite);
            gpuDelayTimer = new System.Threading.Timer(GpuOnTimer, null, Timeout.Infinite, Timeout.Infinite);
        }
        public void TempTimerTick(Object stateInfo)
        {
            bool isConnect = ClevoEcControl.IsServerStarted();
            //Debug.WriteLine($"isConnect: " + isConnect.ToString());

            if (isConnect)
            {
                int val = ClevoEcControl.GetCpuFanRpm();
                int cpuFanRpm = 500;
                if (val == 0)
                {
                    cpuFanRpm = 0;
                }
                else if (val > 300 && val < 5000)
                {
                    cpuFanRpm = 2100000 / val;
                }

                val = ClevoEcControl.GetGpuFanRpm();
                int gpuFanRpm = 500;
                if (val == 0)
                {
                    gpuFanRpm = 0;
                }
                else if (val > 300 && val < 5000)
                {
                    gpuFanRpm = 2100000 / val;
                }

                DispatcherQueue.TryEnqueue(() =>
                {
                    cpuDutySet = (int)Math.Round((CPUFanRadialGauge.Value / 100.0) * 255);
                    gpuDutySet = (int)Math.Round((GPUFanRadialGauge.Value / 100.0) * 255);

                    CPUFanRpmRadialGauge.Value = cpuFanRpm;
                    GPUFanRpmRadialGauge.Value = gpuFanRpm;
                });

                int fan_id = 1;
                ClevoEcControl.ECData data = ClevoEcControl.GetTempFanDuty(fan_id);
                int cpuTemp = data.Remote;
                DispatcherQueue.TryEnqueue(() =>
                {
                    CpuTemp.Value = cpuTemp;
                    CpuTempText.Text = cpuTemp.ToString() + "℃";
                });
                if (oldCpuDutySet != cpuDutySet)
                {
                    ClevoEcControl.SetFanDuty(fan_id, cpuDutySet);
                }

                fan_id = 2;
                data = ClevoEcControl.GetTempFanDuty(fan_id);
                int gpuTemp = data.Remote;
                DispatcherQueue.TryEnqueue(() =>
                {
                    GpuTemp.Value = gpuTemp;
                    GpuTempText.Text = gpuTemp.ToString() + "℃";
                });
                if (oldGpuDutySet != gpuDutySet)
                {
                    ClevoEcControl.SetFanDuty(fan_id, gpuDutySet);
                }
                oldCpuDutySet = cpuDutySet;
                oldGpuDutySet = gpuDutySet;
            }
            else
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    CpuTempText.Text = "N/A℃";
                    GpuTempText.Text = "N/A℃";
                });
            }

        }
        private void ClevoGetFaninfo_Click(object sender, RoutedEventArgs e)
        {
            bool info = ClevoEcControl.InitIo();
            Debug.WriteLine($"info: " + info.ToString());
            string ecVersion = ClevoEcControl.GetECVersion();
            Debug.WriteLine($"ECVersion: " + ecVersion);
            int fanNum = ClevoEcControl.GetFanCount();
            Debug.WriteLine($"info: " + fanNum.ToString());

            int fan_id = 1;
            int cpuDuty = (int)Math.Round((CPUFanRadialGauge.Value / 100.0) * 255);
            ClevoEcControl.ECData data = ClevoEcControl.GetTempFanDuty(fan_id);
            // 使用Debug.WriteLine打印字段
            Debug.WriteLine($"Remote: {data.Remote}");
            //Debug.WriteLine($"Local: {data.Local}");
            Debug.WriteLine($"FanDuty: {data.FanDuty}");
            //Debug.WriteLine($"FanRpm: {data.Reserve}");
            ClevoEcControl.SetFanDuty(fan_id, cpuDuty);
            //CPUFanRadialGauge.Value = (int)Math.Round((duty / 255.0) * 100);
            int cpuFanRpm = 2100000 / ClevoEcControl.GetCpuFanRpm();
            Debug.WriteLine($"FanRpm:" + cpuFanRpm.ToString());

            fan_id = 2;
            int gpuDuty = (int)Math.Round((GPUFanRadialGauge.Value / 100.0) * 255);
            data = ClevoEcControlinfo.ClevoEcControl.GetTempFanDuty(fan_id);
            // 使用Debug.WriteLine打印字段
            Debug.WriteLine($"Remote: {data.Remote}");
            //Debug.WriteLine($"Local: {data.Local}");
            Debug.WriteLine($"FanDuty: {data.FanDuty}");
            //Debug.WriteLine($"Reserve: {data.Reserve}");
            ClevoEcControl.SetFanDuty(fan_id, gpuDuty);
            //GPUFanRadialGauge.Value = (int)Math.Round((duty / 255.0) * 100);
            int gpuFanRpm = 2100000 / ClevoEcControl.GetGpuFanRpm();
            Debug.WriteLine($"FanRpm:" + gpuFanRpm.ToString());
        }

        private void Clevoinfo_Click(object sender, RoutedEventArgs e)
        {
            bool isConnect =  ClevoEcControl.IsServerStarted();
            Debug.WriteLine($"isConnect: " + isConnect.ToString());

            if (isConnect)
            {
                ClevoGetFaninfo.IsEnabled = true;
            }
            else
            {
                ClevoGetFaninfo.IsEnabled= false;
            }
        }
        private void CPUFanRadialGauge_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            // 检查是否是Value属性发生了变化
            cpuDelayTimer.Change(200, Timeout.Infinite);
        }
        private void GPUFanRadialGauge_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            // 检查是否是Value属性发生了变化
            gpuDelayTimer.Change(200, Timeout.Infinite);
        }
        private void CpuOnTimer(object state)
        {
            int cpuFanId = 1;
            // 从RadialGauge获取数据
            DispatcherQueue.TryEnqueue(() =>
            {
                cpuDutySet = (int)Math.Round((CPUFanRadialGauge.Value / 100.0) * 255);
            });
            bool isConnect = ClevoEcControl.IsServerStarted();
            if (isConnect)
            {
                
                ClevoEcControl.SetFanDuty(cpuFanId, cpuDutySet);
                oldCpuDutySet = cpuDutySet;
            }
        }
        private void GpuOnTimer(object state)
        {
            int gpuFanId = 2;
            // 从RadialGauge获取数据
            DispatcherQueue.TryEnqueue(() =>
            {
                gpuDutySet = (int)Math.Round((GPUFanRadialGauge.Value / 100.0) * 255);
            });
            bool isConnect = ClevoEcControl.IsServerStarted();
            if (isConnect)
            {
                
                ClevoEcControl.SetFanDuty(gpuFanId, gpuDutySet);
                oldGpuDutySet = gpuDutySet;
            }
        }
    }
}
