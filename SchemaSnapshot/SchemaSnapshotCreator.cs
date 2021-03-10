using SchemaSnapshot.DatabaseModel;
using SchemaSnapshot.Postgres;
using SchemaSnapshot.SqlServer;
using System.Data;

namespace SchemaSnapshot
{
    public class SchemaSnapshotCreator
    {
        public Schema CreateSqlServerSchemaSnapshot(IDbConnection dbConnection, string schemaName)
        {
            var schemaLoader = new SchemaLoader(
                    new SqlServerTableLoader(),
                    new SqlServerColumnLoader(),
                    new SqlServerConstraintLoader(),
                    schemaName);

            return CreateSchemaSnapshot(schemaLoader, dbConnection);
        }

        public Schema CreatePostgreSQLSchemaSnapshot(IDbConnection dbConnection, string schemaName)
        {
            var schemaLoader = new SchemaLoader(
                    new PostgresTableLoader(),
                    new PostgresColumnLoader(),
                    new PostgresConstraintLoader(),
                    schemaName);

            return CreateSchemaSnapshot(schemaLoader, dbConnection);
        }

        private static Schema CreateSchemaSnapshot(SchemaLoader schemaLoader, IDbConnection connection)
        {
            Schema schema;

            using (var command = connection.CreateCommand())
            {
                schema = schemaLoader.Load(command);
            }

            return schema;
        }
    }
}
