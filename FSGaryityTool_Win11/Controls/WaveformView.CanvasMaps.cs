using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace FSGaryityTool_Win11.Controls
{
    public partial class WaveformView : INotifyPropertyChanged
    {

        private double _viewMinX;
        public double ViewMinX
        {
            get => _viewMinX;
            set
            {
                if (_viewMinX == value) return;
                _viewMinX = value;
                OnPropertyChanged(nameof(ViewMinX));
            }
        }

        private double _viewMaxX;
        public double ViewMaxX
        {
            get => _viewMaxX;
            set
            {
                if (_viewMaxX == value) return;
                _viewMaxX = value;
                OnPropertyChanged(nameof(ViewMaxX));
            }
        }

        private double _viewMinY;
        public double ViewMinY
        {
            get => _viewMinY;
            set
            {
                if (_viewMinY == value) return;
                _viewMinY = value;
                OnPropertyChanged(nameof(ViewMinY));
            }
        }

        private double _viewMaxY;
        public double ViewMaxY
        {
            get => _viewMaxY;
            set
            {
                if (_viewMaxY == value) return;
                _viewMaxY = value;
                OnPropertyChanged(nameof(ViewMaxY));
            }
        }


        // Expose RuntimeCenter as a public CLR wrapper for the dependency property so
        // XAML (including Hot Reload) can set it at design/runtime.
        public Point RuntimeCenter
        {
            get => (Point)GetValue(RuntimeCenterProperty);
            set => SetValue(RuntimeCenterProperty, value);
        }
        private Point DefaultCenter => CalculateDefaultCenter();

        private Point CalculateDefaultCenter()
        {
            double minX = MinHorizontalValue;
            double maxX = MaxHorizontalValue;
            double minY = MinVerticalValue;
            double maxY = MaxVerticalValue;

            // 先用Auto方式计算中心
            Point autoCenter = new Point((minX + maxX) / 2, (minY + maxY) / 2);

            if (ViewCenterPointMode == ViewCenterMode.Auto)
            {
                // 只用范围中心
                return autoCenter;
            }

            // 其它模式先用autoCenter再根据模式调整
            return ViewCenterPointMode switch
            {
                ViewCenterMode.LeftTop => new Point(minX, maxY),
                ViewCenterMode.Top => new Point(autoCenter.X, maxY),
                ViewCenterMode.RightTop => new Point(maxX, maxY),
                ViewCenterMode.Left => new Point(minX, autoCenter.Y),
                ViewCenterMode.Center => autoCenter,
                ViewCenterMode.Right => new Point(maxX, autoCenter.Y),
                ViewCenterMode.LeftBottom => new Point(minX, minY),
                ViewCenterMode.Bottom => new Point(autoCenter.X, minY),
                ViewCenterMode.RightBottom => new Point(maxX, minY),
                ViewCenterMode.ManualPoint => ViewCenterPoint,
                _ => autoCenter,
            };
        }

        private void UpdateResetViewVisibility()
        {
            bool isZoomed = Math.Abs(HorizontalZoomScale - 1.0) > 0.001 ||
                            Math.Abs(VerticalZoomScale - 1.0) > 0.001;
            bool isPanned = RuntimeCenter != DefaultCenter;  // 实时比较

            // 判断是否在原点 (0,0)
            bool isNotOrigin = !RuntimeCenter.Equals(DefaultCenter);

            ResetViewVisibility = (ZoomMode != WaveformZoomMode.Disabled && (isZoomed || isPanned || isNotOrigin))
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        public void ZoomControlToSet(Point point, double zoomSize)
        {
            if (zoomSize <= 0) zoomSize = 1.0;

            double targetScaleX = HorizontalZoomScale;
            double targetScaleY = VerticalZoomScale;

            if (ZoomMode is WaveformZoomMode.Horizontal or WaveformZoomMode.Both)
                targetScaleX = zoomSize;
            if (ZoomMode is WaveformZoomMode.Vertical or WaveformZoomMode.Both)
                targetScaleY = zoomSize;

            // 关键：调用 5 参数版本，传入 point.X 和 point.Y
            UpdateResetViewVisibility();
            StartZoomAnimation(
                centerX: point.X,
                centerY: point.Y,
                targetScaleX: targetScaleX,
                targetScaleY: targetScaleY,
                duration: TimeSpan.FromMilliseconds(280));
        }
        private void ZoomControlToReset()
        {
            StartZoomAnimation(
                centerX: DefaultCenter.X,   // 这里直接用计算属性
                centerY: DefaultCenter.Y,
                targetScaleX: 1.0,
                targetScaleY: 1.0,
                duration: TimeSpan.FromMilliseconds(350));
        }

        /// <summary>
        /// 获取当前的逻辑中心点（即当前视图的中心点，通常等同于 RuntimeCenter）。
        /// 如需自定义逻辑中心点计算，可在此扩展。
        /// </summary>
        private Point CurrentLogicalCenter => RuntimeCenter;
        // 计算并更新当前可视范围
        private void UpdateVisibleRange()
        {
            double zoomX = ZoomMode is WaveformZoomMode.Horizontal or WaveformZoomMode.Both ? HorizontalZoomScale : 1.0;
            double zoomY = ZoomMode is WaveformZoomMode.Vertical or WaveformZoomMode.Both ? VerticalZoomScale : 1.0;

            double rangeX = (MaxHorizontalValue - MinHorizontalValue) / zoomX;
            double rangeY = (MaxVerticalValue - MinVerticalValue) / zoomY;

            ViewMinX = RuntimeCenter.X - rangeX / 2;
            ViewMaxX = RuntimeCenter.X + rangeX / 2;
            ViewMinY = RuntimeCenter.Y - rangeY / 2;
            ViewMaxY = RuntimeCenter.Y + rangeY / 2;
        }

        private static Vector2 MapToCanvas(Point logical, GridBounds grid, double viewMinX, double viewMaxX, double viewMinY, double viewMaxY)
        {
            double viewW = viewMaxX - viewMinX;
            double viewH = viewMaxY - viewMinY;

            if (viewW <= 0 || viewH <= 0) return new Vector2(0, 0);

            float x = grid.Left + (float)((logical.X - viewMinX) / viewW * grid.Width);
            float y = grid.Bottom - (float)((logical.Y - viewMinY) / viewH * grid.Height);

            return new Vector2(x, y);
        }

        public static Point MapToLogical(Point pixel, GridBounds grid, double minX, double maxX, double minY, double maxY)
        {
            double logicalX = minX;
            double logicalY = minY;

            if (grid.Width > 0 && maxX != minX)
            {
                logicalX = minX + Math.Clamp((pixel.X - grid.Left) / grid.Width, 0, 1) * (maxX - minX);
            }

            if (grid.Height > 0 && maxY != minY)
            {
                logicalY = maxY - Math.Clamp((pixel.Y - grid.Top) / grid.Height, 0, 1) * (maxY - minY);
            }

            return new Point(logicalX, logicalY);
        }




    }
}
