using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace libeLog.Infrastructure.Sql
{
    /// <summary>
    /// Вспомогательный класс для валидации SQL-объектов
    /// </summary>
    public static class SqlValidationHelper
    {
        private static readonly Regex ValidNameRegex = new(@"^[A-Za-z_][A-Za-z0-9_]*$", RegexOptions.Compiled);

        /// <summary>
        /// Проверяет корректность имени объекта SQL
        /// </summary>
        /// <param name="name">Имя для проверки</param>
        /// <exception cref="ArgumentException">Если имя не соответствует требованиям</exception>
        public static void ValidateName(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Имя не может быть пустым");

            // Если имя уже в скобках, извлекаем его для проверки
            string unescapedName = name;
            if (name.StartsWith("[") && name.EndsWith("]"))
                unescapedName = name.Substring(1, name.Length - 2);

            if (!ValidNameRegex.IsMatch(unescapedName))
                throw new ArgumentException($"Недопустимое имя: {unescapedName}");
        }

        /// <summary>
        /// Проверяет существование указанных столбцов в списке определений
        /// </summary>
        /// <param name="columns">Имена столбцов для проверки</param>
        /// <param name="definedColumns">Список определений столбцов</param>
        /// <param name="ignoreCase">Игнорировать регистр при сравнении</param>
        /// <exception cref="ArgumentException">Если столбец не найден</exception>
        public static void ValidateColumnsExist(IEnumerable<string> columns, IEnumerable<ColumnDefinition> definedColumns, bool ignoreCase = true)
        {
            var stringComparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

            foreach (var col in columns)
            {
                if (!definedColumns.Any(c => c.Name.Equals(col, stringComparison)))
                    throw new ArgumentException($"Столбец {col} не найден");
            }
        }

        /// <summary>
        /// Проверяет таблицу на корректность всех имен и ссылок
        /// </summary>
        /// <param name="table">Определение таблицы для проверки</param>
        /// <exception cref="ArgumentException">Если найдена ошибка в определении</exception>
        public static void ValidateTable(TableDefinition table)
        {
            ValidateName(table.Name);

            foreach (var col in table.Columns)
            {
                ValidateName(col.Name);
                if (col.SqlType is null or "") throw new ArgumentException($"Тип столбца '{col.Name}' не задан");
            }

            foreach (var name in table.CompositePrimaryKey.Concat(table.CompositeUniques.SelectMany(x => x)))
                ValidateName(name);

            foreach (var fk in table.ForeignKeys)
            {
                ValidateName(fk.ReferencedTable);
                foreach (var col in fk.Columns.Concat(fk.ReferencedColumns))
                    ValidateName(col);

                if (fk.Columns.Count != fk.ReferencedColumns.Count)
                    throw new ArgumentException($"Несовпадение количества колонок в FK для таблицы {table.Name}");
            }
        }

        /// <summary>
        /// Экранирует имя для использования в SQL
        /// </summary>
        /// <param name="name">Имя для экранирования</param>
        /// <returns>Экранированное имя</returns>
        public static string EscapeName(string name)
        {
            if (name.StartsWith("[") && name.EndsWith("]"))
                return name;
            return $"[{name}]";
        }
    }
}