using System;
using System.IO;
using static FSGaryityTool_Win11.SettingsPage;
using System.Diagnostics;
using Microsoft.Windows.Storage;
using Tommy;

namespace FSGaryityTool_Win11.Core.Settings;

internal class SettingsCoreServices
{
    public static TomlTable SettingstomlSp;

    public static string FsGravityToolsFolder { get; } = ApplicationData.GetDefault().LocalPath;
    public static string FsGravityToolsSettingsToml = Path.Combine(FsGravityToolsFolder, "Settings.toml");

    public const string FsGravitySettings = "FSGravitySettings";
    public const string SerialPortSettings = "SerialPortSettings";

    public const string SerialPortComData = "SerialPortCOMData";
    public const string ComData = "COMData";

    public static void CheckSettingFolder()
    {
        //Debug.WriteLine("开始搜索文件夹");
        Debug.WriteLine("开始搜索文件夹  " + FsGravityToolsFolder);            //新建FS文件夹

        if (Directory.Exists(FsGravityToolsFolder))
        {
            //Debug.WriteLine("找到文件夹,跳过新建文件夹");
        }
        else
        {
            //Debug.WriteLine("没有找到文件夹");
            Directory.CreateDirectory(FsGravityToolsFolder);
            //Debug.WriteLine("新建文件夹");
        }
    }

