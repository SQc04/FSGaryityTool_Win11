using ClevoEcControlinfo;
using FSGaryityTool_Win11.Controls;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Numerics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI;

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

    // Temp indicator waveform sources
    private WaveformDataSource cpuTempLineSource;
    private WaveformDataSource gpuTempLineSource;

    public ObservableCollection<WaveformDataSource> CpuWaveformData { get; set; } = new();
    public ObservableCollection<WaveformDataSource> GpuWaveformData { get; set; } = new();

    public class TempViewModel : INotifyPropertyChanged
    {
        private double _cpumTemp;
        private double _gpumTemp;

        public event PropertyChangedEventHandler PropertyChanged;

        public double CpumTemp
        {
            get { return _cpumTemp; }
            set
            {
                if (Math.Abs(_cpumTemp - value) > 0.01) // 避免频繁更新
                {
                    _cpumTemp = value;
                    OnPropertyChanged(nameof(CpumTemp));
                }
            }
        }

        public double GpumTemp
        {
            get { return _gpumTemp; }
            set
            {
                if (Math.Abs(_gpumTemp - value) > 0.01) // 避免频繁更新
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
        _ = InitializeAsync();

        // 假设温度区间为0~100
        float minTemp = 0f;
        float maxTemp = 100f;

        Func<float, float> CpuFanFunction = t =>
        {
            float temp = minTemp + t * (maxTemp - minTemp);
            double result = 0.014 * Math.Pow(temp, 2) - 15;
            return (float)result;
        };

        Func<float, float> GpuFanFunction = t =>
        {
            float temp = minTemp + t * (maxTemp - minTemp);
            double result = 0.014 * Math.Pow(temp, 2) + 25;
            return (float)result;
        };

        var cpuFanCurve = new WaveformDataSource
        {
            Name = "CPU风扇控制曲线",
            StrokeBrush = new SolidColorBrush(Color.FromArgb(255, 237, 28, 36)),
            StrokeThickness = 2f,
            LineStyle = LineStyle.Solid,
            TickMode = TickModeStyle.None,
            OffSetY = 0f,
            FunctionFormulaData = CpuFanFunction
        };

        var gpuFanCurve = new WaveformDataSource
        {
            Name = "GPU风扇控制曲线",
            StrokeBrush = new SolidColorBrush(Color.FromArgb(255, 118, 185, 0)),
            StrokeThickness = 2f,
            LineStyle = LineStyle.Solid,
            TickMode = TickModeStyle.None,
            OffSetY = 0f,
            FunctionFormulaData = GpuFanFunction
        };

        // create temperature indicator line sources (horizontal lines across the logical X range)
        cpuTempLineSource = new WaveformDataSource
        {
            Name = "CPU Temp Line",
            StrokeBrush = new SolidColorBrush(Colors.Orange),
            StrokeThickness = 1f,
            LineStyle = LineStyle.Solid,
            TickMode = TickModeStyle.None,
            OffSetY = 0f,
            PolylinePointsData = new ObservableCollection<(float x, float y)>()
        };

        gpuTempLineSource = new WaveformDataSource
        {
            Name = "GPU Temp Line",
            StrokeBrush = new SolidColorBrush(Colors.Lime),
            StrokeThickness = 1f,
            LineStyle = LineStyle.Solid,
            TickMode = TickModeStyle.None,
            OffSetY = 0f,
            PolylinePointsData = new ObservableCollection<(float x, float y)>()
        };

        CpuWaveformData.Add(cpuFanCurve);
        CpuWaveformData.Add(cpuTempLineSource);
        GpuWaveformData.Add(gpuFanCurve);
        GpuWaveformData.Add(gpuTempLineSource);

        ClevoGetFanSeriver_Click(null, null);
    }

    private async Task InitializeAsync()
    {
        try
        {
            var isConnect = await Task.Run(() => ClevoEcControl.IsServerStarted());
            if (isConnect)
            {
                var isInitialize = await Task.Run(() => ClevoEcControl.InitIo());
                Debug.WriteLine("isInitialize: " + isInitialize);
                if (isInitialize)
                {
                    var fanNum = await Task.Run(() => ClevoEcControl.GetFanCount());
                    Debug.WriteLine("info: " + fanNum);

                    var fanId = 1;
                    var data = await Task.Run(() => ClevoEcControl.GetTempFanDuty(fanId));
                    CpuFanRadialGauge.Value = (int)Math.Round(data.FanDuty / 255.0 * 100);
                    fanId = 2;
                    data = await Task.Run(() => ClevoEcControl.GetTempFanDuty(fanId));
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
        ViewModel.CpumTemp = 25.0;
        ViewModel.GpumTemp = 25.0;
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
            GpuFanRadialGauge.Value = CalculateFanSpeed(ViewModel.GpumTemp, 0.014, 2, 2.2, -25);
        }

        // update temp indicator lines on waveform views
        UpdateCpuTempLine(ViewModel.CpumTemp);
        UpdateGpuTempLine(ViewModel.GpumTemp);
    }
    private int CalculateFanSpeed(double temperature, double math0, double math1, double math2, double math3)
    {
        int math = (int)(math0 * Math.Pow(temperature, math1) - math3);
        //Debug.WriteLine("Temp" + temperature);
        //Debug.WriteLine("Control" + math);
        return math;
    }

    private void UpdateCpuTempLine(double temp)
    {
        if (cpuTempLineSource == null || CpuWaveformView == null) return;
        try
        {
            // Use logical horizontal range of the waveform view
            var bottom = 0f;
            var top = 100f;
            var x = (float)temp;
            cpuTempLineSource.PolylinePointsData = new ObservableCollection<(float x, float y)>
            {
                (x, bottom),
                (x, top)
            };
        }
        catch { }
    }

    private void UpdateGpuTempLine(double temp)
    {
        if (gpuTempLineSource == null || GpuWaveformView == null) return;
        try
        {
            var bottom = 0f;
            var top = 100f;
            var x = (float)temp;
            gpuTempLineSource.PolylinePointsData = new ObservableCollection<(float x, float y)>
            {
                (x, bottom),
                (x, top)
            };
        }
        catch { }
    }

    private void ClevoGetFanSeriver_Click(object sender, RoutedEventArgs e)
    {
        var sercerExePath = ClevoSeriverTextBox.Text + "\\ClevoEcControl.exe";
        try
        {
            var processes = Process.GetProcessesByName("ClevoEcControl");
            if (processes.Length == 0)
            {
                Process.Start(sercerExePath);
                Debug.WriteLine("ClevoEcControl.exe started successfully.");
            }
            else
            {
                Debug.WriteLine("ClevoEcControl.exe is already running.");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Failed to start ClevoEcControl.exe: " + ex.Message);
        }

    }
}
