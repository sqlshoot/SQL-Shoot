using SchemaSnapshot.DatabaseModel;

namespace SchemaSnapshot.SqlServer
{
    internal class SqlServerColumn : Column
    {
        public SqlServerColumn(
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