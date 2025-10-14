using System;
using System.IO;
using static FSGaryityTool_Win11.SettingsPage;
using Windows.ApplicationModel;
using System.Diagnostics;
using Tommy;

namespace FSGaryityTool_Win11.Core.Settings;

internal class SettingsCoreServices
{

    public static TomlTable settingstomlSp;


    public static string APPPACKAGENAME = GetAppPackageName();

    public static string SYSAPLOCAL = Environment.GetFolderPath(folder: Environment.SpecialFolder.LocalApplicationData);
    public static string SYSREDIRECTLOCA = Path.Combine(SYSAPLOCAL, "Packages", APPPACKAGENAME, "LocalCache", "Local");

    public static string FairingStudioFolder = Path.Combine(SYSAPLOCAL, "FAIRINGSTUDIO");
    public static string FSGravityToolsFolder = Path.Combine(FairingStudioFolder, "FSGravityTool");
    public static string FairingStudioRedirectFolder = Path.Combine(SYSREDIRECTLOCA, "FAIRINGSTUDIO");
    public static string FSGravityToolsRedirectFolder = Path.Combine(FairingStudioRedirectFolder, "FSGravityTool");

    public static string FSGravityToolsSettingsToml = Path.Combine(FSGravityToolsFolder, "Settings.toml");
    public static string FSGravityToolsRedirectSettingsToml = Path.Combine(FSGravityToolsRedirectFolder, "Settings.toml");

    public static string fsGravitySettings = "FSGravitySettings";
    public static string serialPortSettings = "SerialPortSettings";

    public static string serialPortComData = "SerialPortCOMData";
    public static string comData = "COMData";

    public static string StartPage = "DefaultNvPage";
    public static string SoftBackgroundActivatedEnable = "SoftBackgroundActivatedEnable";
    public static string SoftBackground = "SoftBackground";

    public static string GetAppPackageName()
    {
        string AppPackageName;
        try
        {
            AppPackageName = Package.Current.Id.FamilyName;
        }
        catch (Exception ex)
        {
            AppPackageName = ""; 
            Debug.WriteLine($"获取包名时发生错误: {ex.Message}");
        }
        return AppPackageName;
    }

    public static void CheckSettingFolder()
    {
        //Debug.WriteLine("开始搜索文件夹");
        Debug.WriteLine("开始搜索文件夹  " + FairingStudioFolder);            //新建FS文件夹

        if (Directory.Exists(FairingStudioFolder))
        {
            //Debug.WriteLine("找到文件夹,跳过新建文件夹");
        }
        else
        {
            //Debug.WriteLine("没有找到文件夹");
            Directory.CreateDirectory(FairingStudioFolder);
            //Debug.WriteLine("新建文件夹");
        }

        if (Directory.Exists(FSGravityToolsFolder))
        {
            //Debug.WriteLine("找到文件夹,跳过新建文件夹");
        }
        else
        {
            //Debug.WriteLine("没有找到文件夹");
            Directory.CreateDirectory(FSGravityToolsFolder);
            //Debug.WriteLine("新建文件夹");
        }
    }

