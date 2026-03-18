using FSGaryityTool_Win11.Controls;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using NAudio.Wave;
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
    private float amplitude = 1f;

    // microphone capture
    private WaveInEvent _waveIn;
    private readonly ConcurrentQueue<float> _micQueueLeft = new();
    private readonly ConcurrentQueue<float> _micQueueRight = new();
    private WaveformDataSource _micLeftWave;
    private WaveformDataSource _micRightWave;
    private WaveformDataSource _micXYWave;
    private float _micSampleX = 0f;
    private const int _micMaxPoints = 4800; // rolling window for display
    // ring buffer for 1s of audio at 48kHz
    private const int _micRingCapacity = 48000; // 1 second @48k
    private readonly float[] _micRingLeft = new float[_micRingCapacity];
    private readonly float[] _micRingRight = new float[_micRingCapacity];
    private int _micRingLeftIndex = 0;
    private int _micRingRightIndex = 0;
    private int _micRingCount = 0; // shared count of valid samples (per-channel)
    private readonly object _micRingLock = new();

    private float _dutyBreathFrequency = 0.8f;     // 占空比呼吸频率，建议0.3~2Hz都很舒服，0.8Hz ≈ 1.25秒一周期
    private float _baseSignalFrequency = 2f;       // 方波基础信号频率（Hz）
    private float _frequencyBreathAmount = 0.25f;  // 频率本身的呼吸幅度（可选），0 = 不呼吸
    public SerialPlotterPage()
    {
        InitializeComponent();
        WaveformSources = new ObservableCollection<WaveformDataSource>();
        Loaded += SerialPlotterPage_Loaded;
        Unloaded += SerialPlotterPage_Unloaded;

    }

    // Exposed collection bound to WaveformView.Data
    public ObservableCollection<WaveformDataSource> WaveformSources { get; }

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

        // Demo data: clear and easy-to-read examples for testing the plotter
        // Use the bound collection so WaveformView.Data is populated via x:Bind

        // --- Real microphone channels (live) ---
        /*
        _micLeftWave = new WaveformDataSource
        {
            Name = "Microphone Left",
            StrokeBrush = new SolidColorBrush(Colors.LightGreen),
            StrokeThickness = 1.5f,
            LineStyle = LineStyle.Solid,
            TickMode = TickModeStyle.None,
            OffSetX = 0,
            OffSetY = -400,
            PolylinePointsData = new ObservableCollection<(float x, float y)>()
        };
        WaveformSources.Add(_micLeftWave);

        _micRightWave = new WaveformDataSource
        {
            Name = "Microphone Right",
            StrokeBrush = new SolidColorBrush(Colors.OrangeRed),
            StrokeThickness = 1.5f,
            LineStyle = LineStyle.Solid,
            TickMode = TickModeStyle.None,
            OffSetX = 0,
            OffSetY = -405,
            PolylinePointsData = new ObservableCollection<(float x, float y)>()
        };
        WaveformSources.Add(_micRightWave);
        //*/
        // Oscilloscope XY mode: will plot left vs right samples in a small logical box
        _micXYWave = new WaveformDataSource
        {
            Name = "Oscilloscope XY",
            StrokeBrush = new SolidColorBrush(Colors.Cyan),
            StrokeThickness = 1.5f,
            LineStyle = LineStyle.None,
            TickMode = TickModeStyle.Pointed,
            OffSetX = -50,
            OffSetY = -50,
            PolylinePointsData = new ObservableCollection<(float x, float y)>()
        };
        WaveformSources.Add(_micXYWave);

        // Start microphone capture (NAudio WaveIn)
        try
        {
            // Try to find device matching common Realtek microphone name
            int selectedDevice = 0;
            string desiredName = "VoiceMeeter Aux Input"; // match prefix
            for (int i = 0; i < WaveIn.DeviceCount; i++)
            {
                var caps = WaveIn.GetCapabilities(i);
                Debug.WriteLine($"WaveIn Device[{i}] = {caps.ProductName}, Channels={caps.Channels}");
                if (caps.ProductName != null && caps.ProductName.IndexOf("VoiceMeeter", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    selectedDevice = i;
                    break;
                }
            }

            var selectedCaps = WaveIn.GetCapabilities(selectedDevice);
            int channels = Math.Max(1, selectedCaps.Channels);

            _waveIn = new WaveInEvent
            {
                DeviceNumber = selectedDevice,
                BufferMilliseconds = 50,
                // use 48k sample rate to match ring buffer capacity (1s @ 48k)
                WaveFormat = new WaveFormat(48000, 16, channels)
            };
            _waveIn.DataAvailable += WaveIn_DataAvailable;
            _waveIn.StartRecording();
            Debug.WriteLine($"Started WaveIn on device {selectedDevice} ({selectedCaps.ProductName}) channels={channels}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Microphone capture start failed: {ex.Message}");
        }
        /*
        // dynamic square wave (updated in OnFrameUpdate)
        _dynamicSquareWave.FunctionFormulaData = x => SquareWaveFunction(x, _baseSignalFrequency, _dutyCycle);
        WaveformSources.Add(_dynamicSquareWave);

        // simple sine (1 Hz)
        WaveformSources.Add(new WaveformDataSource
        {
            Name = "Sine 1Hz",
            StrokeBrush = new SolidColorBrush(Colors.Lime),
            StrokeThickness = 2f,
            LineStyle = LineStyle.Solid,
            TickMode = TickModeStyle.None,
            OffSetX = 0,
            OffSetY = 200,
            FunctionFormulaData = x => amplitude * MathF.Sin(x * 2f * MathF.PI * 1f)
        });

        // triangle wave (0.5 Hz)
        WaveformSources.Add(new WaveformDataSource
        {
            Name = "Triangle 0.5Hz",
            StrokeBrush = new SolidColorBrush(Colors.Orange),
            StrokeThickness = 2f,
            LineStyle = LineStyle.Solid,
            TickMode = TickModeStyle.None,
            OffSetX = 0,
            OffSetY = 100,
            FunctionFormulaData = x =>
            {
                float freq = 0.5f;
                float t = x * freq;
                float frac = t - MathF.Floor(t);
                // triangle from -amplitude..amplitude
                return (2f * amplitude) * (frac < 0.5f ? frac : 1f - frac) - amplitude;
            }
        });

        // sawtooth wave (2 Hz)
        WaveformSources.Add(new WaveformDataSource
        {
            Name = "Sawtooth 2Hz",
            StrokeBrush = new SolidColorBrush(Colors.Cyan),
            StrokeThickness = 2f,
            LineStyle = LineStyle.Solid,
            TickMode = TickModeStyle.None,
            OffSetX = 0,
            OffSetY = 300,
            FunctionFormulaData = x =>
            {
                float freq = 2f;
                float t = x * freq;
                float frac = t - MathF.Floor(t);
                return (2f * frac - 1f) * amplitude; // -amplitude..amplitude
            }
        });

        // noisy sine for testing jitter / point rendering
        WaveformSources.Add(new WaveformDataSource
        {
            Name = "Noisy Sine",
            StrokeBrush = new SolidColorBrush(Colors.Magenta),
            StrokeThickness = 1.5f,
            LineStyle = LineStyle.Dash,
            TickMode = TickModeStyle.Cross,
            OffSetX = 0,
            OffSetY = 400,
            FunctionFormulaData = x => amplitude * MathF.Sin(x * 2f * MathF.PI * 0.25f) + (float)(_rand.NextDouble() - 0.5) * 20f
        });

        // polyline: sampled sine (regular samples)
        var sampled = new ObservableCollection<(float x, float y)>();
        for (int i = 0; i <= 200; i += 4)
        {
            float xf = i;
            float y = 60f * MathF.Sin(xf * 0.05f);
            sampled.Add((xf, y));
        }
        WaveformSources.Add(new WaveformDataSource
        {
            Name = "Sampled Sine (polyline)",
            StrokeBrush = new SolidColorBrush(Colors.LightBlue),
            StrokeThickness = 2f,
            LineStyle = LineStyle.Solid,
            TickMode = TickModeStyle.Intersection,
            OffSetX = 0,
            OffSetY = -100,
            PolylinePointsData = sampled
        });

        // polyline: simulated measurement / ramp with noise
        var measurements = new ObservableCollection<(float x, float y)>();
        float val = 0f;
        for (int i = 0; i <= 120; i += 3)
        {
            val += 0.8f + (float)(_rand.NextDouble() - 0.5) * 0.6f;
            measurements.Add((i, val));
        }
        WaveformSources.Add(new WaveformDataSource
        {
            Name = "Measurements (polyline)",
            StrokeBrush = new SolidColorBrush(Colors.Yellow),
            StrokeThickness = 2f,
            LineStyle = LineStyle.Solid,
            TickMode = TickModeStyle.Cross,
            OffSetX = 0,
            OffSetY = 150,
            PolylinePointsData = measurements
        });

        // --- Parametric vector shapes for debugging ---
        // Circle (radius 80)
        WaveformSources.Add(new WaveformDataSource
        {
            Name = "Parametric: Circle",
            StrokeBrush = new SolidColorBrush(Colors.Red),
            StrokeThickness = 2f,
            LineStyle = LineStyle.Solid,
            TickMode = TickModeStyle.None,
            OffSetX = 0,
            OffSetY = 0,
            ParametricFunctionData = t =>
            {
                double r = 80.0;
                double ang = t * Math.PI * 2.0;
                return (r * Math.Cos(ang), r * Math.Sin(ang));
            }
        });

        // Lissajous curve
        WaveformSources.Add(new WaveformDataSource
        {
            Name = "Parametric: Lissajous",
            StrokeBrush = new SolidColorBrush(Colors.Purple),
            StrokeThickness = 2f,
            LineStyle = LineStyle.Dash,
            TickMode = TickModeStyle.None,
            OffSetX = 0,
            OffSetY = -50,
            ParametricFunctionData = t =>
            {
                double A = 90.0, B = 60.0;
                double a = 3.0, b = 2.0;
                double delta = Math.PI / 2.0;
                double ang = t * Math.PI * 2.0;
                return (A * Math.Sin(a * ang + delta), B * Math.Sin(b * ang));
            }
        });

        // Spiral
        WaveformSources.Add(new WaveformDataSource
        {
            Name = "Parametric: Spiral",
            StrokeBrush = new SolidColorBrush(Colors.Orange),
            StrokeThickness = 1.5f,
            LineStyle = LineStyle.Solid,
            TickMode = TickModeStyle.None,
            OffSetX = 200,
            OffSetY = 0,
            ParametricFunctionData = t =>
            {
                double spins = 4.0;
                double r0 = 10.0;
                double r1 = 140.0;
                double ang = t * Math.PI * 2.0 * spins;
                double r = r0 + (r1 - r0) * t;
                return (r * Math.Cos(ang), r * Math.Sin(ang));
            }
        });
        //*/
        // Data is bound to WaveformSources via x:Bind in XAML

        _phaseTimer.Start();
        CompositionTarget.Rendering += OnFrameUpdate;
        // start background processing loop
        _cts = new CancellationTokenSource();
        Task.Run(() => BackgroundLoop(_cts.Token));
    }

    private CancellationTokenSource _cts;

    private async Task BackgroundLoop(CancellationToken token)
    {
        // run at ~60Hz background update
        TimeSpan delay = TimeSpan.FromMilliseconds(16);
        while (!token.IsCancellationRequested)
        {
            try
            {
                // process audio buffers into ring buffer -> already done in WaveIn callback
                // do light-weight data preparation here if needed
                await Task.Delay(delay, token);
            }
            catch (TaskCanceledException) { break; }
            catch { }
        }
    }

    private void OnFrameUpdate(object sender, object e)
    {
        /*
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
        //*/
        // Update lightweight UI-only state (dynamic functions)
        _dynamicSquareWave.FunctionFormulaData = x => SquareWaveFunction(x, _baseSignalFrequency, _dutyCycle);

        // Snapshot ring buffer state and offload heavy point preparation to background thread
        int snapshotCount, leftIndex, rightIndex;
        lock (_micRingLock)
        {
            snapshotCount = _micRingCount;
            leftIndex = _micRingLeftIndex;
            rightIndex = _micRingRightIndex;
        }

        // Fire-and-forget background preparation; it will apply results to UI thread when ready
        _ = Task.Run(() => PrepareWaveformPointsAsync(snapshotCount, leftIndex, rightIndex));
    }

    private readonly object _preparedLock = new();
    private List<(float x, float y)> _preparedLeft = new();
    private List<(float x, float y)> _preparedRight = new();
    private List<(float x, float y)> _preparedXY = new();

    private void ApplyPreparedToUI()
    {
        // apply prepared lists to UI-bound collections on UI thread
        this.DispatcherQueue.TryEnqueue(() =>
        {
            if (_micLeftWave?.PolylinePointsData != null)
            {
                _micLeftWave.PolylinePointsData.Clear();
                foreach (var p in _preparedLeft) _micLeftWave.PolylinePointsData.Add(p);
            }
            if (_micRightWave?.PolylinePointsData != null)
            {
                _micRightWave.PolylinePointsData.Clear();
                foreach (var p in _preparedRight) _micRightWave.PolylinePointsData.Add(p);
            }
            if (_micXYWave?.PolylinePointsData != null)
            {
                _micXYWave.PolylinePointsData.Clear();
                foreach (var p in _preparedXY) _micXYWave.PolylinePointsData.Add(p);
            }
        });
    }

    private async Task PrepareWaveformPointsAsync(int snapshotCount, int leftIndex, int rightIndex)
    {
        int capacity = _micRingCapacity;
        int count = Math.Min(snapshotCount, capacity);
        if (count <= 0) return;

        int samplesToTake = Math.Min(count, _micMaxPoints);
        int offsetBase = Math.Max(0, count - samplesToTake);

        var leftList = new List<(float x, float y)>(samplesToTake);
        var rightList = new List<(float x, float y)>(samplesToTake);
        var xyList = new List<(float x, float y)>(samplesToTake);

        for (int i = 0; i < samplesToTake; i++)
        {
            int offset = offsetBase + i;
            int lidx = (leftIndex + offset) % capacity;
            int ridx = (rightIndex + offset) % capacity;

            float leftSample, rightSample;
            lock (_micRingLock)
            {
                leftSample = _micRingLeft[lidx];
                rightSample = _micRingRight[ridx];
            }

            float xLogical = i; // sequential mapping for waveform view
            leftList.Add((xLogical, leftSample * amplitude));
            rightList.Add((xLogical, rightSample * amplitude));

            // XY mapping
            float x = 0f + (leftSample + 1f) * 0.5f * 100f;
            float y = 0f + (rightSample + 1f) * 0.5f * 100f;
            xyList.Add((x, y));
        }

        lock (_preparedLock)
        {
            _preparedLeft = leftList;
            _preparedRight = rightList;
            _preparedXY = xyList;
        }

        ApplyPreparedToUI();
    }

    private void UpdateOscilloscopeXY()
    {
        if (_micXYWave == null || _micXYWave.PolylinePointsData == null) return;

        lock (_micRingLock)
        {
            _micXYWave.PolylinePointsData.Clear();

            int count = Math.Min(_micRingCount, _micRingCapacity);
            if (count <= 0) return;

            // limit to _micMaxPoints for display
            int samplesToTake = Math.Min(count, _micMaxPoints);

            // determine start indices (oldest)
            int leftStart = (count >= _micRingCapacity) ? _micRingLeftIndex : 0;
            int rightStart = (count >= _micRingCapacity) ? _micRingRightIndex : 0;
            // if buffer not full, start at 0
            if (count < _micRingCapacity)
            {
                leftStart = 0;
                rightStart = 0;
            }

            // start reading offset so we take the most recent samples
            int offsetBase = Math.Max(0, count - samplesToTake);

            for (int i = 0; i < samplesToTake; i++)
            {
                int offset = offsetBase + i;
                int lidx = (leftStart + offset) % _micRingCapacity;
                int ridx = (rightStart + offset) % _micRingCapacity;

                float leftSample = _micRingLeft[lidx];
                float rightSample = _micRingRight[ridx];

                // map left (-1..1) -> x (4900..5000)
                float x = 4900f + (leftSample + 1f) * 0.5f * 100f;
                // map right (-1..1) -> y (350..450)
                float y = -350f + (rightSample + 1f) * 0.5f * 100f;

                _micXYWave.PolylinePointsData.Add((x, y));
            }
        }
    }

    private void DrainMicQueueToWave(WaveformDataSource wave, ConcurrentQueue<float> queue)
    {
        if (wave == null || wave.PolylinePointsData == null) return;
        // Update polyline points from ring buffer for display (use recent _micMaxPoints samples)
        lock (_micRingLock)
        {
            wave.PolylinePointsData.Clear();

            int count = _micRingCount;
            int capacity = _micRingCapacity;
            // choose channel buffer based on which WaveformDataSource is requested
            bool isLeft = ReferenceEquals(wave, _micLeftWave);

            // Determine oldest index: if buffer full oldest is next write index, otherwise oldest is 0
            int oldestIdx = (count >= capacity) ? _micRingLeftIndex : 0;
            if (!isLeft && count >= capacity) oldestIdx = _micRingRightIndex;
            int emptyLeading = capacity - count;

            // Produce full capacity points (x from 0..capacity-1). For positions before valid data, yield 0.
            for (int k = 0; k < capacity; k++)
            {
                float sample;
                if (k < emptyLeading)
                {
                    sample = 0f;
                }
                else
                {
                    int offset = k - emptyLeading;
                    int idx = (oldestIdx + offset) % capacity;
                    sample = isLeft ? _micRingLeft[idx] : _micRingRight[idx];
                }

                float xLogical = k; // absolute position in 0..capacity-1
                float yLogical = sample * amplitude;
                wave.PolylinePointsData.Add((xLogical, yLogical));
            }
        }
        //wave.Owner?.InvalidateCanvas();
    }

    private void WaveIn_DataAvailable(object? sender, WaveInEventArgs e)
    {
        // 16-bit stereo interleaved
        int bytesPerSample = 2;
        int channels = _waveIn.WaveFormat.Channels;
        int sampleCount = e.BytesRecorded / (bytesPerSample * channels);
        if (sampleCount <= 0)
        {
            Debug.WriteLine($"WaveIn DataAvailable: no samples (BytesRecorded={e.BytesRecorded})");
            return;
        }
        double sumSqL = 0.0;
        double sumSqR = 0.0;
        for (int i = 0; i < sampleCount; i++)
        {
            int offset = i * bytesPerSample * channels;
            short left = BitConverter.ToInt16(e.Buffer, offset);
            float leftF = left / 32768f;
            sumSqL += leftF * leftF;
            // push directly into ring buffer to support 48k ring; avoid enqueuing to reduce latency
            lock (_micRingLock)
            {
                // store inverted sample
                _micRingLeft[_micRingLeftIndex] = leftF;
                _micRingLeftIndex = (_micRingLeftIndex + 1) % _micRingCapacity;
                _micRingCount = Math.Min(_micRingCapacity, _micRingCount + 1);
            }
            if (channels > 1)
            {
                short right = BitConverter.ToInt16(e.Buffer, offset + bytesPerSample);
                float rightF = right / 32768f;
                sumSqR += rightF * rightF;
                lock (_micRingLock)
                {
                    // store inverted sample
                    _micRingRight[_micRingRightIndex] = rightF;
                    _micRingRightIndex = (_micRingRightIndex + 1) % _micRingCapacity;
                }
            }
        }

        double rmsL = Math.Sqrt(sumSqL / sampleCount);
        double rmsR = channels > 1 ? Math.Sqrt(sumSqR / sampleCount) : 0.0;
        //Debug.WriteLine($"WaveIn DataAvailable: Bytes={e.BytesRecorded} Samples={sampleCount} RMS_L={rmsL:F4} RMS_R={rmsR:F4}");
        // prevent _micSampleX unbounded growth
        if (_micSampleX > 1e8f) _micSampleX = 0f;
    }

    private void SerialPlotterPage_Unloaded(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_waveIn != null)
            {
                _waveIn.DataAvailable -= WaveIn_DataAvailable;
                _waveIn.StopRecording();
                _waveIn.Dispose();
                _waveIn = null;
            }
        }
        catch { }
    }


    private float SquareWaveFunction(float x, float freq = 2f, float duty = 0.5f)
    {
        float t = x * freq;
        float frac = t - MathF.Floor(t);  // 等价于 fract(t)
        return frac < duty ? 75f : -75f;
    }

    private readonly Random _rand = new();


}
