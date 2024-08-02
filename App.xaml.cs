using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FSGaryityTool_Win11
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
        }

        public IntPtr MainWindowHandle { get; private set; }
        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            if (m_window == null)
            {
                m_window = new MainWindow();
                MainWindowHandle = WinRT.Interop.WindowNative.GetWindowHandle(m_window);
            }

            // 只有在 m_window.Content 为 null 时才创建 ExtendedSplash
            if (m_window.Content == null)
            {
                // 创建 ExtendedSplash 实例
                ExtendedSplash extendedSplash = new ExtendedSplash(m_window);
                m_window.Content = extendedSplash;
            }

            // 确保当前窗口处于活动状态
            m_window.Activate();

        }


        public static Window m_window;
        public void RemoveExtendedSplash(UIElement mainContent)
        {
            if (m_window.Content is ExtendedSplash)
            {
                // 将 m_window.Content 设置为 MainWindow 的内容
                m_window.Content = mainContent;
            }
        }

    }
}
