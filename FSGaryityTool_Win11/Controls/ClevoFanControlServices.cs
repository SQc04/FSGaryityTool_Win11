using System;
using System.Runtime.CompilerServices;

namespace FSGaryityTool_Win11.Controls
{
    public enum FanId : byte { CPU = 1, GPU = 2 }

    public readonly struct FanInfo
    {
        public double RemoteTemp { get; init; }
        public double LocalTemp { get; init; }
        public double CurrentDutyPercent { get; init; }  // 0~100.0
        public int Rpm { get; init; }
    }

    public static class ClevoFanControlServices
    {
        private const ushort EC_SC = 0x66;
        private const ushort EC_DATA = 0x62;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte ReadPort(ushort port)
            => System.Runtime.InteropServices.Marshal.ReadByte((IntPtr)port);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void WritePort(ushort port, byte value)
            => System.Runtime.InteropServices.Marshal.WriteByte((IntPtr)port, value);

        // 等待输入缓冲区空（IBF=0）
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void WaitWrite()
        {
            int timeout = 50000;
            while ((ReadPort(EC_SC) & 0x02) != 0 && timeout-- > 0)
                System.Threading.Thread.Yield();
        }

        // 等待输出缓冲区满（OBF=1）
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void WaitRead()
        {
            int timeout = 50000;
            while ((ReadPort(EC_SC) & 0x01) == 0 && timeout-- > 0)
                System.Threading.Thread.Yield();
        }

        // 对外：设置风扇占空比（double 0~100）
        public static void SetDuty(FanId fan, double percent0_100)
        {
            if (percent0_100 < 0) percent0_100 = 0;
            if (percent0_100 > 100) percent0_100 = 100;

            byte duty255 = (byte)Math.Round(percent0_100 * 2.55); // 100 → 255

            WaitWrite(); WritePort(EC_SC, 0x99);
            WaitWrite(); WritePort(EC_DATA, (byte)fan);
            WaitWrite(); WritePort(EC_DATA, duty255);
        }

        // 对外：获取完整风扇信息（全 double）
        public static FanInfo GetFanInfo(FanId fan)
        {
            WaitWrite(); WritePort(EC_SC, 0x9E);
            WaitWrite(); WritePort(EC_DATA, (byte)fan);

            WaitRead(); byte remote = ReadPort(EC_DATA);
            WaitRead(); byte local = ReadPort(EC_DATA);
            WaitRead(); byte duty255 = ReadPort(EC_DATA);

            int rpm = GetRpm(fan);

            return new FanInfo
            {
                RemoteTemp = remote,
                LocalTemp = local,
                CurrentDutyPercent = Math.Round(duty255 * 100.0 / 255.0, 1),
                Rpm = rpm
            };
        }

        // 单独读取转速
        public static int GetRpm(FanId fan)
        {
            byte hiCmd = (byte)(0xD0 + ((byte)fan - 1) * 2);
            byte loCmd = (byte)(hiCmd + 1);

            WaitWrite(); WritePort(EC_SC, 0x80); WaitWrite(); WritePort(EC_DATA, hiCmd); WaitRead(); byte hi = ReadPort(EC_DATA);
            WaitWrite(); WritePort(EC_SC, 0x80); WaitWrite(); WritePort(EC_DATA, loCmd); WaitRead(); byte lo = ReadPort(EC_DATA);

            int raw = (hi << 8) | lo;
            return raw is > 300 and < 8000 ? 2100000 / raw : 0;
        }

        // 恢复自动
        public static void SetAuto(FanId fan)
        {
            WaitWrite(); WritePort(EC_SC, 0x99);
            WaitWrite(); WritePort(EC_DATA, 0xFF);
            WaitWrite(); WritePort(EC_DATA, (byte)fan);
        }

        public static void SetAllAuto()
        {
            WaitWrite(); WritePort(EC_SC, 0x99);
            WaitWrite(); WritePort(EC_DATA, 0xFF);
            WaitWrite(); WritePort(EC_DATA, 0xFF);
        }

        // 风扇数量
        public static int GetFanCount()
        {
            try
            {
                WaitWrite(); WritePort(EC_SC, 0x80);
                WaitWrite(); WritePort(EC_DATA, 200);
                WaitRead();
                return ReadPort(EC_DATA);
            }
            catch { return 2; }
        }
    }
}