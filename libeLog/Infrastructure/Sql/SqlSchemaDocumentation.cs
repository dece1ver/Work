using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libeLog.Infrastructure.Sql
{
    public static class SqlSchemaDocumentation
    {
        public static string GenerateMarkdown(TableDefinition table)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"## Таблица: {table.Name}");
            sb.AppendLine();
            sb.AppendLine("| Колонка      | Тип           | NULL | PK | UNIQUE | FK | FK-таблица |");
            sb.AppendLine("|--------------|---------------|------|----|--------|----|-------------|");

            foreach (var col in table.Columns)
            {
                var isNullable = col.IsNullable ? "✔" : "❌";
                var isPk = col.IsPrimaryKey || table.CompositePrimaryKey.Contains(col.Name) ? "✔" : "";
                var isUnique = col.IsUnique || table.CompositeUniques.Any(u => u.Contains(col.Name)) ? "✔" : "";

                // Находим FK-описание, если есть
                var fk = table.ForeignKeys.FirstOrDefault(f => f.Columns.Count == 1 && f.Columns[0] == col.Name);

                var isFk = fk != null ? "✔" : "";
                var fkRef = fk != null ? $"{fk.ReferencedTable}({fk.ReferencedColumns[0]})" : "";

                sb.AppendLine($"| {col.Name,-12} | {col.SqlType,-13} | {isNullable,-4} | {isPk,-2} | {isUnique,-6} | {isFk,-2} | {fkRef,-11} |");
            }

            sb.AppendLine();

            if (table.CompositePrimaryKey.Count > 1)
                sb.AppendLine($"**Составной первичный ключ:** {string.Join(", ", table.CompositePrimaryKey)}");

            if (table.CompositeUniques.Count > 0)
            {
                sb.AppendLine("**Уникальные ограничения:**");
                foreach (var uniq in table.CompositeUniques)
                    sb.AppendLine($"- {string.Join(", ", uniq)}");
            }

            if (table.ForeignKeys.Any(f => f.Columns.Count > 1))
            {
                sb.AppendLine("**Составные внешние ключи:**");
                foreach (var fk in table.ForeignKeys.Where(f => f.Columns.Count > 1))
                {
                    var cols = string.Join(", ", fk.Columns);
                    var refs = string.Join(", ", fk.ReferencedColumns);
                    sb.AppendLine($"- ({cols}) → {fk.ReferencedTable} ({refs})");
                }
            }

            return sb.ToString();
        }

        public static string GenerateMarkdownForTables(IEnumerable<TableDefinition> tables)
        {
            var sb = new StringBuilder();

            foreach (var table in tables)
            {
                sb.AppendLine($"## Таблица: {table.Name}");
                sb.AppendLine();
                sb.AppendLine("| Колонка      | Тип           | NULL | PK | UNIQUE | FK | FK-таблица     |");
                sb.AppendLine("|--------------|---------------|------|----|--------|----|----------------|");

                foreach (var col in table.Columns)
                {
                    var isNullable = col.IsNullable ? "✔" : "❌";
                    var isPk = col.IsPrimaryKey || table.CompositePrimaryKey.Contains(col.Name) ? "✔" : "";
                    var isUnique = col.IsUnique || table.CompositeUniques.Any(u => u.Contains(col.Name)) ? "✔" : "";

                    var fk = table.ForeignKeys.FirstOrDefault(f => f.Columns.Count == 1 && f.Columns[0] == col.Name);
                    var isFk = fk != null ? "✔" : "";
                    var fkRef = fk != null ? $"{fk.ReferencedTable}({fk.ReferencedColumns[0]})" : "";

                    sb.AppendLine($"| {col.Name,-12} | {col.SqlType,-13} | {isNullable,-4} | {isPk,-2} | {isUnique,-6} | {isFk,-2} | {fkRef,-16} |");
                }

                sb.AppendLine();

                if (table.CompositePrimaryKey.Count > 1)
                    sb.AppendLine($"**Составной первичный ключ:** {string.Join(", ", table.CompositePrimaryKey)}");

                if (table.CompositeUniques.Count > 0)
                {
                    sb.AppendLine("**Уникальные ограничения:**");
                    foreach (var uniq in table.CompositeUniques)
                        sb.AppendLine($"- {string.Join(", ", uniq)}");
                }

                if (table.ForeignKeys.Any(f => f.Columns.Count > 1))
                {
                    sb.AppendLine("**Составные внешние ключи:**");
                    foreach (var fk in table.ForeignKeys.Where(f => f.Columns.Count > 1))
                    {
                        var cols = string.Join(", ", fk.Columns);
                        var refs = string.Join(", ", fk.ReferencedColumns);
                        sb.AppendLine($"- ({cols}) → {fk.ReferencedTable} ({refs})");
                    }
                }

                sb.AppendLine("---");
                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}
