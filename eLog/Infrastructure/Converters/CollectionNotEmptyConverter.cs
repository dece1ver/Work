using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace eLog.Infrastructure.Converters
{
    /// <summary>
    /// Конвертер, который проверяет коллекцию и возвращает true, если в ней есть элементы,
    /// и false, если коллекция пуста или равна null.
    /// </summary>
    public class CollectionNotEmptyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ICollection collection)
            {
                return collection.Count > 0;
            }

            if (value is IEnumerable enumerable)
            {
                var enumerator = enumerable.GetEnumerator();
                return enumerator.MoveNext();
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("Обратное преобразование не поддерживается");
        }
    }
}
