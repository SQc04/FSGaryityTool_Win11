using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Tommy;
using System.Diagnostics;
using Windows.ApplicationModel;
using FSGaryityTool_Win11.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using FSGaryityTool_Win11.Core.Settings;
using System.ComponentModel;
using Microsoft.UI.Xaml.Media;

namespace FSGaryityTool_Win11;

public sealed partial class SettingsPage : Page, INotifyPropertyChanged
{
    public static int Fro1 { get; set; }

    public static TomlTable SettingsTomlr { get; set; }

    public static string DefaultStartPage { get; set; }

    public static int DefaultTomlStartPage { get; set; }

    public static string DefaultPageBackGround { get; set; }
    public static bool DefaultSoftBackgroundToggleSwitch { get; set; }
    public static int DefaultTomlPageBackGround { get; set; }

    public static string RedirectedFilePath { get; set; }

    public static string AppFilepath { get; set; }

    public static string AppFolderPath { get; set; }

    private WallpaperChangeListener _wallpaperChangeListener = new();
    public class Folder
    {
        public string Name { get; set; }
    }

    public static SettingsPage Current { get; private set; }

    public event PropertyChangedEventHandler PropertyChanged;

    private bool _windowBackgroundBrushActivatedEnable;
    public bool WindowBackgroundBrushActivatedEnable
    {
        get => _windowBackgroundBrushActivatedEnable;
        set
        {
            if (_windowBackgroundBrushActivatedEnable != value)
            {
                _windowBackgroundBrushActivatedEnable = value;
                OnPropertyChanged(nameof(WindowBackgroundBrushActivatedEnable));
                WindowBackgroundBrushControl.WindowBackgroundBrushActivatedEnable = value; // 同步到 WindowBackgroundBrushControl
                SettingsCoreServices.SetSoftBackgroundActivatedEnableSetting(value.ToString());
            }
        }
    }


    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public SettingsPage()
    {
        InitializeComponent();
        Current = this;
        /*
        Settingsbar.ItemsSource = new ObservableCollection<Folder>
        {
            new Folder { Name = "Settings"},
        };
        Settingsbar.ItemClicked += Settingsbar_ItemClicked;
        */

        LanguageSetting();

        SetDesktopBackground();
        //_wallpaperChangeListener.WallpaperChanged += (s, e) => SetDesktopBackgroundImage();
    }

    public void LanguageSetting()
    {
        var localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var packageName = string.Empty;
        try
        {
            packageName = Package.Current.Id.FamilyName;
        }
        catch (Exception ex)
        {
            // 处理异常，例如记录日志或显示错误消息
            Debug.WriteLine($"获取包名时发生错误: {ex.Message}");
        }

        AppFolderPath = Path.Combine(localAppDataPath, "Packages", packageName, "LocalCache", "Local");
        // 获取被重定向的文件夹路径
        //ApplicationData.Current.LocalFolder.Path
        var redirectedFolderPath = AppFolderPath;
        var fsFolder = Path.Combine(redirectedFolderPath, "FAIRINGSTUDIO");
        var fsGravif = Path.Combine(fsFolder, "FSGravityTool");
        // 获取被重定向的文件路径
        AppFilepath = fsGravif;
        RedirectedFilePath = Path.Combine(fsGravif, "Settings.toml");

        using (var reader = File.OpenText(Page1.FsSetToml))        //打开TOML文件
        {
            var settingstomlr = TOML.Parse(reader);
            DefaultTomlStartPage = int.Parse(settingstomlr["FSGravitySettings"]["DefaultNvPage"]);
            DefaultTomlPageBackGround = int.Parse(settingstomlr["FSGravitySettings"]["SoftBackground"]);
        }

        var startPage = new List<string>()         //新建字符串
        {
            Page1.LanguageText("serialPort"), Page1.LanguageText("download Flash"), Page1.LanguageText("keyboard"), Page1.LanguageText("mouse"), "FANControl", "CameraControl"//, ""
        };
        StartPageCombobox.ItemsSource = startPage;          //将字符串添加到选择框
        DefaultStartPage = DefaultTomlStartPage switch
        {
            //读取TOML设置
            0 => Page1.LanguageText("serialPort"),
            1 => Page1.LanguageText("download Flash"),
            2 => Page1.LanguageText("keyboard"),
            3 => Page1.LanguageText("mouse"),
            4 => "FANControl",
            5 => "CameraControl",
            _ => DefaultStartPage
        };
        //else if (DefaultTomlSTARTPage is 5) DefaultSTARTPage = "";
        StartPageCombobox.SelectedItem = DefaultStartPage;  //将TOML设置添加到选择框

        var pageBackGround = new List<string>()         //新建字符串
        {
            Page1.LanguageText("default"),
            Page1.LanguageText("thin"),
            Page1.LanguageText("defaultDesktopAcrylicBackGround"),
            Page1.LanguageText("base"), 
            Page1.LanguageText("mica"), 
            Page1.LanguageText("micaAlt"),
            //Page1.LanguageText("transparent"),
            //, ""
        };
        SoftBackgroundCombobox.ItemsSource = pageBackGround;
        DefaultPageBackGround = DefaultTomlPageBackGround switch
        {
            0 => Page1.LanguageText("thin"),
            1 => Page1.LanguageText("base"),
            2 => Page1.LanguageText("mica"),
            3 => Page1.LanguageText("micaAlt"),
            4 => Page1.LanguageText("defaultDesktopAcrylicBackGround"),
            5 => Page1.LanguageText("transparent"),
            6 => Page1.LanguageText("default"),
            _ => DefaultPageBackGround
        };

        SoftBackgroundCombobox.SelectedItem = DefaultPageBackGround;  //将TOML设置添加到选择框

        DefaultSoftBackgroundToggleSwitch = bool.Parse(SettingsCoreServices.GetSoftBackgroundActivatedEnableSetting());

        _windowBackgroundBrushActivatedEnable = DefaultSoftBackgroundToggleSwitch;

        // = Page1.LanguageText("");
        StartPage.Header = Page1.LanguageText("defStartPage");
        StartPage.Description = Page1.LanguageText("defPageDescription");

        Generiall.Text = Page1.LanguageText("general");

        SpSwttingsl.Text = Page1.LanguageText("spSettings");
        SptSettingsl.Header = Page1.LanguageText("spSettings");
        DowFlashl.Text = Page1.LanguageText("downloadFlashSettings");
        DfSettingsl.Header = Page1.LanguageText("downloadFlashSettings");

        Aboutl.Text = Page1.LanguageText("about");
        AbputTl.Text = Page1.LanguageText("about");

        OpenToml.Header = Page1.LanguageText("openToml");
        OpenToml.Description = Page1.LanguageText("openTomlDescription");

        SoftToolBackground.Header = Page1.LanguageText("softTranslucentToolBackground");
        SoftToolBackground.Description = Page1.LanguageText("TranslucentBackgroundDescription");

        SoftLanguage.Header = Page1.LanguageText("DefLanguage");
        SoftLanguage.Description = Page1.LanguageText("DefLanguageDescription");
    }

