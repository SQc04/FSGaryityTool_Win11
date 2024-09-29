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
            Size totalSize = new Size(); // �ܵĳߴ�
            double lineHeight = 0; // ��ǰ�еĸ߶�

            foreach (UIElement child in Children) // ����������Ԫ��
            {
                child.Measure(availableSize); // ������Ԫ�صĳߴ�
                FrameworkElement feChild = child as FrameworkElement; // �� child ת��Ϊ FrameworkElement
                double childWidth = double.IsNaN(feChild.Width) ? feChild.MinWidth : feChild.Width; // �����Ԫ���й̶��Ŀ�ȣ���ʹ�� Width������ʹ�� MinWidth

                if (totalSize.Width + childWidth > availableSize.Width) // �����Ԫ�صĿ�ȳ����˿��ÿ�ȣ�����
                {
                    totalSize.Width = 0; // �����ܿ��Ϊ0
                    totalSize.Height += lineHeight; // �����ܸ߶�
                }

                lineHeight = Math.Max(lineHeight, feChild.Height); // �и�Ϊ��ǰ��������Ԫ�������ĸ߶�
                totalSize.Width += childWidth; // �����ܿ��
            }


            totalSize.Height += lineHeight; // ������һ�еĸ߶�

            return totalSize; // �����ܵĳߴ�
        }


        protected override Size ArrangeOverride(Size finalSize)
        {
            Rect finalRect = new Rect(); // ���յĲ��־���
            double lineHeight = 0; // ��ǰ�еĸ߶�
            UIElement lastChild = null; // ��һ����Ԫ��

            double totalWidth = 0; // �����ۼӵ�ǰ�е��ܿ��

            foreach (UIElement child in Children) // ����������Ԫ��
            {
                FrameworkElement feChild = child as FrameworkElement; // �� child ת��Ϊ FrameworkElement
                double childWidth = double.IsNaN(feChild.Width) ? feChild.MinWidth : feChild.Width; // �����Ԫ���й̶��Ŀ�ȣ���ʹ�� Width������ʹ�� MinWidth

                if (finalRect.Left + childWidth > finalSize.Width) // �����Ԫ�صĿ�ȳ����˿��ÿ�ȣ�����
                {
                    // �������һ�е����һ����Ԫ�صĿ�������ʣ��ռ�
                    AdjustLastChildWidth(lastChild, totalWidth, finalSize.Width, lineHeight, finalRect);

                    finalRect.Y += lineHeight; // ����Y���굽��һ��
                    finalRect.X = 0; // X��������Ϊ0
                    lineHeight = child.DesiredSize.Height; // �����и�Ϊ��ǰ��Ԫ�صĸ߶�

                    totalWidth = 0; // ���õ�ǰ�е��ܿ��
                }

                lineHeight = Math.Max(lineHeight, child.DesiredSize.Height); // �и�Ϊ��ǰ��������Ԫ�������ĸ߶�
                finalRect.Width = childWidth; // ���εĿ��Ϊ��ǰ��Ԫ�صĿ��
                finalRect.Height = child.DesiredSize.Height; // ���εĸ߶�Ϊ��ǰ��Ԫ�صĸ߶�

                child.Arrange(finalRect); // ������Ԫ���ھ�����
                finalRect.X += finalRect.Width; // ����X���굽��һ����Ԫ�ص�λ��

                totalWidth += childWidth; // �ۼӵ�ǰ�е��ܿ��

                lastChild = child; // ������һ����Ԫ��Ϊ��ǰ��Ԫ��
            }
            
            // �������һ�е����һ����Ԫ�صĿ�������ʣ��ռ�
            AdjustLastChildWidth(lastChild, totalWidth, finalSize.Width, lineHeight, finalRect);

            return finalSize; // �������յĳߴ�
        }

        private void AdjustLastChildWidth(UIElement lastChild, double totalWidth, double finalWidth, double lineHeight, Rect finalRect)
        {
            FrameworkElement feLast = lastChild as FrameworkElement;
            if (feLast != null && double.IsNaN(feLast.Width))
            {
                double minWidth = feLast.MinWidth;
                double previousWidth = totalWidth - feLast.DesiredSize.Width;

                // �ж���һ���Ƿ�ֻ��һ��Ԫ��
                if (totalWidth == feLast.DesiredSize.Width)
                {
                    // ���ֻ��һ��Ԫ�أ�����Ԫ�صĿ������Ϊ�ؼ������ұ�
                    Rect lastChildRect = new Rect(0, finalRect.Y, finalWidth, lineHeight);
                    lastChild.Arrange(lastChildRect);
                }
                else
                {
                    // ����ж��Ԫ�أ�������ʼ���
                    Rect lastChildRect = new Rect(finalRect.X - feLast.DesiredSize.Width, finalRect.Y, finalWidth - previousWidth, lineHeight);
                    lastChild.Arrange(lastChildRect);
                }
            }
        }


    }
}
