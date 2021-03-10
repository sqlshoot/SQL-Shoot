using SchemaSnapshot.DatabaseModel;

namespace SchemaSnapshot.SqlServer
{
    internal class SqlServerConstraint : Constraint
    {
        public SqlServerConstraint(
            string tableName,
            string indexName,
            string columnName,
            bool isPrimaryKey,
            bool isUniqueConstraint,
            bool isUnique,
            string typeDescription,
            bool isIncludedColumn,
            string type) : base(
            tableName,
            indexName,
            columnName,
            isPrimaryKey,
            isUniqueConstraint,
            isUnique,
            typeDescription,
            isIncludedColumn,
            type)
        { }
    }
}