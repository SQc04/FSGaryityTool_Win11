using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FSGaryityTool_Win11.Reliance
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ECData
    {
        public byte Remote; // 温度
        public byte Local;
        public byte FanDuty; // 风扇负载，0-255
        public byte Reserve;
    }

    public static class ClevoEcInfo
    {
        const string DLL_PATH = @"Reliance\ClevoEcinfo.dll";

        [DllImport(DLL_PATH)]
        public static extern bool InitIo(); // 初始化接口

        [DllImport(DLL_PATH)]
        public static extern void SetFanDuty(int fan_id, int duty); // 设置风扇负载，fan_id为正整数 1:CPU 2:GPU0，duty为0-255负载

        [DllImport(DLL_PATH)]
        public static extern int SetFANDutyAuto(int fan_id); // 设置风扇自动

        [DllImport(DLL_PATH)]
        public static extern ECData GetTempFanDuty(int fan_id); // 得到风扇状态

        [DllImport(DLL_PATH)]
        public static extern int GetFanCount(); // 返回风扇数量

        [DllImport(DLL_PATH)]
        public static extern string GetECVersion(); // 返回版本信息

        [DllImport(DLL_PATH)]
        public static extern int GetCpuFanRpm(); // 得到风扇转速

        [DllImport(DLL_PATH)]
        public static extern int GetGpuFanRpm(); // 得到风扇转速
    }
}
