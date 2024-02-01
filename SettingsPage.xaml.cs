using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using static FSGaryityTool_Win11.SettingsPage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FSGaryityTool_Win11
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        public class Folder
        {
            public string Name { get; set; }
        }


        public SettingsPage()
        {
            this.InitializeComponent();


            Settingsbar.ItemsSource = new ObservableCollection<Folder>
            {
                new Folder { Name = "Settings"},
            };
            Settingsbar.ItemClicked += Settingsbar_ItemClicked;


        }

        private void Aboutp_Click(object sender, RoutedEventArgs e)
        {

            AboutFrame.Navigate(typeof(AboutPage));
            AboutFrame.Visibility = Visibility.Visible;
            Settingsbar.ItemsSource = new ObservableCollection<Folder>
            {
                new Folder { Name = "Settings"},
                new Folder { Name = "About" },
            };
            AboutFrame.Opacity = 1;
            AboutINOUT.Begin();
            Settingsbar.ItemClicked += Settingsbar_ItemClicked;
        }

        private void Settingsbar_ItemClicked(BreadcrumbBar sender, BreadcrumbBarItemClickedEventArgs args)
        {
            var items = Settingsbar.ItemsSource as ObservableCollection<Folder>;
            for (int i = items.Count - 1; i >= args.Index + 1; i--)
            {
                items.RemoveAt(i);
            }
            AboutFrame.Opacity = 0;
            AboutFrame.Visibility = Visibility.Collapsed;
        }






    }
}
