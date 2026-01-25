using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace FSGaryityTool_Win11.Controls
{
    public partial class WaveformView
    {

        /// <summary>
        /// 获取或设置与此实例关联的波形数据源集合。
        /// </summary>
        /// <remarks>
        /// 此属性允许动态更新波形数据源集合。如果使用数据绑定，集合的更改会自动反映到 UI 上。
        /// </remarks>
        public ObservableCollection<WaveformDataSource> Data
        {
            get => (ObservableCollection<WaveformDataSource>)GetValue(DataProperty);
            set => SetValue(DataProperty, value);
        }

        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register(nameof(Data), typeof(ObservableCollection<WaveformDataSource>), typeof(WaveformView), new PropertyMetadata(null, OnDataChanged));

        private static void OnDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (WaveformView)d;

            if (e.OldValue is ObservableCollection<WaveformDataSource> oldCollection)
                oldCollection.CollectionChanged -= control.OnDataCollectionChanged;

            if (e.NewValue is ObservableCollection<WaveformDataSource> newCollection)
                newCollection.CollectionChanged += control.OnDataCollectionChanged;

            control.SubscribeToDataSourceChanges();
            control.InvalidateCanvas(); // 初次绑定时触发重绘
        }

        private void OnDataCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            SubscribeToDataSourceChanges();
            InvalidateCanvas(); // 集合变更时重绘
        }

        // 订阅每个数据源的属性变化事件
        private void SubscribeToDataSourceChanges()
        {
            if (Data == null) return;

            foreach (var source in Data)
            {
                source.PropertyChanged -= OnDataSourceChanged;
                source.PropertyChanged += OnDataSourceChanged;
                source.Owner = this;
            }
        }
        private void OnDataSourceChanged(object sender, PropertyChangedEventArgs e)
        {
            // 可根据属性名过滤是否需要重绘
            if (e.PropertyName is nameof(WaveformDataSource.FunctionFormulaData) or
                nameof(WaveformDataSource.PolylinePointsData) or
                nameof(WaveformDataSource.Count) or
                nameof(WaveformDataSource.StrokeBrush))
            {
                InvalidateCanvas();
            }
        }
        // 计算数据源的最佳采样点数
        private int GetOptimalSampleCount(WaveformDataSource source)
        {
            // 1. 手动模式
            if (!source.Count.Equals("Auto", StringComparison.OrdinalIgnoreCase) &&
                int.TryParse(source.Count, out int manual))
            {
                return Math.Clamp(manual, 200, 100000);
            }

            // 2. Auto 模式：基础计算
            var margin = WaveGridBorderMargin;
            double drawableWidth = ActualWidth - margin.Left - margin.Right;
            if (drawableWidth <= 10) return 1000;

            double viewRangeX = ViewMaxX - ViewMinX;
            if (viewRangeX <= 0) return 1000;

            double pixelsPerUnit = drawableWidth / viewRangeX;
            int desired = (int)Math.Ceiling(pixelsPerUnit * 3.0);

            // 关键！LOD 上限控制（解决 2000 倍卡死）
            const int MaxReasonablePoints = 16000;   // 16000 是 Win2D 舒适上限
            const int UltraZoomThreshold = 2000;     // 放大倍数阈值

            double currentZoom = (MaxHorizontalValue - MinHorizontalValue) / viewRangeX;

            if (currentZoom > UltraZoomThreshold)
            {
                // 超大放大：强制降采样，但保证每像素至少 1 个点
                int minPoints = (int)Math.Ceiling(drawableWidth * 1.5); // 每像素 1.5 点，防锯齿
                desired = Math.Max(minPoints, desired);
                desired = Math.Min(desired, MaxReasonablePoints);
            }
            else
            {
                // 正常放大：允许高密度
                desired = Math.Min(desired, MaxReasonablePoints * 2);
            }

            return Math.Clamp(desired, 500, MaxReasonablePoints);
        }
        // 应用数据源偏移
        private Point ApplyOffset(Point logicalPoint, WaveformDataSource source)
        {
            return new Point(
                logicalPoint.X + source.OffSetX,
                logicalPoint.Y + source.OffSetY
            );
        }


        public void InvalidateCanvas()
        {
            WaveformDemonstratorCanvasControl?.Invalidate(); // 控件内部 CanvasControl 的重绘方法
        }

        private void InvalidateVisual()
        {
            LineDemonstratorCanvasControl?.Invalidate();
            WaveformDemonstratorCanvasControl?.Invalidate();
        }


        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 处理视觉相关依赖属性变更的回调方法。
        /// </summary>
        /// <param name="d">发生属性变更的依赖对象。</param>
        /// <param name="e">属性变更的事件参数，包含旧值和新值等信息。</param>
        /// <remarks>
        /// 此方法用于响应控件视觉属性（如缩放、中心点等）变化，自动更新控件显示状态和相关逻辑。
        /// 包括调试模块可见性、重绘、默认中心点跟随及重置按钮可见性等。
        /// </remarks>
        private static void OnVisualPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not WaveformView view) return;

            if (view.IsDebugMode) view.DebugModuleVisibitly = Visibility.Visible;
            else view.DebugModuleVisibitly = Visibility.Collapsed;

            view.InvalidateVisual();

            // 处理默认中心逻辑
            if (e.Property == MinHorizontalValueProperty ||
                e.Property == MaxHorizontalValueProperty ||
                e.Property == MinVerticalValueProperty ||
                e.Property == MaxVerticalValueProperty ||
                e.Property == ViewCenterPointModeProperty ||
                e.Property == ViewCenterPointProperty)
            {
                // 实时计算的默认中心
                Point newDefaultCenter = view.DefaultCenter;

                // 判断用户当前是否处于“默认视图”（未手动缩放且未平移）
                bool isDefaultZoom =
                    Math.Abs(view.HorizontalZoomScale - 1.0) < 0.0001 &&
                    Math.Abs(view.VerticalZoomScale - 1.0) < 0.0001;

                bool isDefaultCenter = view.RuntimeCenter.Equals(newDefaultCenter);

                if (isDefaultZoom && isDefaultCenter)
                {
                    // 用户当前就是默认视图 → 自动跟随新的默认中心（带动画）
                    view.RuntimeCenter = newDefaultCenter;
                }

                // 更新 Reset 按钮的可见性
                view.UpdateResetViewVisibility();
            }
        }








        private int GetActualRowTickCount(double height)
        {
            return GetActualTickCount(RowTickCount, height, MinRowTickHeight, MaxRowTickHeight,
                MinRowTickCount, MaxRowTickCount, RowAutoGridMode, RowAutoGridMultiple);
        }

        private int GetActualColumnTickCount(double width)
        {
            return GetActualTickCount(ColumnTickCount, width, MinColumnTickWidth, MaxColumnTickWidth,
                MinColumnTickCount, MaxColumnTickCount, ColumnAutoGridMode, ColumnAutoGridMultiple);
        }

        private int GetActualTickCount(string tickCountSetting, double length, double minTickSize, double maxTickSize, int minTickCount, int maxTickCount, AutoGridMode mode, int multiple)
        {
            if (string.Equals(tickCountSetting, "Auto", StringComparison.OrdinalIgnoreCase))
            {
                double clampedTickSize = Math.Clamp(minTickSize, 1.0, maxTickSize);
                int rawCount = (int)(length / clampedTickSize);
                int clampedCount = Math.Clamp(rawCount, minTickCount, maxTickCount);

                return mode switch
                {
                    AutoGridMode.Even => (clampedCount % 2 == 0) ? clampedCount : clampedCount + 1,
                    AutoGridMode.Odd => (clampedCount % 2 == 1) ? clampedCount : clampedCount + 1,
                    AutoGridMode.MultipleOfN => Math.Max(multiple, (clampedCount / multiple) * multiple),
                    AutoGridMode.PowerOfN => GetNearestPowerOfN(clampedCount, multiple),
                    _ => clampedCount
                };
            }

            if (int.TryParse(tickCountSetting, out int manualCount))
                return manualCount;

            return 8;
        }
        private int GetNearestPowerOfN(int target, int baseN)
        {
            if (baseN < 2) baseN = 2; // 最小底数为2
            int power = 1;
            while (power < target)
                power *= baseN;
            return power;
        }





    }
}
