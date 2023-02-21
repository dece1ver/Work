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
    internal class TimeSpanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TimeSpan timeSpan)
            {
                return timeSpan == TimeSpan.Zero ? string.Empty : timeSpan.ToString(Text.TimeSpanFormat);
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value.ToString() ?? string.Empty).TimeParse(out var dateTime) ? dateTime : DependencyProperty.UnsetValue;
        }
    }
}
