using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Input;
using System;
using System.Numerics;
using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;
using Windows.Devices.HumanInterfaceDevice;
using Windows.Foundation;
using Windows.Graphics;
using Windows.Storage.Streams;
using Windows.UI;

namespace FSGaryityTool_Win11.Views.Pages.TestPage;

public sealed partial class TestPage1 : Page
{
    private DispatcherTimer _timer;
    private int _currentStep;
    private Random _random = new Random();

    //====================
    private const int WIDTH = 8;
    private const int HEIGHT = 8;
    private const int NUM_PIXELS = 64;

    private const int STAR_COUNT = 8;
    private const float STAR_RING_INNER_RADIUS = 1.8f;
    private const float STAR_RING_OUTER_RADIUS = 0.8f; // 原代码如此（外环实际上更靠近中心）
    private const float LIGHT_VALUE = 4.0f;
    private const float ANIMATION_SPEED = 60.0f;
    private const int blur_samples = 2;
    private const float exposure_time = 0.2f;
    private const float STAR_RADIUS = 1.1f;

    private float camera_center_x = WIDTH / 2.0f;
    private float camera_center_y = HEIGHT / 2.0f;

    private Star[] stars;
    private Random rand = new Random(42); // 固定种子便于调试，可去掉

    private class Star
    {
        public float x, y;
        public float speed;
        public float angle;
        public float distance;
        public float brightness = 1.0f;
    }
    //====================

    public TestPage1()
    {
        InitializeComponent();
        SetLedBoardColors();
        InitStars();
        // 初始化定时器
        _timer = new DispatcherTimer();
        _timer.Interval = TimeSpan.FromMilliseconds(100);
        _timer.Tick += Timer_Tick;
        _timer.Start();

        //Uri uri1 = new(uriCopilot + launage + darkschemeovr);
        //Webview1.Source = uri1;
        /*
        Uri uri2 = new Uri("https://live.bilibili.com/22409163");
        Uri uri3 = new Uri("https://live.bilibili.com/390508");
        Uri uri4 = new Uri("https://live.bilibili.com/1570342");
        
        Webview2.Source = uri2;
        Webview3.Source = uri3;
        Webview4.Source = uri4;
        */
        InitLampArrayDevice();
    }

    private void Timer_Tick(object sender, object e)
    {
        UpdateLedBoardColors();
    }

