using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace eLog.Infrastructure.Converters
{
    class PositiveIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((int)value >= 0 ? value.ToString() : string.Empty) ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}