using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.UI;
using Microsoft.UI.Composition;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using WinRT;
using WinRT.Interop;

namespace FSGaryityTool_Win11.Controls
{
    internal class TransparentController : IDisposable
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect);

        [DllImport("gdi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DeleteObject(IntPtr hObject);

        [DllImport("dwmapi.dll")]
        private static extern int DwmEnableBlurBehindWindow(IntPtr hWnd, ref DWM_BLURBEHIND pBlurBehind);

        [StructLayout(LayoutKind.Sequential)]
        private struct DWM_BLURBEHIND
        {
            public DWM_BLURBEHIND_Mask dwFlags;
            public bool fEnable;
            public IntPtr hRgnBlur;
            public bool fTransitionOnMaximized;
        }

        [Flags]
        private enum DWM_BLURBEHIND_Mask
        {
            DWM_BB_ENABLE = 0x00000001,
            DWM_BB_BLURREGION = 0x00000002,
            DWM_BB_TRANSITIONONMAXIMIZED = 0x00000004
        }

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_LAYERED = 0x80000;

        private IntPtr _rgn = IntPtr.Zero;
        private bool _applied = false;
        private int _originalExStyle;
        private IntPtr _hwnd = IntPtr.Zero;
        private Window _window;
        private IntPtr _oldWndProc = IntPtr.Zero;
        private WndProcDelegate _wndProcDelegate;
        private bool _hooked = false;

        private const int WM_ERASEBKGND = 0x0014;
        private const int WM_PAINT = 0x000F;
        private const int WM_DWMCOMPOSITIONCHANGED = 0x031E; // 798
        private const int GWLP_WNDPROC = -4;

        private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW")]
        private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW")]
        private static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateSolidBrush(int crColor);

        [DllImport("user32.dll")]
        private static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        private static extern int FillRect(IntPtr hDC, ref RECT lprc, IntPtr hBrush);

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        private void HookWindowProc(IntPtr hwnd)
        {
            if (_hooked) return;
            _wndProcDelegate = new WndProcDelegate(WndProc);
            _oldWndProc = GetWindowLongPtr(hwnd, GWLP_WNDPROC);
            SetWindowLongPtr(hwnd, GWLP_WNDPROC, Marshal.GetFunctionPointerForDelegate(_wndProcDelegate));
            _hooked = true;
        }

        private IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            if (msg == WM_ERASEBKGND)
            {
                try
                {
                    var hdc = wParam;
                    if (ClearBackground(hWnd, hdc))
                    {
                        return new IntPtr(1);
                    }
                }
                catch { }
            }
            else if (msg == WM_PAINT)
            {
                try
                {
                    // get DC and clear
                    var hdc = GetDC(hWnd);
                    if (hdc != IntPtr.Zero)
                    {
                        ClearBackground(hWnd, hdc);
                        ReleaseDC(hWnd, hdc);
                    }
                }
                catch { }
            }
            else if (msg == WM_DWMCOMPOSITIONCHANGED)
            {
                try { ConfigureDwm(hWnd); } catch { }
            }

            // call original
            return CallWindowProc(_oldWndProc, hWnd, msg, wParam, lParam);
        }

        [DllImport("user32.dll")]
        private static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        private bool ClearBackground(IntPtr hwnd, IntPtr hdc)
        {
            if (GetClientRect(hwnd, out var rect))
            {
                var brush = CreateSolidBrush(0);
                FillRect(hdc, ref rect, brush);
                FillRect(hdc, ref rect, brush);
                return true;
            }
            return false;
        }

        public void Apply(Window window, IntPtr hwnd)
        {
            if (_applied) return;

            try
            {
                _window = window;
                _hwnd = hwnd;
                // 保存原始 extended style
                _originalExStyle = GetWindowLong(hwnd, GWL_EXSTYLE);

                // 移除分层窗口样式
                SetWindowLong(hwnd, GWL_EXSTYLE, _originalExStyle & ~WS_EX_LAYERED);

                // create a zero/empty region so DWM blur affects the whole window
                _rgn = CreateRectRgn(1, 1, 2, 2);
                var dmw = new DWM_BLURBEHIND()
                {
                    dwFlags = DWM_BLURBEHIND_Mask.DWM_BB_ENABLE | DWM_BLURBEHIND_Mask.DWM_BB_BLURREGION,
                    fEnable = true,
                    hRgnBlur = _rgn,
                };
                DwmEnableBlurBehindWindow(hwnd, ref dmw);

                // choose a nearly-transparent tint based on window theme to avoid white fallback
                ICompositionSupportsSystemBackdrop brushHolder = window.As<ICompositionSupportsSystemBackdrop>();
                var fe = window.Content as FrameworkElement;
                var theme = fe?.ActualTheme ?? ElementTheme.Default;
                Windows.UI.Color tint;
                if (theme == ElementTheme.Light)
                {
                    // nearly-transparent white to avoid light-theme white fallback showing desktop
                    tint = Windows.UI.Color.FromArgb(1, 255, 255, 255);
                }
                else
                {
                    // nearly-transparent black for dark or default
                    tint = Windows.UI.Color.FromArgb(1, 0, 0, 0);
                }

                var colorBrush = WindowsCompositionHelper.Compositor.CreateColorBrush(tint);
                brushHolder.SystemBackdrop = colorBrush;

                // configure DWM and hook messages to clear background (prevents desktop watermark)
                ConfigureDwm(hwnd);
                HookWindowProc(hwnd);

                _applied = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"TransparentController.Apply failed: {ex.Message}");
            }
        }

        private void ConfigureDwm(IntPtr hwnd)
        {
            try
            {
                // extend frame into client area
                var margins = new MARGINS { cxLeftWidth = 0, cxRightWidth = 0, cyTopHeight = 0, cyBottomHeight = 0 };
                DwmExtendFrameIntoClientArea(hwnd, ref margins);

                // enable blur behind with tiny region
                var rgn = CreateRectRgn(-2, -2, -1, -1);
                var dmw = new DWM_BLURBEHIND()
                {
                    dwFlags = DWM_BLURBEHIND_Mask.DWM_BB_ENABLE | DWM_BLURBEHIND_Mask.DWM_BB_BLURREGION,
                    fEnable = true,
                    hRgnBlur = rgn,
                };
                DwmEnableBlurBehindWindow(hwnd, ref dmw);
            }
            catch { }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MARGINS
        {
            public int cxLeftWidth;
            public int cxRightWidth;
            public int cyTopHeight;
            public int cyBottomHeight;
        }

        [DllImport("dwmapi.dll")]
        private static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMarInset);



        public void Dispose()
        {
            try
            {
                // Disable DWM blur
                if (_hwnd != IntPtr.Zero)
                {
                    var dmw = new DWM_BLURBEHIND()
                    {
                        dwFlags = 0,
                        fEnable = false,
                        hRgnBlur = IntPtr.Zero,
                    };
                    try { DwmEnableBlurBehindWindow(_hwnd, ref dmw); } catch { }
                }

                // Clear system backdrop
                try
                {
                    if (_window is not null)
                    {
                        ICompositionSupportsSystemBackdrop brushHolder = _window.As<ICompositionSupportsSystemBackdrop>();
                        brushHolder.SystemBackdrop = null;
                    }
                }
                catch { }

                // restore original extended style
                try
                {
                    if (_hwnd != IntPtr.Zero && _originalExStyle != 0)
                    {
                        SetWindowLong(_hwnd, GWL_EXSTYLE, _originalExStyle);
                    }
                }
                catch { }

                if (_rgn != IntPtr.Zero)
                {
                    DeleteObject(_rgn);
                    _rgn = IntPtr.Zero;
                }

                // unhook window proc
                try
                {
                    if (_hooked && _hwnd != IntPtr.Zero)
                    {
                        SetWindowLongPtr(_hwnd, GWLP_WNDPROC, _oldWndProc);
                        _hooked = false;
                        _oldWndProc = IntPtr.Zero;
                        _wndProcDelegate = null;
                    }
                }
                catch { }

                _applied = false;
                _window = null;
                _hwnd = IntPtr.Zero;
            }
            catch { }
        }

        public void RestoreWindowStyle(IntPtr hwnd)
        {
            try
            {
                if (_originalExStyle != 0)
                {
                    SetWindowLong(hwnd, GWL_EXSTYLE, _originalExStyle);
                }
            }
            catch { }
        }
    }
}
