using System.Collections.Generic;
using DatabaseInteraction;
using SchemaSnapshot.DatabaseModel;

namespace SchemaSnapshot.SqlServer
{
    internal class SqlServerTableLoader : ITableLoader
    {
        public List<Table> LoadTablesInSchema(string schemaName, ISqlExecutor sqlExecutor)
        {
            var tables = new List<Table>();

            var sql = $@"
                SELECT name FROM sys.objects
                    WHERE schema_id = SCHEMA_ID('{schemaName}')
                    and type = 'U'";

            sqlExecutor.Execute(sql, reader =>
            {
                var name = reader.ReadString("name");

                tables.Add(new SqlServerTable(schemaName, name));
            });

            return tables;
        }
    }
}
