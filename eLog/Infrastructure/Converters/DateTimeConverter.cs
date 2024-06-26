﻿using libeLog;
using System;
using System.Globalization;
using System.Windows.Data;

namespace eLog.Infrastructure.Converters
{
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
            DateTime.TryParseExact(value.ToString(), Constants.DateTimeFormat, null, DateTimeStyles.None, out var dateTime);
            return dateTime;
        }
    }
}