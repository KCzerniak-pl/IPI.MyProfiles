using System;
using System.Globalization;
using System.Windows.Data;

namespace MyProfiles.Converters
{
    [ValueConversion(typeof(object), typeof(bool))]
    public class ObjectToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object TrueOrFalse = value as object;
            return TrueOrFalse != null ? true : false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
