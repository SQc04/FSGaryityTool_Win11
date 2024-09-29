using Microsoft.UI.Xaml;
using System.Threading.Tasks;

namespace FSGaryityTool_Win11;

public partial class App : Application
{
    public App() => InitializeComponent();

    public static ulong MainWindowHandle => Window.AppWindow.Id.Value;

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        Window = new MainWindow();

        // 只有在 Window.Content 为 null 时才创建 ExtendedSplash
        if (Window.Content is null)
        {
            // 创建 ExtendedSplash 实例
            var extendedSplash = new ExtendedSplash(Window);
            Window.Content = extendedSplash;
        }

        // 确保当前窗口处于活动状态
        await Task.Delay(100);
        Window.Activate();
    }

    public static Window Window { get; private set; }

    public static void RemoveExtendedSplash(UIElement mainContent)
    {
        if (Window.Content is ExtendedSplash)
        {
            // 将 Window.Content 设置为 MainWindow 的内容
            Window.Content = mainContent;
        }
    }
}
