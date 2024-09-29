using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using System;
using System.Diagnostics;
using ClevoEcControlinfo;
using System.Threading;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.Graphics.Canvas.Geometry;
using System.Numerics;
using Windows.UI;

namespace FSGaryityTool_Win11.Views.Pages.FanControlPage;

public sealed partial class Page5 : Page
{
    private Timer _cpuDelayTimer;

    private Timer _gpuDelayTimer;

    public Timer TempTimer { get; set; }

    public Timer ServerRunCheckTimer { get; set; }

    public string CpuTempDisplay => $"{CpuTemp.Value}℃";

    public string GpuTempDisplay => $"{GpuTemp.Value}℃";

    public static int CpuFanDuty { get; set; }

    public static int GpuFanDuty{ get; set; }

    public static int CpuDutySet { get; set; } = 166;

    public static int GpuDutySet { get; set; } = 166;

    public Page5()
    {
        InitializeComponent();

        try
        {
            var isConnect = ClevoEcControl.IsServerStarted();
            if (isConnect)
            {
                var isInitialize = ClevoEcControl.InitIo();
                Debug.WriteLine("isInitialize: " + isInitialize);
                if (isInitialize)
                {
                    var fanNum = ClevoEcControl.GetFanCount();
                    Debug.WriteLine("info: " + fanNum);

                    var fanId = 1;
                    var data = ClevoEcControl.GetTempFanDuty(fanId);
                    CpuFanRadialGauge.Value = (int)Math.Round(data.FanDuty / 255.0 * 100);
                    fanId = 2;
                    data = ClevoEcControl.GetTempFanDuty(fanId);
                    GpuFanRadialGauge.Value = (int)Math.Round(data.FanDuty / 255.0 * 100);
                }
                else
                {
                    CpuFanRadialGauge.Value = 60;
                    GpuFanRadialGauge.Value = 60;
                    CpuTempText.Text = "N/A℃";
                    GpuTempText.Text = "N/A℃";
                    CpuFanRpmRadialGauge.Value = 0;
                    GpuFanRpmRadialGauge.Value = 0;
                }
            }
            else
            {
                CpuFanRadialGauge.Value = 60;
                GpuFanRadialGauge.Value = 60;
                CpuTempText.Text = "N/A℃";
                GpuTempText.Text = "N/A℃";
                CpuFanRpmRadialGauge.Value = 0;
                GpuFanRpmRadialGauge.Value = 0;
            }
        }
        catch
        {
            CpuFanRadialGauge.Value = 60;
            GpuFanRadialGauge.Value = 60;
            CpuTempText.Text = "N/A℃";
            GpuTempText.Text = "N/A℃";
            CpuFanRpmRadialGauge.Value = 0;
            GpuFanRpmRadialGauge.Value = 0;
        }

        if (Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length > 1)
        {
            Debug.WriteLine("An instance of this application is already running.");
            return;
        }
        else
        {
            CpuFanRadialGauge.ValueChanged += CPUFanRadialGauge_ValueChanged;
            GpuFanRadialGauge.ValueChanged += GPUFanRadialGauge_ValueChanged;
            // 初始化定时器，但不启动
            _cpuDelayTimer = new(CpuOnTimer, null, Timeout.Infinite, Timeout.Infinite);
            _gpuDelayTimer = new(GpuOnTimer, null, Timeout.Infinite, Timeout.Infinite);

            Clevoinfo_Click(null, null);

            ServerRunCheckTimer = new(ServerRunCheckTimeTick, null, 0, 5000);
        }
    }

    public bool ServerRunInfo { get; set; }

    public bool ServerRun { get; set; }

    public int ServerTimeout { get; set; }

    public int ServerTimeoutInfo { get; set; }

    public void ServerRunCheckTimeTick(object stateInfo)
    {
        try
        {
            var isConnect = ClevoEcControl.IsServerStarted();
            if (isConnect)
            {
                if (!ServerRunInfo)
                {
                    var isInitialize = ClevoEcControl.InitIo();
                    Debug.WriteLine("ServerRunInitialize: " + isInitialize);

                    if (isInitialize)
                    {
                        Thread.Sleep(500);
                        TempTimer = new(TempTimerTick, null, 0, 1000);
                        ServerRun = true;
                    }
                }
                ServerRunInfo = true;
            }
            else
            {
                HandleServerDisconnection();
            }
            Debug.WriteLine("ServerRun: " + isConnect);

            DispatcherQueue.TryEnqueue(() =>
            {
                ServerTimeoutInfoTextBlock.Text = "  serverTimeoutInfo: " + ServerTimeoutInfo;
            });
        }
        catch
        {
            ServerRunInfo = false;
        }
    }

