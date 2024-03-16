using libeLog;
using libeLog.Extensions;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace eLog.Infrastructure.Converters
{
    internal class TimeSpanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TimeSpan timeSpan)
            {
                return timeSpan == TimeSpan.Zero ? string.Empty : timeSpan.ToString(Constants.TimeSpanFormat);
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value.ToString() ?? string.Empty).TimeParse(out var dateTime) ? dateTime : DependencyProperty.UnsetValue;
        }
    }
}