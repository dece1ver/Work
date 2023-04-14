using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;
using eLog.Models;

namespace eLog.Infrastructure.Converters
{
    public class DownTimesValidationConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool hasErrors = false;
            foreach (object value in values)
            {
                if ((bool)value == true)
                {
                    hasErrors = true;
                    break;
                }
            }
            return !hasErrors;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