    public static void AddTomlFile(bool IsReset = false)
    {
        if (File.Exists(FSGravityToolsSettingsToml) && !IsReset)             //生成TOML
        {
            Debug.WriteLine("找到TOML文件,跳过新建文件");
            return;
        }
        else
        {
            if (IsReset) Debug.WriteLine("重置TOML文件");
            else Debug.WriteLine("没有找到TOML文件,新建文件");

            string[] cOMSaveDeviceinf = { "0" };
            var settingstoml = new TomlTable
            {
                ["Version"] = MainWindow.FSSoftVersion,

                [fsGravitySettings] =
                {
                    Comment =
                        "FSGaryityTool Settings:",
                    ["DefaultNvPage"] = "0",
                    ["SoftBackground"] = "0",
                    ["SoftBackgroundToggleSwitch"] = "color",
                    ["SoftBackgroundActivatedEnable"] = "false",
                    ["SoftDefLanguage"] = "zh-CN",
                    ["DefNavigationViewMode"] = "0",
                    ["DefaultNavigationViewPaneOpen"] = "true",
                    ["BackgroundImageSourse"] = "",
                    ["BackgroundImageOpacity"] = "0.0",
                    ["BackgroundColor"] = "#FFFFFF00",
                    ["DefaultWindowWidth"] = "1840",
                    ["DefaultWindowHight"] = "960",
                    //[""] = "",
                },

                [serialPortSettings] =
                {
                    Comment =
                        "FSGaryityTool SerialPort Settings:\r\n" +
                        "Parity:None,Odd,Even,Mark,Space\r\n" +
                        "STOPbits:None,One,OnePointFive,Two\r\n" +
                        "DATAbits:5~8",

                    ["DefaultBAUD"] = "115200",
                    ["DefaultParity"] = "None",
                    ["DefaultSTOP"] = "One",
                    ["DefaultDATA"] = "8",
                    ["DefaultEncoding"] = "utf-8",
                    ["DefaultRXHEX"] = "0",
                    ["DefaultTXHEX"] = "0",
                    ["DefaultDTR"] = "1",
                    ["DefaultRTS"] = "0",
                    ["DefaultSTime"] = "0",
                    ["DefaultAUTOSco"] = "1",
                    ["AutoDaveSet"] = "1",
                    ["AutoSerichCom"] = "1",
                    ["AutoConnect"] = "1",
                    ["DefaultTXNewLine"] = "1"
                },

                [serialPortComData] =
                {
                    Comment =
                        "This is a cache of information for all serial devices.\r\n" +
                        "",
                    ["CheckTime"] = "2024-04-12 19:48:55",                  //串口设备信息更新的时间
                    ["CheckCounter"] = "0",                                 //串口设备信息更新次数
                    ["COMSaveDeviceinf"] = string.Join(",", cOMSaveDeviceinf),//已保存串口设备的映射表
                },

                [comData] =
                {
                    Comment =
                        "This is an example of cached serial device information.\r\n",
                    ["COM0"] =
                    {
                        ["Icon"] = "\uE88E",                            //串口设备自定义的图标
                        ["Description"] = "An example of a serial device format",                       //串口设备描述
                        ["Name"] = "example",                              //串口设备名字
                        ["Manufacturer"] = "FairingStudio",             //串口设备制造商
                        ["RSTBaudRate"] = "115200",                     //自动重启上电打印波特率
                        ["RSTTime"] = "300",                            //自动重启上电打印延时
                        ["RSTMode"] = "0",                              //重启模式
                    },

                },

            };

            using (var writer = File.CreateText(FSGravityToolsSettingsToml))
            {
                settingstoml.WriteTo(writer);
                Debug.WriteLine("写入Toml");
                // Remember to flush the data if needed!
                writer.Flush();
            }
            Debug.WriteLine("新建TOML");
        }
    }

    private static string TomlCheckNulls(string Mode, string Menu, string Name)
    {
        var data = "0";
        using (var reader = File.OpenText(FSGravityToolsSettingsToml))
        {
            var SPsettingstomlr = TOML.Parse(reader);             //读取TOML

            if (SPsettingstomlr[Menu][Name] != "Tommy.TomlLazy") data = SPsettingstomlr[Menu][Name];
            else
            {
                data = Mode;
            }
        }
        return data;
    }

    public static void CheckSettingsFileVersion()
    {
        string TomlfsVersion;       //版本号比较

        try
        {
            using (var reader = File.OpenText(FSGravityToolsSettingsToml))
            {
                var settingstomlr = TOML.Parse(reader);
                TomlfsVersion = settingstomlr["Version"];
            }

            var TomlVersion = new Version(TomlfsVersion);
            var FSGrVersion = new Version(MainWindow.FSSoftVersion);

            if (FSGrVersion > TomlVersion)
            {
                UpdateSettingsFile();
            }
            else if (FSGrVersion < TomlVersion)
            {
                DowngradeSettingsFile();
            }
            else if (FSGrVersion == TomlVersion)
            {
                CheckSettingsFile();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"设置文件已损坏: {ex.Message}");
            ResetSettingsFile();
        }
    }


