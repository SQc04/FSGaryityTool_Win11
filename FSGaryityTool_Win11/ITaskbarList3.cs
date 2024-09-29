using System.Runtime.InteropServices;

namespace FSGaryityTool_Win11;

[ComImport]
[Guid("ea1afb91-9e28-4b86-90e9-9e9f8a5eefaf")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public interface ITaskbarList3
{
    // ITaskbarList
    void HrInit();
    void AddTab(nint hwnd);
    void DeleteTab(nint hwnd);
    void ActivateTab(nint hwnd);
    void SetActiveAlt(nint hwnd);

    // ITaskbarList2
    void MarkFullscreenWindow(nint hwnd, [MarshalAs(UnmanagedType.Bool)] bool fFullscreen);

    // ITaskbarList3
    void SetProgressValue(nint hwnd, ulong ullCompleted, ulong ullTotal);
    void SetProgressState(nint hwnd, TBPFLAG tbpFlags);
}

public enum TBPFLAG
{
    TBPF_NOPROGRESS = 0,
    TBPF_INDETERMINATE = 0x1,
    TBPF_NORMAL = 0x2,
    TBPF_ERROR = 0x4,
    TBPF_PAUSED = 0x8
}

[ComImport]
[Guid("56FDF344-FD6D-11d0-958A-006097C9A090")]
public class TaskbarList;
