using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using System;

namespace FSGaryityTool_Win11.Controls;

public class PaneWidthConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var splitView = (SplitView)value;
        var width = splitView.IsPaneOpen ? splitView.OpenPaneLength : splitView.CompactPaneLength;
        System.Diagnostics.Debug.WriteLine($"IsPaneOpen: {splitView.IsPaneOpen}, Width: {width}");
        return width;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
