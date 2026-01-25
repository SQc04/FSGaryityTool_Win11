using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSGaryityTool_Win11.Controls
{
    public enum LineStyle
    {
        None,
        Dot,
        Dash,
        Solid,
    }
    public enum TickModeStyle
    {
        None,
        Intersection,
        Cross
    }
    public enum AutoGridMode
    {
        Integer,
        Even,
        Odd,
        MultipleOfN,
        PowerOfN
    }

    public enum WaveformZoomMode
    {
        Disabled,
        Horizontal,
        Vertical,
        Both
    }
    public enum ViewScrollBarVisibility
    {
        Collapsed,
        Visible,
        ViewBar,
        Bar,
        Auto
    }


    public enum WaveformScrollingMode
    {
        Disabled,
        Scroll,
        Looping
    }
    public enum ViewScrollMode
    {
        Disabled,
        Enabled,
        Auto
    }
    public enum WaveformScrollingAxis
    {
        Horizontal,
        Vertical,
        Both
    }

    public enum CrosshairCursorMode
    {
        Disabled,
        HorizontalOnly,
        VerticalOnly,
        Full
    }

    public enum ViewLabelMode
    {
        Enable,
        Disabled
    }


    public enum IndicatorMode
    {
        None,
        Mouse,
        HorizontalData,
        VerticalData,
        ClosestData
    }

    public enum ViewCenterMode
    {
        LeftTop,
        Top,
        RightTop,
        Left,
        Center,
        Right,
        LeftBottom,
        Bottom,
        RightBottom,
        ManualPoint,
        Auto
    }
    public enum SnappingAxis
    {
        Closest,
        Horizontal,
        Vertical
    }
    public enum DrawMode
    {
        WinCanvas,
        BranchPrediction
    }
}
