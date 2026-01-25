using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace FSGaryityTool_Win11.Controls
{
    public partial class WaveformView
    {

        private Vector2 _inertiaVelocity;

        private DispatcherTimer? _inertiaTimer;

        private const double DragSensitivity = 1.0;     // 拖动灵敏度，可调
        private const double InertiaFriction = 0.94;    // 惯性衰减（0.9~0.95 手感好）
        private const double MinInertiaSpeed = 1.0;

        private void StartInertiaAnimation()
        {
            _inertiaTimer?.Stop();
            _inertiaTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1) };

            _inertiaTimer.Tick += (_, _) =>
            {
                if (_inertiaVelocity.Length() < MinInertiaSpeed)
                {
                    _inertiaTimer.Stop();
                    return;
                }

                var margin = WaveGridBorderMargin;
                double drawableWidth = ActualWidth - margin.Left - margin.Right;
                double drawableHeight = ActualHeight - margin.Top - margin.Bottom;

                if (drawableWidth <= 0 || drawableHeight <= 0)
                {
                    _inertiaTimer.Stop();
                    return;
                }

                double viewW = ViewMaxX - ViewMinX;
                double viewH = ViewMaxY - ViewMinY;

                double deltaX = _inertiaVelocity.X * viewW / drawableWidth;
                double deltaY = -_inertiaVelocity.Y * viewH / drawableHeight;

                RuntimeCenter = new Point(
                    RuntimeCenter.X - deltaX * DragSensitivity,
                    RuntimeCenter.Y - deltaY * DragSensitivity);

                _inertiaVelocity *= (float)InertiaFriction;

                InvalidateCanvas();
                UpdateResetViewVisibility();
            };

            _inertiaTimer.Start();
        }

        private DispatcherTimer _zoomTimer;

        private void StartZoomAnimation(double centerX, double centerY, double targetScaleX, double targetScaleY, TimeSpan duration)
        {
            var startCenter = RuntimeCenter;
            var startScaleX = HorizontalZoomScale;
            var startScaleY = VerticalZoomScale;

            targetScaleX = Math.Clamp(targetScaleX, 0.001, 80000);
            targetScaleY = Math.Clamp(targetScaleY, 0.001, 80000);

            _zoomTimer?.Stop();
            _zoomTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1) };
            var startTime = DateTime.Now;

            _zoomTimer.Tick += (_, _) =>
            {
                double elapsed = (DateTime.Now - startTime).TotalMilliseconds;
                double progress = Math.Min(elapsed / duration.TotalMilliseconds, 1.0);
                double eased = 1 - Math.Pow(1 - progress, 3); // EaseOutCubic
                
                RuntimeCenter = new Point(
                    startCenter.X + (centerX - startCenter.X) * eased,
                    startCenter.Y + (centerY - startCenter.Y) * eased);
                
                HorizontalZoomScale = startScaleX + (targetScaleX - startScaleX) * eased;
                VerticalZoomScale = startScaleY + (targetScaleY - startScaleY) * eased;
                UpdateResetViewVisibility();
                if (progress >= 1.0) _zoomTimer.Stop();
            };

            _zoomTimer.Start();
        }


    }
}
