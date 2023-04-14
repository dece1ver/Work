using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;
using System.Windows.Controls;
using eLog.Models;

namespace eLog.Infrastructure.Converters
{
    internal class PreviousShiftsLineConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not Part part) return Visibility.Collapsed;
            var date = part.StartSetupTime.Date;
            var todayParts = AppSettings.Instance.Parts.Where(p =>
                p.Shift == AppSettings.Instance.CurrentShift && p.StartSetupTime.Date == DateTime.Today.Date).ToList();
            return todayParts.Count > 0 && part == todayParts[^1] ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
