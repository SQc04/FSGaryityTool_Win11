using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FSGaryityTool_Win11.Controls
{
    // 版本阶段
    public enum TieleBadgeVersionStage
    {
        None, // 默认值，无标签
        Dev,
        Alpha,
        Beta,
        Canary,
        Preview,
        RC,
        Nightly,
        Stable
    }

    // 用户群体
    public enum TieleBadgeUserGroup
    {
        None, // 默认值，无标签
        Insider,
        Community,
        Enterprise,
        Pro,
        Custom,
        Legacy
    }

    // 构建类型
    public enum TieleBadgeBuildType
    {
        None, // 默认值，无标签
        Experimental,
        Debug,
        Sandbox,
        Prototype
    }

    public sealed partial class AppTitleBar : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // AppLogoSource 依赖属性
        public static readonly DependencyProperty AppLogoSourceProperty =
            DependencyProperty.Register(
                nameof(AppLogoSource),
                typeof(ImageSource),
                typeof(AppTitleBar),
                new PropertyMetadata(null, OnAppLogoSourceChanged));

        public ImageSource AppLogoSource
        {
            get => (ImageSource)GetValue(AppLogoSourceProperty);
            set
            {
                SetValue(AppLogoSourceProperty, value);
                OnPropertyChanged(nameof(AppLogoSource));
            }
        }

        private static void OnAppLogoSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (AppTitleBar)d;
            control.OnPropertyChanged(nameof(AppLogoSource));
        }

        public static readonly DependencyProperty CurrentVersionStageProperty =
            DependencyProperty.Register(
                nameof(CurrentVersionStage),
                typeof(TieleBadgeVersionStage),
                typeof(AppTitleBar),
                new PropertyMetadata(TieleBadgeVersionStage.None, OnCurrentVersionStageChanged));

        public TieleBadgeVersionStage CurrentVersionStage
        {
            get => (TieleBadgeVersionStage)GetValue(CurrentVersionStageProperty);
            set 
            {
                SetValue(CurrentVersionStageProperty, value);
                OnPropertyChanged(nameof(CurrentVersionStage));
            }
        }

        private static void OnCurrentVersionStageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (AppTitleBar)d;
            control.SetVersionStage((TieleBadgeVersionStage)e.NewValue);
            control.OnPropertyChanged(nameof(CurrentVersionStage));
        }

        // CurrentUserGroup 依赖属性
        public static readonly DependencyProperty CurrentUserGroupProperty =
            DependencyProperty.Register(
                nameof(CurrentUserGroup),
                typeof(TieleBadgeUserGroup),
                typeof(AppTitleBar),
                new PropertyMetadata(TieleBadgeUserGroup.None, OnCurrentUserGroupChanged));

        public TieleBadgeUserGroup CurrentUserGroup
        {
            get => (TieleBadgeUserGroup)GetValue(CurrentUserGroupProperty);
            set
            {
                SetValue(CurrentUserGroupProperty, value);
                OnPropertyChanged(nameof(CurrentUserGroup));
            }
        }

        private static void OnCurrentUserGroupChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (AppTitleBar)d;
            control.SetUserGroup((TieleBadgeUserGroup)e.NewValue);
            control.OnPropertyChanged(nameof(CurrentUserGroup));
        }

        // CurrentBuildType 依赖属性
        public static readonly DependencyProperty CurrentBuildTypeProperty =
            DependencyProperty.Register(
                nameof(CurrentBuildType),
                typeof(TieleBadgeBuildType),
                typeof(AppTitleBar),
                new PropertyMetadata(TieleBadgeBuildType.None, OnCurrentBuildTypeChanged));

        public TieleBadgeBuildType CurrentBuildType
        {
            get => (TieleBadgeBuildType)GetValue(CurrentBuildTypeProperty);
            set
            {
                SetValue(CurrentBuildTypeProperty, value);
                OnPropertyChanged(nameof(CurrentBuildType));
            }
        }

        private static void OnCurrentBuildTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (AppTitleBar)d;
            control.SetBuildType((TieleBadgeBuildType)e.NewValue);
            control.OnPropertyChanged(nameof(CurrentBuildType));
        }

        public static readonly DependencyProperty AppTitleNameProperty =
            DependencyProperty.Register(
                nameof(AppTitleName),
                typeof(string),
                typeof(AppTitleBar),
                new PropertyMetadata(string.Empty, OnAppTitleNameChanged));

        public string AppTitleName
        {
            get => (string)GetValue(AppTitleNameProperty);
            set
            {
                SetValue(AppTitleNameProperty, value);
                OnPropertyChanged(nameof(AppTitleName));
            }
        }

        private static void OnAppTitleNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (AppTitleBar)d;
            control.OnPropertyChanged(nameof(AppTitleName));
        }
        

        public AppTitleBar()
        {
            InitializeComponent();
        }

        public void UpDateTieleBadge()
        {
            SetVersionStage(CurrentVersionStage);
            SetUserGroup(CurrentUserGroup);
            SetBuildType(CurrentBuildType);
        }

        public void SetVersionStage(TieleBadgeVersionStage versionStage)
        {
            ClearBuildType();
            // 设置版本阶段标签
            switch (versionStage)
            {
                case TieleBadgeVersionStage.Dev: TitleDev.Visibility = Visibility.Visible; break;
                case TieleBadgeVersionStage.Alpha: TitleAlpha.Visibility = Visibility.Visible; break;
                case TieleBadgeVersionStage.Beta: TitleBeta.Visibility = Visibility.Visible; break;
                case TieleBadgeVersionStage.Canary: TitleCanary.Visibility = Visibility.Visible; break;
                case TieleBadgeVersionStage.Preview: TitlePreview.Visibility = Visibility.Visible; break;
                case TieleBadgeVersionStage.RC: TitleRC.Visibility = Visibility.Visible; break;
                case TieleBadgeVersionStage.Nightly: TitleNightly.Visibility = Visibility.Visible; break;
                case TieleBadgeVersionStage.Stable: TitleStable.Visibility = Visibility.Visible; break;
                case TieleBadgeVersionStage.None: break; // 不显示任何版本阶段标签
            }
        }

        public void ClearVersionStage()
        {
            TitleDev.Visibility = Visibility.Collapsed;
            TitleAlpha.Visibility = Visibility.Collapsed;
            TitleBeta.Visibility = Visibility.Collapsed;
            TitleCanary.Visibility = Visibility.Collapsed;
            TitlePreview.Visibility = Visibility.Collapsed;
            TitleRC.Visibility = Visibility.Collapsed;
            TitleNightly.Visibility = Visibility.Collapsed;
            TitleStable.Visibility = Visibility.Collapsed;
        }

        public void SetUserGroup(TieleBadgeUserGroup userGroup)
        {
            ClearUserGroup();
            // 设置用户群体标签
            switch (userGroup)
            {
                case TieleBadgeUserGroup.Insider: TitleInsider.Visibility = Visibility.Visible; break;
                case TieleBadgeUserGroup.Community: TitleCommunity.Visibility = Visibility.Visible; break;
                case TieleBadgeUserGroup.Enterprise: TitleEnterprise.Visibility = Visibility.Visible; break;
                case TieleBadgeUserGroup.Pro: TitlePro.Visibility = Visibility.Visible; break;
                case TieleBadgeUserGroup.Custom: TitleCustom.Visibility = Visibility.Visible; break;
                case TieleBadgeUserGroup.Legacy: TitleLegacy.Visibility = Visibility.Visible; break;
                case TieleBadgeUserGroup.None: break; // 不显示任何用户群体标签
            }
        }

        public void ClearUserGroup()
        {
            TitleInsider.Visibility = Visibility.Collapsed;
            TitleCommunity.Visibility = Visibility.Collapsed;
            TitleEnterprise.Visibility = Visibility.Collapsed;
            TitlePro.Visibility = Visibility.Collapsed;
            TitleCustom.Visibility = Visibility.Collapsed;
            TitleLegacy.Visibility = Visibility.Collapsed;
        }

        public void SetBuildType(TieleBadgeBuildType buildType)
        {
            ClearBuildType();
            // 设置构建类型标签
            switch (buildType)
            {
                case TieleBadgeBuildType.Experimental: TitleExperimental.Visibility = Visibility.Visible; break;
                case TieleBadgeBuildType.Debug: TitleDebug.Visibility = Visibility.Visible; break;
                case TieleBadgeBuildType.Sandbox: TitleSandbox.Visibility = Visibility.Visible; break;
                case TieleBadgeBuildType.Prototype: TitlePrototype.Visibility = Visibility.Visible; break;
                case TieleBadgeBuildType.None: break; // 不显示任何构建类型标签
            }
        }

        public void ClearBuildType()
        {
            TitleExperimental.Visibility = Visibility.Collapsed;
            TitleDebug.Visibility = Visibility.Collapsed;
            TitleSandbox.Visibility = Visibility.Collapsed;
            TitlePrototype.Visibility = Visibility.Collapsed;
        }

    }
}
