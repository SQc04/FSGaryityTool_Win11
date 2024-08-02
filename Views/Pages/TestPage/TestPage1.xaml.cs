using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System.Net.NetworkInformation;
using FSGaryityTool_Win11.Controls;



// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FSGaryityTool_Win11.Views.Pages.TestPage
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TestPage1 : Page
    {
        private DispatcherTimer timer;
        public TestPage1()
        {
            this.InitializeComponent();

            // 初始化定时器
            //timer = new DispatcherTimer();
            //timer.Interval = TimeSpan.FromMilliseconds(100);
            //timer.Tick += Timer_Tick;
            //timer.Start();
        }

        private void Timer_Tick(object sender, object e)
        {
            //TestCustomTextBox.Text += "Test\n";
        }
    }

}
