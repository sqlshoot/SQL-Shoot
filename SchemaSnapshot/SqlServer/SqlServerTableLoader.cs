using System.Collections.Generic;
using System.Data;
using SchemaSnapshot.DatabaseModel;

namespace SchemaSnapshot.SqlServer
{
    internal class SqlServerTableLoader : ITableLoader
    {
        public List<Table> LoadTablesInSchema(string schemaName, IDbCommand dbCommand)
        {
            var tables = new List<Table>();

            dbCommand.CommandText = $@"
                SELECT name FROM sys.objects
                    WHERE schema_id = SCHEMA_ID('{schemaName}')
                    and type = 'U'";

            using var dr = dbCommand.ExecuteReader();

            while (dr.Read())
            {
                var name = (string)dr["name"];

                tables.Add(new SqlServerTable(schemaName, name));
            }

            return tables;
        }
    }
}
