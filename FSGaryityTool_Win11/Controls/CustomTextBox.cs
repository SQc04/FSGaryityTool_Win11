using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FSGaryityTool_Win11.Controls
{
    public class CustomTextBox : TextBox
    {
        private ScrollViewer _scrollViewer;

        public static readonly DependencyProperty ScrollToBottomProperty =
            DependencyProperty.Register(
                "IsScrollToBottom",
                typeof(bool),
                typeof(CustomTextBox),
                new PropertyMetadata(false, OnScrollToBottomChanged));

        public bool IsScrollToBottom
        {
            get { return (bool)GetValue(ScrollToBottomProperty); }
            set { SetValue(ScrollToBottomProperty, value); }
        }

        private static void OnScrollToBottomChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var textBox = d as CustomTextBox;
            if (textBox != null && (bool)e.NewValue)
            {
                textBox.ScrollToBottom();
            }
        }

        public CustomTextBox()
        {
            this.TextChanged += CustomTextBox_TextChanged;
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
            if (_scrollViewer != null && this.Text.Length > 0)
            {
                _scrollViewer.ChangeView(null, _scrollViewer.ExtentHeight, null);
            }
        }
    }

}
