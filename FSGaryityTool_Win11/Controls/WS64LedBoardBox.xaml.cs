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
using System.Runtime.CompilerServices;
using System.ComponentModel;
using Windows.UI;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FSGaryityTool_Win11.Controls
{
    public sealed partial class WS64LedBoardBox : UserControl, INotifyPropertyChanged
    {
        private Color[,] _colors = new Color[8, 8];

        public event PropertyChangedEventHandler PropertyChanged;

        public WS64LedBoardBox()
        {
            this.InitializeComponent();
        }

        public Color[,] Colors
        {
            get => _colors;
            set
            {
                _colors = value;
                OnPropertyChanged();
                UpdateLedColors();
            }
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void UpdateLedColors()
        {
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    int snakePosition = GetSnakePosition(row, col);
                    var border = (Border)FindName($"LED{snakePosition}");
                    if (border != null)
                    {
                        border.Background = new SolidColorBrush(_colors[row, col]);
                    }
                }
            }
        }

        private int GetSnakePosition(int row, int col)
        {
            if (row % 2 == 0)
            {
                return row * 8 + col;
            }
            else
            {
                return row * 8 + (7 - col);
            }
        }
    }
}
