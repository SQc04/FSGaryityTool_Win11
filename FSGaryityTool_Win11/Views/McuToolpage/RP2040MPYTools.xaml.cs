using FSGaryityTool_Win11.Views.Pages.SerialPortPage;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using static FSGaryityTool_Win11.Views.Pages.SerialPortPage.MainPage1;

namespace FSGaryityTool_Win11.McuToolpage;

public sealed partial class RP2040MPYTools : Page
{
    public RP2040MPYTools()
    {
        InitializeComponent();
    }

    private void RSTButton_Click(object sender, RoutedEventArgs e)
    {
        if (SerialPortToolsPage.PortIsConnect is 1)
        {
            var rsttext = "machine.reset()";
            Page1.CommonRes.SerialPort.Write(rsttext + "\r\n");
            Task.Run(() =>
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    Current.SerialPortConnectToggleButton_Click(null, null);
                });
                Thread.Sleep(4000); 
                DispatcherQueue.TryEnqueue(() =>
                {
                    Current.SerialPortConnectToggleButton_Click(null, null);
                });
            });
            Debug.WriteLine(rsttext);
        }
    }

    private void SoftRSTButton_Click(object sender, RoutedEventArgs e)
    {
        if (SerialPortToolsPage.PortIsConnect is 1)
        {
            var rsttext = "machine.soft_reset()";
            Page1.CommonRes.SerialPort.Write(rsttext + "\r\n");
            Debug.WriteLine(rsttext);
        }
    }
}
