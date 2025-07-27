using Microsoft.UI.Windowing;
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

namespace FSGaryityTool_Win11.Views.Pages.TestPage
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class StarWindow : Window
    {
        public StarWindow()
        {
            this.InitializeComponent();

            // 设置窗口大小
            this.AppWindow.Resize(new Windows.Graphics.SizeInt32(400, 800));
            this.AppWindow.Title = "Star Animation";

            // 设置窗口图标（可选）
            // this.AppWindow.SetIcon("Assets/Tiles/GalleryIcon.ico");

            // 设置标题栏主题（可选）
            // this.AppWindow.TitleBar.PreferredTheme = Microsoft.UI.Windowing.TitleBarTheme.UseDefaultAppMode;

            // 设置窗口最大/最小尺寸和行为
            var presenter = OverlappedPresenter.Create();
            presenter.PreferredMinimumWidth = 600;
            presenter.PreferredMinimumHeight = 400;
            presenter.PreferredMaximumWidth = 1200;
            presenter.PreferredMaximumHeight = 800;
            presenter.IsMaximizable = false;
            this.AppWindow.SetPresenter(presenter);
            MainFrame.Navigate(typeof(Star));
        }
    }
}
