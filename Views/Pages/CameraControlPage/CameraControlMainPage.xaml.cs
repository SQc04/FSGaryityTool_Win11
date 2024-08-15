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
using OpenCvSharp;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Capture;
using Windows.Media.Capture.Frames;
using Windows.Media.Core;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FSGaryityTool_Win11.Views.Pages.CameraControlPage
{
    public sealed partial class CameraControlMainPage : Page
    {
        private MediaCapture mediaCapture;
        private MediaFrameSourceGroup selectedFrameSourceGroup;
        private VideoCapture topCameraCapture;
        private VideoCapture bottomCameraCapture;

        public CameraControlMainPage()
        {
            this.InitializeComponent();
            LoadCameras();
            InitializeExposureComboBoxes();
        }

        private async void LoadCameras()
        {
            var groups = await MediaFrameSourceGroup.FindAllAsync();
            TopCameraSelectComboBox.ItemsSource = groups.Select(g => new { g.DisplayName, g.Id }).ToList();
            BottomCameraSelectComboBox.ItemsSource = groups.Select(g => new { g.DisplayName, g.Id }).ToList();
        }

        private void InitializeExposureComboBoxes()
        {
            var exposureValues = Enumerable.Range(-13, 14).Select(i => (double)i).ToList(); // 曝光值范围 -13 到 0
            TopCameraExposureComboBox.ItemsSource = exposureValues;
            BottomCameraExposureComboBox.ItemsSource = exposureValues;
        }

        private async void TopCameraSelectComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedGroup = (dynamic)TopCameraSelectComboBox.SelectedItem;
            selectedFrameSourceGroup = (await MediaFrameSourceGroup.FindAllAsync()).FirstOrDefault(g => g.Id == selectedGroup.Id);
            await InitializeMediaCapture(selectedFrameSourceGroup, TopCameraMediaPlayerElement);
            InitializeOpenCVCapture(ref topCameraCapture, 0); // 假设第一个摄像头索引为0
        }

        private async void BottomCameraSelectComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedGroup = (dynamic)BottomCameraSelectComboBox.SelectedItem;
            selectedFrameSourceGroup = (await MediaFrameSourceGroup.FindAllAsync()).FirstOrDefault(g => g.Id == selectedGroup.Id);
            await InitializeMediaCapture(selectedFrameSourceGroup, BottomCameraMediaPlayerElement);
            InitializeOpenCVCapture(ref bottomCameraCapture, 1); // 假设第二个摄像头索引为1
        }

        private async System.Threading.Tasks.Task InitializeMediaCapture(MediaFrameSourceGroup frameSourceGroup, MediaPlayerElement mediaPlayerElement)
        {
            mediaCapture = new MediaCapture();
            var settings = new MediaCaptureInitializationSettings
            {
                SourceGroup = frameSourceGroup,
                SharingMode = MediaCaptureSharingMode.SharedReadOnly,
                StreamingCaptureMode = StreamingCaptureMode.Video,
                MemoryPreference = MediaCaptureMemoryPreference.Cpu
            };
            await mediaCapture.InitializeAsync(settings);

            var frameSource = mediaCapture.FrameSources[frameSourceGroup.SourceInfos[0].Id];
            mediaPlayerElement.Source = MediaSource.CreateFromMediaFrameSource(frameSource);
        }

        private void InitializeOpenCVCapture(ref VideoCapture capture, int cameraIndex)
        {
            capture = new VideoCapture(cameraIndex);
            if (!capture.IsOpened())
            {
                Console.WriteLine($"无法打开摄像头 {cameraIndex}");
                return;
            }
        }

        private void TopCameraAutoExposureToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            SetExposureMode(topCameraCapture, true);
        }

        private void TopCameraAutoExposureToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            SetExposureMode(topCameraCapture, false);
        }

        private void BottomCameraAutoExposureToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            SetExposureMode(bottomCameraCapture, true);
        }

        private void BottomCameraAutoExposureToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            SetExposureMode(bottomCameraCapture, false);
        }

        private void TopCameraExposureComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TopCameraExposureComboBox.SelectedItem != null)
            {
                double exposureValue = (double)TopCameraExposureComboBox.SelectedItem;
                SetExposureValue(topCameraCapture, exposureValue);
            }
        }

        private void BottomCameraExposureComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BottomCameraExposureComboBox.SelectedItem != null)
            {
                double exposureValue = (double)BottomCameraExposureComboBox.SelectedItem;
                SetExposureValue(bottomCameraCapture, exposureValue);
            }
        }

        private void SetExposureMode(VideoCapture capture, bool autoExposure)
        {
            if (autoExposure)
            {
                capture.Set(VideoCaptureProperties.AutoExposure, 0.75); // 启用自动曝光
            }
            else
            {
                capture.Set(VideoCaptureProperties.AutoExposure, 0.25); // 禁用自动曝光
            }
        }

        private void SetExposureValue(VideoCapture capture, double exposureValue)
        {
            capture.Set(VideoCaptureProperties.Exposure, exposureValue); // 设置手动曝光值
        }
    }
}
