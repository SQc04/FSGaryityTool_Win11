using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FSGaryityTool_Win11.Controls
{
    public class TaskbarProgress
    {
        public ITaskbarList3 _taskbarInstance = (ITaskbarList3)new TaskbarList();

        public nint hWnd;

        public static MainWindow Instance { get; private set; }

        [DllImport("user32.dll")]
        private static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

        [StructLayout(LayoutKind.Sequential)]
        private struct FLASHWINFO
        {
            public uint cbSize;
            public IntPtr hwnd;
            public uint dwFlags;
            public uint uCount;
            public uint dwTimeout;
        }

        private const uint FLASHW_STOP = 0x00000000;
        private const uint FLASHW_ALL = 0x00000003;
        private const uint FLASHW_TIMERNOFG = 0x0000000C;

        private void FlashTaskbar(IntPtr hWnd)
        {
            var fw = new FLASHWINFO
            {
                cbSize = (uint)Marshal.SizeOf(typeof(FLASHWINFO)),
                hwnd = hWnd,
                dwFlags = FLASHW_ALL,
                uCount = 3,
                dwTimeout = 0
            };
            FlashWindowEx(ref fw);
        }
        private void StopFlashing(IntPtr hWnd)
        {
            var fw = new FLASHWINFO
            {
                cbSize = (uint)Marshal.SizeOf(typeof(FLASHWINFO)),
                hwnd = hWnd,
                dwFlags = FLASHW_STOP,
                uCount = 0,
                dwTimeout = 0
            };
            FlashWindowEx(ref fw);
        }


        public enum TaskbarProgressState
        {
            Indeterminate,  // 不确定进度（加载动画）
            Normal,         // 正常进度
            Paused,         // 暂停状态
            Error,          // 错误状态
            NoProgress      // 无进度/隐藏
        }

        public void SetTaskbarProgressValue(int? value, TaskbarProgressState state)
        {
            if (hWnd == IntPtr.Zero) return;
            else Debug.WriteLine(hWnd);

            try
            {
                switch (state)
                {
                    case TaskbarProgressState.Indeterminate:
                        _taskbarInstance.SetProgressState(hWnd, TBPFLAG.TBPF_INDETERMINATE);
                        break;

                    case TaskbarProgressState.Normal:
                        _taskbarInstance.SetProgressState(hWnd, TBPFLAG.TBPF_NORMAL);
                        if (value is not null)
                            _taskbarInstance.SetProgressValue(hWnd, (ulong)Math.Clamp(value.Value, 0, 100), 100);
                        break;

                    case TaskbarProgressState.Paused:
                        if (value is not null)
                        {
                            _taskbarInstance.SetProgressState(hWnd, TBPFLAG.TBPF_PAUSED);
                            _taskbarInstance.SetProgressValue(hWnd, (ulong)Math.Clamp(value.Value, 0, 100), 100);
                        }
                        else
                            FlashTaskbar(hWnd);
                        break;

                    case TaskbarProgressState.Error:
                        if (value is not null)
                        {
                            _taskbarInstance.SetProgressState(hWnd, TBPFLAG.TBPF_ERROR);
                            _taskbarInstance.SetProgressValue(hWnd, (ulong)Math.Clamp(value.Value, 0, 100), 100);
                        }
                        else
                            FlashTaskbar(hWnd);
                        break;

                    case TaskbarProgressState.NoProgress:
                    default:
                        _taskbarInstance.SetProgressState(hWnd, TBPFLAG.TBPF_NOPROGRESS);
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"设置任务栏进度失败: {ex.Message}");
            }
        }
    }
}
