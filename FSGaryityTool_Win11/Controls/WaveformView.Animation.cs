using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Input;
using System;
using System.Diagnostics;
using System.Numerics;
using System.Linq;
using Windows.Foundation;

namespace FSGaryityTool_Win11.Controls
{
    public partial class WaveformView
    {
        // Panning animator
        private bool _isPanningActive;
        private Point _panTargetCenter;
        // Preserve runtime center at drag start to avoid unexpected instant jumps on release
        private Point? _preReleaseRuntimeCenter;
        // Track last runtime center applied during dragging
        private Point _lastKnownRuntimeCenter;
        private long _lastDragSampleTimestamp = 0;
        private long _lastRenderTimestamp = 0;

        // 使用 InertiaAccumulator（定义于另一个 partial 文件）驱动惯性动画

        // 新增字段：用于缩放动画
        private bool _isZoomActive;
        private Stopwatch? _zoomStopwatch;
        private Point _zoomStartCenter;
        private double _zoomStartScaleX, _zoomStartScaleY;
        private double _zoomTargetCenterX, _zoomTargetCenterY;
        private double _zoomTargetScaleX, _zoomTargetScaleY;
        private TimeSpan _zoomDuration;
        private TimeSpan _zoomStartTime;

        // Use CompositionTarget.Rendering for high-frequency, VSync-aligned updates

        private const double DragSensitivity = 1.0;

        private void HandleWaveformPointerMoved(PointerRoutedEventArgs e)
        {
            var point = e.GetCurrentPoint(WaveformDemonstratorCanvasControl);
            _cursorX = (float)point.Position.X;
            _cursorY = (float)point.Position.Y;
            InvalidateCanvas();
        }

        private void HandlePointerPressed(PointerRoutedEventArgs e)
        {
            var point = e.GetCurrentPoint(WaveformDemonstratorCanvasControl);
            _activePointers[point.PointerId] = point;

            if (_activePointers.Count == 2)
            {
                BeginPinchGesture();
                e.Handled = true;
                return;
            }

            if (_activePointers.Count == 1 && ZoomMode != WaveformZoomMode.Disabled)
            {
                BeginDragGesture(point.Position, e.Pointer);
                e.Handled = true;
            }
        }

        private void HandlePointerMoved(PointerRoutedEventArgs e)
        {
            var point = e.GetCurrentPoint(WaveformDemonstratorCanvasControl);
            if (_activePointers.ContainsKey(point.PointerId))
                _activePointers[point.PointerId] = point;

            if (_isMultiTouch && _activePointers.Count == 2)
            {
                UpdatePinchGesture();
                e.Handled = true;
                return;
            }

            if (_isDragging && ZoomMode != WaveformZoomMode.Disabled)
            {
                UpdateDragGesture(point.Position);
                e.Handled = true;
            }
        }

        private void HandlePointerReleased(PointerRoutedEventArgs e)
        {
            var point = e.GetCurrentPoint(WaveformDemonstratorCanvasControl);
            _activePointers.Remove(point.PointerId);

            if (_activePointers.Count < 2)
                _isMultiTouch = false;

            if (_isDragging)
            {
                EndDragGesture(e.Pointer);
                e.Handled = true;
            }
        }

        private void HandlePointerCanceled(PointerRoutedEventArgs e)
        {
            var point = e.GetCurrentPoint(WaveformDemonstratorCanvasControl);
            _activePointers.Remove(point.PointerId);
            if (_activePointers.Count < 2)
                _isMultiTouch = false;

            if (_isDragging)
                EndDragGesture(e.Pointer);
        }

        private void BeginDragGesture(Point position, Pointer pointer)
        {
            _isDragging = true;
            _lastDragPosition = position;
            _preReleaseRuntimeCenter = RuntimeCenter;
            _lastKnownRuntimeCenter = RuntimeCenter;
            _lastDragSampleTimestamp = Stopwatch.GetTimestamp();
            CapturePointer(pointer);
            StopInertia();
            UpdateResetViewVisibility();
        }

        private void UpdateDragGesture(Point position)
        {
            var delta = new Vector2(
                (float)(position.X - _lastDragPosition.X),
                (float)(position.Y - _lastDragPosition.Y));

            if (delta.Length() < 0.5f)
                return;

            ApplyLogicalDragDelta(delta);
            UpdateDragVelocity(delta);
            _lastDragPosition = position;
            // remember last applied runtime center so we can detect jumps
            _lastKnownRuntimeCenter = RuntimeCenter;
        }

