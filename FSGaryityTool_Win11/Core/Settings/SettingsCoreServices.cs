using System;
using System.IO;
using static FSGaryityTool_Win11.SettingsPage;
using System.Diagnostics;
using Tommy;

namespace FSGaryityTool_Win11.Core.Settings;

internal class SettingsCoreServices
{

    public static TomlTable settingstomlSp;

    public static string SYSAPLOCAL = Environment.GetFolderPath(folder: Environment.SpecialFolder.LocalApplicationData);
    public static string FairingStudioFolder = Path.Combine(SYSAPLOCAL, "FAIRINGSTUDIO");
    public static string FSGravityToolsFolder = Path.Combine(FairingStudioFolder, "FSGravityTool");
    public static string FSGravityToolsSettingsToml = Path.Combine(FSGravityToolsFolder, "Settings.toml");

    public static string fsGravitySettings = "FSGravitySettings";
    public static string serialPortSettings = "SerialPortSettings";

    public static string serialPortComData = "SerialPortCOMData";
    public static string comData = "COMData";

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

    public static void AddTomlFile()
    {
        if (File.Exists(FSGravityToolsSettingsToml))             //生成TOML
        {
            Debug.WriteLine("找到TOML文件,跳过新建文件");
            return;
        }
        else
        {
            Debug.WriteLine("没有找到TOML文件");
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
                    ["SoftDefLanguage"] = "zh-CN",
                    ["DefNavigationViewMode"] = "0",
                    ["DefaultNavigationViewPaneOpen"] = "true",
                    ["BackgroundImageSourse"] = "",
                    ["BackgroundImageOpacity"] = "0.0",
                    //[""] = "",
                },

                [serialPortSettings] =
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

    public static string TomlCheckNulls(string Mode, string Menu, string Name)
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

        string[] cOMSaveDeviceinf = { "0", "1" };
        string cOMDeviceinf;

        using (var reader = File.OpenText(FSGravityToolsSettingsToml))                    //打开TOML文件
        {
            settingstomlSp = TOML.Parse(reader);

            defpage = TomlCheckNulls("0", fsGravitySettings, "DefaultNvPage");
            defPageBackground = TomlCheckNulls("0", fsGravitySettings, "SoftBackground");
            defNavigationViewMode = TomlCheckNulls("0", fsGravitySettings, "DefNavigationViewMode");
            defaultNavigationViewPaneOpen = TomlCheckNulls("true", fsGravitySettings, "DefaultNavigationViewPaneOpen");
            backgroundImageOpacity = TomlCheckNulls("0.0", fsGravitySettings, "BackgroundImageOpacity");

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
}
