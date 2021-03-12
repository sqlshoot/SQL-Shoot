using Microsoft.Data.SqlClient;
using Npgsql;
using SqlShootEngine.DatabaseInteraction.PostgreSQL;
using SqlShootEngine.DatabaseInteraction.SQLite;
using SqlShootEngine.DatabaseInteraction.SqlServer;
using System.Data.SQLite;

namespace SqlShootEngine.DatabaseInteraction
{
    public class DatabaseInteractorFactory
    {
        public IDatabaseInteractor CreatePostgreSQLDatabaseInteractor(string connectionString, bool useTransactions)
        {
            var dbConnection = new NpgsqlConnection(connectionString);
            var sqlExecutor = new PostgreSQLSqlExecutor(new SqlExecutor(dbConnection));
            var databaseVersionProvider = new PostgreSQLDatabaseVersionProvider();

            dbConnection.Open();
            var databaseVersion = databaseVersionProvider.QueryForDatabaseVersion(sqlExecutor);
            dbConnection.Close();

            var scriptExecutor = new ScriptExecutor(
                sqlExecutor,
                new PostgreSQLParser(),
                new PostgreSQLNonTransactionalSqlDetector(databaseVersion),
                useTransactions);

            var databaseInteractor = new PostgreSQLInteractor(
                sqlExecutor,
                new PostgreSQLSchemaNuker(sqlExecutor),
                databaseVersionProvider,
                scriptExecutor,
                dbConnection);

            return databaseInteractor;
        }

        public IDatabaseInteractor CreateSQLiteDatabaseInteractor(string connectionString, bool useTransactions)
        {
            var dbConnection = new SQLiteConnection(connectionString);
            var sqlExecutor = new SQLiteSqlExecutor(new SqlExecutor(dbConnection));

            var scriptExecutor = new ScriptExecutor(
                sqlExecutor,
                new SQLiteParser(),
                new SQLiteNonTransactionalSqlDetector(),
                useTransactions);

            var databaseInteractor = new SQLiteInteractor(
                sqlExecutor,
                new SQLiteSchemaNuker(sqlExecutor),
                new SQLiteDatabaseVersionProvider(),
                scriptExecutor,
                dbConnection);

            return databaseInteractor;
        }

        public IDatabaseInteractor CreateSQLServerDatabaseInteractor(string connectionString, bool useTransactions)
        {
            var dbConnection = new SqlConnection(connectionString);
            var sqlExecutor = new SqlExecutor(dbConnection);

            var scriptExecutor = new ScriptExecutor(
                sqlExecutor,
                new SqlServerParser(),
                new SqlServerNonTransactionalSqlDetector(),
                useTransactions);

            var databaseInteractor = new SqlServerInteractor(
                sqlExecutor,
                new SqlServerSchemaNuker(sqlExecutor),
                new SqlServerDatabaseVersionProvider(),
                scriptExecutor,
                dbConnection);

            return databaseInteractor;
        }
    }
}
