using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace libeLog.Infrastructure.Sql
{
    /// <summary>
    /// Класс для сравнения и обновления схемы базы данных
    /// </summary>
    public class SqlSchemaDiffHelper
    {
        private readonly string _connectionString;

        public SqlSchemaDiffHelper(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Проверяет существование таблицы в базе данных
        /// </summary>
        /// <param name="tableName">Имя таблицы</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        /// <returns>True, если таблица существует</returns>
        public async Task<bool> TableExistsAsync(string tableName, CancellationToken cancellationToken = default)
        {
            SqlValidationHelper.ValidateName(tableName);

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            var sql = @"
                SELECT COUNT(*) 
                FROM INFORMATION_SCHEMA.TABLES 
                WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = @TableName";

            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@TableName", tableName);

            var result = await command.ExecuteScalarAsync(cancellationToken);
            return Convert.ToInt32(result) > 0;
        }

        /// <summary>
        /// Получает список существующих столбцов таблицы
        /// </summary>
        /// <param name="tableName">Имя таблицы</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        /// <returns>Список имен столбцов</returns>
        public async Task<List<string>> GetExistingColumnsAsync(string tableName, CancellationToken cancellationToken = default)
        {
            SqlValidationHelper.ValidateName(tableName);

            var columns = new List<string>();

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            var sql = @"
                SELECT COLUMN_NAME 
                FROM INFORMATION_SCHEMA.COLUMNS 
                WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = @TableName";

            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@TableName", tableName);

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                columns.Add(reader.GetString(0));
            }

            return columns;
        }

        /// <summary>
        /// Находит столбцы, которые нужно добавить в существующую таблицу
        /// </summary>
        /// <param name="tableName">Имя таблицы</param>
        /// <param name="tableDefinition">Полное определение таблицы</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        /// <returns>Список столбцов для добавления</returns>
        public async Task<List<ColumnDefinition>> GetColumnsToAddAsync(string tableName, TableDefinition tableDefinition, CancellationToken cancellationToken = default)
        {
            if (!await TableExistsAsync(tableName, cancellationToken))
                return tableDefinition.Columns;

            var existingColumns = await GetExistingColumnsAsync(tableName, cancellationToken);
            return tableDefinition.Columns
                .Where(c => !existingColumns.Contains(c.Name, StringComparer.OrdinalIgnoreCase))
                .ToList();
        }

        /// <summary>
        /// Выполняет SQL-скрипт без возврата результатов
        /// </summary>
        /// <param name="sql">SQL-скрипт</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        /// <returns>Количество затронутых строк</returns>
        private async Task<int> ExecuteNonQueryAsync(string sql, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(sql))
                return 0;

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            using var command = new SqlCommand(sql, connection);
            return await command.ExecuteNonQueryAsync(cancellationToken);
        }

        public async Task<bool> ApplyMissingColumnsAndConstraintsAsync(TableDefinition tableDefinition, IProgress<(string, Status?)>? progress = null, CancellationToken cancellationToken = default)
        {
            await Task.Delay(10, cancellationToken);
            var tableName = tableDefinition.Name;
            var helper = new SqlSchemaHelper();

            if (!await TableExistsAsync(tableName, cancellationToken))
            {
                helper.AddTable(tableDefinition);
                await ExecuteNonQueryAsync(helper.GenerateCreateScript(), cancellationToken);
                return true;
            }

            var updated = false;

            // Добавляем недостающие столбцы
            var columnsToAdd = await GetColumnsToAddAsync(tableName, tableDefinition, cancellationToken);
            if (columnsToAdd.Count > 0)
            {
                progress?.Report(($"    | Требуется добавление столбцов:\n\t{string.Join(", ", columnsToAdd.Select(c => c.Name))}.", null));
                var columnScript = SqlSchemaAlterHelper.GenerateAddColumnsScript(tableName, columnsToAdd);
                await ExecuteNonQueryAsync(columnScript, cancellationToken);
                updated = true;
            }

            // Проверяем и добавляем недостающие ограничения
            var constraintsToAdd = new List<string>();

            // Проверка первичного ключа
            if (tableDefinition.CompositePrimaryKey.Count > 0)
            {
                if (!await PrimaryKeyExistsAsync(tableName, tableDefinition.CompositePrimaryKey, cancellationToken))
                {
                    progress?.Report(("    | Добавление первичного ключа...", null));
                    constraintsToAdd.Add($"PRIMARY KEY ({string.Join(", ", tableDefinition.CompositePrimaryKey.Select(SqlValidationHelper.EscapeName))})");
                }
            }

            // Проверка уникальных ограничений
            foreach (var uniqueColumns in tableDefinition.CompositeUniques)
            {
                if (!await UniqueConstraintExistsAsync(tableName, uniqueColumns, cancellationToken))
                {
                    progress?.Report(($"    | Добавление уникального ограничения на столбцы: {string.Join(", ", uniqueColumns)}...", null));
                    var constraintName = $"UQ_{tableName}_{string.Join("_", uniqueColumns)}";
                    constraintsToAdd.Add($"CONSTRAINT {SqlValidationHelper.EscapeName(constraintName)} UNIQUE ({string.Join(", ", uniqueColumns.Select(SqlValidationHelper.EscapeName))})");
                }
            }

            // Проверка внешних ключей
            foreach (var fk in tableDefinition.ForeignKeys)
            {
                if (!await ForeignKeyExistsAsync(tableName, fk, cancellationToken))
                {
                    progress?.Report(($"    | Добавление внешнего ключа к таблице {fk.ReferencedTable}...", null));
                    var constraintName = $"FK_{tableName}_{fk.ReferencedTable}_{string.Join("_", fk.Columns)}";
                    var fkScript = $"CONSTRAINT {SqlValidationHelper.EscapeName(constraintName)} FOREIGN KEY ({string.Join(", ", fk.Columns.Select(SqlValidationHelper.EscapeName))}) REFERENCES {SqlValidationHelper.EscapeName(fk.ReferencedTable)} ({string.Join(", ", fk.ReferencedColumns.Select(SqlValidationHelper.EscapeName))})";

                    if (fk.OnDelete != ForeignKeyAction.NoAction)
                        fkScript += $" ON DELETE {GetForeignKeyActionString(fk.OnDelete)}";
                    if (fk.OnUpdate != ForeignKeyAction.NoAction)
                        fkScript += $" ON UPDATE {GetForeignKeyActionString(fk.OnUpdate)}";

                    constraintsToAdd.Add(fkScript);
                }
            }

            // Применяем ограничения, если есть что добавлять
            if (constraintsToAdd.Count > 0)
            {
                var alterScript = $"ALTER TABLE {SqlValidationHelper.EscapeName(tableName)} ADD {string.Join(", ", constraintsToAdd)}";
                await ExecuteNonQueryAsync(alterScript, cancellationToken);
                updated = true;
            }

            return updated;
        }

        private static string GetForeignKeyActionString(ForeignKeyAction action)
        {
            return action switch
            {
                ForeignKeyAction.Cascade => "CASCADE",
                ForeignKeyAction.SetNull => "SET NULL",
                ForeignKeyAction.SetDefault => "SET DEFAULT",
                ForeignKeyAction.Restrict => "RESTRICT",
                _ => "NO ACTION"
            };
        }

        /// <summary>
        /// Получает список существующих ограничений таблицы
        /// </summary>
        /// <param name="tableName">Имя таблицы</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Словарь с типами и именами ограничений</returns>
        public async Task<Dictionary<string, List<string>>> GetExistingConstraintsAsync(string tableName, CancellationToken cancellationToken = default)
        {
            SqlValidationHelper.ValidateName(tableName);

            var constraints = new Dictionary<string, List<string>>
            {
                ["PRIMARY KEY"] = new List<string>(),
                ["UNIQUE"] = new List<string>(),
                ["FOREIGN KEY"] = new List<string>(),
                ["CHECK"] = new List<string>(),
                ["DEFAULT"] = new List<string>()
            };

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            // Получаем все ограничения таблицы
            var sql = @"
        SELECT 
            tc.CONSTRAINT_NAME,
            tc.CONSTRAINT_TYPE,
            COALESCE(STRING_AGG(kcu.COLUMN_NAME, ','), '') as COLUMNS
        FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
        LEFT JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu 
            ON tc.CONSTRAINT_NAME = kcu.CONSTRAINT_NAME 
            AND tc.TABLE_SCHEMA = kcu.TABLE_SCHEMA
            AND tc.TABLE_NAME = kcu.TABLE_NAME
        WHERE tc.TABLE_SCHEMA = 'dbo' AND tc.TABLE_NAME = @TableName
        GROUP BY tc.CONSTRAINT_NAME, tc.CONSTRAINT_TYPE
        ORDER BY tc.CONSTRAINT_TYPE, tc.CONSTRAINT_NAME";

            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@TableName", tableName);

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                var constraintName = reader.GetString("CONSTRAINT_NAME");
                var constraintType = reader.GetString("CONSTRAINT_TYPE");
                var columns = reader.GetString("COLUMNS");

                if (constraints.ContainsKey(constraintType))
                {
                    constraints[constraintType].Add($"{constraintName}({columns})");
                }
            }

            return constraints;
        }

        /// <summary>
        /// Проверяет существование первичного ключа на указанных столбцах
        /// </summary>
        /// <param name="tableName">Имя таблицы</param>
        /// <param name="columns">Столбцы первичного ключа</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>True, если первичный ключ существует</returns>
        public async Task<bool> PrimaryKeyExistsAsync(string tableName, List<string> columns, CancellationToken cancellationToken = default)
        {
            if (columns == null || columns.Count == 0) return true; // Нет PK для проверки

            SqlValidationHelper.ValidateName(tableName);

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            var sql = @"
                SELECT STRING_AGG(kcu.COLUMN_NAME, ',') as PK_COLUMNS
                FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
                JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu 
                    ON tc.CONSTRAINT_NAME = kcu.CONSTRAINT_NAME 
                    AND tc.TABLE_SCHEMA = kcu.TABLE_SCHEMA
                    AND tc.TABLE_NAME = kcu.TABLE_NAME
                WHERE tc.TABLE_SCHEMA = 'dbo' 
                    AND tc.TABLE_NAME = @TableName 
                    AND tc.CONSTRAINT_TYPE = 'PRIMARY KEY'
                GROUP BY tc.CONSTRAINT_NAME";

            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@TableName", tableName);

            var result = await command.ExecuteScalarAsync(cancellationToken);
            if (result == null) return false;

            var existingColumns = result.ToString()?.Split(',').Select(c => c.Trim()).OrderBy(c => c);
            var requestedColumns = columns.OrderBy(c => c);

            return existingColumns.SequenceEqual(requestedColumns, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Проверяет существование внешнего ключа
        /// </summary>
        /// <param name="tableName">Имя таблицы</param>
        /// <param name="foreignKey">Определение внешнего ключа</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>True, если внешний ключ существует</returns>
        public async Task<bool> ForeignKeyExistsAsync(string tableName, ForeignKeyDefinition foreignKey, CancellationToken cancellationToken = default)
        {
            SqlValidationHelper.ValidateName(tableName);

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            var sql = @"
        SELECT 1
        FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS rc
        JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu1 
            ON rc.CONSTRAINT_NAME = kcu1.CONSTRAINT_NAME
        JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu2 
            ON rc.UNIQUE_CONSTRAINT_NAME = kcu2.CONSTRAINT_NAME
        WHERE kcu1.TABLE_SCHEMA = 'dbo' 
            AND kcu1.TABLE_NAME = @TableName
            AND kcu2.TABLE_NAME = @ReferencedTable
            AND kcu1.COLUMN_NAME = @Column
            AND kcu2.COLUMN_NAME = @ReferencedColumn";

            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@TableName", tableName);
            command.Parameters.AddWithValue("@ReferencedTable", foreignKey.ReferencedTable);

            // Проверяем каждую пару столбцов FK
            for (int i = 0; i < foreignKey.Columns.Count; i++)
            {
                command.Parameters.Clear();
                command.Parameters.AddWithValue("@TableName", tableName);
                command.Parameters.AddWithValue("@ReferencedTable", foreignKey.ReferencedTable);
                command.Parameters.AddWithValue("@Column", foreignKey.Columns[i]);
                command.Parameters.AddWithValue("@ReferencedColumn", foreignKey.ReferencedColumns[i]);

                var result = await command.ExecuteScalarAsync(cancellationToken);
                if (result == null) return false;
            }

            return true;
        }

        /// <summary>
        /// Проверяет существование уникального ограничения на указанных столбцах
        /// </summary>
        /// <param name="tableName">Имя таблицы</param>
        /// <param name="columns">Столбцы уникального ограничения</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>True, если уникальное ограничение существует</returns>
        public async Task<bool> UniqueConstraintExistsAsync(string tableName, List<string> columns, CancellationToken cancellationToken = default)
        {
            if (columns == null || columns.Count == 0) return true;

            SqlValidationHelper.ValidateName(tableName);

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            var sql = @"
        SELECT STRING_AGG(kcu.COLUMN_NAME, ',') as UNIQUE_COLUMNS
        FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
        JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu 
            ON tc.CONSTRAINT_NAME = kcu.CONSTRAINT_NAME 
            AND tc.TABLE_SCHEMA = kcu.TABLE_SCHEMA
            AND tc.TABLE_NAME = kcu.TABLE_NAME
        WHERE tc.TABLE_SCHEMA = 'dbo' 
            AND tc.TABLE_NAME = @TableName 
            AND tc.CONSTRAINT_TYPE = 'UNIQUE'
        GROUP BY tc.CONSTRAINT_NAME";

            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@TableName", tableName);

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var requestedColumns = columns.OrderBy(c => c).ToList();

            while (await reader.ReadAsync(cancellationToken))
            {
                var existingColumns = reader.GetString("UNIQUE_COLUMNS")
                    .Split(',')
                    .Select(c => c.Trim())
                    .OrderBy(c => c)
                    .ToList();

                if (existingColumns.SequenceEqual(requestedColumns, StringComparer.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }
    }
}