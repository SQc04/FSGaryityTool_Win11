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

using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.Graphics.Canvas.Geometry;
using System.Numerics;

using Microsoft.UI;
using Windows.UI;



// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FSGaryityTool_Win11.Views.Pages.FanControlPage
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Page5 : Page
    {
        public System.Threading.Timer tempTimer;
        public Timer ServerRunCheckTimer;
        private System.Threading.Timer cpuDelayTimer;
        private System.Threading.Timer gpuDelayTimer;

        public string CpuTempDisplay => $"{CpuTemp.Value}��";
        public string GpuTempDisplay => $"{GpuTemp.Value}��";

        public static int cpuFanDuty, gpuFanDuty, cpuDutySet = 166, gpuDutySet = 166;

        public Page5()
        {
            this.InitializeComponent();

            try
            {
                bool isConnect = ClevoEcControl.IsServerStarted();
                if (isConnect)
                {
                    bool isInitialize = ClevoEcControl.InitIo();
                    Debug.WriteLine($"isInitialize: " + isInitialize.ToString());
                    if (isInitialize)
                    {
                        int fanNum = ClevoEcControl.GetFanCount();
                        Debug.WriteLine($"info: " + fanNum.ToString());

                        int fan_id = 1;
                        ClevoEcControl.ECData data = ClevoEcControl.GetTempFanDuty(fan_id);
                        CPUFanRadialGauge.Value = (int)Math.Round((data.FanDuty / 255.0) * 100);
                        fan_id = 2;
                        data = ClevoEcControl.GetTempFanDuty(fan_id);
                        GPUFanRadialGauge.Value = (int)Math.Round((data.FanDuty / 255.0) * 100);
                    }
                    else
                    {
                        CPUFanRadialGauge.Value = 60;
                        GPUFanRadialGauge.Value = 60;
                        CpuTempText.Text = "N/A��";
                        GpuTempText.Text = "N/A��";
                        CPUFanRpmRadialGauge.Value = 0;
                        GPUFanRpmRadialGauge.Value = 0;
                    }
                }
                else
                {
                    CPUFanRadialGauge.Value = 60;
                    GPUFanRadialGauge.Value = 60;
                    CpuTempText.Text = "N/A��";
                    GpuTempText.Text = "N/A��";
                    CPUFanRpmRadialGauge.Value = 0;
                    GPUFanRpmRadialGauge.Value = 0;
                }
            }
            catch
            {
                CPUFanRadialGauge.Value = 60;
                GPUFanRadialGauge.Value = 60;
                CpuTempText.Text = "N/A��";
                GpuTempText.Text = "N/A��";
                CPUFanRpmRadialGauge.Value = 0;
                GPUFanRpmRadialGauge.Value = 0;
            }
            
            

            if (Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length > 1)
            {
                Debug.WriteLine("An instance of this application is already running.");
                return;
            }
            else
            {
                CPUFanRadialGauge.ValueChanged += CPUFanRadialGauge_ValueChanged;
                GPUFanRadialGauge.ValueChanged += GPUFanRadialGauge_ValueChanged;
                // ��ʼ����ʱ������������
                cpuDelayTimer = new System.Threading.Timer(CpuOnTimer, null, Timeout.Infinite, Timeout.Infinite);
                gpuDelayTimer = new System.Threading.Timer(GpuOnTimer, null, Timeout.Infinite, Timeout.Infinite);

                Clevoinfo_Click(null, null);

                ServerRunCheckTimer = new Timer(ServerRunCheckTimeTick, null, 0, 5000);
            }

        }

        public bool serverRunInfo = false;
        public bool serverRun = false;
        public int serverTimeout = 0;
        public int serverTimeoutInfo = 0;
        public void ServerRunCheckTimeTick(Object stateInfo)
        {
            try
            {
                bool isConnect = ClevoEcControl.IsServerStarted();
                if (isConnect)
                {
                    if (!serverRunInfo)
                    {
                        bool isInitialize = ClevoEcControl.InitIo();
                        Debug.WriteLine($"ServerRunInitialize: " + isInitialize.ToString());

                        if (isInitialize)
                        {
                            Thread.Sleep(500);
                            tempTimer = new System.Threading.Timer(TempTimerTick, null, 0, 1000);
                            serverRun = true;
                        }
                    }
                    serverRunInfo = true;
                }
                else
                {
                    HandleServerDisconnection();
                }
                Debug.WriteLine($"ServerRun: " + isConnect.ToString());

                DispatcherQueue.TryEnqueue(() =>
                {
                    ServerTimeoutInfoTextBlock.Text = "  serverTimeoutInfo: " + serverTimeoutInfo.ToString();
                });
            }
            catch
            {
                serverRunInfo = false;
            }
        }

        private void HandleServerDisconnection()
        {
            if (serverRun)
            {
                tempTimer.Dispose();
                serverRun = false;
            }
            serverRunInfo = false;

            Process[] processes = Process.GetProcessesByName("ClevoEcControl");
            if (processes.Length == 0)
            {
                // ��� ClevoEcControl.exe û�����У���ô������
                //Process.Start("ClevoEcControl.exe");
            }
            else
            {
                HandleServerTimeout(processes);
            }
        }

        private void HandleServerTimeout(Process[] processes)
        {
            serverTimeout++;
            serverTimeoutInfo++;
            if (serverTimeout > 3)
            {
                foreach (var process in processes)
                {
                    process.Kill();
                }
                serverTimeout = 0;
            }
        }

        public void TempTimerTick(Object stateInfo)
        {
            bool isConnect = ClevoEcControl.IsServerStarted();
            //Debug.WriteLine($"isConnect: " + isConnect.ToString());

            if (isConnect)
            {
                int val = ClevoEcControl.GetCpuFanRpm();
                int cpuFanRpm = 0;
                if (val == 0)
                {
                    cpuFanRpm = 0;
                }
                else if (val > 300 && val < 5000)
                {
                    cpuFanRpm = 2100000 / val;
                }

                val = ClevoEcControl.GetGpuFanRpm();
                int gpuFanRpm = 0;
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
                cpuFanDuty = data.FanDuty;
                DispatcherQueue.TryEnqueue(() =>
                {
                    CpuTemp.Value = cpuTemp;
                    CpuTempText.Text = cpuTemp.ToString() + "��";
                });
                if (cpuFanDuty != cpuDutySet)
                {
                    ClevoEcControl.SetFanDuty(fan_id, cpuDutySet);
                }

                fan_id = 2;
                data = ClevoEcControl.GetTempFanDuty(fan_id);
                int gpuTemp = data.Remote;
                gpuFanDuty = data.FanDuty;
                DispatcherQueue.TryEnqueue(() =>
                {
                    GpuTemp.Value = gpuTemp;
                    GpuTempText.Text = gpuTemp.ToString() + "��";
                });
                if (gpuFanDuty != gpuDutySet)
                {
                    ClevoEcControl.SetFanDuty(fan_id, gpuDutySet);
                }
                cpuFanDuty = cpuDutySet;
                gpuFanDuty = gpuDutySet;
            }
            else
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    CpuTempText.Text = "N/A��";
                    GpuTempText.Text = "N/A��";
                    CPUFanRpmRadialGauge.Value = 0;
                    GPUFanRpmRadialGauge.Value = 0;
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
            // ʹ��Debug.WriteLine��ӡ�ֶ�
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
            // ʹ��Debug.WriteLine��ӡ�ֶ�
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

                bool isInitialize = ClevoEcControl.InitIo();
                Debug.WriteLine($"isInitialize: " + isInitialize.ToString());
            }
            else
            {
                ClevoGetFaninfo.IsEnabled= false;
            }
        }
        private void CPUFanRadialGauge_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            // ����Ƿ���Value���Է����˱仯
            cpuDelayTimer.Change(200, Timeout.Infinite);
        }

        private void WatchDogStart_Click(object sender, RoutedEventArgs e)
        {
            bool isStart = ClevoEcControl.IsWatchDogStarted();
            Debug.WriteLine($"isStart: " + isStart.ToString());
            if (!isStart)
            {
                ClevoEcControl.SetWatchDogStarted();
            }
            Debug.WriteLine("WatchDogserver is Start");
        }

        private void WatchDogClose_Click(object sender, RoutedEventArgs e)
        {
            bool isStart = ClevoEcControl.IsWatchDogStarted();
            Debug.WriteLine($"isStart: " + isStart.ToString());
            if (isStart)
            {
                ClevoEcControl.SetWatchDogClosed();
            }
            Debug.WriteLine("WatchDogserver is Close");
        }

        private void GPUFanRadialGauge_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            // ����Ƿ���Value���Է����˱仯
            gpuDelayTimer.Change(200, Timeout.Infinite);
        }
        private void CpuOnTimer(object state)
        {
            int cpuFanId = 1;
            // ��RadialGauge��ȡ����
            DispatcherQueue.TryEnqueue(() =>
            {
                cpuDutySet = (int)Math.Round((CPUFanRadialGauge.Value / 100.0) * 255);
            });
            bool isConnect = ClevoEcControl.IsServerStarted();
            if (isConnect)
            {
                if (cpuFanDuty != cpuDutySet)
                {
                    ClevoEcControl.SetFanDuty(cpuFanId, cpuDutySet);
                }
                cpuFanDuty = cpuDutySet;
            }
        }
        private void GpuOnTimer(object state)
        {
            int gpuFanId = 2;
            // ��RadialGauge��ȡ����
            DispatcherQueue.TryEnqueue(() =>
            {
                gpuDutySet = (int)Math.Round((GPUFanRadialGauge.Value / 100.0) * 255);
            });
            bool isConnect = ClevoEcControl.IsServerStarted();
            if (isConnect)
            {
                if (cpuFanDuty != gpuDutySet)
                {
                    ClevoEcControl.SetFanDuty(gpuFanId, gpuDutySet);
                }
            }
        }
        private void OnDraw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            Vector2 point1 = new Vector2(0, 200); // ���
            Vector2 controlPoint1 = new Vector2(100, 100); // ���Ƶ�1
            Vector2 controlPoint2 = new Vector2(200, 150); // ���Ƶ�2
            Vector2 point2 = new Vector2(300, 0); // �յ�

            var pathBuilder = new CanvasPathBuilder(sender);
            pathBuilder.BeginFigure(point1);
            pathBuilder.AddCubicBezier(controlPoint1, controlPoint2, point2);
            pathBuilder.EndFigure(CanvasFigureLoop.Open);

            var path = CanvasGeometry.CreatePath(pathBuilder);

            Color color;
            if (Application.Current.RequestedTheme == ApplicationTheme.Dark)
            {
                color = (Color)Application.Current.Resources["SystemAccentColorLight2"];
            }
            else
            {
                color = (Color)Application.Current.Resources["SystemAccentColorDark1"];
            }

            args.DrawingSession.DrawGeometry(path, color, 3);
        }

        /**/
    }
}