    private void SetLedBoardColors()
    {
        var colors1 = new Color[8, 8];
        var colors2 = new Color[8, 8];
        var colors3 = new Color[8, 8];

        var color = Colors.Green;

        // 初始化颜色数据
        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                // 第一个控件按照列表顺序渲染
                int position1 = GetSnakePosition(row, col);
                colors1[row, col] = GetRainbowColor(position1, 64);

                // 第二个控件按照对角线渲染
                int position2 = GetDiagonalPosition(row, col);
                colors2[row, col] = GetRainbowColor(position2, 15);

                // 第三个控件随机渲染
                colors3[row, col] = GetRandomColor();
            }
        }

        // 设置 WS64LedBoardBox 控件的 Colors 属性
        ledBoard1.Colors = colors1;
        ledBoard2.Colors = colors2;
        ledBoard3.Colors = colors3;
    }

    private DateTime lastTime = DateTime.UtcNow;
    private void UpdateLedBoardColors()
    {
        var colors1 = new Color[8, 8];
        var colors2 = new Color[8, 8];
        var colors3 = new Color[8, 8];

        DateTime now = DateTime.UtcNow;
        float deltaTime = (float)(now - lastTime).TotalSeconds;
        lastTime = now;
        UpdateStars(deltaTime);
        // 更新颜色数据
        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                // 第一个控件按照列表顺序渲染
                int position1 = (GetSnakePosition(row, col) + _currentStep) % 64;
                colors1[row, col] = GetRainbowColor(position1, 64);

                // 第二个控件按照对角线渲染
                int position2 = (GetDiagonalPosition(row, col) + _currentStep) % 15;
                colors2[row, col] = GetRainbowColor(position2, 15);

                // 第三个控件随机渲染
                colors3 = RenderStars();
            }
        }

        // 设置 WS64LedBoardBox 控件的 Colors 属性
        ledBoard1.Colors = colors1;
        ledBoard2.Colors = colors2;
        ledBoard3.Colors = colors3;

        _currentStep++;
    }

    private int GetDiagonalPosition(int row, int col)
    {
        return row + col;
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

    private Color GetRandomColor()
    {
        return Color.FromArgb(255, (byte)_random.Next(256), (byte)_random.Next(256), (byte)_random.Next(256));
    }

    private Color GetRainbowColor(int position, int total)
    {
        double ratio = (double)position / total;
        int r = (int)(Math.Sin(ratio * Math.PI * 2) * 127 + 128);
        int g = (int)(Math.Sin(ratio * Math.PI * 2 + 2 * Math.PI / 3) * 127 + 128);
        int b = (int)(Math.Sin(ratio * Math.PI * 2 + 4 * Math.PI / 3) * 127 + 128);
        return Color.FromArgb(255, (byte)r, (byte)g, (byte)b);
    }

    private void InitStars()
    {
        stars = new Star[STAR_COUNT];
        for (int i = 0; i < STAR_COUNT; i++)
        {
            float r = STAR_RING_INNER_RADIUS + (float)rand.NextDouble() * (STAR_RING_OUTER_RADIUS - STAR_RING_INNER_RADIUS);
            float angle = (float)(rand.NextDouble() * Math.PI * 2);
            stars[i] = new Star
            {
                x = r * (float)Math.Cos(angle),
                y = r * (float)Math.Sin(angle),
                distance = r,
                angle = angle,
                brightness = 1.0f
            };

            float speed_factor = (STAR_RING_OUTER_RADIUS - r) / (STAR_RING_OUTER_RADIUS - STAR_RING_INNER_RADIUS);
            stars[i].speed = 0.01f + speed_factor * 0.10f; // 0.01 ~ 0.11
        }
    }

    private void UpdateStars(float deltaTime)
    {
        const float fade_in_rate = 2.5f;
        const float fade_out_rate = 2.0f;

        foreach (var s in stars)
        {
            s.distance += s.speed * deltaTime * ANIMATION_SPEED;
            s.x = s.distance * (float)Math.Cos(s.angle);
            s.y = s.distance * (float)Math.Sin(s.angle);

            float global_x = s.x + camera_center_x;
            float global_y = s.y + camera_center_y;

            bool nearBoundary = global_x < 1 || global_x > WIDTH - 1 ||
                                global_y < 1 || global_y > HEIGHT - 1;

            if (nearBoundary)
            {
                s.brightness -= fade_out_rate * deltaTime;
                if (s.brightness <= 0)
                {
                    // 重生
                    float r = STAR_RING_INNER_RADIUS + (float)rand.NextDouble() * (STAR_RING_OUTER_RADIUS - STAR_RING_INNER_RADIUS);
                    float angle = (float)(rand.NextDouble() * Math.PI * 2);
                    s.x = r * (float)Math.Cos(angle);
                    s.y = r * (float)Math.Sin(angle);
                    s.distance = r;
                    s.angle = angle;
                    float speed_factor = (STAR_RING_OUTER_RADIUS - r) / (STAR_RING_OUTER_RADIUS - STAR_RING_INNER_RADIUS);
                    s.speed = 0.01f + speed_factor * 0.10f;
                    s.brightness = 0f; // 从暗开始淡入
                }
            }
            else if (s.brightness < 1.0f)
            {
                s.brightness += fade_in_rate * deltaTime;
                if (s.brightness > 1.0f) s.brightness = 1.0f;
            }
        }
    }
    private Color[,] RenderStars()
    {
        float[] r_buffer = new float[NUM_PIXELS];
        float[] g_buffer = new float[NUM_PIXELS];
        float[] b_buffer = new float[NUM_PIXELS];

        foreach (var s in stars)
        {
            float base_x = s.x;
            float base_y = s.y;

            float trail_length_factor = Math.Max(s.speed / 0.11f, 0.5f);
            float dx = s.speed * (float)Math.Cos(s.angle) * exposure_time * trail_length_factor / blur_samples;
            float dy = s.speed * (float)Math.Sin(s.angle) * exposure_time * trail_length_factor / blur_samples;

            for (int sample = 0; sample < blur_samples; sample++)
            {
                float sx = base_x + sample * dx;
                float sy = base_y + sample * dy;
                float trail_weight = 1.0f - (float)sample / blur_samples;

                float global_x = sx + camera_center_x;
                float global_y = sy + camera_center_y;

                int center_x = (int)Math.Floor(global_x);
                int center_y = (int)Math.Floor(global_y);

                // 基础颜色（近中心：偏绿深蓝）
                float r = 0f;
                float g = 50f;
                float b = 87f;

                // 根据速度插值到蓝紫（速度越快越紫）
                float speed_factor = 1.0f - (s.speed - 0.01f) / 0.10f;
                speed_factor = Math.Clamp(speed_factor, 0f, 1f);
                r += (58f - r) * speed_factor;
                g += (0f - g) * speed_factor;
                b += (128f - b) * speed_factor;

                r *= LIGHT_VALUE * s.brightness * trail_weight;
                g *= LIGHT_VALUE * s.brightness * trail_weight;
                b *= LIGHT_VALUE * s.brightness * trail_weight;

                /// 次像素渲染：改用 offsetX / offsetY 避免变量名冲突
                for (int offsetY = -1; offsetY <= 1; offsetY++)
                {
                    for (int offsetX = -1; offsetX <= 1; offsetX++)
                    {
                        int px = center_x + offsetX;
                        int py = center_y + offsetY;

                        if (px >= 0 && px < WIDTH && py >= 0 && py < HEIGHT)
                        {
                            float dist = (float)Math.Sqrt(
                                Math.Pow(px + 0.5f - global_x, 2) +
                                Math.Pow(py + 0.5f - global_y, 2));

                            float weight = Math.Max(0f, 1f - dist / STAR_RADIUS);

                            // 蛇形走线索引
                            int local_x = (py % 2 == 0) ? px : (WIDTH - 1 - px);
                            int index = py * WIDTH + local_x;

                            r_buffer[index] += r * weight;
                            g_buffer[index] += g * weight;
                            b_buffer[index] += b * weight;
                        }
                    }
                }
            }
        }

        // 归一化并转换为 Color[,]
        Color[,] colors = new Color[HEIGHT, WIDTH];
        for (int i = 0; i < NUM_PIXELS; i++)
        {
            int r_final = (int)Math.Min(255, r_buffer[i] / blur_samples);
            int g_final = (int)Math.Min(255, g_buffer[i] / blur_samples);
            int b_final = (int)Math.Min(255, b_buffer[i] / blur_samples);

            int y = i / WIDTH;
            int local_x = i % WIDTH;
            int x = (y % 2 == 0) ? local_x : (WIDTH - 1 - local_x);

            colors[y, x] = Color.FromArgb(255, (byte)r_final, (byte)g_final, (byte)b_final);
        }

        return colors;
    }


    private Window _starWindow;

    private void OpenStarWindowButton_Click(object sender, RoutedEventArgs e)
    {
        if (_starWindow == null)
        {
            _starWindow = new StarWindow();
            _starWindow.Closed += (s, args) => _starWindow = null;
            _starWindow.Activate();
        }
        else
        {
            _starWindow.Activate();
        }
    }

    private void ScrollViewer_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        var scrollViewer = sender as ScrollViewer;
        if (scrollViewer != null)
        {
            // 开始拖动
            isDragging = true;
            currentScrollViewer = scrollViewer;
            lastMousePosition = e.GetCurrentPoint(scrollViewer).Position;
            scrollViewer.CapturePointer(e.Pointer); // 捕获指针
            e.Handled = true;
        }
    }

    private void ScrollViewer_PointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if (isDragging && currentScrollViewer != null)
        {
            var currentPosition = e.GetCurrentPoint(currentScrollViewer).Position;
            // 计算鼠标移动的偏移量
            double deltaX = lastMousePosition.X - currentPosition.X;
            double deltaY = lastMousePosition.Y - currentPosition.Y;

            // 更新 ScrollViewer 的偏移量
            currentScrollViewer.ChangeView(
                currentScrollViewer.HorizontalOffset + deltaX,
                currentScrollViewer.VerticalOffset + deltaY,
                null, // 保持当前缩放比例
                true // 禁用动画以提高流畅度
            );

            // 更新上一次鼠标位置
            lastMousePosition = currentPosition;
            e.Handled = true;
        }
    }

    private void ScrollViewer_PointerReleased(object sender, PointerRoutedEventArgs e)
    {
        if (isDragging)
        {
            isDragging = false;
            currentScrollViewer?.ReleasePointerCapture(e.Pointer); // 释放指针
            currentScrollViewer = null;
            e.Handled = true;
        }
    }

    private bool isDragging;
    private Point lastMousePosition;
    private ScrollViewer currentScrollViewer;
    private enum SyncSource { None, ScrollViewer1, ScrollViewer2 }
    private SyncSource syncSource = SyncSource.None;

    private void ScrollViewer1_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
    {
        if (syncSource == SyncSource.ScrollViewer2) return;

        float zoomDelta = Math.Abs(scrollViewer2.ZoomFactor - scrollViewer1.ZoomFactor);
        float hOffsetDelta = (float)Math.Abs(scrollViewer2.HorizontalOffset - scrollViewer1.HorizontalOffset);
        float vOffsetDelta = (float)Math.Abs(scrollViewer1.VerticalOffset - scrollViewer2.VerticalOffset);

        if (zoomDelta < 0.01f && hOffsetDelta < 1f && vOffsetDelta < 1f) return;

        syncSource = SyncSource.ScrollViewer1;

        var centerPoint = new Point(scrollViewer1.ActualWidth / 2, scrollViewer1.ActualHeight / 2);
        scrollViewer2.ChangeView(scrollViewer1.HorizontalOffset, scrollViewer1.VerticalOffset, scrollViewer1.ZoomFactor, true);

        syncSource = SyncSource.None;
    }

    private void ScrollViewer2_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
    {
        if (syncSource == SyncSource.ScrollViewer1) return;

        float zoomDelta = Math.Abs(scrollViewer1.ZoomFactor - scrollViewer2.ZoomFactor);
        float hOffsetDelta = (float)Math.Abs(scrollViewer1.HorizontalOffset - scrollViewer2.HorizontalOffset);
        float vOffsetDelta = (float)Math.Abs(scrollViewer2.VerticalOffset - scrollViewer1.VerticalOffset);

        if (zoomDelta < 0.01f && hOffsetDelta < 1f && vOffsetDelta < 1f) return;

        syncSource = SyncSource.ScrollViewer2;

        var centerPoint = new Point(scrollViewer2.ActualWidth / 2, scrollViewer2.ActualHeight / 2);
        scrollViewer1.ChangeView(scrollViewer2.HorizontalOffset, scrollViewer2.VerticalOffset, scrollViewer2.ZoomFactor, true);

        syncSource = SyncSource.None;
    }

    private HidDevice lampArrayDevice;
    private async void InitLampArrayDevice()
    {
        // 替换为你的设备 VID/PID
        string selector = HidDevice.GetDeviceSelector(0x03, 0x00); // HID Class
        var devices = await DeviceInformation.FindAllAsync(selector);

        foreach (var dev in devices)
        {
            if (dev.Id.Contains("VID_2E8A") && dev.Id.Contains("PID_F401"))
            {
                lampArrayDevice = await HidDevice.FromIdAsync(dev.Id, Windows.Storage.FileAccessMode.ReadWrite);
                if (lampArrayDevice != null)
                {
                    System.Diagnostics.Debug.WriteLine("✅ LampArray HID 已连接");
                }
            }
        }
    }

    private async void LampArrayHIDTestButton_Click(object sender, RoutedEventArgs e)
    {
        if (lampArrayDevice == null)
        {
            System.Diagnostics.Debug.WriteLine("❌ HID 设备未连接");
            return;
        }

        // 构造一个 HID Output Report
        // 格式: [ReportID, LampId, R, G, B]
        byte reportId = 0x04;   // 对应 LampArray_HandleSetReport
        byte lampId = 0x00;   // 第 0 号灯
        byte red = 0xFF;
        byte green = 0x00;
        byte blue = 0x00;

        HidOutputReport outReport = lampArrayDevice.CreateOutputReport(reportId);
        DataWriter writer = new DataWriter();
        writer.WriteByte(lampId);
        writer.WriteByte(red);
        writer.WriteByte(green);
        writer.WriteByte(blue);

        outReport.Data = writer.DetachBuffer();

        await lampArrayDevice.SendOutputReportAsync(outReport);
        System.Diagnostics.Debug.WriteLine("✅ 已发送 HID 报告: LampId=0 红色");
    }
}