    public static void AddTomlFile()
    {
        if (File.Exists(FsGravityToolsSettingsToml))             //生成TOML
        {
            Debug.WriteLine("找到TOML文件,跳过新建文件");
            return;
        }
        else
        {
            Debug.WriteLine("没有找到TOML文件");
            string[] cOmSaveDeviceinf = ["0"];
            var settingstoml = new TomlTable
            {
                ["Version"] = MainWindow.FSSoftVersion,

                [FsGravitySettings] =
                {
                    Comment =
                        "FSGaryityTool Settings:",
                    ["DefaultNvPage"] = "0",
                    ["SoftBackground"] = "0",
                    ["SoftDefLanguage"] = "zh-CN",
                    ["DefNavigationViewMode"] = "0",
                    ["DefaultNavigationViewPaneOpen"] = "true",
                    ["BackgroundImageSourse"] = "",
                    ["BackgroundImageOpacity"] = "0.0",
                    //[""] = "",
                },

                [SerialPortSettings] =
                {
                    Comment =
                        "FSGaryityTool SerialPort Settings:\r\n" +
                        "Parity:None,Odd,Even,Mark,Space\r\n" +
                        "STOPbits:None,One,OnePointFive,Two\r\n" +
                        "DATAbits:5~9",

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

                [SerialPortComData] =
                {
                    Comment =
                        "This is a cache of information for all serial devices.\r\n" +
                        "",
                    ["CheckTime"] = "2024-04-12 19:48:55",                  //串口设备信息更新的时间
                    ["CheckCounter"] = "0",                                 //串口设备信息更新次数
                    ["COMSaveDeviceinf"] = string.Join(",", cOmSaveDeviceinf),//已保存串口设备的映射表
                },

                [ComData] =
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

            using (var writer = File.CreateText(FsGravityToolsSettingsToml))
            {
                settingstoml.WriteTo(writer);
                Debug.WriteLine("写入Toml");
                // Remember to flush the data if needed!
                writer.Flush();
            }
            Debug.WriteLine("新建TOML");
        }
    }

    public static string TomlCheckNulls(string mode, string menu, string name)
    {
        using var reader = File.OpenText(FsGravityToolsSettingsToml);
        var sPsettingstomlr = TOML.Parse(reader);             //读取TOML

        string data = sPsettingstomlr[menu][name] == "Tommy.TomlLazy" ? mode : sPsettingstomlr[menu][name];

        return data;
    }

    public static void CheckSettingsFileVersion()
    {
        string tomlfsVersion;       //版本号比较

        using (var reader = File.OpenText(FsGravityToolsSettingsToml))
        {
            var settingstomlr = TOML.Parse(reader);
            tomlfsVersion = settingstomlr["Version"];
        }

        var tomlVersion = new Version(tomlfsVersion);
        var fsGrVersion = new Version(MainWindow.FSSoftVersion);

        if (fsGrVersion > tomlVersion)
        {
            UpdateSettingsFile();
        }
        else if (fsGrVersion < tomlVersion)
        {
            DowngradeSettingsFile();
        }
        else
        {
            CheckSettingsFile();
        }
    }

    public static void UpdateSettingsFile()
    {
        Debug.WriteLine(">");

        //缓存设置
        string defpage, defPageBackground, defLaunage, defNavigationViewMode, defaultNavigationViewPaneOpen, backgroundImageOpacity;
        string baud, party, stop, data, encoding, rxhex, txhex, dtr, rts, shtime, autosco, autosavrset, autosercom, autoconnect, txnewline;
        string checkTime, checkCounter;

        string[] cOmSaveDeviceinf = ["0", "1"];
        string cOmDeviceinf;

        using (var reader = File.OpenText(FsGravityToolsSettingsToml))                    //打开TOML文件
        {
            SettingstomlSp = TOML.Parse(reader);

            defpage = TomlCheckNulls("0", FsGravitySettings, "DefaultNvPage");
            defPageBackground = TomlCheckNulls("0", FsGravitySettings, "SoftBackground");
            defNavigationViewMode = TomlCheckNulls("0", FsGravitySettings, "DefNavigationViewMode");
            defaultNavigationViewPaneOpen = TomlCheckNulls("true", FsGravitySettings, "DefaultNavigationViewPaneOpen");
            backgroundImageOpacity = TomlCheckNulls("0.0", FsGravitySettings, "BackgroundImageOpacity");

            var culture = System.Globalization.CultureInfo.CurrentUICulture;
            var lang = culture.Name;
            defLaunage = SettingstomlSp["FSGravitySettings"]["SoftDefLanguage"] == "Tommy.TomlLazy"
                ? lang
                : SettingstomlSp["FSGravitySettings"]["SoftDefLanguage"];

            baud = TomlCheckNulls("115200", SerialPortSettings, "DefaultBAUD");
            party = TomlCheckNulls("None", SerialPortSettings, "DefaultParity");
            stop = TomlCheckNulls("One", SerialPortSettings, "DefaultSTOP");
            data = TomlCheckNulls("8", SerialPortSettings, "DefaultDATA");
            encoding = TomlCheckNulls("utf-8", SerialPortSettings, "DefaultEncoding");

            rxhex = TomlCheckNulls("0", SerialPortSettings, "DefaultRXHEX");
            txhex = TomlCheckNulls("0", SerialPortSettings, "DefaultTXHEX");
            dtr = TomlCheckNulls("1", SerialPortSettings, "DefaultDTR");
            rts = TomlCheckNulls("0", SerialPortSettings, "DefaultRTS");
            shtime = TomlCheckNulls("0", SerialPortSettings, "DefaultSTime");
            autosco = TomlCheckNulls("1", SerialPortSettings, "DefaultAUTOSco");
            autosavrset = TomlCheckNulls("1", SerialPortSettings, "AutoDaveSet");
            autosercom = TomlCheckNulls("1", SerialPortSettings, "AutoSerichCom");
            autoconnect = TomlCheckNulls("1", SerialPortSettings, "AutoConnect");
            txnewline = TomlCheckNulls("1", SerialPortSettings, "DefaultTXNewLine");

            //if (settingstomlSp["SerialPortSettings"] is not null)  = ;
            checkTime = SettingstomlSp["SerialPortCOMData"]["CheckTime"] == "Tommy.TomlLazy"
                ? "2024-04-12 19:48:55"
                : SettingstomlSp["SerialPortCOMData"]["CheckTime"];

            checkCounter = TomlCheckNulls("0", SerialPortComData, "CheckCounter");
            cOmDeviceinf = TomlCheckNulls("0", SerialPortComData, "COMSaveDeviceinf");

            SettingstomlSp = new()
            {
                ["Version"] = MainWindow.FSSoftVersion,

                ["FSGravitySettings"] =
                {
                    Comment =
                        "FSGaryityTool Settings:",
                    ["DefaultNvPage"] = defpage,
                    ["SoftBackground"] = defPageBackground,
                    ["SoftDefLanguage"] = defLaunage,
                    ["DefNavigationViewMode"] = defNavigationViewMode,
                    ["DefaultNavigationViewPaneOpen"] = defaultNavigationViewPaneOpen,
                    ["BackgroundImageSourse"] = "",
                    ["BackgroundImageOpacity"] = backgroundImageOpacity,
                    //[""] = "",
                },

                ["SerialPortSettings"] =
                {
                    Comment =
                        "FSGaryityTool SerialPort Settings:\r\n" +
                        "Parity:None,Odd,Even,Mark,Space\r\n" +
                        "STOPbits:None,One,OnePointFive,Two\r\n" +
                        "DATAbits:5~9",

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
                    ["COMSaveDeviceinf"] = cOmDeviceinf//String.Join(",", cOMSaveDeviceinf)
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
        using (var writer = File.CreateText(FsGravityToolsSettingsToml))                  //将设置写入TOML文件
        {
            SettingstomlSp.WriteTo(writer);
            //Debug.WriteLine("写入Toml" + settingstomlSp["FSGravitySettings"]["DefaultNvPage"]);
            // Remember to flush the data if needed!
            writer.Flush();
        }
    }

    public static void DowngradeSettingsFile()
    {
        Debug.WriteLine("<");
    }

    public static void CheckSettingsFile()
    {
        Debug.WriteLine("=");
    }

    public static void SaveSetting(string menuName, string name, string settingItem)
    {
        using (var reader = File.OpenText(FsGravityToolsSettingsToml))                    //打开TOML文件
        {
            SettingsTomlr = TOML.Parse(reader);

            SettingsTomlr[menuName][name] = settingItem;
        }

        using (var writer = File.CreateText(FsGravityToolsSettingsToml))                  //将设置写入TOML文件
        {
            SettingsTomlr.WriteTo(writer);
            writer.Flush();
        }
    }
}
