namespace libeLog.Infrastructure.Sql
{
    public class ColumnDefinition
    {
        public string Name { get; set; } = null!;
        public string SqlType { get; set; } = null!;
        public bool IsNullable { get; set; } = true;
        public bool IsPrimaryKey { get; set; }
        public bool IsUnique { get; set; }
        public bool AutoIncrement { get; set; }
        public ForeignKeyDefinition? ForeignKey { get; set; }
        public string? DefaultValueSql { get; set; }
        public string? ComputedExpression { get; set; }
        public bool IsPersisted { get; set; } = true;
        public bool IsComputed => !string.IsNullOrEmpty(ComputedExpression);
    }
}