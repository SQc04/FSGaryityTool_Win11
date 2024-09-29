using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace FSGaryityTool_Win11.McuToolpage;

public sealed partial class ESP8266Tools : Page
{
    public ESP8266Tools()
    {
        InitializeComponent();
    }

    private void RSTButton_Click(object sender, RoutedEventArgs e)      //自动重启
    {

        Page1.RstButtonRes();
    }
}
