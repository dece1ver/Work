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
    class TimeSpanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value is TimeSpan time ? time : default).ToString(@"hh\:mm\:ss");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value.ToString() ?? string.Empty).TimeParse(out var dateTime) ? dateTime : DependencyProperty.UnsetValue;
        }
    }
}