    private void Aboutp_Click(object sender, RoutedEventArgs e)
    {

        AboutFrame.Navigate(typeof(AboutPage));
        AboutFrame.Visibility = Visibility.Visible;

        MainSettingsPage.Current.SettingsBar.ItemsSource = new ObservableCollection<Folder>
        {
            new() { Name = Page1.LanguageText("settings")},
            new() { Name = Page1.LanguageText("about")},
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
        string StartPageSetting = (string)StartPageCombobox.SelectedItem switch
        {
            var item when item == Page1.LanguageText("serialPort") => "0",
            var item when item == Page1.LanguageText("download Flash") => "1",
            var item when item == Page1.LanguageText("keyboard") => "2",
            var item when item == Page1.LanguageText("mouse") => "3",
            var item when item == "FANControl" => "4",
            var item when item == "CameraControl" => "5",
            _ => SettingsCoreServices.GetStartPageSetting()
        };

        SettingsCoreServices.SetStartPageSetting(StartPageSetting);
    }

    private void OpenToml_click(object sender, RoutedEventArgs e)
    {
        //Debug.WriteLine(appFolderPath);
        Debug.WriteLine(AppFilepath);
        if (Directory.Exists(AppFilepath))
        {
            //Debug.WriteLine("找到文件夹,跳过新建文件夹");
            Process.Start("explorer.exe", RedirectedFilePath);
        }
        else
        {
            Process.Start("explorer.exe", Page1.FsSetToml);
            //Debug.WriteLine("没有找到文件夹");
        }
        //System.Diagnostics.Process.Start("explorer.exe", Page1.FSSetToml);
        //System.Diagnostics.Process.Start("explorer.exe", redirectedFilePath);
    }

    private void SoftBackgroundCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        string SoftBackgroundSetting = (string)SoftBackgroundCombobox.SelectedItem switch
        {
            var item when item == Page1.LanguageText("thin") => "0",
            var item when item == Page1.LanguageText("base") => "1",
            var item when item == Page1.LanguageText("mica") => "2",
            var item when item == Page1.LanguageText("micaAlt") => "3",
            var item when item == Page1.LanguageText("defaultDesktopAcrylicBackGround") => "4",
            var item when item == Page1.LanguageText("transparent") => "5",
            var item when item == Page1.LanguageText("default") => "6",
            _ => SettingsCoreServices.GetSoftBackgroundSetting()
        };
        SettingsCoreServices.SetSoftBackgroundSetting(SoftBackgroundSetting);

        // 获取MainWindow的实例
        var mainWindow = MainWindow.Instance;

        // 更新窗口的背景
        mainWindow.WindowBackSetting();
    }

    private void SetDesktopBackground()
    {
        var wallpaperPath = WallpaperHelper.GetWallpaperPath();
        if (!string.IsNullOrEmpty(wallpaperPath))
        {
            var bitmapImage = new BitmapImage(new(wallpaperPath));
            DesktopBackgroundImage.Source = bitmapImage;
            Debug.WriteLine("壁纸已更改！");
        }
        else
        {
            // 如果Windows背景设置为纯色，则设置DesktopBackgroundBorder的背景
            var backgroundColor = WallpaperHelper.GetSolidColorBackground();
            if (backgroundColor != null)
            {
                DesktopBackgroundBorder.Background = new SolidColorBrush(backgroundColor.Value);
                Debug.WriteLine("纯色背景已设置！");
            }
        }
    }

    private void SoftLanguageCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
    }

    
}
