using FSGaryityTool_Win11.Views.Pages.SerialPortPage;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using static FSGaryityTool_Win11.Views.Pages.SerialPortPage.MainPage1;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FSGaryityTool_Win11.McuToolpage
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class RP2040MPYTools : Page
    {
        public RP2040MPYTools()
        {
            this.InitializeComponent();
        }

        private void RSTButton_Click(object sender, RoutedEventArgs e)
        {
            if (SerialPortToolsPage.portIsConnect == 1)
            {
                string rsttext = "machine.reset()";
                Page1.CommonRes._serialPort.Write(rsttext + "\r\n");
                Task.Run(() =>
                {
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        mainPage1.SerialPortConnectToggleButton_Click(null, null);
                    });
                    Thread.Sleep(4000); 
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        mainPage1.SerialPortConnectToggleButton_Click(null, null);
                    });
                });
                Debug.WriteLine(rsttext);
            }
        }

        private void SoftRSTButton_Click(object sender, RoutedEventArgs e)
        {
            if (SerialPortToolsPage.portIsConnect == 1)
            {
                string rsttext = "machine.soft_reset()";
                Page1.CommonRes._serialPort.Write(rsttext + "\r\n");
                Debug.WriteLine(rsttext);
            }
        }
    }
}
