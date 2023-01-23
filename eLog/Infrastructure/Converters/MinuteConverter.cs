using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace eLog.Infrastructure.Converters
{
    class MinuteConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return $"{value} мин";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
