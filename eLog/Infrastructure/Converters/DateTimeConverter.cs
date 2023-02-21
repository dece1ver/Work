using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;
using eLog.Infrastructure.Extensions;

namespace eLog.Infrastructure.Converters
{
    class DateTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime dateTime)
            {
                return dateTime == DateTime.MinValue ? string.Empty : dateTime.ToString(Text.DateTimeFormat);
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DateTime.TryParseExact(value.ToString(), "dd.MM.yyyy HH:mm", null, DateTimeStyles.None, out var dateTime);
            return dateTime;
        }
    }
}