        // Modified EndDragGesture to accept release position and align content to the release point
        private void EndDragGesture(Pointer pointer, Point? releasePosition = null)
        {
            _isDragging = false;

            // If we have an actual release position, align logical center using mapping
            if (releasePosition.HasValue)
            {
                var margin = WaveGridBorderMargin;
                double drawableWidth = ActualWidth - margin.Left - margin.Right;
                double drawableHeight = ActualHeight - margin.Top - margin.Bottom;

                if (drawableWidth > 0 && drawableHeight > 0)
                {
                    // Do not apply extra positional snap here — the last UpdateDragGesture should
                    // have already applied the final movement. Only update the last recorded
                    // pointer position/time so inertia uses the recent history.
                    _lastDragPosition = releasePosition.Value;
                    // keep sample timestamp as-is (or update to now)
                    _lastDragSampleTimestamp = Stopwatch.GetTimestamp();
                }
                else
                {
                    // Fallback: apply pixel delta directly
                    var releaseDelta = new Vector2(
                        (float)(releasePosition.Value.X - _lastDragPosition.X),
                        (float)(releasePosition.Value.Y - _lastDragPosition.Y));
                    if (releaseDelta.LengthSquared() > 0.000001f)
                        ApplyLogicalDragDelta(releaseDelta);
                    _lastDragPosition = releasePosition.Value;
                }
            }

            ReleasePointerCapture(pointer);

            // Clamp inertia velocity to avoid extreme instantaneous jumps
            var vel = _inertiaAccumulator.Velocity;
            const float MaxPixelsPerSecond = 8000f;
            if (vel.Length() > MaxPixelsPerSecond)
            {
                var clamped = Vector2.Normalize(vel) * MaxPixelsPerSecond;
                // Add a clamped sample to smooth to safe range
                _inertiaAccumulator.AddInputVelocity(clamped);
            }

            if (_inertiaAccumulator.Velocity.Length() >= _inertiaAccumulator.MinVelocity)
                StartInertiaAnimation();
            else
                StopInertia();
        }

        private void BeginPinchGesture()
        {
            StopInertia();
            var pts = _activePointers.Values.ToList();
            if (pts.Count < 2)
                return;

            _isMultiTouch = true;
            _isDragging = false;
            _lastDistance = GetDistance(pts[0].Position, pts[1].Position);
            _lastCenter = GetCenter(pts[0].Position, pts[1].Position);
        }

        private void UpdatePinchGesture()
        {
            if (_activePointers.Count != 2)
                return;

            var pts = _activePointers.Values.ToList();
            Point p1 = pts[0].Position;
            Point p2 = pts[1].Position;

            Point newCenter = GetCenter(p1, p2);
            double newDistance = GetDistance(p1, p2);
            if (_lastDistance <= 0.0001)
            {
                _lastDistance = newDistance;
                _lastCenter = newCenter;
                return;
            }

            double scale = newDistance / _lastDistance;
            var grid = GetGridBounds(WaveformDemonstratorCanvasControl.ActualWidth, WaveformDemonstratorCanvasControl.ActualHeight);
            var logicalAnchor = MapToLogical(_lastCenter, grid, ViewMinX, ViewMaxX, ViewMinY, ViewMaxY);

            double targetScaleX = HorizontalZoomScale * scale;
            double targetScaleY = VerticalZoomScale * scale;
            double rangeX = (MaxHorizontalValue - MinHorizontalValue) / targetScaleX;
            double rangeY = (MaxVerticalValue - MinVerticalValue) / targetScaleY;

            double percentX = (_lastCenter.X - grid.Left) / grid.Width;
            double percentY = 1.0 - (_lastCenter.Y - grid.Top) / grid.Height;
            double newCenterX = logicalAnchor.X - (percentX - 0.5) * rangeX;
            double newCenterY = logicalAnchor.Y - (percentY - 0.5) * rangeY;

            double dx = newCenter.X - _lastCenter.X;
            double dy = newCenter.Y - _lastCenter.Y;
            double logicDeltaX = -dx * rangeX / grid.Width;
            double logicDeltaY = dy * rangeY / grid.Height;

            HorizontalZoomScale = targetScaleX;
            VerticalZoomScale = targetScaleY;
            RuntimeCenter = new Point(newCenterX + logicDeltaX, newCenterY + logicDeltaY);
            InvalidateCanvas();
            UpdateResetViewVisibility();
            StartAnimationLoop();

            _lastDistance = newDistance;
            _lastCenter = newCenter;
        }