    private void HandleServerDisconnection()
    {
        if (ServerRun)
        {
            TempTimer.Dispose();
            ServerRun = false;
        }
        ServerRunInfo = false;

        var processes = Process.GetProcessesByName("ClevoEcControl");
        if (processes.Length is 0)
        {
            // 如果 ClevoEcControl.exe 没有运行，那么启动它
            // Process.Start("ClevoEcControl.exe");
        }
        else
        {
            HandleServerTimeout(processes);
        }
    }

    private void HandleServerTimeout(Process[] processes)
    {
        ServerTimeout++;
        ServerTimeoutInfo++;
        if (ServerTimeout > 3)
        {
            foreach (var process in processes)
            {
                process.Kill();
            }
            ServerTimeout = 0;
        }
    }

    public void TempTimerTick(object stateInfo)
    {
        var isConnect = ClevoEcControl.IsServerStarted();
        //Debug.WriteLine($"isConnect: " + isConnect.ToString());

        if (isConnect)
        {
            var val = ClevoEcControl.GetCpuFanRpm();
            var cpuFanRpm = val switch
            {
                0 => 0,
                > 300 and < 5000 => 2100000 / val,
                _ => 0
            };

            val = ClevoEcControl.GetGpuFanRpm();
            var gpuFanRpm = val switch
            {
                0 => 0,
                > 300 and < 5000 => 2100000 / val,
                _ => 0
            };

            DispatcherQueue.TryEnqueue(() =>
            {
                CpuDutySet = (int)Math.Round(CpuFanRadialGauge.Value / 100.0 * 255);
                GpuDutySet = (int)Math.Round(GpuFanRadialGauge.Value / 100.0 * 255);

                CpuFanRpmRadialGauge.Value = cpuFanRpm;
                GpuFanRpmRadialGauge.Value = gpuFanRpm;
            });

            var fanId = 1;
            var data = ClevoEcControl.GetTempFanDuty(fanId);
            int cpuTemp = data.Remote;
            CpuFanDuty = data.FanDuty;
            DispatcherQueue.TryEnqueue(() =>
            {
                CpuTemp.Value = cpuTemp;
                CpuTempText.Text = cpuTemp + "℃";
            });
            if (CpuFanDuty != CpuDutySet)
            {
                ClevoEcControl.SetFanDuty(fanId, CpuDutySet);
            }

            fanId = 2;
            data = ClevoEcControl.GetTempFanDuty(fanId);
            int gpuTemp = data.Remote;
            GpuFanDuty = data.FanDuty;
            DispatcherQueue.TryEnqueue(() =>
            {
                GpuTemp.Value = gpuTemp;
                GpuTempText.Text = gpuTemp + "℃";
            });
            if (GpuFanDuty != GpuDutySet)
            {
                ClevoEcControl.SetFanDuty(fanId, GpuDutySet);
            }
            CpuFanDuty = CpuDutySet;
            GpuFanDuty = GpuDutySet;
        }
        else
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                CpuTempText.Text = "N/A℃";
                GpuTempText.Text = "N/A℃";
                CpuFanRpmRadialGauge.Value = 0;
                GpuFanRpmRadialGauge.Value = 0;
            });
        }
    }

    private void ClevoGetFaninfo_Click(object sender, RoutedEventArgs e)
    {
        var info = ClevoEcControl.InitIo();
        Debug.WriteLine("info: " + info);
        var ecVersion = ClevoEcControl.GetECVersion();
        Debug.WriteLine("ECVersion: " + ecVersion);
        var fanNum = ClevoEcControl.GetFanCount();
        Debug.WriteLine("info: " + fanNum);

        var fanId = 1;
        var cpuDuty = (int)Math.Round(CpuFanRadialGauge.Value / 100.0 * 255);
        var data = ClevoEcControl.GetTempFanDuty(fanId);
        // 使用Debug.WriteLine打印字段
        Debug.WriteLine("Remote: " + data.Remote);
        //Debug.WriteLine($"Local: {data.Local}");
        Debug.WriteLine("FanDuty: " + data.FanDuty);
        //Debug.WriteLine($"FanRpm: {data.Reserve}");
        ClevoEcControl.SetFanDuty(fanId, cpuDuty);
        //CPUFanRadialGauge.Value = (int)Math.Round((duty / 255.0) * 100);
        var cpuFanRpm = 2100000 / ClevoEcControl.GetCpuFanRpm();
        Debug.WriteLine("FanRpm:" + cpuFanRpm);

        fanId = 2;
        var gpuDuty = (int)Math.Round(GpuFanRadialGauge.Value / 100.0 * 255);
        data = ClevoEcControl.GetTempFanDuty(fanId);
        // 使用Debug.WriteLine打印字段
        Debug.WriteLine("Remote: " + data.Remote);
        //Debug.WriteLine($"Local: {data.Local}");
        Debug.WriteLine("FanDuty: "+ data.FanDuty);
        //Debug.WriteLine($"Reserve: {data.Reserve}");
        ClevoEcControl.SetFanDuty(fanId, gpuDuty);
        //GPUFanRadialGauge.Value = (int)Math.Round((duty / 255.0) * 100);
        var gpuFanRpm = 2100000 / ClevoEcControl.GetGpuFanRpm();
        Debug.WriteLine("FanRpm:" + gpuFanRpm);
    }

    private void Clevoinfo_Click(object sender, RoutedEventArgs e)
    {
        var isConnect =  ClevoEcControl.IsServerStarted();
        Debug.WriteLine("isConnect: " + isConnect);

            

        if (isConnect)
        {
            ClevoGetFaninfo.IsEnabled = true;

            var isInitialize = ClevoEcControl.InitIo();
            Debug.WriteLine("isInitialize: " + isInitialize);
        }
        else
        {
            ClevoGetFaninfo.IsEnabled= false;
        }
    }
    private void CPUFanRadialGauge_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
        // 检查是否是Value属性发生了变化
        _cpuDelayTimer.Change(200, Timeout.Infinite);
    }

    private void WatchDogStart_Click(object sender, RoutedEventArgs e)
    {
        var isStart = ClevoEcControl.IsWatchDogStarted();
        Debug.WriteLine("isStart: " + isStart);
        if (!isStart)
        {
            ClevoEcControl.SetWatchDogStarted();
        }
        Debug.WriteLine("WatchDogServer is Start");
    }

    private void WatchDogClose_Click(object sender, RoutedEventArgs e)
    {
        var isStart = ClevoEcControl.IsWatchDogStarted();
        Debug.WriteLine("isStart: " + isStart);
        if (isStart)
        {
            ClevoEcControl.SetWatchDogClosed();
        }
        Debug.WriteLine("WatchDogServer is Close");
    }

    private void GPUFanRadialGauge_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
        // 检查是否是Value属性发生了变化
        _gpuDelayTimer.Change(200, Timeout.Infinite);
    }
    private void CpuOnTimer(object state)
    {
        var cpuFanId = 1;
        // 从RadialGauge获取数据
        DispatcherQueue.TryEnqueue(() =>
        {
            CpuDutySet = (int)Math.Round(CpuFanRadialGauge.Value / 100.0 * 255);
        });
        var isConnect = ClevoEcControl.IsServerStarted();
        if (isConnect)
        {
            if (CpuFanDuty != CpuDutySet)
            {
                ClevoEcControl.SetFanDuty(cpuFanId, CpuDutySet);
            }
            CpuFanDuty = CpuDutySet;
        }
    }

    private void GpuOnTimer(object state)
    {
        var gpuFanId = 2;
        // 从RadialGauge获取数据
        DispatcherQueue.TryEnqueue(() =>
        {
            GpuDutySet = (int)Math.Round(GpuFanRadialGauge.Value / 100.0 * 255);
        });
        var isConnect = ClevoEcControl.IsServerStarted();
        if (isConnect)
        {
            if (CpuFanDuty != GpuDutySet)
            {
                ClevoEcControl.SetFanDuty(gpuFanId, GpuDutySet);
            }
        }
    }

    private void OnDraw(CanvasControl sender, CanvasDrawEventArgs args)
    {
        var point1 = new Vector2(0, 200); // 起点
        var controlPoint1 = new Vector2(100, 100); // 控制点1
        var controlPoint2 = new Vector2(200, 150); // 控制点2
        var point2 = new Vector2(300, 0); // 终点

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
}
