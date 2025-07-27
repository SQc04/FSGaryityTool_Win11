using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.ComponentModel;

namespace FSGaryityTool_Win11.Controls;

public sealed partial class FanAUserControl : UserControl, INotifyPropertyChanged
{
    public FanAUserControl()
    {
        InitializeComponent();

    }

    // 温度属性
    private int _temperature;
    public int Temperature
    {
        get => _temperature;
        set
        {
            if (_temperature != value)
            {
                _temperature = value;
                OnPropertyChanged(nameof(Temperature));
            }
        }
    }

    public string FanName
    {
        get => (string)GetValue(FanNameProperty);
        set
        {
            SetValue(FanNameProperty, value);
            OnPropertyChanged(nameof(FanName));
        }
    }

    private int _fanRpm;
    public int FanRpm
    {
        get => _fanRpm;
        set
        {
            if (_fanRpm != value)
            {
                _fanRpm = value;
                OnPropertyChanged(nameof(FanRpm));
            }
        }
    }

    private double _fanControlPercentage;
    public double FanControlPercentage
    {
        get => _fanControlPercentage;
        set
        {
            if (_fanControlPercentage != value)
            {
                _fanControlPercentage = value;
                OnPropertyChanged(nameof(FanControlPercentage));
            }
        }
    }

    public static readonly DependencyProperty FanNameProperty =
        DependencyProperty.Register(nameof(FanName), typeof(string), typeof(FanAUserControl), new("N/A FAN"));

    public double FanRpmMaximum
    {
        get => (double)GetValue(FanRpmMaximumProperty);
        set
        {
            SetValue(FanRpmMaximumProperty, value);
            FanRpmRadialGauge.Maximum = value;
            OnPropertyChanged(nameof(FanRpmMaximum));
        }
    }

    public static readonly DependencyProperty FanRpmMaximumProperty =
        DependencyProperty.Register(nameof(FanRpmMaximum), typeof(double), typeof(FanAUserControl), new(8000.0));

    // INotifyPropertyChanged实现
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
