using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libeLog.Infrastructure.Sql
{
    public class SqlSchemaHelper
    {
        private readonly List<TableDefinition> _tables = new();

        public SqlSchemaHelper AddTable(TableDefinition table)
        {
            SqlValidationHelper.ValidateTable(table);
            _tables.Add(table);
            return this;
        }

        public string GenerateCreateScript()
        {
            var sb = new StringBuilder();

            foreach (var table in _tables)
            {
                sb.AppendLine($"CREATE TABLE [dbo].[{table.Name}] (");

                for (int i = 0; i < table.Columns.Count; i++)
                {
                    var col = table.Columns[i];
                    sb.Append($"    [{col.Name}] {col.SqlType}");

                    if (col.AutoIncrement) sb.Append(" IDENTITY(1,1)");

                    if (!col.IsNullable) sb.Append(" NOT NULL");

                    if (col.IsPrimaryKey && table.CompositePrimaryKey.Count == 0)
                        sb.Append(" PRIMARY KEY");

                    if (col.IsUnique) sb.Append(" UNIQUE");

                    if (i < table.Columns.Count - 1 || NeedsPostColumnConstraints(table)) sb.Append(",");
                    sb.AppendLine();
                }

                AddConstraints(sb, table);

                RemoveTrailingComma(sb);

                sb.AppendLine(");");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private void AddConstraints(StringBuilder sb, TableDefinition table)
        {
            if (table.CompositePrimaryKey.Count > 0)
            {
                string escapedColumns = string.Join(", ", table.CompositePrimaryKey.Select(c => $"[{c}]"));
                sb.AppendLine($"    CONSTRAINT [PK_{table.Name}] PRIMARY KEY ({escapedColumns}),");
            }

            for (int i = 0; i < table.CompositeUniques.Count; i++)
            {
                var unique = table.CompositeUniques[i];
                string escapedColumns = string.Join(", ", unique.Select(c => $"[{c}]"));
                sb.AppendLine($"    CONSTRAINT [UQ_{table.Name}_{i}] UNIQUE ({escapedColumns}),");
            }

            for (int i = 0; i < table.ForeignKeys.Count; i++)
            {
                var fk = table.ForeignKeys[i];
                string escapedColumns = string.Join(", ", fk.Columns.Select(c => $"[{c}]"));
                string escapedRefColumns = string.Join(", ", fk.ReferencedColumns.Select(c => $"[{c}]"));

                sb.Append($"    CONSTRAINT [FK_{table.Name}_{fk.ReferencedTable}_{i}] FOREIGN KEY ({escapedColumns}) ");
                sb.Append($"REFERENCES [dbo].[{fk.ReferencedTable}] ({escapedRefColumns})");

                if (fk.OnDelete != ForeignKeyAction.NoAction)
                    sb.Append($" ON DELETE {ToSqlAction(fk.OnDelete)}");

                if (fk.OnUpdate != ForeignKeyAction.NoAction)
                    sb.Append($" ON UPDATE {ToSqlAction(fk.OnUpdate)}");

                sb.AppendLine(",");
            }
        }

        private static void RemoveTrailingComma(StringBuilder sb)
        {
            int length = sb.Length;
            int i = length - 1;

            while (i >= 0 && (sb[i] == ' ' || sb[i] == '\r' || sb[i] == '\n'))
                i--;

            if (i >= 0 && sb[i] == ',')
                sb.Remove(i, 1);
        }

        private static bool NeedsPostColumnConstraints(TableDefinition table) =>
            table.CompositePrimaryKey.Count > 0 || table.CompositeUniques.Count > 0 || table.ForeignKeys.Count > 0;

        public static string ToSqlAction(ForeignKeyAction action) => action switch
        {
            ForeignKeyAction.Cascade => "CASCADE",
            ForeignKeyAction.SetNull => "SET NULL",
            ForeignKeyAction.SetDefault => "SET DEFAULT",
            _ => "NO ACTION"
        };
    }
}