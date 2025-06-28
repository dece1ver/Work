using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using libeLog.Models;

namespace libeLog.Extensions
{
    public static class EnumerableExtensions
    {
        public enum PartNameNormalizeOption
        {
            None, Simple, NormalizeAndRemoveParentheses
        }

        /// <summary>
        /// Объединяет две последовательности, возвращая элементы из первой коллекции,
        /// дополненные недостающими элементами из второй без дубликатов.
        /// Сохраняет порядок элементов из обеих коллекций в порядке появления.
        /// </summary>
        /// <typeparam name="T">Тип элементов последовательностей.</typeparam>
        /// <param name="source">Основная последовательность, сохраняемая первой в результате.</param>
        /// <param name="other">Дополнительная последовательность, элементы которой добавляются, если отсутствуют в основной.</param>
        /// <param name="comparer">
        /// Необязательный компаратор для сравнения элементов. Если не указан, используется компаратор по умолчанию для типа <typeparamref name="T"/>.
        /// </param>
        /// <returns>
        /// Последовательность, содержащая все уникальные элементы из <paramref name="source"/>,
        /// дополненную уникальными элементами из <paramref name="other"/>, без повторений.
        /// </returns>
        public static IEnumerable<T> UnionAppend<T>(
        this IEnumerable<T> source,
        IEnumerable<T> other,
        IEqualityComparer<T>? comparer = null)
        {
            var seen = new HashSet<T>(comparer ?? EqualityComparer<T>.Default);

            foreach (var item in source)
            {
                if (seen.Add(item))
                    yield return item;
            }

            foreach (var item in other)
            {
                if (seen.Add(item))
                    yield return item;
            }
        }

        /// <summary>
        /// Получает множество наименований деталей из коллекции с применением выбранной стратегии нормализации.
        /// </summary>
        /// <param name="source">Коллекция объектов SerialPart.</param>
        /// <param name="partNameNormalizeOption">Опция нормализации названий.</param>
        /// <returns>Хэш-множество нормализованных названий деталей.</returns>
        public static HashSet<string> PartNamesHashSet(this IEnumerable<SerialPart> source, PartNameNormalizeOption partNameNormalizeOption = PartNameNormalizeOption.None)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));

            Func<SerialPart, string> selector = partNameNormalizeOption switch
            {
                PartNameNormalizeOption.None => p => p.PartName,
                PartNameNormalizeOption.Simple => p => p.PartName.NormalizedPartName(),
                PartNameNormalizeOption.NormalizeAndRemoveParentheses => p => p.PartName.NormalizedPartNameWithoutComments(),
                _ => throw new ArgumentOutOfRangeException(nameof(partNameNormalizeOption),
                        $"Опция нормализации '{partNameNormalizeOption}' не поддерживается.")
            };

            return source.Select(selector).ToHashSet();
        }
    }
}
