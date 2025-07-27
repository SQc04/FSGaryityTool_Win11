using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Numerics;

namespace FSGaryityTool_Win11.Controls
{
    public class SliderValueToVector3Converter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value switch
            {
                StopBits.One => new Vector3(1f, 1f, 1f),
                StopBits.OnePointFive => new Vector3(1.5f, 1f, 1f),
                StopBits.Two => new Vector3(2f, 1f, 1f),
                double d when d == 1.0 => new Vector3(1f, 1f, 1f),
                double d when d == 1.5 => new Vector3(1.5f, 1f, 1f),
                double d when d == 2.0 => new Vector3(2f, 1f, 1f),
                float f when f == 1.0f => new Vector3(1f, 1f, 1f),
                float f when f == 1.5f => new Vector3(1.5f, 1f, 1f),
                float f when f == 2.0f => new Vector3(2f, 1f, 1f),
                string s when s == "1.0" => new Vector3(1f, 1f, 1f),
                string s when s == "1.5" => new Vector3(1.5f, 1f, 1f),
                string s when s == "2.0" => new Vector3(2f, 1f, 1f),
                _ => new Vector3(1f, 1f, 1f)
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
