using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System.Collections.ObjectModel;

namespace FSGaryityTool_Win11;

public sealed partial class MainSettingsPage : Page
{
    public static MainSettingsPage Current { get; private set; }

    public MainSettingsPage()
    {
        InitializeComponent();
        Current = this;
        SettingsFrame.Navigate(typeof(SettingsPage));

        SettingsBar.ItemsSource = new ObservableCollection<SettingsPage.Folder>
        {
            new() { Name = SettingsPageResources.Settings }
        };
        SettingsBar.ItemClicked += SettingsBar_ItemClicked;
    }

    public void SettingsBar_ItemClicked(BreadcrumbBar sender, BreadcrumbBarItemClickedEventArgs args)
    {
        var items = SettingsBar.ItemsSource as ObservableCollection<SettingsPage.Folder>;
        for (var i = items.Count - 1; i >= args.Index + 1; i--)
        {
            items.RemoveAt(i);
        }
        SettingsFrame.Navigate(typeof(SettingsPage), null, new SlideNavigationTransitionInfo { Effect = SlideNavigationTransitionEffect.FromLeft });
        SettingsPage.Current.AboutFrame.Visibility = Visibility.Collapsed;
    }
}
