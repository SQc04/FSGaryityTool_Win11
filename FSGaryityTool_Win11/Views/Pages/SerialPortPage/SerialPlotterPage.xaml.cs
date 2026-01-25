using FSGaryityTool_Win11.Controls;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Numerics;
using Windows.Foundation;
using Windows.UI;

namespace FSGaryityTool_Win11.Views.Pages.SerialPortPage;

public sealed partial class SerialPlotterPage : Page
{
    private WaveformDataSource _dynamicSquareWave;
    private Stopwatch _phaseTimer = new();
    private TimeSpan _lastUpdate = TimeSpan.Zero;
    private float _dutyCycle = 0.1f;
    private float amplitude = 75f;

    private float _dutyBreathFrequency = 0.8f;     // 占空比呼吸频率，建议0.3~2Hz都很舒服，0.8Hz ≈ 1.25秒一周期
    private float _baseSignalFrequency = 2f;       // 方波基础信号频率（Hz）
    private float _frequencyBreathAmount = 0.25f;  // 频率本身的呼吸幅度（可选），0 = 不呼吸
    public SerialPlotterPage()
    {
        InitializeComponent();
        Loaded += SerialPlotterPage_Loaded;

    }

    private void SerialPlotterPage_Loaded(object sender, RoutedEventArgs e)
    {
        
        float spacing = 200f;

        var StrokeBrush = new LinearGradientBrush
        {
            StartPoint = new Point(0, 0),
            EndPoint = new Point(1, 0),
            GradientStops = new GradientStopCollection
            {
                new GradientStop { Color = Colors.Red, Offset = 0.0 },
                new GradientStop { Color = Colors.Orange, Offset = 0.2 },
                new GradientStop { Color = Colors.Yellow, Offset = 0.4 },
                new GradientStop { Color = Colors.Green, Offset = 0.6 },
                new GradientStop { Color = Colors.Blue, Offset = 0.8 },
                new GradientStop { Color = Colors.Purple, Offset = 1.0 }
            }
        };

        _dynamicSquareWave = new WaveformDataSource
        {
            Name = "动态方波",
            StrokeBrush = new SolidColorBrush(Colors.Yellow),
            StrokeThickness = 2f,
            LineStyle = LineStyle.Solid,
            TickMode = TickModeStyle.Intersection,
            OffSetY = -1.5f * spacing,
            FunctionFormulaData = x => SquareWaveFunction(x, _dutyCycle)
        };

        PlotterWaveformView.Data = new ObservableCollection<WaveformDataSource>
        {
            _dynamicSquareWave,
            new WaveformDataSource
            {
                Name = "方波",
                StrokeBrush = StrokeBrush,
                StrokeThickness = 2f,
                LineStyle = LineStyle.Solid,
                TickMode = TickModeStyle.Intersection,
                OffSetY = -150,
                OffSetX = 0,
                FunctionFormulaData = x => SquareWaveFunction(x, _dutyCycle)
            },
            new WaveformDataSource
            {
                Name = "正弦波",
                StrokeBrush = new SolidColorBrush(Colors.Lime),
                StrokeThickness = 2f,
                LineStyle = LineStyle.Solid,
                TickMode = TickModeStyle.None,
                OffSetX = 0,
                OffSetY = 200,
                FunctionFormulaData = x => amplitude * MathF.Sin(x * 2 * MathF.PI * 2)
            },
            new WaveformDataSource
            {
                Name = "正弦波",
                StrokeBrush = new SolidColorBrush(Colors.Lime),
                StrokeThickness = 2f,
                LineStyle = LineStyle.Solid,
                TickMode = TickModeStyle.None,
                OffSetX = 0,
                OffSetY = 250,
                FunctionFormulaData = x => amplitude * MathF.Sin(x * 2 * MathF.PI * 2)
            },
            new WaveformDataSource
            {
                Name = "正弦波",
                StrokeBrush = new SolidColorBrush(Colors.Lime),
                StrokeThickness = 2f,
                LineStyle = LineStyle.Solid,
                TickMode = TickModeStyle.None,
                OffSetX = 0,
                OffSetY = 300,
                FunctionFormulaData = x => amplitude * MathF.Sin(x * 2 * MathF.PI * 2)
            },
            new WaveformDataSource
            {
                Name = "正弦波",
                StrokeBrush = new SolidColorBrush(Colors.Lime),
                StrokeThickness = 2f,
                LineStyle = LineStyle.Solid,
                TickMode = TickModeStyle.None,
                OffSetX = 0,
                OffSetY = 350,
                FunctionFormulaData = x => amplitude * MathF.Sin(x * 2 * MathF.PI * 2)
            },
            new WaveformDataSource
            {
                Name = "正弦波",
                StrokeBrush = new SolidColorBrush(Colors.Lime),
                StrokeThickness = 2f,
                LineStyle = LineStyle.Solid,
                TickMode = TickModeStyle.None,
                OffSetX = 0,
                OffSetY = 400,
                FunctionFormulaData = x => amplitude * MathF.Sin(x * 2 * MathF.PI * 2)
            },
            new WaveformDataSource
            {
                Name = "正弦波",
                StrokeBrush = new SolidColorBrush(Colors.Lime),
                StrokeThickness = 2f,
                LineStyle = LineStyle.Solid,
                TickMode = TickModeStyle.None,
                OffSetX = 0,
                OffSetY = 450,
                FunctionFormulaData = x => amplitude * MathF.Sin(x * 2 * MathF.PI * 2)
            },
            new WaveformDataSource
            {
                Name = "正弦波2",
                StrokeBrush = new SolidColorBrush(Colors.Lime),
                StrokeThickness = 2f,
                LineStyle = LineStyle.Solid,
                TickMode = TickModeStyle.None,
                OffSetX = 30,
                OffSetY = 100,
                FunctionFormulaData = x => amplitude * MathF.Sin(x * 2 * MathF.PI * 2)
            },
            new WaveformDataSource
            {
                Name = "测试折线",
                StrokeBrush = new SolidColorBrush(Colors.Cyan),
                StrokeThickness = 2f,
                LineStyle = LineStyle.Solid,
                TickMode = TickModeStyle.Cross,
                OffSetX = 0,
                OffSetY = 230,
                PolylinePointsData = new ObservableCollection<(float x, float y)>
                {
                    (0f, 0f),
                    (20.2f, 30f),
                    (40.4f, -20f),
                    (60.6f, 50f),
                    (80.8f, -10f),
                    (100f, 0f)
                }
            },
            new WaveformDataSource
            {
                Name = "测试折线2",
                StrokeBrush = new SolidColorBrush(Colors.Cyan),
                StrokeThickness = 2f,
                LineStyle = LineStyle.Solid,
                TickMode = TickModeStyle.Intersection,
                OffSetX = 50,
                OffSetY = 240,
                PolylinePointsData = new ObservableCollection<(float x, float y)>
                {
                    (0f, 0f),
                    (20.2f, 30f),
                    (40.4f, -20f),
                    (60.6f, 50f),
                    (80.8f, -10f),
                    (100f, 0f)
                }
            },
            
        };

        _phaseTimer.Start();
        CompositionTarget.Rendering += OnFrameUpdate;
    }

