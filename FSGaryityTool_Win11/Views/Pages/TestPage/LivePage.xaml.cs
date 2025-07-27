using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FSGaryityTool_Win11.Views.Pages.TestPage;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class LivePage : Page
{
    public LivePage()
    {
        InitializeComponent();
    }

    private void Textbox1_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (Uri.TryCreate(Textbox1.Text, UriKind.Absolute, out var uri))
        {
            Webview1.Source = uri;
        }
    }

    private void Textbox2_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (Uri.TryCreate(Textbox2.Text, UriKind.Absolute, out var uri))
        {
            Webview2.Source = uri;
        }
    }

    private void Textbox3_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (Uri.TryCreate(Textbox3.Text, UriKind.Absolute, out var uri))
        {
            Webview3.Source = uri;
        }
    }

    private void Textbox4_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (Uri.TryCreate(Textbox4.Text, UriKind.Absolute, out var uri))
        {
            Webview4.Source = uri;
        }
    }
}
