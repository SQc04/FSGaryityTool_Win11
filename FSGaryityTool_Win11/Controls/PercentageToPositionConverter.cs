using Microsoft.UI.Xaml.Data;
using System;

namespace FSGaryityTool_Win11.Controls;

public class PercentageToPositionConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is double percentage && parameter is double totalLength)
        {
            return percentage * totalLength;
        }
        return 0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
