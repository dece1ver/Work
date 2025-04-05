using Microsoft.Data.SqlClient;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace libeLog.Extensions
{
    public static class SqlDataReaderExt
    {
        /// <summary>
        /// Возвращает nullable значение указанного типа из заданной колонки.
        /// </summary>
        /// <typeparam name="T">Тип данных, который должен быть nullable.</typeparam>
        /// <param name="reader">Читатель данных.</param>
        /// <param name="ordinal">Индекс колонки.</param>
        /// <returns>Значение типа <c>T?</c>, или <c>null</c>, если в колонке <c>DBNull</c>.</returns>
        public static T? GetNullable<T>(this DbDataReader reader, int ordinal) where T : struct
        {
            return reader.IsDBNull(ordinal) ? (T?)null : reader.GetFieldValue<T>(ordinal);
        }

        /// <summary>
        /// Безопасно возвращает nullable-логическое значение из указанной колонки.
        /// </summary>
        /// <param name="reader">Читатель данных.</param>
        /// <param name="ordinal">Индекс колонки.</param>
        /// <returns>Значение типа <c>bool</c>, или <c>null</c>, если в колонке <c>DBNull</c>.</returns>
        public static bool? GetNullableBoolean(this SqlDataReader reader, int ordinal)
        {
            if (!reader.IsDBNull(ordinal))
            {
                return reader.GetBoolean(ordinal);
            }
            return null;
        }

        public static double? GetNullableDouble(this SqlDataReader reader, int ordinal)
        {
            if (!reader.IsDBNull(ordinal))
            {
                return reader.GetDouble(ordinal);
            }
            return null;
        }

        public static string GetStringOrEmpty(this SqlDataReader reader, int index) =>
            reader.IsDBNull(index) ? string.Empty : reader.GetString(index);

        public static DateTime GetDateTimeOrMinValue(this SqlDataReader reader, int index) =>
            reader.IsDBNull(index) ? DateTime.MinValue : reader.GetDateTime(index);

        /// <summary>
        /// Асинхронно получает значение из БД с обработкой DBNull.
        /// </summary>
        public static async Task<T> GetValueOrDefaultAsync<T>(
            this SqlDataReader reader,
            int index,
            T defaultValue = default!,
            CancellationToken cancellationToken = default)
        {
            if (await reader.IsDBNullAsync(index, cancellationToken).ConfigureAwait(false))
            {
                return defaultValue;
            }

            Type type = typeof(T);
            Type? underlyingType = Nullable.GetUnderlyingType(type);

            try
            {
                if (type == typeof(string))
                    return (T)(object)await reader.GetFieldValueAsync<string>(index, cancellationToken).ConfigureAwait(false);
                else if (type == typeof(int) || (underlyingType == typeof(int)))
                    return (T)(object)await reader.GetFieldValueAsync<int>(index, cancellationToken).ConfigureAwait(false);
                else if (type == typeof(long) || (underlyingType == typeof(long)))
                    return (T)(object)await reader.GetFieldValueAsync<long>(index, cancellationToken).ConfigureAwait(false);
                else if (type == typeof(double) || (underlyingType == typeof(double)))
                    return (T)(object)await reader.GetFieldValueAsync<double>(index, cancellationToken).ConfigureAwait(false);
                else if (type == typeof(decimal) || (underlyingType == typeof(decimal)))
                    return (T)(object)await reader.GetFieldValueAsync<decimal>(index, cancellationToken).ConfigureAwait(false);
                else if (type == typeof(DateTime) || (underlyingType == typeof(DateTime)))
                    return (T)(object)await reader.GetFieldValueAsync<DateTime>(index, cancellationToken).ConfigureAwait(false);
                else if (type == typeof(bool) || (underlyingType == typeof(bool)))
                    return (T)(object)await reader.GetFieldValueAsync<bool>(index, cancellationToken).ConfigureAwait(false);
                else if (type == typeof(Guid) || (underlyingType == typeof(Guid)))
                    return (T)(object)await reader.GetFieldValueAsync<Guid>(index, cancellationToken).ConfigureAwait(false);

                try
                {
                    return await reader.GetFieldValueAsync<T>(index, cancellationToken).ConfigureAwait(false);
                }
                catch
                {
                    object value = await reader.GetFieldValueAsync<object>(index, cancellationToken).ConfigureAwait(false);
                    return (T)Convert.ChangeType(value, underlyingType ?? type);
                }
            }
            catch (InvalidCastException ex)
            {
                throw new InvalidCastException(
                    $"Не удалось преобразовать значение из столбца {index} в тип {type.Name}",
                    ex);
            }
        }

        /// <summary>
        /// Получает значение по имени столбца
        /// </summary>
        public static Task<T> GetValueOrDefaultAsync<T>(
            this SqlDataReader reader,
            string columnName,
            T defaultValue = default!,
            CancellationToken cancellationToken = default)
        {
            int index = reader.GetOrdinal(columnName);
            return reader.GetValueOrDefaultAsync(index, defaultValue, cancellationToken);
        }

        /// <summary>
        /// Перегрузка только с CancellationToken
        /// </summary>
        public static Task<T> GetValueOrDefaultAsync<T>(
            this SqlDataReader reader,
            int index,
            CancellationToken cancellationToken) =>
            reader.GetValueOrDefaultAsync<T>(index, default!, cancellationToken);
    }
}
