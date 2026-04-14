using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MCBS.WpfApp.CustomControls
{
    /// <summary>
    /// 按照步骤 1a 或 1b 操作，然后执行步骤 2 以在 XAML 文件中使用此自定义控件。
    ///
    /// 步骤 1a) 在当前项目中存在的 XAML 文件中使用该自定义控件。
    /// 将此 XmlNamespace 特性添加到要使用该特性的标记文件的根
    /// 元素中:
    ///
    ///     xmlns:MyNamespace="clr-namespace:MCBS.WpfApp.CustomControls"
    ///
    ///
    /// 步骤 1b) 在其他项目中存在的 XAML 文件中使用该自定义控件。
    /// 将此 XmlNamespace 特性添加到要使用该特性的标记文件的根
    /// 元素中:
    ///
    ///     xmlns:MyNamespace="clr-namespace:MCBS.WpfApp.CustomControls;assembly=MCBS.WpfApp.CustomControls"
    ///
    /// 您还需要添加一个从 XAML 文件所在的项目到此项目的项目引用，
    /// 并重新生成以避免编译错误:
    ///
    ///     在解决方案资源管理器中右击目标项目，然后依次单击
    ///     “添加引用”->“项目”->[浏览查找并选择此项目]
    ///
    ///
    /// 步骤 2)
    /// 继续操作并在 XAML 文件中使用控件。
    ///
    ///     <MyNamespace:ClippingBorder/>
    ///
    /// </summary>
    public class ClippingBorder : Border
    {
        static ClippingBorder()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ClippingBorder), new FrameworkPropertyMetadata(typeof(ClippingBorder)));
        }

        protected override void OnRender(DrawingContext dc)
        {
            OnApplyChildClip();
            base.OnRender(dc);
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            OnApplyChildClip();
        }

        private void OnApplyChildClip()
        {
            if (Child == null) return;

            // 如果所有圆角都为 0，直接使用矩形裁剪
            if (CornerRadius.TopLeft == 0 && CornerRadius.TopRight == 0 &&
                CornerRadius.BottomRight == 0 && CornerRadius.BottomLeft == 0)
            {
                Child.Clip = new RectangleGeometry(new Rect(0, 0, ActualWidth, ActualHeight));
                return;
            }

            // 使用 StreamGeometry 创建带独立圆角的矩形
            var geometry = new StreamGeometry();
            using (var ctx = geometry.Open())
            {
                DrawRoundedRectangle(ctx, new Rect(0, 0, ActualWidth, ActualHeight), CornerRadius);
            }

            // 冻结几何图形以提高性能
            geometry.Freeze();
            Child.Clip = geometry;
        }

        private static void DrawRoundedRectangle(StreamGeometryContext ctx, Rect rect, CornerRadius cornerRadius)
        {
            double width = rect.Width;
            double height = rect.Height;

            // 确保圆角半径不会超过矩形尺寸的一半
            double topLeft = Math.Min(cornerRadius.TopLeft, Math.Min(width / 2, height / 2));
            double topRight = Math.Min(cornerRadius.TopRight, Math.Min(width / 2, height / 2));
            double bottomRight = Math.Min(cornerRadius.BottomRight, Math.Min(width / 2, height / 2));
            double bottomLeft = Math.Min(cornerRadius.BottomLeft, Math.Min(width / 2, height / 2));

            // 从左上角开始绘制
            ctx.BeginFigure(new Point(rect.Left + topLeft, rect.Top), true, true);

            // 上边
            ctx.LineTo(new Point(rect.Right - topRight, rect.Top), true, false);

            // 右上角圆弧
            if (topRight > 0)
            {
                ctx.ArcTo(
                    new Point(rect.Right, rect.Top + topRight),
                    new Size(topRight, topRight),
                    0, false, SweepDirection.Clockwise, true, false);
            }
            else
            {
                ctx.LineTo(new Point(rect.Right, rect.Top), true, false);
            }

            // 右边
            ctx.LineTo(new Point(rect.Right, rect.Bottom - bottomRight), true, false);

            // 右下角圆弧
            if (bottomRight > 0)
            {
                ctx.ArcTo(
                    new Point(rect.Right - bottomRight, rect.Bottom),
                    new Size(bottomRight, bottomRight),
                    0, false, SweepDirection.Clockwise, true, false);
            }
            else
            {
                ctx.LineTo(new Point(rect.Right, rect.Bottom), true, false);
            }

            // 下边
            ctx.LineTo(new Point(rect.Left + bottomLeft, rect.Bottom), true, false);

            // 左下角圆弧
            if (bottomLeft > 0)
            {
                ctx.ArcTo(
                    new Point(rect.Left, rect.Bottom - bottomLeft),
                    new Size(bottomLeft, bottomLeft),
                    0, false, SweepDirection.Clockwise, true, false);
            }
            else
            {
                ctx.LineTo(new Point(rect.Left, rect.Bottom), true, false);
            }

            // 左边
            ctx.LineTo(new Point(rect.Left, rect.Top + topLeft), true, false);

            // 左上角圆弧
            if (topLeft > 0)
            {
                ctx.ArcTo(
                    new Point(rect.Left + topLeft, rect.Top),
                    new Size(topLeft, topLeft),
                    0, false, SweepDirection.Clockwise, true, false);
            }
        }
    }
}
