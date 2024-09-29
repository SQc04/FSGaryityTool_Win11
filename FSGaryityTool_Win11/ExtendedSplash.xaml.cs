using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml.Media.Imaging;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FSGaryityTool_Win11
{
    public sealed partial class ExtendedSplash : UserControl
    {
        private Window mainWindow;
        private Rect splashImageRect;

        public ExtendedSplash(Window mainWindow)
        {
            this.InitializeComponent();
            this.mainWindow = mainWindow;

            // ���ó�ʼ��Ļͼ���Դ
            extendedSplashImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/SplashScreen.scale-400.png"));

            // ����չ��ʼ��Ļͼ��λ����ϵͳ��ʼ��Ļͼ����ͬ��λ��
            splashImageRect = new Rect(0, 0, mainWindow.Bounds.Width, mainWindow.Bounds.Height);
            PositionImage();
        }


        // ��λͼ��
        void PositionImage()
        {
            extendedSplashImage.SetValue(Canvas.LeftProperty, splashImageRect.X);
            extendedSplashImage.SetValue(Canvas.TopProperty, splashImageRect.Y);
            extendedSplashImage.Height = splashImageRect.Height;
            extendedSplashImage.Width = splashImageRect.Width;
        }

        // �����ڴ�С����ʱ�����³�ʼ��Ļͼ�������
        public void ExtendedSplash_OnResize(Object sender, WindowSizeChangedEventArgs e)
        {
            // ��ȫ�ظ�����չ��ʼ��Ļͼ������ꡣ�˺������ܻ�����ӦӦ�ó�����ͼ״̬���Ļ򴰿ڴ�С�����¼�ʱ������
            if (mainWindow != null)
            {
                // ���³�ʼ��Ļͼ�������
                PositionImage();
            }
        }

        // ��������������ע�� SizeChanged �¼�������
        public void RegisterSizeChangedEvent()
        {
            Window.Current.SizeChanged += ExtendedSplash_OnResize;
        }
    }
}

