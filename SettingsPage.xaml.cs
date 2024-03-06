using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
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
using System.Numerics;
using Tommy;
using System.Diagnostics;
using System.Reflection.PortableExecutable;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FSGaryityTool_Win11
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        public static int fro1 = 0;
        public static TomlTable settingstomlr;

        public static string DefaultSTARTPage;
        public static int DefaultTomlSTARTPage;

        public class Folder
        {
            public string Name { get; set; }
        }

        public static SettingsPage settingPage;

        public SettingsPage()
        {
            this.InitializeComponent();
            settingPage = this;
            /*
            Settingsbar.ItemsSource = new ObservableCollection<Folder>
            {
                new Folder { Name = "Settings"},
            };
            Settingsbar.ItemClicked += Settingsbar_ItemClicked;
            */

            
            

            using (StreamReader reader = File.OpenText(Page1.FSSetToml))        //打开TOML文件
            {
                TomlTable settingstomlr = TOML.Parse(reader);
                DefaultTomlSTARTPage = int.Parse(settingstomlr["FSGravitySettings"]["DefaultNvPage"]);
            }

            List<string> STARTPage = new List<string>()         //新建字符串
            {
                "Serial Port", "Download Flash", "Keyboard", "Mouse"//, ""
            };
            StartPageCombobox.ItemsSource = STARTPage;          //将字符串添加到选择框
                                                                //读取TOML设置
            if (DefaultTomlSTARTPage == 0) DefaultSTARTPage = "Serial Port";
            else if (DefaultTomlSTARTPage == 1)  DefaultSTARTPage = "Download Flash";
            else if (DefaultTomlSTARTPage == 2) DefaultSTARTPage = "Keyboard";
            else if (DefaultTomlSTARTPage == 3) DefaultSTARTPage = "Mouse";
            //else if (DefaultTomlSTARTPage == 4) DefaultSTARTPage = "";


            StartPageCombobox.SelectedItem = DefaultSTARTPage;  //将TOML设置添加到选择框

        }

        private void Aboutp_Click(object sender, RoutedEventArgs e)
        {

            AboutFrame.Navigate(typeof(AboutPage));
            AboutFrame.Visibility = Visibility.Visible;

            MainSettingsPage.settingmPage.Settingsbar.ItemsSource = new ObservableCollection<Folder>
            {
                new Folder { Name = "Settings"},
                new Folder { Name = "About" },
            };
            //AboutFrame.Opacity = 1;
            //AboutINOUT.Begin();
            MainSettingsPage.settingmPage.SettingsFrame.Navigate(typeof(AboutPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
            //AboutFrame.Navigate(typeof(AboutPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
        }
        /*
        private void Settingsbar_ItemClicked(BreadcrumbBar sender, BreadcrumbBarItemClickedEventArgs args)
        {

            var items = Settingsbar.ItemsSource as ObservableCollection<Folder>;
            for (int i = items.Count - 1; i >= args.Index + 1; i--)
            {
                items.RemoveAt(i);
            }
            //AboutFrame.Opacity = 0;
            //AboutFrame.Visibility = Visibility.Collapsed;
        }
        */

        /*
        private void Aboutq_Click(object sender, RoutedEventArgs e)
        {
            if(fro1 == 0)
            {
                Fro1.Rotation = 180;
                Fro1.Translation = new Vector3(12, 20, 12);

                fro1 = 1;
            }
            else
            {
                Fro1.Rotation = 0;
                Fro1.Translation = new Vector3(0, 0, 0);

                fro1 = 0;
            }
        }
        */

        private void SPSettings_Click(object sender, RoutedEventArgs e)
        {

        }

        private void StartPageCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            using (StreamReader reader = File.OpenText(Page1.FSSetToml))                    //打开TOML文件
            {
                settingstomlr = TOML.Parse(reader);

                if ((string)StartPageCombobox.SelectedItem == "Serial Port") settingstomlr["FSGravitySettings"]["DefaultNvPage"] = "0";
                else if ((string)StartPageCombobox.SelectedItem == "Download Flash") settingstomlr["FSGravitySettings"]["DefaultNvPage"] = "1";
                else if ((string)StartPageCombobox.SelectedItem == "Keyboard") settingstomlr["FSGravitySettings"]["DefaultNvPage"] = "2";
                else if ((string)StartPageCombobox.SelectedItem == "Mouse") settingstomlr["FSGravitySettings"]["DefaultNvPage"] = "3";
                //else if ((string)StartPageCombobox.SelectedItem == "") settingstomlr["FSGravitySettings"]["DefaultNvPage"] = "4";



            }

            using (StreamWriter writer = File.CreateText(Page1.FSSetToml))                  //将设置写入TOML文件
            {
                settingstomlr.WriteTo(writer);
                Debug.WriteLine("写入Toml" + settingstomlr["FSGravitySettings"]["DefaultNvPage"]);
                // Remember to flush the data if needed!
                writer.Flush();
            }
        }
    }
}
