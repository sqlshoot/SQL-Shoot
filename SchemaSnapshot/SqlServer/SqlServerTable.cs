using SchemaSnapshot.DatabaseModel;

namespace SchemaSnapshot.SqlServer
{
    internal class SqlServerTable : Table
    {
        public SqlServerTable(string parentSchema, string name) : base(parentSchema, name) { }
    }
}