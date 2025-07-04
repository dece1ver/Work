using System.Collections.Generic;

namespace libeLog.Infrastructure.Sql
{
    public class ForeignKeyDefinition
    {
        public List<string> Columns { get; set; } = new();
        public string ReferencedTable { get; set; } = null!;
        public List<string> ReferencedColumns { get; set; } = new();
        public ForeignKeyAction OnDelete { get; set; } = ForeignKeyAction.NoAction;
        public ForeignKeyAction OnUpdate { get; set; } = ForeignKeyAction.NoAction;
    }
}