using DatabaseInteraction;
using SchemaSnapshot.DatabaseModel;
using SchemaSnapshot.Postgres;
using SchemaSnapshot.SqlServer;

namespace SchemaSnapshot
{
    public class SchemaSnapshotCreator
    {
        private readonly DatabaseInteractorFactory _databaseInteractorFactory = new DatabaseInteractorFactory();

        public Schema CreateSqlServerSchemaSnapshot(string connectionString, string schemaName, string databaseName)
        {
            var schemaLoader = new SchemaLoader(
                    new SqlServerTableLoader(),
                    new SqlServerColumnLoader(),
                    new SqlServerConstraintLoader(),
                    schemaName,
                    databaseName);

            var databaseInteractor = _databaseInteractorFactory.CreateSQLServerDatabaseInteractor(connectionString, true);

            return CreateSchemaSnapshot(schemaLoader, databaseInteractor);
        }

        public Schema CreatePostgreSQLSchemaSnapshot(string connectionString, string schemaName, string databaseName)
        {
            var schemaLoader = new SchemaLoader(
                    new PostgresTableLoader(),
                    new PostgresColumnLoader(),
                    new PostgresConstraintLoader(),
                    schemaName,
                    databaseName);

            var databaseInteractor = _databaseInteractorFactory.CreatePostgreSQLDatabaseInteractor(connectionString, true);

            return CreateSchemaSnapshot(schemaLoader, databaseInteractor);
        }

        private static Schema CreateSchemaSnapshot(SchemaLoader schemaLoader, IDatabaseInteractor databaseInteractor)
        {
            return schemaLoader.Load(databaseInteractor);
        }
    }
}
