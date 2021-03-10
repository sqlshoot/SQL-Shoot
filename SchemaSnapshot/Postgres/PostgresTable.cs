using SchemaSnapshot.DatabaseModel;

namespace SchemaSnapshot.Postgres
{
    internal class PostgresTable : Table
    {
        public PostgresTable(string parentSchema, string name) : base(parentSchema, name) { }
    }
}