    private void OnFrameUpdate(object sender, object e)
    {
        var now = _phaseTimer.Elapsed;
        var delta = now - _lastUpdate;
        _lastUpdate = now;

        double totalSeconds = now.TotalSeconds;   // 改用秒，更直观

        // ── 1. 占空比呼吸（独立控制） ────────────────────────────────
        float dutyPhase = (float)(totalSeconds * _dutyBreathFrequency);
        float dutyT = (MathF.Sin(dutyPhase * MathF.PI * 2f) + 1f) * 0.5f; // 0～1
        _dutyCycle = 0.1f + dutyT * 0.8f;                                 // 0.1 ↔ 0.9


        // ── 2. 信号频率（可以固定，也可以加一点缓慢呼吸） ────────────
        float currentFreq = _baseSignalFrequency;

        // 如果想要频率也轻微呼吸，取消注释下面几行
        // float freqBreathPhase = (float)(totalSeconds * _dutyBreathFrequency * 0.4f); // 可以用不同的频率
        // float freqMod = 1f + _frequencyBreathAmount * MathF.Sin(freqBreathPhase * MathF.PI * 2f);
        // currentFreq *= freqMod;   // 例如 2Hz ± 0.5Hz 范围内呼吸


        // ── 3. 更新动态方波的公式 ────────────────────────────────────
        _dynamicSquareWave.FunctionFormulaData = x => SquareWaveFunction(x, currentFreq, _dutyCycle);
    }


    private float SquareWaveFunction(float x, float freq = 2f, float duty = 0.5f)
    {
        float t = x * freq;
        float frac = t - MathF.Floor(t);  // 等价于 fract(t)
        return frac < duty ? 75f : -75f;
    }

    private readonly Random _rand = new();


}
