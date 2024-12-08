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
using Microsoft.UI.Xaml.Media.Animation;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FSGaryityTool_Win11.Controls
{
    public partial class DataItem : INotifyPropertyChanged
    {
        private string _timesr;
        public string Timesr
        {
            get => _timesr;
            set
            {
                if (_timesr != value)
                {
                    _timesr = value;
                    OnPropertyChanged(nameof(Timesr));
                }
            }
        }

        private string _rxstr;
        public string Rxstr
        {
            get => _rxstr;
            set
            {
                if (_rxstr != value)
                {
                    _rxstr = value;
                    OnPropertyChanged(nameof(Rxstr));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    public partial class ViewModel : INotifyPropertyChanged
    {

        private ObservableCollection<DataItem> _dataList;
        public ObservableCollection<DataItem> DataList
        {
            get => _dataList;
            set
            {
                _dataList = value;
                OnPropertyChanged(nameof(DataList));
            }
        }

        private string _rxTextinfo;
        public string RxTextinfo
        {
            get => _rxTextinfo;
            set
            {
                if (_rxTextinfo != value)
                {
                    _rxTextinfo = value;
                    OnPropertyChanged(nameof(RxTextinfo));
                    ProcessRxTextinfo();
                }
            }
        }

        private StringBuilder _buffer = new StringBuilder();

        public ViewModel()
        {
            DataList = new ObservableCollection<DataItem>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ProcessRxTextinfo()
        {
            // 将新的数据追加到缓冲区
            _buffer.Append(_rxTextinfo);

            // 检查缓冲区是否包含换行符
            string bufferContent = _buffer.ToString();
            string[] lines = bufferContent.Split(new[] { "\n" }, StringSplitOptions.None);

            // 处理每一行数据
            for (int i = 0; i < lines.Length - 1; i++)
            {
                AddDataItem(new DataItem { Rxstr = lines[i] });
            }

            // 更新缓冲区，保留最后一行未完成的数据
            _buffer.Clear();
            _buffer.Append(lines.Last());
        }

        private void AddDataItem(DataItem item)
        {
            // 判断是否需要新建一行或追加到上一行
            if (DataList.Count > 0 && !DataList.Last().Rxstr.EndsWith("\n"))
            {
                DataList.Last().Rxstr += item.Rxstr;
                OnPropertyChanged(nameof(DataList));
            }
            else
            {
                item.Timesr = DateTime.Now.ToString("HH:mm:ss");
                DataList.Add(item);
            }
        }
    }

    public sealed partial class SerialPortTextListBox : UserControl
    {

        public SerialPortTextListBox()
        {
            InitializeComponent();
            var viewModel = new ViewModel();
            this.DataContext = viewModel;

            this.Loaded += OnLoaded;
            this.Unloaded += OnUnloaded;
            this.ActualThemeChanged += OnActualThemeChanged;
            this.SizeChanged += RxBoxGrid_SizeChanged;
        }

        public void AddData(Byte[] bytes)
        {
            string hex = BitConverter.ToString(bytes).Replace("-", " ");
            var viewModel = (ViewModel)DataContext;
            viewModel.RxTextinfo += hex;
        }

        private void RXListView_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            // 获取鼠标右键点击的位置
            var point = e.GetPosition(null);

            if (RxListView.Items.Count >= 0)
            {
                // 根据位置找到对应的ListViewItem
                // 如果找到了ListViewItem，将其设置为选中状态
                if (VisualTreeHelper.FindElementsInHostCoordinates(point, RxListView).FirstOrDefault(x => x is ListViewItem) is ListViewItem listViewItem)
                {
                    listViewItem.IsSelected = true;
                }
            }
        }
        private void RxstrTextBlock_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (sender is TextBlock { Parent: Grid grid })
            // 获取TextBlock的父控件（即Grid）
            {
                // 获取Grid的DataContext（即ListView的项）
                var dataItem = grid.DataContext;
                // 设置ListView的选中项
                RxListView.SelectedItem = dataItem;
            }
        }

        public static readonly DependencyProperty IsConnectProperty =
                DependencyProperty.Register("IsConnect", typeof(bool), typeof(SerialPortTextListBox), new PropertyMetadata(false, OnIsConnectChanged));

        public bool IsConnect
        {
            get { return (bool)GetValue(IsConnectProperty); }
            set { SetValue(IsConnectProperty, value); }
        }

        private static void OnIsConnectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as SerialPortTextListBox;
            control.UpdateBorderBrush((bool)e.NewValue);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            UpdateBorderBrush(IsConnect);
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            this.ActualThemeChanged -= OnActualThemeChanged;
        }

        private void OnActualThemeChanged(FrameworkElement sender, object args)
        {
            UpdateBorderBrush(IsConnect);
        }

        private void UpdateBorderBrush(bool isConnect)
        {
            BorderBackRx.BorderBrush = (Brush)Application.Current.Resources[isConnect ? "TextControlElevationBorderFocusedBrush" : "TextControlElevationBorderBrush"];
        }

        private void RxBoxGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var widthTriggerWide = (WidthStateTrigger)Wide.StateTriggers.First();
            widthTriggerWide.UpdateTrigger(RxBoxGrid.ActualWidth);
            var widthTriggerNarrow = (WidthStateTrigger)Narrow.StateTriggers.First();
            widthTriggerNarrow.UpdateTrigger(RxBoxGrid.ActualWidth);
        }

        private void OnFadeOutCompleted(object sender, object e)
        {
            if (RxBoxGrid.ActualWidth >= 300)
            {
                RxBoxButtonStackPanel.Orientation = Orientation.Horizontal;
            }
            else 
            { 
                RxBoxButtonStackPanel.Orientation = Orientation.Vertical; 
            }

            var fadeInStoryboard = (Storyboard)this.Resources["FadeInStoryboard"]; fadeInStoryboard.Begin();
        }
    }
}
