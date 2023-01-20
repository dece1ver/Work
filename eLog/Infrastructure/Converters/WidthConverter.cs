using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace eLog.Infrastructure.Converters
{
    class WidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ListView l = value as ListView;
            GridView g = l.View as GridView;
            double total = 0;
            for (int i = 0; i < g.Columns.Count - 1; i++)
            {
                total += g.Columns[i].Width;
            }
            return (l.ActualWidth - total);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
