using MCBS.WpfApp.Models;
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
    ///     <MyNamespace:ProgressButton/>
    ///
    /// </summary>
    public class ProgressButton : Button
    {
        static ProgressButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ProgressButton), new FrameworkPropertyMetadata(typeof(ProgressButton)));
        }

        // ==================== 依赖属性 ====================

        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(ProgressButton),
                new PropertyMetadata(new CornerRadius(0)));

        public static readonly DependencyProperty ThemeProperty =
            DependencyProperty.Register(nameof(Theme), typeof(ButtonTheme), typeof(ProgressButton),
                new PropertyMetadata(ButtonTheme.Default));

        public static readonly DependencyProperty HoverBackgroundProperty =
            DependencyProperty.Register(nameof(HoverBackground), typeof(Brush), typeof(ProgressButton),
                new PropertyMetadata(null));

        public static readonly DependencyProperty HoverBorderBrushProperty =
            DependencyProperty.Register(nameof(HoverBorderBrush), typeof(Brush), typeof(ProgressButton),
                new PropertyMetadata(null));

        public static readonly DependencyProperty PressedBackgroundProperty =
            DependencyProperty.Register(nameof(PressedBackground), typeof(Brush), typeof(ProgressButton),
                new PropertyMetadata(null));

        public static readonly DependencyProperty PressedBorderBrushProperty =
            DependencyProperty.Register(nameof(PressedBorderBrush), typeof(Brush), typeof(ProgressButton),
                new PropertyMetadata(null));

        public static readonly DependencyProperty IsBusyProperty =
            DependencyProperty.Register(nameof(IsBusy), typeof(bool?), typeof(ProgressButton),
                new PropertyMetadata(false));

        public static readonly DependencyProperty ProgressProperty =
            DependencyProperty.Register(nameof(Progress), typeof(double), typeof(ProgressButton),
                new PropertyMetadata(0.0, OnProgressChanged, CoerceProgress));

        public static readonly DependencyProperty IndicatorBackgroundProperty =
            DependencyProperty.Register(nameof(IndicatorBackground), typeof(Brush),
                typeof(ProgressButton), new PropertyMetadata(null));

        // ==================== 属性包装器 ====================

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        public ButtonTheme Theme
        {
            get => (ButtonTheme)GetValue(ThemeProperty);
            set => SetValue(ThemeProperty, value);
        }

        public Brush HoverBackground
        {
            get => (Brush)GetValue(HoverBackgroundProperty);
            set => SetValue(HoverBackgroundProperty, value);
        }

        public Brush HoverBorderBrush
        {
            get => (Brush)GetValue(HoverBorderBrushProperty);
            set => SetValue(HoverBorderBrushProperty, value);
        }

        public Brush PressedBackground
        {
            get => (Brush)GetValue(PressedBackgroundProperty);
            set => SetValue(PressedBackgroundProperty, value);
        }

        public Brush PressedBorderBrush
        {
            get => (Brush)GetValue(PressedBorderBrushProperty);
            set => SetValue(PressedBorderBrushProperty, value);
        }

        public bool? IsBusy
        {
            get => (bool?)GetValue(IsBusyProperty);
            set => SetValue(IsBusyProperty, value);
        }

        public double Progress
        {
            get => (double)GetValue(ProgressProperty);
            set => SetValue(ProgressProperty, value);
        }

        public Brush IndicatorBackground
        {
            get => (Brush)GetValue(IndicatorBackgroundProperty);
            set => SetValue(IndicatorBackgroundProperty, value);
        }

        // ==================== 回调方法 ====================

        private static void OnProgressChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // TODO: 进度变化时可添加额外逻辑
        }

        private static object CoerceProgress(DependencyObject d, object baseValue)
        {
            double value = (double)baseValue;
            return Math.Clamp(value, 0.0, 1.0);
        }
    }
}
