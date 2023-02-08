using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace eLog.Infrastructure.Converters
{
    internal class ArrayConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is string[] { Length: > 0 } values ? string.Join("; ", values) : string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString()?.Split(';').Select(q => q.Trim()).ToArray() ?? Array.Empty<string>();
        }
    }
}
