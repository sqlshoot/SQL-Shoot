using SchemaSnapshot.DatabaseModel;

namespace SchemaSnapshot.SqlServer
{
    internal class SqlServerConstraint : Constraint
    {
        public bool IsPrimaryKey { get; }
        public bool IsUniqueConstraint { get; }
        public bool IsUnique { get; }
        public string TypeDescription { get; }
        public bool IsIncludedColumn { get; }

        public SqlServerConstraint(
            string tableName,
            string name,
            string columnName,
            bool isPrimaryKey,
            bool isUniqueConstraint,
            bool isUnique,
            string typeDescription,
            bool isIncludedColumn,
            string type) : base(
            tableName,
            name,
            columnName,
            type)
        {
            IsPrimaryKey = isPrimaryKey;
            IsUniqueConstraint = isUniqueConstraint;
            IsUnique = isUnique;
            TypeDescription = typeDescription;
            IsIncludedColumn = isIncludedColumn;
        }
    }
}