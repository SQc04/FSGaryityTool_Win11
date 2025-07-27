using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Numerics;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FSGaryityTool_Win11.Views.Pages.TestPage
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Star : Page
    {
        const int SCREEN_COLS = 4;
        const int SCREEN_ROWS = 3;
        const int PIXELS_PER_SCREEN = 64; // 8x8
        const int NUM_PIXELS = PIXELS_PER_SCREEN * SCREEN_COLS * SCREEN_ROWS;
        const float LIGHT_VALUE = 0.9f;
        const int STAR_COUNT = 16;
        const float ANIMATION_SPEED = 75.0f;
        const float STAR_RADIUS = 0.8f;

        float camera_center_x;
        float camera_center_y;

        Random rand = new Random();

        class StarObj
        {
            public float x, y, speed, angle, distance, brightness;
        }

        List<StarObj> stars = new List<StarObj>();

        DispatcherTimer timer;
        DateTime lastTime;

        public Star()
        {
            this.InitializeComponent();

            camera_center_x = (SCREEN_COLS * 8) / 2.0f;
            camera_center_y = (SCREEN_ROWS * 8) / 2.0f;

            InitStars();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(16); // ~60fps
            timer.Tick += Timer_Tick;
            lastTime = DateTime.Now;
            timer.Start();
        }

        void InitStars()
        {
            stars.Clear();
            for (int i = 0; i < STAR_COUNT; i++)
            {
                stars.Add(new StarObj
                {
                    x = 0,
                    y = 0,
                    speed = (float)(rand.Next(100) / 1000.0 + 0.01),
                    angle = (float)(rand.Next(360) * Math.PI / 180.0),
                    distance = 0,
                    brightness = 1.0f
                });
            }
        }

        void Timer_Tick(object sender, object e)
        {
            var now = DateTime.Now;
            float deltaTime = (float)(now - lastTime).TotalSeconds;
            lastTime = now;

            UpdateStars(deltaTime);
            RenderStars();
        }

        void UpdateStars(float deltaTime)
        {
            float max_distance = 0.0f;
            int farthest_star_index = -1;

            // 重置所有星星的标记
            foreach (var s in stars)
                s.speed = Math.Abs(s.speed);

            // 更新星星位置并找到最远的星星
            for (int i = 0; i < stars.Count; i++)
            {
                var s = stars[i];
                s.distance += s.speed * deltaTime * ANIMATION_SPEED;
                s.x = s.distance * (float)Math.Cos(s.angle);
                s.y = s.distance * (float)Math.Sin(s.angle);

                float global_x = s.x + camera_center_x;
                float global_y = s.y + camera_center_y;

                float total_width = SCREEN_COLS * 8;
                float total_height = SCREEN_ROWS * 8;
                float fade_margin = 1.0f;
                float fade_rate = 2.0f;

                bool is_near_boundary = (global_x < fade_margin || global_x > total_width - fade_margin ||
                    global_y < fade_margin || global_y > total_height - fade_margin);

                if (is_near_boundary)
                {
                    s.brightness -= fade_rate * deltaTime;
                    if (s.brightness < 0.0f) s.brightness = 0.0f;
                }
                else
                {
                    if (s.brightness < 1.0f)
                    {
                        s.brightness += fade_rate * deltaTime;
                        if (s.brightness > 1.0f) s.brightness = 1.0f;
                    }
                }

                if (s.brightness <= 0.0f)
                {
                    s.x = 0.0f;
                    s.y = 0.0f;
                    s.speed = (float)(rand.Next(100) / 1000.0 + 0.01);
                    s.angle = (float)(rand.Next(360) * Math.PI / 180.0);
                    s.distance = 0.0f;
                    s.brightness = 1.0f;
                }

                if (s.distance > max_distance)
                {
                    max_distance = s.distance;
                    farthest_star_index = i;
                }
            }

            // 标记最远的星星
            if (farthest_star_index != -1)
                stars[farthest_star_index].speed = -stars[farthest_star_index].speed;
        }

        void RenderStars()
        {
            StarCanvas.Children.Clear();

            // 动态模糊采样点数
            const int blur_samples = 5;
            const float exposure_time = 0.1f;

            foreach (var s in stars)
            {
                float base_x = s.x;
                float base_y = s.y;
                float base_distance = s.distance;

                float dx = s.speed * (float)Math.Cos(s.angle) * exposure_time / blur_samples;
                float dy = s.speed * (float)Math.Sin(s.angle) * exposure_time / blur_samples;
                float d_distance = s.speed * exposure_time / blur_samples;

                for (int sample = 0; sample < blur_samples; sample++)
                {
                    float sample_distance = base_distance + sample * d_distance;
                    float sample_x = base_x + sample * dx;
                    float sample_y = base_y + sample * dy;

                    float global_x = sample_x + camera_center_x;
                    float global_y = sample_y + camera_center_y;

                    // 颜色计算
                    float y_factor = MathF.Max(MathF.Abs(sample_x) / (SCREEN_COLS * 4.0f), MathF.Abs(sample_y) / (SCREEN_ROWS * 4.0f));
                    y_factor = MathF.Min(y_factor, 1.0f);
                    float y_nonlinear = y_factor * y_factor;

                    float speed_factor = 1.0f - (MathF.Abs(s.speed) - 0.01f) / (0.11f - 0.01f);
                    speed_factor = MathF.Max(0.0f, MathF.Min(1.0f, speed_factor));


                    float r, g, b;
                    if (s.speed < 0)
                    {
                        r = 58.0f; g = 0.0f; b = 128.0f;
                    }
                    else
                    {
                        r = 0.0f; g = 50.0f; b = 87.0f;
                        r += (255.0f - 0.0f) * y_nonlinear;
                        g += (255.0f - 50.0f) * y_nonlinear;
                        b += (255.0f - 87.0f) * y_nonlinear;
                        r += (58.0f - r) * speed_factor;
                        g += (0.0f - g) * speed_factor;
                        b += (128.0f - b) * speed_factor;
                    }
                    r *= LIGHT_VALUE * s.brightness;
                    g *= LIGHT_VALUE * s.brightness;
                    b *= LIGHT_VALUE * s.brightness;

                    // 绘制星星（用Ellipse模拟模糊）
                    var ellipse = new Ellipse
                    {
                        Width = 2.0 * STAR_RADIUS * 8, // 放大以便可见
                        Height = 2.0 * STAR_RADIUS * 8,
                        Fill = new SolidColorBrush(Color.FromArgb(255, (byte)r, (byte)g, (byte)b)),
                        Opacity = 0.5
                    };
                    Canvas.SetLeft(ellipse, global_x * 8 - ellipse.Width / 2);
                    Canvas.SetTop(ellipse, global_y * 8 - ellipse.Height / 2);
                    StarCanvas.Children.Add(ellipse);
                }
            }
        }
    }
}
