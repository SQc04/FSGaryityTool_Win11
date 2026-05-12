using System;
using System.Diagnostics;
using System.Management; // 必须引用 System.Management NuGet 包
using System.Linq;

namespace FSGaryityTool_Win11.Controls
{
    public enum FanId : byte { CPU = 1, GPU = 2 }

    public readonly struct FanInfo
    {
        public double RemoteTemp { get; init; }
        public double LocalTemp { get; init; }
        public double CurrentDutyPercent { get; init; }
        public int Rpm { get; init; }
    }

    public static class ClevoFanControlServices
    {
        // 蓝天模具常见的 WMI 类名和命名空间
        // 注意：不同 BIOS 版本可能不同，常见的有 "WMI_ACPI", "Clevo_WMI", "MS_SystemInformation"
        private const string WmiNamespace = "root\\WMI";
        private static readonly string[] PossibleClasses = { "WMI_ACPI", "Clevo_WMI", "MS_SystemInformation" };

        private static ManagementClass _wmiClass = null;
        private static bool _isInitialized = false;

        public static void Initialize()
        {
            if (_isInitialized) return;

            try
            {
                Debug.WriteLine("正在尝试通过 ACPI 初始化风扇控制...");

                // 尝试寻找可用的 WMI 类
                foreach (var className in PossibleClasses)
                {
                    try
                    {
                        var searcher = new ManagementObjectSearcher(WmiNamespace, $"SELECT * FROM {className}");
                        var collection = searcher.Get();

                        if (collection.Count > 0)
                        {
                            _wmiClass = new ManagementClass($"\\\\.\\{WmiNamespace}:{className}");
                            Debug.WriteLine($"✅ 找到 ACPI 类: {className}");
                            _isInitialized = true;
                            return;
                        }
                    }
                    catch { continue; }
                }

                Debug.WriteLine("❌ 未找到支持的 ACPI 类，风扇控制可能不可用。");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ACPI 初始化失败: {ex.Message}");
            }
        }

        // --- 核心修改：使用 WMI 替代 端口读写 ---

        // 注意：ACPI 无法像端口那样直接 WaitWrite/WaitRead，
        // 它是通过调用方法来同步执行的。

