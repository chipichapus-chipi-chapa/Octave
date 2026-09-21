using System;
using System.Globalization;
using System.Windows.Data;

namespace kurs_musik_club.converters
{
    public class BooleanToAvailabilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? "Доступно" : "Недоступно";
            }
            return "Неизвестно";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue)
            {
                return stringValue == "Доступно";
            }
            return false;
        }
    }
} 