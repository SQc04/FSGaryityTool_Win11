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
using static System.Net.Mime.MediaTypeNames;
using Windows.ApplicationModel;

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
        public static string redirectedFilePath;

        public static string appFilepath;
        public static string appFolderPath;

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

            string localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string packageName = Package.Current.Id.FamilyName;
            appFolderPath = Path.Combine(localAppDataPath, "Packages", packageName, "LocalCache", "Local");
            // 获取被重定向的文件夹路径
            //ApplicationData.Current.LocalFolder.Path
            string redirectedFolderPath = appFolderPath;
            string FSFolder = Path.Combine(redirectedFolderPath, "FAIRINGSTUDIO");
            string FSGravif = Path.Combine(FSFolder, "FSGravityTool");
            // 获取被重定向的文件路径
            appFilepath = FSGravif;
            redirectedFilePath = Path.Combine(FSGravif, "Settings.toml");


            using (StreamReader reader = File.OpenText(Page1.FSSetToml))        //打开TOML文件
            {
                TomlTable settingstomlr = TOML.Parse(reader);
                DefaultTomlSTARTPage = int.Parse(settingstomlr["FSGravitySettings"]["DefaultNvPage"]);
            }

            List<string> STARTPage = new List<string>()         //新建字符串
            {
                Page1.LanguageText("serialPort"), Page1.LanguageText("download Flash"), Page1.LanguageText("keyboard"), Page1.LanguageText("mouse")//, ""
            };
            StartPageCombobox.ItemsSource = STARTPage;          //将字符串添加到选择框
                                                                //读取TOML设置
            if (DefaultTomlSTARTPage == 0) DefaultSTARTPage = Page1.LanguageText("serialPort");
            else if (DefaultTomlSTARTPage == 1)  DefaultSTARTPage = Page1.LanguageText("download Flash");
            else if (DefaultTomlSTARTPage == 2) DefaultSTARTPage = Page1.LanguageText("keyboard");
            else if (DefaultTomlSTARTPage == 3) DefaultSTARTPage = Page1.LanguageText("mouse");
            //else if (DefaultTomlSTARTPage == 4) DefaultSTARTPage = "";


            StartPageCombobox.SelectedItem = DefaultSTARTPage;  //将TOML设置添加到选择框


            // = Page1.LanguageText("");
            StartPage.Header = Page1.LanguageText("defStartPage");
            StartPage.Description = Page1.LanguageText("defPageDescription");

            Generiall.Text = Page1.LanguageText("general");

            SpSwttingsl.Text = Page1.LanguageText("spSettings");
            SPTSettingsl.Header = Page1.LanguageText("spSettings");
            DowFlashl.Text = Page1.LanguageText("downloadFlashSettings");
            DFSettingsl.Header = Page1.LanguageText("downloadFlashSettings");

            Aboutl.Text = Page1.LanguageText("about");
            AbputTl.Text = Page1.LanguageText("about");

            OpenToml.Header = Page1.LanguageText("openToml");
            OpenToml.Description = Page1.LanguageText("openTomlDescription");

            SoftToolBackground.Header = Page1.LanguageText("softToolBackground");
            SoftToolBackground.Description = Page1.LanguageText("BackgroundDescription");

            SoftLanguage.Header = Page1.LanguageText("DefLanguage");
            SoftLanguage.Description = Page1.LanguageText("DefLanguageDescription");
        }

        private void Aboutp_Click(object sender, RoutedEventArgs e)
        {

            AboutFrame.Navigate(typeof(AboutPage));
            AboutFrame.Visibility = Visibility.Visible;

            MainSettingsPage.settingmPage.Settingsbar.ItemsSource = new ObservableCollection<Folder>
            {
                new Folder { Name = Page1.LanguageText("settings")},
                new Folder { Name = Page1.LanguageText("about")},
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

                if ((string)StartPageCombobox.SelectedItem == Page1.LanguageText("serialPort")) settingstomlr["FSGravitySettings"]["DefaultNvPage"] = "0";
                else if ((string)StartPageCombobox.SelectedItem == Page1.LanguageText("download Flash")) settingstomlr["FSGravitySettings"]["DefaultNvPage"] = "1";
                else if ((string)StartPageCombobox.SelectedItem == Page1.LanguageText("keyboard")) settingstomlr["FSGravitySettings"]["DefaultNvPage"] = "2";
                else if ((string)StartPageCombobox.SelectedItem == Page1.LanguageText("mouse")) settingstomlr["FSGravitySettings"]["DefaultNvPage"] = "3";
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

        private void OpenToml_click(object sender, RoutedEventArgs e)
        {
            //Debug.WriteLine(appFolderPath);
            Debug.WriteLine(appFilepath);
            if (Directory.Exists(appFilepath))
            {
                //Debug.WriteLine("找到文件夹,跳过新建文件夹");
                System.Diagnostics.Process.Start("explorer.exe", redirectedFilePath);
            }
            else
            {
                System.Diagnostics.Process.Start("explorer.exe", Page1.FSSetToml);
                //Debug.WriteLine("没有找到文件夹");
            }
            //System.Diagnostics.Process.Start("explorer.exe", Page1.FSSetToml);
            //System.Diagnostics.Process.Start("explorer.exe", redirectedFilePath);
            
        }

        private void SoftBackgroundCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void SoftLanguageCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
