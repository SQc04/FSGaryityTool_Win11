using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;
using Windows.Foundation;

namespace FSGaryityTool_Win11.Controls;

public class CustomWrapPanel : Panel
{
    protected override Size MeasureOverride(Size availableSize)
    {
        var totalSize = new Size();
        double lineHeight = 0;
        double lineWidth = 0;
        int flexibleChildren = 0;
        double fixedWidthSum = 0;

        foreach (var child in Children)
        {
            child.Measure(availableSize);
            var feChild = child as FrameworkElement;
            double childWidth = double.IsNaN(feChild.Width) ? feChild.MinWidth : feChild.Width;

            if (lineWidth + childWidth > availableSize.Width && lineWidth > 0)
            {
                totalSize.Height += lineHeight;
                lineWidth = 0;
                lineHeight = 0;
                flexibleChildren = 0;
                fixedWidthSum = 0;
            }

            lineHeight = Math.Max(lineHeight, feChild.DesiredSize.Height);
            lineWidth += childWidth;

            if (double.IsNaN(feChild.Width))
                flexibleChildren++;
            else
                fixedWidthSum += childWidth;
        }

        totalSize.Height += lineHeight;
        totalSize.Width = Math.Min(availableSize.Width, Math.Max(totalSize.Width, lineWidth));

        return totalSize;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var currentRect = new Rect();
        double lineHeight = 0;
        double lineWidth = 0;
        int flexibleChildren = 0;
        double fixedWidthSum = 0;
        var lineChildren = new System.Collections.Generic.List<(UIElement Element, double Width, bool IsFlexible)>();

        foreach (var child in Children)
        {
            var feChild = child as FrameworkElement;
            double childWidth = double.IsNaN(feChild.Width) ? feChild.MinWidth : feChild.Width;
            bool isFlexible = double.IsNaN(feChild.Width);

            if (lineWidth + childWidth > finalSize.Width && lineWidth > 0)
            {
                ArrangeLine(lineChildren, fixedWidthSum, finalSize.Width, lineHeight, currentRect.Y);
                currentRect.Y += lineHeight;
                lineWidth = 0;
                lineHeight = 0;
                flexibleChildren = 0;
                fixedWidthSum = 0;
                lineChildren.Clear();
            }

            lineHeight = Math.Max(lineHeight, feChild.DesiredSize.Height);
            lineWidth += childWidth;
            if (isFlexible)
                flexibleChildren++;
            else
                fixedWidthSum += childWidth;

            lineChildren.Add((child, childWidth, isFlexible));
        }

        if (lineChildren.Count > 0)
        {
            ArrangeLine(lineChildren, fixedWidthSum, finalSize.Width, lineHeight, currentRect.Y);
        }

        return finalSize;
    }

    private void ArrangeLine(System.Collections.Generic.List<(UIElement Element, double Width, bool IsFlexible)> lineChildren,
                           double fixedWidthSum, double availableWidth, double lineHeight, double yOffset)
    {
        double xOffset = 0;
        double remainingWidth = availableWidth - fixedWidthSum;
        int flexibleCount = lineChildren.Count(c => c.IsFlexible);

        if (flexibleCount == 0)
        {
            // 无未限定宽度控件，按原宽度排列
            foreach (var child in lineChildren)
            {
                var rect = new Rect(xOffset, yOffset, child.Width, lineHeight);
                child.Element.Arrange(rect);
                xOffset += child.Width;
            }
        }
        else
        {
            // 找到最后一个未限定宽度的控件
            int lastFlexibleIndex = -1;
            for (int i = lineChildren.Count - 1; i >= 0; i--)
            {
                if (lineChildren[i].IsFlexible)
                {
                    lastFlexibleIndex = i;
                    break;
                }
            }

            // 重新计算剩余宽度，排除非最后一个未限定宽度控件的 MinWidth
            double nonLastFlexibleWidthSum = 0;
            for (int i = 0; i < lineChildren.Count; i++)
            {
                if (lineChildren[i].IsFlexible && i != lastFlexibleIndex)
                {
                    nonLastFlexibleWidthSum += lineChildren[i].Width; // Width 是 MinWidth
                }
            }
            remainingWidth -= nonLastFlexibleWidthSum;

            // 排列控件
            for (int i = 0; i < lineChildren.Count; i++)
            {
                var child = lineChildren[i];
                var feChild = child.Element as FrameworkElement;
                double finalWidth;

                if (child.IsFlexible && i == lastFlexibleIndex)
                {
                    // 最后一个未限定宽度控件填充剩余空间
                    finalWidth = Math.Clamp(remainingWidth, feChild.MinWidth, feChild.MaxWidth);
                }
                else
                {
                    // 固定宽度控件或其他未限定宽度控件使用 MinWidth
                    finalWidth = child.IsFlexible ? feChild.MinWidth : child.Width;
                }

                var rect = new Rect(xOffset, yOffset, finalWidth, lineHeight);
                child.Element.Arrange(rect);
                xOffset += finalWidth;
            }
        }
    }
}