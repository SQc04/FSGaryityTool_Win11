using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Tommy;
using System.Diagnostics;
using Windows.System;
using FSGaryityTool_Win11.Controls;
using FSGaryityTool_Win11.Core.Settings;
using Microsoft.UI.Xaml.Media.Imaging;

namespace FSGaryityTool_Win11;

public sealed partial class SettingsPage : Page
{
    public static TomlTable SettingsTomlr { get; set; }

    public static string DefaultStartPage { get; set; }

    public static int DefaultTomlStartPage { get; set; }

    public static string DefaultPageBackGround { get; set; }

    public static int DefaultTomlPageBackGround { get; set; }

    public class Folder
    {
        public string Name { get; set; }
    }

    public static SettingsPage Current { get; private set; }

    public SettingsPage()
    {
        InitializeComponent();
        Current = this;

        LanguageSetting();

        SetDesktopBackgroundImage();
        var listener = new WallpaperChangeListener();
        listener.WallpaperChanged += (_, _) => SetDesktopBackgroundImage();
    }

    public void LanguageSetting()
    {
        using var reader = File.OpenText(SettingsCoreServices.FsGravityToolsSettingsToml);
        var settingstomlr = TOML.Parse(reader);
        DefaultTomlStartPage = int.Parse(settingstomlr["FSGravitySettings"]["DefaultNvPage"]);
        DefaultTomlPageBackGround = int.Parse(settingstomlr["FSGravitySettings"]["SoftBackground"]);

        var startPage = new List<string>() //新建字符串
        {
            SettingsPageResources.SPort,
            SettingsPageResources.DFlash,
            SettingsPageResources.Keyboard,
            SettingsPageResources.Mouse,
            "FANControl",
            "CameraControl" //, ""
        };
        StartPageCombobox.ItemsSource = startPage;          //将字符串添加到选择框
        if (DefaultTomlStartPage is > -1 and < 6)
            StartPageCombobox.SelectedItem = DefaultStartPage = startPage[DefaultTomlStartPage];

        var pageBackGround = new List<string>() //新建字符串
        {
            SettingsPageResources.Acrylic,
            SettingsPageResources.Mica,
            SettingsPageResources.MicaAlt
        };
        SoftBackgroundCombobox.ItemsSource = pageBackGround;
        if (DefaultTomlPageBackGround is > -1 and < 4)
            SoftBackgroundCombobox.SelectedItem = DefaultPageBackGround = pageBackGround[DefaultTomlPageBackGround];
    }

    private void Aboutp_Click(object sender, RoutedEventArgs e)
    {
        AboutFrame.Navigate(typeof(AboutPage));

        MainSettingsPage.Current.SettingsBar.ItemsSource = new ObservableCollection<Folder>
        {
            new() { Name = SettingsPageResources.Settings },
            new() { Name = SettingsPageResources.AboutHeader }
        };
        //AboutFrame.Opacity = 1;
        //AboutINOUT.Begin();
        MainSettingsPage.Current.SettingsFrame.Navigate(typeof(AboutPage), null, new SlideNavigationTransitionInfo { Effect = SlideNavigationTransitionEffect.FromRight });
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
        if(fro1 is 0)
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
        using (var reader = File.OpenText(SettingsCoreServices.FsGravityToolsSettingsToml))
        {
            SettingsTomlr = TOML.Parse(reader);

            if (StartPageCombobox.SelectedIndex is > -1 and < 6)
                SettingsTomlr["FSGravitySettings"]["DefaultNvPage"] = StartPageCombobox.SelectedIndex.ToString();
        }
        using var writer = File.CreateText(SettingsCoreServices.FsGravityToolsSettingsToml);
        SettingsTomlr.WriteTo(writer);
        Debug.WriteLine("写入Toml" + SettingsTomlr["FSGravitySettings"]["DefaultNvPage"]);
        // Remember to flush the data if needed!
        writer.Flush();
    }

    private async void OpenToml_Click(object sender, RoutedEventArgs e)
    {
        //Debug.WriteLine(appFolderPath);
        Debug.WriteLine(SettingsCoreServices.FsGravityToolsFolder);
        //Debug.WriteLine("找到文件夹,跳过新建文件夹");
        await Launcher.LaunchUriAsync(new(SettingsCoreServices.FsGravityToolsSettingsToml));
        //Debug.WriteLine("没有找到文件夹");
        //System.Diagnostics.Process.Start("explorer.exe", Page1.FSSetToml);
        //System.Diagnostics.Process.Start("explorer.exe", redirectedFilePath);
    }

    private void SoftBackgroundCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // 打开TOML文件
        if (StartPageCombobox.SelectedIndex is <= -1 or >= 4)
            return;

        using (var reader = File.OpenText(SettingsCoreServices.FsGravityToolsSettingsToml))
        {
            SettingsTomlr = TOML.Parse(reader);

            SettingsTomlr["FSGravitySettings"]["SoftBackground"] = SoftBackgroundCombobox.SelectedIndex.ToString();
        }

        // 将设置写入TOML文件
        using (var writer = File.CreateText(SettingsCoreServices.FsGravityToolsSettingsToml))
        {
            SettingsTomlr.WriteTo(writer);
            Debug.WriteLine("写入Toml" + SettingsTomlr["FSGravitySettings"]["SoftBackground"]);
            // Remember to flush the data if needed!
            writer.Flush();
        }

        if (e.RemovedItems is not [])
            // 更新窗口的背景
            MainWindow.Instance.WindowBackSetting();
    }

    private void SetDesktopBackgroundImage()
    {
        var wallpaperPath = WallpaperHelper.GetWallpaperPath();
        if (!string.IsNullOrEmpty(wallpaperPath))
        {
            var bitmapImage = new BitmapImage(new(wallpaperPath));
            DesktopBackgroundImage.Source = bitmapImage;
        }
    }

    private void SoftLanguageCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
    }
}