        private void ApplyLogicalDragDelta(Vector2 delta)
        {
            var margin = WaveGridBorderMargin;
            double drawableWidth = ActualWidth - margin.Left - margin.Right;
            double drawableHeight = ActualHeight - margin.Top - margin.Bottom;
            if (drawableWidth <= 0 || drawableHeight <= 0)
                return;

            double viewW = ViewMaxX - ViewMinX;
            double viewH = ViewMaxY - ViewMinY;

            double logicDeltaX = delta.X * viewW / drawableWidth;
            double logicDeltaY = -delta.Y * viewH / drawableHeight;

            RuntimeCenter = new Point(
                RuntimeCenter.X - logicDeltaX * DragSensitivity,
                RuntimeCenter.Y - logicDeltaY * DragSensitivity);
            InvalidateCanvas();
            UpdateResetViewVisibility();
        }

        private void UpdateDragVelocity(Vector2 delta)
        {
            var now = Stopwatch.GetTimestamp();
            var dtTicks = now - _lastDragSampleTimestamp;
            if (dtTicks <= 0)
            {
                _lastDragSampleTimestamp = now;
                return;
            }

            double dt = dtTicks / (double)Stopwatch.Frequency;
            // ignore extremely short samples (noise) — require at least 4ms between samples
            if (dt < 0.004)
            {
                _lastDragSampleTimestamp = now;
                return;
            }

            var velocity = new Vector2((float)(delta.X / dt), (float)(delta.Y / dt));

            // clamp velocity to reasonable maximum to avoid spikes on very fast moves or small dt
            const float MaxPixelsPerSecond = 8000f;
            if (velocity.Length() > MaxPixelsPerSecond)
                velocity = Vector2.Normalize(velocity) * MaxPixelsPerSecond;

            _inertiaAccumulator.AddInputVelocity(velocity);
            _lastDragSampleTimestamp = now;
        }


