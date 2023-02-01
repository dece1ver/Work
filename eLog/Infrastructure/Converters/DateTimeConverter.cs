using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;

namespace eLog.Infrastructure.Converters
{
    class DateTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value is DateTime time ? time : default).ToString("dd.MM.yyyy HH:mm");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DateTime.TryParseExact(value.ToString(), "dd.MM.yyyy HH:mm", null,DateTimeStyles.None, out var dateTime) ? dateTime : DependencyProperty.UnsetValue;
        }
    }
}
