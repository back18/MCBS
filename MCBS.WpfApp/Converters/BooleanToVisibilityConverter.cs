using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace MCBS.WpfApp.Converters
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not bool boolValue)
                return Visibility.Collapsed;

            bool isReversed = IsReversed(parameter);

            if (isReversed)
                boolValue = !boolValue;

            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not Visibility visibility)
                return false;

            bool isReversed = IsReversed(parameter);
            bool result = visibility == Visibility.Visible;

            return isReversed ? !result : result;
        }

        private static bool IsReversed(object? parameter)
        {
            if (parameter is null)
                return false;

            string? paramStr = parameter.ToString()?.ToLowerInvariant();

            return paramStr == "true" ||
                   paramStr == "1" ||
                   paramStr == "reverse" ||
                   paramStr == "inverse";
        }
    }
}
