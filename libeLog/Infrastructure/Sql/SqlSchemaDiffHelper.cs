using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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
        /// <returns>True, если таблица существует</returns>
        public async Task<bool> TableExistsAsync(string tableName)
        {
            SqlValidationHelper.ValidateName(tableName);

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var sql = @"
                SELECT COUNT(*) 
                FROM INFORMATION_SCHEMA.TABLES 
                WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = @TableName";

            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@TableName", tableName);

            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result) > 0;
        }

        /// <summary>
        /// Получает список существующих столбцов таблицы
        /// </summary>
        /// <param name="tableName">Имя таблицы</param>
        /// <returns>Список имен столбцов</returns>
        public async Task<List<string>> GetExistingColumnsAsync(string tableName)
        {
            SqlValidationHelper.ValidateName(tableName);

            var columns = new List<string>();

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var sql = @"
                SELECT COLUMN_NAME 
                FROM INFORMATION_SCHEMA.COLUMNS 
                WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = @TableName";

            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@TableName", tableName);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
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
        /// <returns>Список столбцов для добавления</returns>
        public async Task<List<ColumnDefinition>> GetColumnsToAddAsync(string tableName, TableDefinition tableDefinition)
        {
            if (!await TableExistsAsync(tableName))
                return tableDefinition.Columns; // Если таблица не существует, добавляем все столбцы

            var existingColumns = await GetExistingColumnsAsync(tableName);
            return tableDefinition.Columns
                .Where(c => !existingColumns.Contains(c.Name, StringComparer.OrdinalIgnoreCase))
                .ToList();
        }

        /// <summary>
        /// Выполняет SQL-скрипт без возврата результатов
        /// </summary>
        /// <param name="sql">SQL-скрипт</param>
        /// <returns>Количество затронутых строк</returns>
        private async Task<int> ExecuteNonQueryAsync(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql))
                return 0;

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand(sql, connection);
            return await command.ExecuteNonQueryAsync();
        }

        public async Task<bool> ApplyMissingColumnsAndConstraintsAsync(TableDefinition tableDefinition)
        {
            var tableName = tableDefinition.Name;
            var helper = new SqlSchemaHelper();

            if (!await TableExistsAsync(tableName))
            {
                helper.AddTable(tableDefinition);
                await ExecuteNonQueryAsync(helper.GenerateCreateScript());
                return true;
            }

            var updated = false;

            var columnsToAdd = await GetColumnsToAddAsync(tableName, tableDefinition);
            if (columnsToAdd.Count > 0)
            {
                var columnScript = SqlSchemaAlterHelper.GenerateAddColumnsScript(tableName, columnsToAdd);
                await ExecuteNonQueryAsync(columnScript);
                updated = true;
            }

            var constraintScript = SqlSchemaAlterHelper.GenerateAddConstraintsScript(tableName, tableDefinition);
            if (!string.IsNullOrWhiteSpace(constraintScript))
            {
                await ExecuteNonQueryAsync(constraintScript);
                updated = true;
            }

            return updated;
        }
    }
}