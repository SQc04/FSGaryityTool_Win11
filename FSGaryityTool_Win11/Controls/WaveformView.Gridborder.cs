using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSGaryityTool_Win11.Controls
{
    public partial class WaveformView
    {
        public struct GridBounds
        {
            public float Left { get; set; }
            public float Top { get; set; }
            public float Right { get; set; }
            public float Bottom { get; set; }

            public float Width => Right - Left;
            public float Height => Bottom - Top;

            public float CenterX => (Left + Right) / 2f;
            public float CenterY => (Top + Bottom) / 2f;

            public GridBounds(float left, float top, float right, float bottom)
            {
                Left = left;
                Top = top;
                Right = right;
                Bottom = bottom;
            }

            public bool Contains(float x, float y)
            {
                return x >= Left && x <= Right && y >= Top && y <= Bottom;
            }

            public override string ToString()
            {
                return $"GridBounds({Left}, {Top}, {Right}, {Bottom})";
            }
        }
        private struct TickColors
        {
            public Windows.UI.Color BorderColor, RowColor, ColColor, RowCenterColor, ColCenterColor;
        }

        private GridBounds GetGridBounds(double width, double height)
        {
            var margin = WaveGridBorderMargin;
            return new GridBounds
            {
                Left = (float)margin.Left,
                Top = (float)margin.Top,
                Right = (float)(width - margin.Right),
                Bottom = (float)(height - margin.Bottom)
            };
        }

        private List<float> GetTickPositions(float start, float length, int count)
        {
            var positions = new List<float>();
            for (int i = 1; i < count; i++)
                positions.Add(start + length * i / count);
            return positions;
        }

        private int GetCenterIndex(int count) => (count % 2 == 0) ? (count / 2 - 1) : -1;

        private TickColors GetTickColors()
        {
            return new TickColors
            {
                BorderColor = GetBrushColor(WaveGridBorderBrush, GetThemeColor("TextFillColorTertiaryBrush", Microsoft.UI.Colors.Gray)),
                RowColor = GetBrushColor(RowTickLineBrush, GetThemeColor("TextFillColorTertiaryBrush", Microsoft.UI.Colors.Gray)),
                ColColor = GetBrushColor(ColumnTickLineBrush, GetThemeColor("TextFillColorTertiaryBrush", Microsoft.UI.Colors.Gray)),
                RowCenterColor = GetBrushColor(RowCenterTickLineBrush, GetThemeColor("AccentTextFillColorPrimaryBrush", Microsoft.UI.Colors.DeepSkyBlue)),
                ColCenterColor = GetBrushColor(ColumnCenterTickLineBrush, GetThemeColor("AccentTextFillColorPrimaryBrush", Microsoft.UI.Colors.DeepSkyBlue))
            };
        }
        private Windows.UI.Color GetBrushColor(Brush brush, Windows.UI.Color fallback)
        {
            if (brush is SolidColorBrush solid)
                return solid.Color;

            return fallback;
        }

        // 默认版本使用系统主题色
        private Windows.UI.Color GetBrushColor(Brush brush)
        {
            return GetBrushColor(brush, GetThemeColor("AccentTextFillColorTertiaryBrush", Microsoft.UI.Colors.DeepSkyBlue));
        }

        private Windows.UI.Color GetThemeColor(string resourceKey, Windows.UI.Color fallback)
        {
            if (Application.Current.Resources.TryGetValue(resourceKey, out var brushObj) && brushObj is SolidColorBrush brush)
                return brush.Color;
            return fallback;
        }
    }
}
