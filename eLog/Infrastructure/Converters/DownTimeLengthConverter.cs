﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace eLog.Infrastructure.Converters
{
    class DownTimeLengthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is double num and > 0 ? $"({num} мин." : "(В процессе";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}