using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FSGaryityTool_Win11.Controls
{

    public enum BeyondTextBoxEditorMode
    {
        Entry,
        Text,
        Binary
    }
    public enum BeyondTextBoxTextAlignment
    {
        Left,
        Center,
        Right
    }

    public class TextCharItem
    {
        public string Char { get; set; }
        public bool IsSelected { get; set; }
        public bool IsCaret { get; set; }
    }

    public class EditorModeToIndexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
            => value is BeyondTextBoxEditorMode mode ? (int)mode : 0;

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => Enum.IsDefined(typeof(BeyondTextBoxEditorMode), value) ? (BeyondTextBoxEditorMode)(int)value : BeyondTextBoxEditorMode.Entry;
    }

    public class TextAlignmentToIndexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
            => value is BeyondTextBoxTextAlignment align ? (int)align : 0;

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => Enum.IsDefined(typeof(BeyondTextBoxTextAlignment), value) ? (BeyondTextBoxTextAlignment)(int)value : BeyondTextBoxTextAlignment.Left;
    }
    public class FontSizeToCaretHeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is double fontSize)
            {
                // 推荐系数：1.3，可根据实际视觉调整
                return fontSize * 1.32;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }
    
    public sealed partial class BeyondTextBox : UserControl, INotifyPropertyChanged
    {
        // INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public static readonly DependencyProperty IsInsertModeProperty =
        DependencyProperty.Register(
            nameof(IsInsertMode),
            typeof(bool),
            typeof(BeyondTextBox),
            new PropertyMetadata(true, OnIsInsertModeChanged)
        );

        public bool IsInsertMode
        {
            get => (bool)GetValue(IsInsertModeProperty);
            set
            {
                if (IsInsertMode != value)
                {
                    SetValue(IsInsertModeProperty, value);
                    OnPropertyChanged(nameof(IsInsertMode));
                }
            }
        }

        private static void OnIsInsertModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is BeyondTextBox control)
            {
                control.UpdateCaretWidthByMode();
                control.BeginCaretBlinkStoryboard();
            }
        }
        // Text
        public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(BeyondTextBox),
            new PropertyMetadata(string.Empty, OnTextChanged)
        );

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set
            {
                if (Text != value)
                {
                    SetValue(TextProperty, value);
                    OnPropertyChanged(nameof(Text));
                }
            }
        }
        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is BeyondTextBox control)
            {
                control.OnPropertyChanged(nameof(Text));
                control.UpdateCharItems();
            }
        }
        

        // TextAlignment
        public static readonly DependencyProperty TextAlignmentProperty =
        DependencyProperty.Register(
            nameof(TextAlignment),
            typeof(BeyondTextBoxTextAlignment),
            typeof(BeyondTextBox),
            new PropertyMetadata(BeyondTextBoxTextAlignment.Left, OnTextAlignmentChanged)
        );

        public BeyondTextBoxTextAlignment TextAlignment
        {
            get => (BeyondTextBoxTextAlignment)GetValue(TextAlignmentProperty);
            set
            {
                if (TextAlignment != value)
                {
                    SetValue(TextAlignmentProperty, value);
                    OnPropertyChanged(nameof(TextAlignment));
                }
            }
        }
        private static void OnTextAlignmentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is BeyondTextBox control)
                control.OnPropertyChanged(nameof(TextAlignment));
        }

        // EditorMode
        public static readonly DependencyProperty EditorModeProperty =
        DependencyProperty.Register(
            nameof(EditorMode),
            typeof(BeyondTextBoxEditorMode),
            typeof(BeyondTextBox),
            new PropertyMetadata(BeyondTextBoxEditorMode.Entry, OnEditorModeChanged)
        );

        public BeyondTextBoxEditorMode EditorMode
        {
            get => (BeyondTextBoxEditorMode)GetValue(EditorModeProperty);
            set
            {
                if (EditorMode != value)
                {
                    SetValue(EditorModeProperty, value);
                    OnPropertyChanged(nameof(EditorMode));
                }
            }
        }
        private static void OnEditorModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is BeyondTextBox control)
            {
                control.OnPropertyChanged(nameof(EditorMode));
            }
        }

        // IsConnect
        public static readonly DependencyProperty IsConnectProperty =
        DependencyProperty.Register(
            nameof(IsConnect),
            typeof(bool),
            typeof(BeyondTextBox),
            new PropertyMetadata(false, OnIsConnectChanged)
        );

        public bool IsConnect
        {
            get => (bool)GetValue(IsConnectProperty);
            set
            {
                if (IsConnect != value)
                {
                    SetValue(IsConnectProperty, value);
                    OnPropertyChanged(nameof(IsConnect));
                }
            }
        }
        private static void OnIsConnectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is BeyondTextBox control)
            {
                control.UpdateBorderBrush((bool)e.NewValue);
                control.OnPropertyChanged(nameof(IsConnect));
            }
        }
        private double _caretWidth = 2;
        private double CaretWidth
        {
            get => _caretWidth;
            set
            {
                if (_caretWidth != value)
                {
                    _caretWidth = value;
                    OnPropertyChanged(nameof(CaretWidth));
                }
            }
        }
        public InputCursor InputCursor
        {
            get => ProtectedCursor;
            set => ProtectedCursor = value;
        }
        public BeyondTextBox()
        {
            InitializeComponent();
            this.Loaded += OnLoaded;
            this.Unloaded += OnUnloaded;
            this.ActualThemeChanged += OnActualThemeChanged;
            this.SizeChanged += BoxRootGrid_SizeChanged;

            this.KeyDown += BeyondTextBox_KeyDown;

            IsInsertMode = false;
        }


        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            UpdateBorderBrush(IsConnect);
            LostCaretBorder(_isFocused);
            UpdateCaretWidthByMode();

        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            this.ActualThemeChanged -= OnActualThemeChanged;
        }


        private void OnActualThemeChanged(FrameworkElement sender, object args)
        {
            UpdateBorderBrush(IsConnect);
        }

        private void BeyondTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Insert)
            {
                IsInsertMode = !IsInsertMode;
                e.Handled = true;
            }
        }

        private void UpdateBorderBrush(bool isConnect)
        {
            if (isConnect)
            {
                BoxRootGrid.BorderBrush = (Brush)Application.Current.Resources["TextControlElevationBorderFocusedBrush"];
            }
            else
            {
                BoxRootGrid.BorderBrush = (Brush)Application.Current.Resources[_isFocused
                    ? "TextControlElevationBorderFocusedBrush"
                    : "TextControlElevationBorderBrush"];
            }
        }

        private void LostCaretBorder(bool isFocused)
        {
            if (isFocused)
            {
                CaretBorder.Visibility = Visibility.Visible;
            }
            else
            {
                CaretBorder.Visibility = Visibility.Collapsed;
            }
        }

        private void BoxRootGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var widthTriggerWide = (WidthStateTrigger)Wide.StateTriggers.First();
            widthTriggerWide.UpdateTrigger(BoxRootGrid.ActualWidth);
            var widthTriggerNarrow = (WidthStateTrigger)Narrow.StateTriggers.First();
            widthTriggerNarrow.UpdateTrigger(BoxRootGrid.ActualWidth);
        }

        private void BoxSettingsButton_PointerEntered(object sender, PointerRoutedEventArgs e) { }
        private void BoxSettingsButton_PointerExited(object sender, PointerRoutedEventArgs e) { }

        private void OnFadeOutCompleted(object sender, object e)
        {
            if (BoxRootGrid.ActualWidth >= 350)
            {
                BoxButtonStackPanel.Orientation = Orientation.Horizontal;
            }
            else
            {
                BoxButtonStackPanel.Orientation = Orientation.Vertical;
            }

            var fadeInStoryboard = (Storyboard)this.Resources["FadeInStoryboard"];
            fadeInStoryboard.Begin();
        }

        private void BoxMenuButton_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            AnimatedIcon.SetState(this.SearchAnimatedIcon, "PointerOver");
        }

        private void BoxMenuButton_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            AnimatedIcon.SetState(this.SearchAnimatedIcon, "Normal");
        }


        private bool _isFocused = false;
        private InputCursor? OriginalInputCursor { get; set; }

        private void BoxRootGrid_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (!_isFocused)
                VisualStateManager.GoToState(this, "PointerOver", true);

            OriginalInputCursor = InputCursor ?? InputSystemCursor.Create(InputSystemCursorShape.Arrow);
            InputCursor = InputSystemCursor.Create(InputSystemCursorShape.IBeam);
        }

        private void BoxRootGrid_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (!_isFocused)
                VisualStateManager.GoToState(this, "Normal", true);
            if (OriginalInputCursor != null)
            {
                InputCursor = OriginalInputCursor;
            }
        }
        private void BoxRootGrid_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            BoxRootGrid.Focus(FocusState.Programmatic);
            e.Handled = true;
        }
        private void BoxRootGrid_GotFocus(object sender, RoutedEventArgs e)
        {
            _isFocused = true;
            VisualStateManager.GoToState(this, "Focused", true);
            UpdateBorderBrush(IsConnect);
            LostCaretBorder(_isFocused);
            UpdateCaretWidthByMode();
            BeginCaretBlinkStoryboard();
        }

        private void BoxRootGrid_LostFocus(object sender, RoutedEventArgs e)
        {
            _isFocused = false;
            VisualStateManager.GoToState(this, "Normal", true);
            UpdateBorderBrush(IsConnect);
            UpdateCaretWidthByMode();
            LostCaretBorder(_isFocused);
        }
        private void BeginCaretBlinkStoryboard()
        {
            var storyboard = (Storyboard)this.Resources["CaretBlinkStoryboard"];
            storyboard.Stop(); // 确保每次都从头开始
            storyboard.Begin();
        }
        private double GetCurrentCharWidth()
        {
            
            return 16;
        }
        private int GetSystemCaretWidth()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop");
                var regValue = key?.GetValue("CaretWidth")?.ToString();
                if (int.TryParse(regValue, out int caretWidth))
                    return Math.Max(caretWidth, 1);
            }
            catch { }
            return 2; // 默认宽度
        }


        private void UpdateCaretWidthByMode()
        {
            if (IsInsertMode) // INS模式
                CaretWidth = GetCurrentCharWidth();
            else
                CaretWidth = GetSystemCaretWidth(); // 非INS模式宽度
        }


        private List<TextCharItem> _charItems = new();
        public List<TextCharItem> CharItems
        {
            get => _charItems;
            set
            {
                _charItems = value;
                OnPropertyChanged(nameof(CharItems));
            }
        }

        private int _caretIndex = 0;
        public int CaretIndex
        {
            get => _caretIndex;
            set
            {
                if (_caretIndex != value)
                {
                    _caretIndex = value;
                    UpdateCaret();
                }
            }
        }

        private void UpdateCharItems()
        {
            
        }

        private void UpdateCaret()
        {
            for (int i = 0; i < CharItems.Count; i++)
                CharItems[i].IsCaret = (i == CaretIndex);
            OnPropertyChanged(nameof(CharItems));
        }


    }
}
