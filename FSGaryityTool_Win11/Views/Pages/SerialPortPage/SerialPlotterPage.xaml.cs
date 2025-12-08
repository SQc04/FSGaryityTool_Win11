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
    private WaveformDataSource _randomPolylineWave;
    private Stopwatch _phaseTimer = new();
    private TimeSpan _lastUpdate = TimeSpan.Zero;
    private float _dutyCycle = 0.1f;
    private bool _increasing = true;
    private float amplitude = 75f;

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
        };

        _phaseTimer.Start();
        CompositionTarget.Rendering += OnFrameUpdate;
    }

    private void OnFrameUpdate(object sender, object e)
    {
        var now = _phaseTimer.Elapsed;
        var delta = now - _lastUpdate;
        _lastUpdate = now;

        double totalMs = now.TotalMilliseconds;

        // 方波基础频率：2Hz（每500ms一个周期）
        const float baseFrequency = 2f;

        // 占空比变化频率：8Hz（每125ms呼吸一次）→ 正是 baseFrequency 的 4 倍！
        float dutyCycleFrequency = baseFrequency * 1.5f;

        // 计算占空比（0.1 → 0.9 呼吸）
        float dutyPhase = (float)(totalMs * 0.001 * dutyCycleFrequency); // 转成秒 × 频率
        float dutyT = (MathF.Sin(dutyPhase * MathF.PI * 2f) + 1f) * 0.5f; // [-1,1] → [0,1]
        _dutyCycle = 0.1f + dutyT * 0.8f; // 0.1 → 0.9

        // 可选：让频率也微微呼吸（更炫）
        float freqBreath = 1f + 0.3f * MathF.Sin(dutyPhase * 0.5f); // 频率在 1.7~2.6 之间轻微波动
        float currentFreq = baseFrequency * freqBreath;

        // 更新函数（频率 + 占空比双动态！）
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
