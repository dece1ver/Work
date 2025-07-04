using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libeLog.Extensions
{
    public static class CollectionExtensions
    {
        /// <summary>
        /// Преобразует перечисление в ObservableCollection.
        /// </summary>
        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> source)
        {
            return source is null
                ? new ObservableCollection<T>()
                : new ObservableCollection<T>(source);
        }
    }
}
