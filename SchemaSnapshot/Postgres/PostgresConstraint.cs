using SchemaSnapshot.DatabaseModel;

namespace SchemaSnapshot.Postgres
{
    internal class PostgresConstraint : Constraint
    {
        public string Definition { get; }

        public PostgresConstraint(
            string tableName,
            string name,
            string columnName,
            string type,
            string definition) : base(
            tableName,
            name,
            columnName,
            type)
        {
            Definition = definition;
        }
    }
}