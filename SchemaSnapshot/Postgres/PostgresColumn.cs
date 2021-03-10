using SchemaSnapshot.DatabaseModel;

namespace SchemaSnapshot.Postgres
{
    internal class PostgresColumn : Column
    {
        public PostgresColumn(
            string parentTableName,
            string name,
            int ordinalPosition,
            bool isNullable,
            string dataType)
            : base(parentTableName,
                name,
                ordinalPosition,
                isNullable,
                dataType) { }
    }
}