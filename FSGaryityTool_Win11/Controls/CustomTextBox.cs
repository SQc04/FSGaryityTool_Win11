using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace FSGaryityTool_Win11.Controls;

public class CustomTextBox : TextBox
{
    private ScrollViewer _scrollViewer;

    public static readonly DependencyProperty ScrollToBottomProperty =
        DependencyProperty.Register(
            nameof(IsScrollToBottom),
            typeof(bool),
            typeof(CustomTextBox),
            new(false, OnScrollToBottomChanged));

    public bool IsScrollToBottom
    {
        get => (bool)GetValue(ScrollToBottomProperty);
        set => SetValue(ScrollToBottomProperty, value);
    }

    private static void OnScrollToBottomChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CustomTextBox textBox && (bool)e.NewValue)
        {
            textBox.ScrollToBottom();
        }
    }

    public CustomTextBox()
    {
        TextChanged += CustomTextBox_TextChanged;
    }

    private void CustomTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (IsScrollToBottom)
        {
            ScrollToBottom();
        }
    }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _scrollViewer = GetTemplateChild("ScrollViewer") as ScrollViewer;
    }

    public void ScrollToBottom()
    {
        if (_scrollViewer is not null && Text.Length > 0)
        {
            _scrollViewer.ChangeView(null, _scrollViewer.ExtentHeight, null);
        }
    }
}
