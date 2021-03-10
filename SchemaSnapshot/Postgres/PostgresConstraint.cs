using SchemaSnapshot.DatabaseModel;

namespace SchemaSnapshot.Postgres
{
    internal class PostgresConstraint : Constraint
    {
        public PostgresConstraint(
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