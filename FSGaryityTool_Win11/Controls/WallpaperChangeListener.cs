using Microsoft.Win32;
using System;

namespace FSGaryityTool_Win11.Controls;

public class WallpaperChangeListener : IDisposable
{
    public event EventHandler WallpaperChanged;

    public WallpaperChangeListener()
    {
        SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;
    }

    private void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
    {
        if (e.Category is UserPreferenceCategory.Desktop)
        {
            WallpaperChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        SystemEvents.UserPreferenceChanged -= OnUserPreferenceChanged;
    }
}
