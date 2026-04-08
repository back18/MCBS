using QuanLib.TextFormat;
using System;
using System.Globalization;
using System.Windows.Data;

namespace MCBS.WpfApp.Converters
{
    public class BytesFormatConverter : IValueConverter
    {
        private static readonly BytesFormatter _bytesFormatter = new(AbbreviationBytesUnitText.Default);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is long size)
                return _bytesFormatter.Format(size);

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}