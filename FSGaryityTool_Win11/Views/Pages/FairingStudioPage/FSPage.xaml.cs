using Microsoft.UI.Xaml.Controls;

namespace FSGaryityTool_Win11.Views.Pages.FairingStudioPage;

public sealed partial class FSPage : Page
{
    public static FSPage fSPage;

    public FSPage()
    {
        InitializeComponent();
        fSPage = this;
    }
}
