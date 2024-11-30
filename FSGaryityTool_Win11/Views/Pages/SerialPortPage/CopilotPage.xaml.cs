using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FSGaryityTool_Win11.Views.Pages.SerialPortPage
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CopilotPage : Page
    {
        //https://edgeservices.bing.com/edgesvc/chat?udsframed=1&form=SHORUN&clientscopes=chat,noheader,udsedgeshop,channeldev,ntpquery,devtoolsapi,udsinwin11,udsdlpconsent,udsmrefresh,cspgrd,&shellsig=8d42ac22ff80ab54ae73c04e3f6e05b0a770f841&setlang=zh-CN&darkschemeovr=1&udsps=0&udspp=0
        private string uriCopilot = "https://edgeservices.bing.com/edgesvc/chat?udsframed=1&form=SHORUN" + "&clientscopes=chat,noheader,udsedgeshop,channeldev,ntpquery,devtoolsapi,udsinwin11,udsdlpconsent,udsmrefresh,cspgrd,";
        private string darkschemeovr = "&darkschemeovr=" + "1";
        private string launage = "&setlang=zh-CN";

        public CopilotPage()
        {
            InitializeComponent();

            Uri uri1 = new(uriCopilot + launage + darkschemeovr);
            Webview1.Source = uri1;
        }
        private void CopilotRefreshButton_Click(object sender, RoutedEventArgs e)
        {
            Uri uri = new("https://fairingstudio.com/");
            Webview1.Source = uri;
            uri = new(uriCopilot + launage + darkschemeovr);
            Webview1.Source = uri;
        }


    }
}
