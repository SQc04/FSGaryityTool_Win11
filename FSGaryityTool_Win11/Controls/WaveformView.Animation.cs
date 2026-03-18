using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System;
using System.Diagnostics;
using System.Numerics;
using Windows.Foundation;

namespace FSGaryityTool_Win11.Controls
{
    public partial class WaveformView
    {
        private Vector2 _inertiaVelocity = Vector2.Zero;

        // Panning animator
        private bool _isPanningActive;
        private Point _panTargetCenter;
        // samples for velocity estimation during drag
        // Stored in WaveformView.xaml.cs as _dragSamples (to keep pointer logic together)
        private long _lastRenderTimestamp = 0;

        // 新增字段：用于惯性
        private bool _isInertiaActive;
        private Stopwatch? _inertiaStopwatch;
        private TimeSpan _lastInertiaTime;

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
        // Inertia velocity decay per second (0.0-1.0). 0.9 means velocity reduces to 90% after 1s.
        private const double InertiaDecayPerSecond = 0.90;
        private const double MinInertiaSpeed = 1.0;
        private const float InertiaVelocitySmoothing = 0.7f;


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
                        if (!_isInertiaActive && !_isZoomActive)
                            StopAnimationLoop();
                    }
                }
            }

            if (_isInertiaActive)
            {
                if (_inertiaStopwatch == null)
                {
                    _inertiaStopwatch = Stopwatch.StartNew();
                    _lastInertiaTime = TimeSpan.Zero;
                }

                var currentTime = _inertiaStopwatch.Elapsed;
                var deltaTime = currentTime - _lastInertiaTime;
                _lastInertiaTime = currentTime;

                float inertiaDeltaSeconds = (float)deltaTime.TotalSeconds;

                if (_inertiaVelocity.Length() < MinInertiaSpeed)
                {
                    StopInertia();
                }
                else
                {
                    var margin = WaveGridBorderMargin;
                    double drawableWidth = ActualWidth - margin.Left - margin.Right;
                    double drawableHeight = ActualHeight - margin.Top - margin.Bottom;

                    if (drawableWidth <= 0 || drawableHeight <= 0)
                    {
                        StopInertia();
                        return;
                    }

                    double viewW = ViewMaxX - ViewMinX;
                    double viewH = ViewMaxY - ViewMinY;

                    // Convert pixel/sec velocity to logical units/sec
                    double logicVelX = _inertiaVelocity.X * (viewW / drawableWidth);
                    double logicVelY = _inertiaVelocity.Y * (viewH / drawableHeight);

                    // Apply movement: note Y direction sign matches pointer logic (positive pixel Y moves center negative)
                    RuntimeCenter = new Point(
                        RuntimeCenter.X - logicVelX * inertiaDeltaSeconds * DragSensitivity,
                        RuntimeCenter.Y - logicVelY * inertiaDeltaSeconds * DragSensitivity);

                    // Apply exponential decay per second
                    float decay = (float)Math.Pow(InertiaDecayPerSecond, inertiaDeltaSeconds);
                    _inertiaVelocity *= decay;

                    InvalidateCanvas();
                    UpdateResetViewVisibility();
                }
            }

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
            StopInertia();
            if (_inertiaVelocity.Length() < MinInertiaSpeed)
                return;

            _isInertiaActive = true;
            _inertiaStopwatch?.Restart();
            _lastInertiaTime = TimeSpan.Zero;

            // 关键：强制让系统认为有渲染需求
            InvalidateCanvas();           // 或 this.InvalidateArrange(); / UpdateLayout();
            UpdateResetViewVisibility();
            StartAnimationLoop();
        }

        private void StopInertia()
        {
            _isInertiaActive = false;
            _inertiaStopwatch?.Stop();
            _inertiaVelocity = Vector2.Zero; // 可选：彻底清零
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
}