    private static void UpdateSettingsFile()
    {
        Debug.WriteLine(">");

        //缓存设置
        string defpage, defPageBackground, defsoftBackgroundToggleSwitch, softBackgroundActivatedEnable, defLaunage, defNavigationViewMode, defaultNavigationViewPaneOpen, backgroundImageOpacity, backgroundColor;
        string defaultWindowWidth, defaultWindowHight;
        string baud, party, stop, data, encoding, rxhex, txhex, dtr, rts, shtime, autosco, autosavrset, autosercom, autoconnect, txnewline;
        string checkTime, checkCounter;

        string[] cOMSaveDeviceinf = { "0", "1" };
        string cOMDeviceinf;

        using (var reader = File.OpenText(FSGravityToolsSettingsToml))                    //打开TOML文件
        {
            settingstomlSp = TOML.Parse(reader);

            defpage = TomlCheckNulls("0", fsGravitySettings, "DefaultNvPage");
            defPageBackground = TomlCheckNulls("0", fsGravitySettings, "SoftBackground");
            defsoftBackgroundToggleSwitch = TomlCheckNulls("color", fsGravitySettings, "SoftBackgroundToggleSwitch");
            softBackgroundActivatedEnable = TomlCheckNulls("true", fsGravitySettings, "SoftBackgroundActivatedEnable");
            defNavigationViewMode = TomlCheckNulls("0", fsGravitySettings, "DefNavigationViewMode");
            defaultNavigationViewPaneOpen = TomlCheckNulls("true", fsGravitySettings, "DefaultNavigationViewPaneOpen");
            backgroundImageOpacity = TomlCheckNulls("0.0", fsGravitySettings, "BackgroundImageOpacity");
            backgroundColor = TomlCheckNulls("#FFFFFF00", fsGravitySettings, "BackgroundColor");
            defaultWindowWidth = TomlCheckNulls("1840", fsGravitySettings, "DefaultWindowWidth");
            defaultWindowHight = TomlCheckNulls("960", fsGravitySettings, "DefaultWindowHight");

            var culture = System.Globalization.CultureInfo.CurrentUICulture;
            var lang = culture.Name;
            if ((string)settingstomlSp["FSGravitySettings"]["SoftDefLanguage"] != "Tommy.TomlLazy") defLaunage = settingstomlSp["FSGravitySettings"]["SoftDefLanguage"];
            else defLaunage = lang;

            baud = TomlCheckNulls("115200", serialPortSettings, "DefaultBAUD");
            party = TomlCheckNulls("None", serialPortSettings, "DefaultParity");
            stop = TomlCheckNulls("One", serialPortSettings, "DefaultSTOP");
            data = TomlCheckNulls("8", serialPortSettings, "DefaultDATA");
            encoding = TomlCheckNulls("utf-8", serialPortSettings, "DefaultEncoding");

            rxhex = TomlCheckNulls("0", serialPortSettings, "DefaultRXHEX");
            txhex = TomlCheckNulls("0", serialPortSettings, "DefaultTXHEX");
            dtr = TomlCheckNulls("1", serialPortSettings, "DefaultDTR");
            rts = TomlCheckNulls("0", serialPortSettings, "DefaultRTS");
            shtime = TomlCheckNulls("0", serialPortSettings, "DefaultSTime");
            autosco = TomlCheckNulls("1", serialPortSettings, "DefaultAUTOSco");
            autosavrset = TomlCheckNulls("1", serialPortSettings, "AutoDaveSet");
            autosercom = TomlCheckNulls("1", serialPortSettings, "AutoSerichCom");
            autoconnect = TomlCheckNulls("1", serialPortSettings, "AutoConnect");
            txnewline = TomlCheckNulls("1", serialPortSettings, "DefaultTXNewLine");

            //if (settingstomlSp["SerialPortSettings"] is not null)  = ;

            if ((string)settingstomlSp["SerialPortCOMData"]["CheckTime"] != "Tommy.TomlLazy") checkTime = settingstomlSp["SerialPortCOMData"]["CheckTime"];
            else checkTime = "2024-04-12 19:48:55";

            checkCounter = TomlCheckNulls("0", serialPortComData, "CheckCounter");
            cOMDeviceinf = TomlCheckNulls("0", serialPortComData, "COMSaveDeviceinf");

            settingstomlSp = new()
            {
                ["Version"] = MainWindow.FSSoftVersion,

                ["FSGravitySettings"] =
                {
                    Comment =
                        "FSGaryityTool Settings:",
                    ["DefaultNvPage"] = defpage,
                    ["SoftBackground"] = defPageBackground,
                    ["SoftBackgroundToggleSwitch"] = defsoftBackgroundToggleSwitch,
                    ["SoftBackgroundActivatedEnable"] = softBackgroundActivatedEnable,
                    ["SoftDefLanguage"] = defLaunage,
                    ["DefNavigationViewMode"] = defNavigationViewMode,
                    ["DefaultNavigationViewPaneOpen"] = defaultNavigationViewPaneOpen,
                    ["BackgroundImageSourse"] = "",
                    ["BackgroundImageOpacity"] = backgroundImageOpacity,
                    ["BackgroundColor"] = backgroundColor,
                    ["DefaultWindowWidth"] = defaultWindowWidth,
                    ["DefaultWindowHight"] = defaultWindowHight,
                    //[""] = "",
                },

                ["SerialPortSettings"] =
                {
                    Comment =
                        "FSGaryityTool SerialPort Settings:\r\n" +
                        "Parity:None,Odd,Even,Mark,Space\r\n" +
                        "STOPbits:None,One,OnePointFive,Two\r\n" +
                        "DATAbits:5~8",

                    ["DefaultBAUD"] = baud,
                    ["DefaultParity"] = party,
                    ["DefaultSTOP"] = stop,
                    ["DefaultDATA"] = data,
                    ["DefaultEncoding"] = encoding,
                    ["DefaultRXHEX"] = rxhex,
                    ["DefaultTXHEX"] = txhex,
                    ["DefaultDTR"] = dtr,
                    ["DefaultRTS"] = rts,
                    ["DefaultSTime"] = shtime,
                    ["DefaultAUTOSco"] = autosco,
                    ["AutoDaveSet"] = autosavrset,
                    ["AutoSerichCom"] = autosercom,
                    ["AutoConnect"] = autoconnect,
                    ["DefaultTXNewLine"] = txnewline,
                },

                ["SerialPortCOMData"] =
                {
                    Comment =
                        "This is a cache of information for all serial devices.\r\n",

                    ["CheckTime"] = "2024-04-12 19:48:55",
                    ["CheckCounter"] = "0",
                    ["COMSaveDeviceinf"] = cOMDeviceinf//String.Join(",", cOMSaveDeviceinf)
                },
                ["COMData"] =
                {
                    Comment =
                        "This is an example of cached serial device information.\r\n",
                    ["COM0"] =
                    {
                        ["Icon"] = "\uE88E",
                        ["Description"] = "An example of a serial device format",
                        ["Name"] = "example",
                        ["Manufacturer"] = "FairingStudio",
                        ["RSTBaudRate"] = "115200",
                        ["RSTTime"] = "300",
                        ["RSTMode"] = "0",
                    },
                },

            };
        }
        //更新Toml
        using (var writer = File.CreateText(FSGravityToolsSettingsToml))                  //将设置写入TOML文件
        {
            settingstomlSp.WriteTo(writer);
            //Debug.WriteLine("写入Toml" + settingstomlSp["FSGravitySettings"]["DefaultNvPage"]);
            // Remember to flush the data if needed!
            writer.Flush();
        }
    }