        private void OnCompositionRendering(object? sender, object? e)
        {
            var now = Stopwatch.GetTimestamp();  // 高精度计时
            double deltaSeconds = 0.016; // default
            if (_lastRenderTimestamp != 0)
            {
                deltaSeconds = (now - _lastRenderTimestamp) / (double)Stopwatch.Frequency;
            }
            _lastRenderTimestamp = now;

            
            // First, handle panning smoothing (driven by pointer events updating _panTargetCenter)
            if (_isPanningActive)
            {
                // smooth toward target center
                double panLerp = Math.Clamp(deltaSeconds * 12.0, 0.01, 0.9);

                RuntimeCenter = new Point(
                    RuntimeCenter.X + (_panTargetCenter.X - RuntimeCenter.X) * panLerp,
                    RuntimeCenter.Y + (_panTargetCenter.Y - RuntimeCenter.Y) * panLerp);

                InvalidateCanvas();
                UpdateResetViewVisibility();
            }

            // If panning is active, stop it when close enough to target (in pixel space)
            if (_isPanningActive)
            {
                var margin = WaveGridBorderMargin;
                double drawableWidth = ActualWidth - margin.Left - margin.Right;
                double drawableHeight = ActualHeight - margin.Top - margin.Bottom;
                if (drawableWidth > 0 && drawableHeight > 0)
                {
                    double viewW = ViewMaxX - ViewMinX;
                    double viewH = ViewMaxY - ViewMinY;
                    double dxPixels = (_panTargetCenter.X - RuntimeCenter.X) / (viewW == 0 ? 1.0 : viewW) * drawableWidth;
                    double dyPixels = (_panTargetCenter.Y - RuntimeCenter.Y) / (viewH == 0 ? 1.0 : viewH) * drawableHeight;
                    double dist = Math.Sqrt(dxPixels * dxPixels + dyPixels * dyPixels);
                    if (dist < 0.5)
                    {
                        _isPanningActive = false;
                        // stop animation loop if no other animations
                        if (!_inertiaAccumulator.IsAnimating && !_isZoomActive)
                            StopAnimationLoop();
                    }
                }
            }

            // Inertia handled by InertiaAccumulator
            _inertiaAccumulator.Update();
            if (_inertiaAccumulator.IsAnimating)
            {
                var vel = _inertiaAccumulator.Velocity; // pixels/sec

                if (vel.Length() >= _inertiaAccumulator.MinVelocity)
                {
                    var margin = WaveGridBorderMargin;
                    double drawableWidth = ActualWidth - margin.Left - margin.Right;
                    double drawableHeight = ActualHeight - margin.Top - margin.Bottom;

                    if (drawableWidth <= 0 || drawableHeight <= 0)
                    {
                        _inertiaAccumulator.Stop();
                    }
                    else
                    {
                        double viewW = ViewMaxX - ViewMinX;
                        double viewH = ViewMaxY - ViewMinY;

                        // Convert pixel/sec velocity to logical units/sec
                        double logicVelX = vel.X * (viewW / drawableWidth);
                        double logicVelY = vel.Y * (viewH / drawableHeight);

                        // Apply movement using frame deltaSeconds
                        RuntimeCenter = new Point(
                            RuntimeCenter.X - logicVelX * deltaSeconds * DragSensitivity,
                            RuntimeCenter.Y + logicVelY * deltaSeconds * DragSensitivity);

                        InvalidateCanvas();
                        UpdateResetViewVisibility();
                    }
                }
            }

            if (!_inertiaAccumulator.IsAnimating && !_isPanningActive && !_isZoomActive)
                StopAnimationLoop();

            // ── 处理缩放动画 ────────────────────────────────────────────
            if (_isZoomActive && _zoomStopwatch != null)
            {
                double elapsedMs = _zoomStopwatch.Elapsed.TotalMilliseconds;
                double progress = elapsedMs / _zoomDuration.TotalMilliseconds;

                if (progress >= 1.0)
                {
                    // 到达终点
                    RuntimeCenter = new Point(_zoomTargetCenterX, _zoomTargetCenterY);
                    HorizontalZoomScale = _zoomTargetScaleX;
                    VerticalZoomScale = _zoomTargetScaleY;
                    StopZoom();
                }
                else
                {
                    double eased = 1 - Math.Pow(1 - progress, 3); // EaseOutCubic

                    RuntimeCenter = new Point(
                        _zoomStartCenter.X + (_zoomTargetCenterX - _zoomStartCenter.X) * eased,
                        _zoomStartCenter.Y + (_zoomTargetCenterY - _zoomStartCenter.Y) * eased);

                    HorizontalZoomScale = _zoomStartScaleX + (_zoomTargetScaleX - _zoomStartScaleX) * eased;
                    VerticalZoomScale = _zoomStartScaleY + (_zoomTargetScaleY - _zoomStartScaleY) * eased;

                    UpdateResetViewVisibility();
                    InvalidateCanvas();  // 如果缩放需要实时刷新波形
                }
            }
        }

        private void StartInertiaAnimation()
        {
            if (_inertiaAccumulator.Velocity.Length() < _inertiaAccumulator.MinVelocity)
                return;

            _inertiaAccumulator.Start();
            InvalidateCanvas();
            UpdateResetViewVisibility();
            StartAnimationLoop();
        }

        private void StopInertia()
        {
            _inertiaAccumulator.Stop();
            StopAnimationLoop();
        }

        private void StartZoomAnimation(double centerX, double centerY, double targetScaleX, double targetScaleY, TimeSpan duration)
        {
            StopZoom();

            _zoomStartCenter = RuntimeCenter;
            _zoomStartScaleX = HorizontalZoomScale;
            _zoomStartScaleY = VerticalZoomScale;

            targetScaleX = Math.Clamp(targetScaleX, 0.001, 80000);
            targetScaleY = Math.Clamp(targetScaleY, 0.001, 80000);

            _zoomTargetCenterX = centerX;
            _zoomTargetCenterY = centerY;
            _zoomTargetScaleX = targetScaleX;
            _zoomTargetScaleY = targetScaleY;
            _zoomDuration = duration;

            _zoomStopwatch = Stopwatch.StartNew();
            _isZoomActive = true;
            StartAnimationLoop();
        }

