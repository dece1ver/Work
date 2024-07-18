using eLog.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace eLog.Infrastructure.Converters
{
    public class TaskInfoTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Part.PartTaskInfo pti)
            {
                return pti switch
                {
                    Part.PartTaskInfo.InList => "В списке",
                    Part.PartTaskInfo.NotInList => "Не в списке",
                    _ => "Нет данных",
                };
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
