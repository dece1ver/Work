using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace libeLog.Infrastructure.Sql
{
    public class ApplicationFunctions
    {
        /// <summary>
        /// Определение SQL-функции
        /// </summary>
        public class FunctionDefinition
        {
            public string Schema { get; init; } = "dbo";
            public string Name { get; init; } = null!;
            public List<FunctionParameter> Parameters { get; init; } = new();
            public string ReturnType { get; init; } = null!;
            public List<string> Options { get; init; } = new();
            public string Body { get; init; } = null!;

            public string FullName => $"{Schema}.{Name}";
        }

        /// <summary>
        /// Параметр функции
        /// </summary>
        public record FunctionParameter(string Name, string DataType);

        /// <summary>
        /// Строитель SQL-кода для функций
        /// </summary>
        public class FunctionSqlBuilder
        {
            public string BuildCreateScript(FunctionDefinition func)
            {
                var sb = new StringBuilder();
                sb.AppendLine($"CREATE FUNCTION {func.FullName}(");
                if (func.Parameters.Any())
                {
                    sb.AppendLine(string.Join(",\n", func.Parameters.Select(p => $"    {p.Name} {p.DataType}")));
                }
                sb.AppendLine(")");
                sb.AppendLine($"RETURNS {func.ReturnType}");
                if (func.Options.Any())
                    sb.AppendLine($"WITH {string.Join(", ", func.Options)}");
                sb.AppendLine("AS");
                sb.AppendLine("BEGIN");
                sb.AppendLine(func.Body);
                sb.AppendLine("END");
                return sb.ToString();
            }

            public string BuildDropScript(FunctionDefinition func)
            {
                return $@"
                DECLARE @sql NVARCHAR(MAX) = N'';
                SELECT @sql += 'ALTER TABLE [' + s.name + '].[' + t.name + '] DROP COLUMN [' + c.name + '];' + CHAR(13)
                FROM sys.computed_columns c
                JOIN sys.tables t ON t.object_id = c.object_id
                JOIN sys.schemas s ON s.schema_id = t.schema_id
                JOIN sys.sql_expression_dependencies d 
                    ON d.referencing_id = c.object_id
                WHERE d.referenced_entity_name = '{func.Name}'
                  AND d.referenced_schema_name = '{func.Schema}';

                IF LEN(@sql) > 0
                    EXEC sp_executesql @sql;
                IF OBJECT_ID('{func.FullName}', 'FN') IS NOT NULL
                    DROP FUNCTION {func.FullName};
                ";
            }

        }

        /// <summary>
        /// Менеджер создания/обновления функций
        /// </summary>
        public class FunctionManager
        {
            private readonly string _connectionString;
            private readonly FunctionSqlBuilder _builder;
            private readonly List<FunctionDefinition> _functions = new();

            public FunctionManager(string connectionString, FunctionSqlBuilder builder)
            {
                _connectionString = connectionString;
                _builder = builder;
            }

            /// <summary>
            /// Добавляет функцию в список на деплой
            /// </summary>
            public void Register(FunctionDefinition func)
            {
                _functions.Add(func);
            }

            /// <summary>
            /// Создаёт или заменяет все зарегистрированные функции
            /// </summary>
            public void DeployAll()
            {
                var script = BuildFullScript();
                ExecuteBatches(script);
            }

            private string BuildFullScript()
            {
                var sb = new StringBuilder();
                foreach (var func in _functions)
                {
                    sb.AppendLine(_builder.BuildDropScript(func));
                    sb.AppendLine("GO");
                    sb.AppendLine(_builder.BuildCreateScript(func));
                    sb.AppendLine("GO");
                }
                return sb.ToString();
            }

            private void ExecuteBatches(string script)
            {
                var batches = Regex.Split(script, @"^\s*GO\s*($|--.*)", RegexOptions.Multiline | RegexOptions.IgnoreCase);
                using var conn = new SqlConnection(_connectionString);
                conn.Open();
                foreach (var batch in batches)
                {
                    var txt = batch.Trim();
                    if (string.IsNullOrEmpty(txt)) continue;
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = txt;
                    cmd.ExecuteNonQuery();
                }
            }

            /// <summary>
            /// Имена и возвращаемые типы зарегистрированных функций
            /// </summary>
            public IEnumerable<string> Functions => _functions.Select(fn => $"{fn.FullName} -> {fn.ReturnType}");
        }
    }
}
