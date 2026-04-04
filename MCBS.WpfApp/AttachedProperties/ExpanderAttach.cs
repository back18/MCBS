using System.Windows;
using System.Windows.Controls;

namespace MCBS.WpfApp.AttachedProperties
{
    public static class ExpanderAttach
    {
        #region CornerRadius
        [AttachedPropertyBrowsableForType(typeof(Expander))]
        public static CornerRadius GetCornerRadius(Expander expander)
        {
            return (CornerRadius)expander.GetValue(CornerRadiusProperty);
        }

        public static void SetCornerRadius(Expander expander, CornerRadius value)
        {
            expander.SetValue(CornerRadiusProperty, value);
        }

        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.RegisterAttached(
                "CornerRadius",
                typeof(CornerRadius),
                typeof(ExpanderAttach),
                new PropertyMetadata(new CornerRadius(0)));
        #endregion
    }
}
