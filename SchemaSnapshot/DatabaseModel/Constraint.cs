namespace SchemaSnapshot.DatabaseModel
{
    public class Constraint
    {
        public string TableName { get; }
        public string IndexName { get; }
        public string ColumnName { get; }
        public bool IsPrimaryKey { get; }
        public bool IsUniqueConstraint { get; }
        public bool IsUnique { get; }
        public string TypeDescription { get; }
        public bool IsIncludedColumn { get; }
        public string Type { get; }

        public Constraint(
            string tableName,
            string indexName,
            string columnName,
            bool isPrimaryKey,
            bool isUniqueConstraint,
            bool isUnique,
            string typeDescription,
            bool isIncludedColumn,
            string type)
        {
            TableName = tableName;
            IndexName = indexName;
            ColumnName = columnName;
            IsPrimaryKey = isPrimaryKey;
            IsUniqueConstraint = isUniqueConstraint;
            IsUnique = isUnique;
            TypeDescription = typeDescription;
            IsIncludedColumn = isIncludedColumn;
            Type = type;
        }
    }
}