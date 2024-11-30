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
using System.ComponentModel;
using Windows.System;
using System.Reflection;

namespace FSGaryityTool_Win11.Views.Pages.FanControlPage;

public sealed partial class Page5 : Page
{
    private Timer _cpuDelayTimer;

    private Timer _gpuDelayTimer;

    public Timer TempTimer { get; set; }
    private DispatcherTimer TempSettimer = new DispatcherTimer();

    private KalmanFilter cpuTempFilter = new KalmanFilter(1, 1, 1, 25); // 参数可根据实际情况调整
    private KalmanFilter gpuTempFilter = new KalmanFilter(1, 1, 1, 25);
    public Timer ServerRunCheckTimer { get; set; }

    public string CpuTempDisplay => $"{CpuTemp.Value}℃";

    public string GpuTempDisplay => $"{GpuTemp.Value}℃";

    public static int CpuFanDuty { get; set; }

    public static int GpuFanDuty{ get; set; }

    public static int CpuDutySet { get; set; } = 166;

    public static int GpuDutySet { get; set; } = 166;

    public TempViewModel ViewModel { get; set; }
    public class TempViewModel : INotifyPropertyChanged
    {
        private int _cpumTemp;
        private int _gpumTemp;

        public event PropertyChangedEventHandler PropertyChanged;

        public int CpumTemp
        {
            get { return _cpumTemp; }
            set
            {
                if (_cpumTemp != value)
                {
                    _cpumTemp = value;
                    OnPropertyChanged(nameof(CpumTemp));
                }
            }
        }

        public int GpumTemp
        {
            get { return _gpumTemp; }
            set
            {
                if (_gpumTemp != value)
                {
                    _gpumTemp = value;
                    OnPropertyChanged(nameof(GpumTemp));
                }
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        }
    }

    

    public Page5()
    {
        this.InitializeComponent();

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
                    ResetUiForDisconnected(2);
                }
            }
            else
            {
                ResetUiForDisconnected(2);
            }
        }
        catch
        {
            ResetUiForDisconnected(2);
        }

        if (Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length > 1)
        {
            Debug.WriteLine("An instance of this application is already running.");
            ResetUiForDisconnected(0);
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

            //自动温度控制
            TempSettimer.Interval = new TimeSpan(0, 0, 2); // 每2秒检查一次
            TempSettimer.Tick += TempSetTimer_Tick;
            TempSettimer.Start();
        }
        ViewModel = new TempViewModel();
        this.DataContext = ViewModel;
        ViewModel.CpumTemp = 25;
        ViewModel.GpumTemp = 25;
    }

    public class KalmanFilter
    {
        private double Q; // 过程噪声协方差
        private double R; // 测量噪声协方差
        private double P; // 估计误差协方差
        private double X; // 状态估计值
        private double K; // 卡尔曼增益

        public KalmanFilter(double processNoise, double measurementNoise, double estimatedError, double initialValue)
        {
            Q = processNoise;
            R = measurementNoise;
            P = estimatedError;
            X = initialValue;
        }

        public double Update(double measurement)
        {
            // 预测更新
            P = P + Q;

            // 计算卡尔曼增益
            K = P / (P + R);

            // 状态更新
            X = X + K * (measurement - X);

            // 更新估计误差协方差
            P = (1 - K) * P;

            return X;
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
            var cpuFanRpm = FanRpmCalculation(ClevoEcControl.GetCpuFanRpm());
            var gpuFanRpm = FanRpmCalculation(ClevoEcControl.GetGpuFanRpm());

            DispatcherQueue.TryEnqueue(() =>
            {
                CpuDutySet = UpdateFanDutySet(CpuFanRadialGauge.Value);
                GpuDutySet = UpdateFanDutySet(GpuFanRadialGauge.Value);

                CpuFanRpmRadialGauge.Value = cpuFanRpm;
                GpuFanRpmRadialGauge.Value = gpuFanRpm;
            });

            UpdateFanData(1, CpuDutySet, CpuFanDuty, CpuTempText, cpuTempFilter, temp => ViewModel.CpumTemp = temp);
            UpdateFanData(2, GpuDutySet, GpuFanDuty, GpuTempText, gpuTempFilter, temp => ViewModel.GpumTemp = temp);
        }
        else
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                ResetUiForDisconnected(1);
            });
        }
    }
    private void UpdateFanData(int fanId, int dutySet, int duty, TextBlock tempText, KalmanFilter tempFilter, Action<int> setTemp) 
    { 
        var data = ClevoEcControl.GetTempFanDuty(fanId);
        int temp = data.Remote; 
        duty = data.FanDuty;
        double predictedTemp = tempFilter.Update(temp);
        DispatcherQueue.TryEnqueue(() => 
        {
            setTemp((int)predictedTemp);
            tempText.Text = temp + "℃"; 
        });
        if (duty != dutySet)
        { 
            ClevoEcControl.SetFanDuty(fanId, dutySet); 
        }
        duty = dutySet;
    }

    private int UpdateFanDutySet(double value) 
    { 
        int str = (int)Math.Round(value / 100.0 * 255); 
        return str;
    }

    private int FanRpmCalculation(int Value)
    {
        var fanRpm = Value switch
        {
            0 => 0,
            > 300 and < 5000 => 2100000 / Value,
            _ => 0
        };
        return fanRpm;
    }

    private void ResetUiForDisconnected(int Model)
    {
        if (Model == 0)
        {
            CpuTempText.Text = "N/A℃";
            GpuTempText.Text = "N/A℃";
        }
        else if (Model == 1)
        {
            CpuTempText.Text = "N/A℃";
            GpuTempText.Text = "N/A℃";
            CpuFanRpmRadialGauge.Value = 0;
            GpuFanRpmRadialGauge.Value = 0;
        }
        else if (Model == 2)
        {
            CpuFanRadialGauge.Value = 60;
            GpuFanRadialGauge.Value = 60;
            CpuTempText.Text = "N/A℃";
            GpuTempText.Text = "N/A℃";
            CpuFanRpmRadialGauge.Value = 0;
            GpuFanRpmRadialGauge.Value = 0;
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
        var cpuDuty = UpdateFanDutySet(CpuFanRadialGauge.Value);
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
        var gpuDuty = UpdateFanDutySet(GpuFanRadialGauge.Value);
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
            CpuDutySet = UpdateFanDutySet(CpuFanRadialGauge.Value);
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
            GpuDutySet = UpdateFanDutySet(GpuFanRadialGauge.Value);
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
    private void TempSetTimer_Tick(object sender, object e)
    {
        // 更新界面上的风扇转速显示
        if (CpuFanControlToggleButton.IsChecked == true)
        {
            CpuFanRadialGauge.Value = CalculateFanSpeed(ViewModel.CpumTemp, 0.014, 2, 2.2, 15);
        }
        if (GpuFanControlToggleButton.IsChecked == true)
        {
            GpuFanRadialGauge.Value = CalculateFanSpeed(ViewModel.GpumTemp, 0.014, 2, 2.2, 0);
        }
    }
    private int CalculateFanSpeed(int temperature, double math0, double math1, double math2, double math3)
    {
        int math = (int)(math0 * Math.Pow(temperature, math1) - math3);
        //Debug.WriteLine("Temp" + temperature);
        //Debug.WriteLine("Control" + math);
        return math;
    }


}
