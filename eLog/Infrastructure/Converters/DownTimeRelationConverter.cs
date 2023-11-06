using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows;
using eLog.Models;

namespace eLog.Infrastructure.Converters;

class DownTimeRelationConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return $"во время {(value is DownTime.Relations.Setup ? "наладки" : "изготовления")})";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return DependencyProperty.UnsetValue;
    }
}
