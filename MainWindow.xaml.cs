using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.InteropServices;
using Windows.Foundation;
using Windows.Foundation.Collections;


using System.Text;
using Windows.Networking.Sockets;
using System.Collections.ObjectModel;
using Microsoft.UI.Composition.SystemBackdrops;

using Windows.UI.Popups;
using System.Threading;

using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams;
using System.Threading.Tasks;
using System.IO.Ports;
using Microsoft.UI;           // Needed for WindowId.
using Microsoft.UI.Windowing; // Needed for AppWindow.
using WinRT;
using WinRT.Interop;
using Windows.UI;          // Needed for XAML/HWND interop.
using Windows.ApplicationModel.Core;
using Windows.UI.ViewManagement;
using Windows.ApplicationModel;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FSGaryityTool_Win11
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {


        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            var selectedItem = (NavigationViewItem)args.SelectedItem;
            if ((string)selectedItem.Tag == "MainPage1") FSnvf.Navigate(typeof(MainPage1));
            else if ((string)selectedItem.Tag == "Page2") FSnvf.Navigate(typeof(Page2));
            else if ((string)selectedItem.Tag == "Page3") FSnvf.Navigate(typeof(Page3));

            if (args.IsSettingsSelected)
            {
                FSnvf.Navigate(typeof(MainSettingsPage));
            }
        }


        private AppWindow m_AppWindow;



        public NavigationFailedEventHandler OnNavigationFailed { get; private set; }

        public MainWindow()
        {
            this.InitializeComponent();

            // 将窗口的标题栏设置为自定义标题栏
            this.ExtendsContentIntoTitleBar = true;
            
            m_AppWindow = GetAppWindowForCurrentWindow();
            m_AppWindow.Title = "FSGravityTool";//Set AppWindow
            m_AppWindow.SetIcon("FSsoftH.ico");
            

            FSnv.SelectedItem = FSnv.MenuItems[0];

            /*
            if (AppWindowTitleBar.IsCustomizationSupported())
            {
                AppWindowTitleBar m_TitleBar = m_AppWindow.TitleBar;

                // Set active window colors.
                // Note: No effect when app is running on Windows 10
                // because color customization is not supported.
                m_TitleBar.ForegroundColor = Color.FromArgb(255, 255, 255, 255);
                m_TitleBar.BackgroundColor = Color.FromArgb(255, 22, 22, 22);
                m_TitleBar.ButtonForegroundColor = Color.FromArgb(255, 255, 255, 255);
                m_TitleBar.ButtonBackgroundColor = Color.FromArgb(255, 22, 22, 22);
                m_TitleBar.ButtonHoverForegroundColor = Color.FromArgb(255, 0, 0, 0);
                m_TitleBar.ButtonHoverBackgroundColor = Color.FromArgb(255, 230, 224, 0);
                m_TitleBar.ButtonPressedForegroundColor = Color.FromArgb(255, 0, 0, 0);
                m_TitleBar.ButtonPressedBackgroundColor = Color.FromArgb(255, 230, 224, 0);

                // Set inactive window colors.
                // Note: No effect when app is running on Windows 10
                // because color customization is not supported.
                m_TitleBar.InactiveForegroundColor = Colors.Gainsboro;
                m_TitleBar.InactiveBackgroundColor = Color.FromArgb(255, 22, 22, 22);
                m_TitleBar.ButtonInactiveForegroundColor = Colors.Gainsboro;
                m_TitleBar.ButtonInactiveBackgroundColor = Color.FromArgb(255, 22, 22, 22);

                
            }*/

            TitleBarTextBlock.Text = "FSGravityTool";
            //SystemBackdrop = new MicaBackdrop()
            //{ Kind = MicaKind.BaseAlt };
            SystemBackdrop = new DesktopAcrylicBackdrop();


        }

        // Call your extend acrylic code in the OnLaunched event, after
        // calling Window.Current.Activate.



        private AppWindow GetAppWindowForCurrentWindow()
        {
            IntPtr hWnd = WindowNative.GetWindowHandle(this);
            WindowId wndId = Win32Interop.GetWindowIdFromWindow(hWnd);
            return AppWindow.GetFromWindowId(wndId);
        }

        /*
        
        public class Tab1
        {
            private SerialPort serialPort1;

            public Tab1()
            {
                this.serialPort1 = new SerialPort();
                // 配置并打开serialPort1
            }

            // 使用serialPort1进行通信
        }

        public class Tab2
        {
            private SerialPort serialPort2;

            public Tab2()
            {
                this.serialPort2 = new SerialPort();
                // 配置并打开serialPort2
            }

            // 使用serialPort2进行通信
        }
        */

    }

}