    private static void DowngradeSettingsFile()
    {
        Debug.WriteLine("<");
    }

    private static void CheckSettingsFile()
    {
        Debug.WriteLine("=");
    }
    private static void DeleteSettingsFile()
    {
        Debug.WriteLine("X");
        try
        {
            File.Delete(FSGravityToolsSettingsToml);
            Debug.WriteLine("删除设置文件");
            AddTomlFile();
            Debug.WriteLine("重建设置文件");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"删除设置文件时发生错误: {ex.Message}");
        }
    }
    private static void ResetSettingsFile()
    {
        Debug.WriteLine("R");
        try
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            File.Copy(FSGravityToolsSettingsToml, FSGravityToolsSettingsToml + $"_{timestamp}.corrupted", true);
            Debug.WriteLine("备份设置文件");
            AddTomlFile(true);
            Debug.WriteLine("重建设置文件");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"重置设置文件时发生错误: {ex.Message}");
        }
    }

    private static string GetSetting(string menuName, string name)
    {
        string settingItem;
        try
        {
            
            using (var reader = File.OpenText(FSGravityToolsSettingsToml))                    //打开TOML文件
            {
                SettingsTomlr = TOML.Parse(reader);
                settingItem = SettingsTomlr[menuName][name];
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"获取设置时发生错误: {ex.Message}");
            ResetSettingsFile();
            settingItem = "Error";
        }
        return settingItem;
    }
    private static void SaveSetting(string menuName, string name, string settingItem)
    {
        try
        {
            using (var reader = File.OpenText(FSGravityToolsSettingsToml))                    //打开TOML文件
            {
                SettingsTomlr = TOML.Parse(reader);

                SettingsTomlr[menuName][name] = settingItem;
            }

            using (var writer = File.CreateText(FSGravityToolsSettingsToml))                  //将设置写入TOML文件
            {
                SettingsTomlr.WriteTo(writer);
                writer.Flush();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"保存设置时发生错误: {ex.Message}");
            ResetSettingsFile();
        }
    }

    //MainWindow's settings
    public static bool GetMainWindowNavigationPaneInfo()
    {
        bool DefaultNavigationPaneIsOpen;
        try
        {
            DefaultNavigationPaneIsOpen = Convert.ToBoolean(GetSetting(fsGravitySettings, "DefaultNavigationViewPaneOpen"));
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"获取导航栏设置时发生错误: {ex.Message}");
            DefaultNavigationPaneIsOpen = true;
            ResetSettingsFile();
        }
        return DefaultNavigationPaneIsOpen;
    }
    public static void SetMainWindowNavigationPaneInfo(bool DefaultNavigationPaneIsOpen)
    {
        SaveSetting(fsGravitySettings, "DefaultNavigationViewPaneOpen", Convert.ToString(DefaultNavigationPaneIsOpen));
    }

    public static string GetStartPageSetting()
    {
        string StartPageSetting;
        try
        {
            StartPageSetting = GetSetting(fsGravitySettings, StartPage);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"获取默认页面设置时发生错误: {ex.Message}");
            StartPageSetting = "0";
            ResetSettingsFile();
        }
        return StartPageSetting;
    }
    public static void SetStartPageSetting(string StartPageSetting)
    {
        SaveSetting(fsGravitySettings, StartPage, StartPageSetting);
    }

    public static string GetSoftBackgroundActivatedEnableSetting()
    {
        string SoftBackgroundActivatedEnableSetting;
        try
        {
            SoftBackgroundActivatedEnableSetting = GetSetting(fsGravitySettings, SoftBackgroundActivatedEnable);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"获取背景设置时发生错误: {ex.Message}");
            SoftBackgroundActivatedEnableSetting = "false";
            ResetSettingsFile();
        }
        return SoftBackgroundActivatedEnableSetting;
    }

    public static void SetSoftBackgroundActivatedEnableSetting(string SoftBackgroundActivatedEnableSetting)
    {
        SaveSetting(fsGravitySettings, SoftBackgroundActivatedEnable, SoftBackgroundActivatedEnableSetting);
    }

    public static string GetSoftBackgroundSetting()
    {
        string SoftBackgroundSetting;
        try
        {
            SoftBackgroundSetting = GetSetting(fsGravitySettings, SoftBackground);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"获取背景设置时发生错误: {ex.Message}");
            SoftBackgroundSetting = "0";
            ResetSettingsFile();
        }
        return SoftBackgroundSetting;
    }

    public static void SetSoftBackgroundSetting(string SoftBackgroundSetting)
    {
        SaveSetting(fsGravitySettings, SoftBackground, SoftBackgroundSetting);
    }

    public static (int width, int height) GetDefaultWindow()
    {
        string widthStr = GetSetting(fsGravitySettings, "DefaultWindowWidth");
        string heightStr = GetSetting(fsGravitySettings, "DefaultWindowHight");
        int width = int.TryParse(widthStr, out var w) ? w : 1840;
        int height = int.TryParse(heightStr, out var h) ? h : 960;
        return (width, height);
    }

    public static void SetDefaultWindow(int width, int height)
    {
        SaveSetting(fsGravitySettings, "DefaultWindowWidth", width.ToString());
        SaveSetting(fsGravitySettings, "DefaultWindowHight", height.ToString());
    }
}