        public static void SetDuty(FanId fan, double percent0_100)
        {
            if (!_isInitialized) return; // 如果 ACPI 不可用，直接退出

            try
            {
                // 蓝天 ACPI 控制风扇通常使用 "SET_FAN" 或 "SFNT" 方法
                // 参数通常是 (FanID, Level)
                // 注意：很多 BIOS 屏蔽了写入权限，如果下面这行报错，说明你的电脑不支持 ACPI 控速

                // 尝试调用 SET_FAN (这是最常见的通用方法名)
                // 如果报错 "Not Supported"，说明 BIOS 不支持写入
                _wmiClass.InvokeMethod("SET_FAN", new object[] { (uint)fan, (uint)(percent0_100 * 2.55) });
            }
            catch (ManagementException ex)
            {
                // 错误码 0x80041011 通常表示对象不存在或方法不支持
                Debug.WriteLine($"ACPI 控速失败 (可能 BIOS 不支持写入): {ex.Message}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"设置风扇异常: {ex.Message}");
            }
        }

        public static FanInfo GetFanInfo(FanId fan)
        {
            // 读取数据通常比写入更容易成功
            // 但 ACPI 返回的数据结构各不相同，这里尝试读取通用属性

            // 注意：由于无法直接读取 0x62/0x66，我们只能尝试读取 WMI 暴露的属性
            // 很多蓝天本子只暴露转速，不暴露具体的温度寄存器值

            double temp = 0;
            int rpm = 0;
            double duty = 0;

            try
            {
                if (_wmiClass != null)
                {
                    // 尝试获取实例
                    var instances = _wmiClass.GetInstances();
                    foreach (var instance in instances)
                    {
                        // 尝试读取转速 (常见属性名: CurrentFanSpeed, Fan1RPM)
                        if (instance.Properties["CurrentFanSpeed"] != null)
                        {
                            rpm = Convert.ToInt32(instance.Properties["CurrentFanSpeed"].Value);
                        }

                        // 尝试读取温度 (常见属性名: CurrentTemperature, CPUTemperature)
                        // 注意：WMI 温度通常是 Kelvin (273.1 + C)，需要转换
                        if (instance.Properties["CurrentTemperature"] != null)
                        {
                            temp = Convert.ToDouble(instance.Properties["CurrentTemperature"].Value) - 273.15;
                        }

                        // 尝试读取当前占空比
                        if (instance.Properties["CurrentDuty"] != null)
                        {
                            duty = Convert.ToDouble(instance.Properties["CurrentDuty"].Value);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"读取信息失败: {ex.Message}");
            }

            // 如果 WMI 读不到转速，尝试用简单的 fallback (如果 GiveIO 还在项目中可以混用，但这里假设纯 ACPI)
            // 纯 ACPI 模式下，如果读不到就返回 0

            return new FanInfo
            {
                RemoteTemp = temp,
                LocalTemp = temp, // ACPI 通常只给一个总温度
                CurrentDutyPercent = duty,
                Rpm = rpm
            };
        }

        // 转速读取 (ACPI 模式下复用 GetFanInfo 的逻辑，或者尝试特定方法)
        public static int GetRpm(FanId fan)
        {
            // 简单起见，这里返回 0，具体逻辑在 GetFanInfo 中处理
            // 或者你可以尝试调用 "GET_FAN_RPM" 方法
            return GetFanInfo(fan).Rpm;
        }

        public static void SetAuto(FanId fan)
        {
            // 恢复自动通常也是调用 SET_FAN 传入特殊值 (如 0 或 256)
            // 蓝天通常是传入 0xFF (255) 给 ID，或者 Level 为 0
            SetDuty(fan, 100); // 保守做法：全速
        }

        public static void SetAllAuto()
        {
            SetDuty(FanId.CPU, 100);
            SetDuty(FanId.GPU, 100);
        }

        public static int GetFanCount()
        {
            // ACPI 很难直接数风扇，通常默认返回 2
            return 2;
        }
    }
}
/*
using System;
using System.Diagnostics;
using System.Management; // 必须引用 System.Management NuGet 包
using System.Linq;

namespace FSGaryityTool_Win11.Controls
{
    public enum FanId : byte { CPU = 1, GPU = 2 }

    public readonly struct FanInfo
    {
        public double RemoteTemp { get; init; }
        public double LocalTemp { get; init; }
        public double CurrentDutyPercent { get; init; }
        public int Rpm { get; init; }
    }

    public static class ClevoFanControlServices
    {
        // 蓝天模具常见的 WMI 类名和命名空间
        // 注意：不同 BIOS 版本可能不同，常见的有 "WMI_ACPI", "Clevo_WMI", "MS_SystemInformation"
        private const string WmiNamespace = "root\\WMI";
        private static readonly string[] PossibleClasses = { "WMI_ACPI", "Clevo_WMI", "MS_SystemInformation" };
        
        private static ManagementClass _wmiClass = null;
        private static bool _isInitialized = false;

        public static void Initialize()
        {
            if (_isInitialized) return;

            try
            {
                Debug.WriteLine("正在尝试通过 ACPI 初始化风扇控制...");
                
                // 尝试寻找可用的 WMI 类
                foreach (var className in PossibleClasses)
                {
                    try
                    {
                        var searcher = new ManagementObjectSearcher(WmiNamespace, $"SELECT * FROM {className}");
                        var collection = searcher.Get();
                        
                        if (collection.Count > 0)
                        {
                            _wmiClass = new ManagementClass($"\\\\.\\{WmiNamespace}:{className}");
                            Debug.WriteLine($"✅ 找到 ACPI 类: {className}");
                            _isInitialized = true;
                            return;
                        }
                    }
                    catch { continue; }
                }

                Debug.WriteLine("❌ 未找到支持的 ACPI 类，风扇控制可能不可用。");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ACPI 初始化失败: {ex.Message}");
            }
        }

        // --- 核心修改：使用 WMI 替代 端口读写 ---

        // 注意：ACPI 无法像端口那样直接 WaitWrite/WaitRead，
        // 它是通过调用方法来同步执行的。

        public static void SetDuty(FanId fan, double percent0_100)
        {
            if (!_isInitialized) return; // 如果 ACPI 不可用，直接退出

            try
            {
                // 蓝天 ACPI 控制风扇通常使用 "SET_FAN" 或 "SFNT" 方法
                // 参数通常是 (FanID, Level)
                // 注意：很多 BIOS 屏蔽了写入权限，如果下面这行报错，说明你的电脑不支持 ACPI 控速
                
                // 尝试调用 SET_FAN (这是最常见的通用方法名)
                // 如果报错 "Not Supported"，说明 BIOS 不支持写入
                _wmiClass.InvokeMethod("SET_FAN", new object[] { (uint)fan, (uint)(percent0_100 * 2.55) });
            }
            catch (ManagementException ex)
            {
                // 错误码 0x80041011 通常表示对象不存在或方法不支持
                Debug.WriteLine($"ACPI 控速失败 (可能 BIOS 不支持写入): {ex.Message}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"设置风扇异常: {ex.Message}");
            }
        }

        public static FanInfo GetFanInfo(FanId fan)
        {
            // 读取数据通常比写入更容易成功
            // 但 ACPI 返回的数据结构各不相同，这里尝试读取通用属性
            
            // 注意：由于无法直接读取 0x62/0x66，我们只能尝试读取 WMI 暴露的属性
            // 很多蓝天本子只暴露转速，不暴露具体的温度寄存器值
            
            double temp = 0;
            int rpm = 0;
            double duty = 0;

            try
            {
                if (_wmiClass != null)
                {
                    // 尝试获取实例
                    var instances = _wmiClass.GetInstances();
                    foreach (var instance in instances)
                    {
                        // 尝试读取转速 (常见属性名: CurrentFanSpeed, Fan1RPM)
                        if (instance.Properties["CurrentFanSpeed"] != null)
                        {
                            rpm = Convert.ToInt32(instance.Properties["CurrentFanSpeed"].Value);
                        }
                        
                        // 尝试读取温度 (常见属性名: CurrentTemperature, CPUTemperature)
                        // 注意：WMI 温度通常是 Kelvin (273.1 + C)，需要转换
                        if (instance.Properties["CurrentTemperature"] != null)
                        {
                            temp = Convert.ToDouble(instance.Properties["CurrentTemperature"].Value) - 273.15;
                        }

                        // 尝试读取当前占空比
                        if (instance.Properties["CurrentDuty"] != null)
                        {
                            duty = Convert.ToDouble(instance.Properties["CurrentDuty"].Value);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"读取信息失败: {ex.Message}");
            }

            // 如果 WMI 读不到转速，尝试用简单的 fallback (如果 GiveIO 还在项目中可以混用，但这里假设纯 ACPI)
            // 纯 ACPI 模式下，如果读不到就返回 0
            
            return new FanInfo
            {
                RemoteTemp = temp,
                LocalTemp = temp, // ACPI 通常只给一个总温度
                CurrentDutyPercent = duty,
                Rpm = rpm
            };
        }

        // 转速读取 (ACPI 模式下复用 GetFanInfo 的逻辑，或者尝试特定方法)
        public static int GetRpm(FanId fan)
        {
            // 简单起见，这里返回 0，具体逻辑在 GetFanInfo 中处理
            // 或者你可以尝试调用 "GET_FAN_RPM" 方法
            return GetFanInfo(fan).Rpm;
        }

        public static void SetAuto(FanId fan)
        {
            // 恢复自动通常也是调用 SET_FAN 传入特殊值 (如 0 或 256)
            // 蓝天通常是传入 0xFF (255) 给 ID，或者 Level 为 0
            SetDuty(fan, 100); // 保守做法：全速
        }

        public static void SetAllAuto()
        {
            SetDuty(FanId.CPU, 100);
            SetDuty(FanId.GPU, 100);
        }

        public static int GetFanCount()
        {
            // ACPI 很难直接数风扇，通常默认返回 2
            return 2;
        }
    }
}
 */