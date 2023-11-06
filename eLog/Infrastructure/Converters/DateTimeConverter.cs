using System;
using System.Globalization;
using System.Windows.Data;
using eLog.Infrastructure.Extensions;
using libeLog;

namespace eLog.Infrastructure.Converters;

class DateTimeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DateTime dateTime)
        {
            return dateTime == DateTime.MinValue ? string.Empty : dateTime.ToString(Constants.DateTimeFormat);
        }
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        DateTime.TryParseExact(value.ToString(), "dd.MM.yyyy HH:mm", null, DateTimeStyles.None, out var dateTime);
        return dateTime;
    }
}