        private void StartAnimationLoop()
        {
            // Subscribe to CompositionTarget.Rendering to get VSynced callbacks
            Microsoft.UI.Xaml.Media.CompositionTarget.Rendering -= OnCompositionRendering;
            Microsoft.UI.Xaml.Media.CompositionTarget.Rendering += OnCompositionRendering;
            // Reset render timestamp to avoid a large deltaSeconds on the first frame after starting the loop
            _lastRenderTimestamp = Stopwatch.GetTimestamp();
        }

        private void StopAnimationLoop()
        {
            Microsoft.UI.Xaml.Media.CompositionTarget.Rendering -= OnCompositionRendering;
        }

        private void StopZoom()
        {
            _isZoomActive = false;
            _zoomStopwatch?.Stop();
        }


    }

    public class InertiaAccumulator
    {
        // --- 配置参数 ---
        public float Friction { get; set; } = 0.92f;          // 摩擦力 (0.90-0.95 手感较好)
        public float MinVelocity { get; set; } = 5f;          // 停止阈值
        public int HistorySize { get; set; } = 5;             // 缓冲区大小 (保留最近 5 帧)
        public float HistoryWeight { get; set; } = 0.7f;      // 历史权重 (0.0-1.0，越高越平滑，越低越跟手)

        // --- 内部状态 ---
        private readonly Vector2[] _history;                  // 环形缓冲区
        private int _historyIndex = 0;                        // 当前写入位置
        private Vector2 _currentVelocity = Vector2.Zero;      // 当前输出速度
        private bool _isAnimating = false;
        private readonly Stopwatch _timer = new Stopwatch();

        // --- 事件 ---
        public event Action<Vector2>? OnUpdate; // 传递当前速度
        public event Action? OnStop;

        public Vector2 Velocity => _currentVelocity;
        public bool IsAnimating => _isAnimating;

        public InertiaAccumulator()
        {
            _history = new Vector2[HistorySize];
        }

        public void Start()
        {
            if (_currentVelocity.Length() < MinVelocity)
                return;

            _isAnimating = true;
            _timer.Restart();
        }

        /// <summary>
        /// 1. 输入阶段：在 PointerMoved 中调用
        /// </summary>
        public void AddInputVelocity(Vector2 velocity)
        {
            // 将当前瞬时速度写入环形缓冲区
            // 这里的逻辑是：新数据覆盖最旧的数据
            _history[_historyIndex] = velocity;
            _historyIndex = (_historyIndex + 1) % HistorySize;

            // 仅更新当前速度，不在输入阶段进入动画
            Vector2 weightedVelocity = Vector2.Zero;
            for (int i = 0; i < HistorySize; i++)
            {
                int index = (_historyIndex - 1 - i + HistorySize) % HistorySize;
                Vector2 sample = _history[index];
                float weight = 1.0f - (i * (1.0f - HistoryWeight) / HistorySize);
                weightedVelocity += sample * weight;
            }

            weightedVelocity /= HistorySize;
            if (weightedVelocity.Length() < 1.0f)
                weightedVelocity = Vector2.Zero;

            _currentVelocity = Vector2.Lerp(_currentVelocity, weightedVelocity, 0.5f);
        }

        /// <summary>
        /// 2. 渲染阶段：在 CompositionTarget.Rendering 中调用
        /// </summary>
        public void Update()
        {
            if (!_isAnimating) return;

            // 计算时间差
            long elapsedTicks = _timer.ElapsedTicks;
            _timer.Restart();
            float dt = elapsedTicks / (float)Stopwatch.Frequency;

            // 应用摩擦力: V = V * (friction ^ dt)
            float decay = (float)Math.Pow(Friction, dt * 60);
            _currentVelocity *= decay;

            // --- 停止检查 ---
            if (_currentVelocity.Length() < MinVelocity)
            {
                _currentVelocity = Vector2.Zero;
                _isAnimating = false;
                _timer.Stop();
                OnStop?.Invoke();
            }
            else
            {
                OnUpdate?.Invoke(_currentVelocity);
            }
        }

        public void Stop()
        {
            _isAnimating = false;
            _currentVelocity = Vector2.Zero;
            _timer.Stop();
            Array.Clear(_history, 0, _history.Length);
            _historyIndex = 0;
            OnStop?.Invoke();
        }
    }
}