using eLog.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace eLog.Infrastructure.Converters
{
    public class TaskInfoVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            parameter ??= Part.PartTaskInfo.NoData;
            if (value is Part.PartTaskInfo pti && parameter is Part.PartTaskInfo par)
            {
                if (pti == par) return Visibility.Collapsed;
                return Visibility.Visible;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
