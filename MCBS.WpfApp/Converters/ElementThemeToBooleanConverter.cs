using iNKORE.UI.WPF.Modern;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace MCBS.WpfApp.Converters
{
    public class ElementThemeToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ElementTheme theme)
                return theme != ElementTheme.Light;

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isChecked)
                return isChecked ? ElementTheme.Dark : ElementTheme.Light;

            return ElementTheme.Light;
        }
    }
}
