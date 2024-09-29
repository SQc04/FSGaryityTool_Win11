using Microsoft.UI.Xaml.Controls;
using Windows.Media.Capture;
using Windows.Media.Capture.Frames;

namespace FSGaryityTool_Win11.Views.Pages.CameraControlPage;

public sealed partial class CameraControlMainPage : Page
{
    private MediaCapture _mediaCapture;
    private MediaFrameSourceGroup _selectedFrameSourceGroup;

    public CameraControlMainPage() => InitializeComponent();
}
