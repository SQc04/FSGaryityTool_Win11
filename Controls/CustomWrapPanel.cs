using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using Windows.Foundation;

namespace FSGaryityTool_Win11.Controls
{
    public class CustomWrapPanel : Panel
    {
        protected override Size MeasureOverride(Size availableSize)
        {
            Size totalSize = new Size(); // 总的尺寸
            double lineHeight = 0; // 当前行的高度

            foreach (UIElement child in Children) // 遍历所有子元素
            {
                child.Measure(availableSize); // 测量子元素的尺寸
                FrameworkElement feChild = child as FrameworkElement; // 将 child 转换为 FrameworkElement
                double childWidth = double.IsNaN(feChild.Width) ? feChild.MinWidth : feChild.Width; // 如果子元素有固定的宽度，则使用 Width，否则使用 MinWidth

                if (totalSize.Width + childWidth > availableSize.Width) // 如果子元素的宽度超过了可用宽度，换行
                {
                    totalSize.Width = 0; // 重置总宽度为0
                    totalSize.Height += lineHeight; // 增加总高度
                }

                lineHeight = Math.Max(lineHeight, feChild.Height); // 行高为当前行所有子元素中最大的高度
                totalSize.Width += childWidth; // 增加总宽度
            }


            totalSize.Height += lineHeight; // 添加最后一行的高度

            return totalSize; // 返回总的尺寸
        }


        protected override Size ArrangeOverride(Size finalSize)
        {
            Rect finalRect = new Rect(); // 最终的布局矩形
            double lineHeight = 0; // 当前行的高度
            UIElement lastChild = null; // 上一个子元素

            double totalWidth = 0; // 用于累加当前行的总宽度

            foreach (UIElement child in Children) // 遍历所有子元素
            {
                FrameworkElement feChild = child as FrameworkElement; // 将 child 转换为 FrameworkElement
                double childWidth = double.IsNaN(feChild.Width) ? feChild.MinWidth : feChild.Width; // 如果子元素有固定的宽度，则使用 Width，否则使用 MinWidth

                if (finalRect.Left + childWidth > finalSize.Width) // 如果子元素的宽度超过了可用宽度，换行
                {
                    // 调整最后一行的最后一个子元素的宽度以填充剩余空间
                    AdjustLastChildWidth(lastChild, totalWidth, finalSize.Width, lineHeight, finalRect);

                    finalRect.Y += lineHeight; // 更新Y坐标到下一行
                    finalRect.X = 0; // X坐标重置为0
                    lineHeight = child.DesiredSize.Height; // 更新行高为当前子元素的高度

                    totalWidth = 0; // 重置当前行的总宽度
                }

                lineHeight = Math.Max(lineHeight, child.DesiredSize.Height); // 行高为当前行所有子元素中最大的高度
                finalRect.Width = childWidth; // 矩形的宽度为当前子元素的宽度
                finalRect.Height = child.DesiredSize.Height; // 矩形的高度为当前子元素的高度

                child.Arrange(finalRect); // 安排子元素在矩形内
                finalRect.X += finalRect.Width; // 更新X坐标到下一个子元素的位置

                totalWidth += childWidth; // 累加当前行的总宽度

                lastChild = child; // 更新上一个子元素为当前子元素
            }
            
            // 调整最后一行的最后一个子元素的宽度以填充剩余空间
            AdjustLastChildWidth(lastChild, totalWidth, finalSize.Width, lineHeight, finalRect);

            return finalSize; // 返回最终的尺寸
        }

        private void AdjustLastChildWidth(UIElement lastChild, double totalWidth, double finalWidth, double lineHeight, Rect finalRect)
        {
            FrameworkElement feLast = lastChild as FrameworkElement;
            if (feLast != null && double.IsNaN(feLast.Width))
            {
                double minWidth = feLast.MinWidth;
                double previousWidth = totalWidth - feLast.DesiredSize.Width;

                // 判断这一行是否只有一个元素
                if (totalWidth == feLast.DesiredSize.Width)
                {
                    // 如果只有一个元素，将该元素的宽度设置为控件的最右边
                    Rect lastChildRect = new Rect(0, finalRect.Y, finalWidth, lineHeight);
                    lastChild.Arrange(lastChildRect);
                }
                else
                {
                    // 如果有多个元素，计算起始宽度
                    Rect lastChildRect = new Rect(finalRect.X - feLast.DesiredSize.Width, finalRect.Y, finalWidth - previousWidth, lineHeight);
                    lastChild.Arrange(lastChildRect);
                }
            }
        }


    }
}
