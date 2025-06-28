using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace eLog.Infrastructure.Converters
{
    /// <summary>
    /// Преобразует логическое значение в кисть для отображения в Foreground.
    /// true — активный цвет, false — неактивный.
    /// </summary>
    public class BoolToBrushConverter : IValueConverter
    {
        public Brush TrueBrush { get; set; } = Brushes.DeepPink;

        public Brush FalseBrush { get; set; } = Brushes.LightGray;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
                return b ? TrueBrush : FalseBrush;

            return FalseBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            Binding.DoNothing;
    }
}
//Bookmark
//Currency
//
//
//
//