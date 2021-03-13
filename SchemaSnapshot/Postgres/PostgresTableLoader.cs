using System.Collections.Generic;
using DatabaseInteraction;
using SchemaSnapshot.DatabaseModel;

namespace SchemaSnapshot.Postgres
{
    internal class PostgresTableLoader : ITableLoader
    {
        public List<Table> LoadTablesInSchema(string schemaName, ISqlExecutor sqlExecutor)
        {
            var tables = new List<Table>();

            var sql = $@"
                SELECT
                    t.table_name FROM information_schema.tables t
                WHERE
                    table_schema = '{schemaName}'
                AND
                    table_type='BASE TABLE'";

            sqlExecutor.Execute(sql, reader =>
            {
                var name = reader.ReadString("TABLE_NAME");

                tables.Add(new PostgresTable(schemaName, name));
            });

            return tables;
        }
    }
}
