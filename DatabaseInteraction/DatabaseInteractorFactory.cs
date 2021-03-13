#region original-work-license
/*
 * This file is part of SQL Shoot.
 *
 * SQL Shoot is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * SQL Shoot is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser General Public License
 * along with SQL Shoot. If not, see <https://www.gnu.org/licenses/>.
 */
#endregion
using DatabaseInteraction.PostgreSQL;
using DatabaseInteraction.SQLite;
using DatabaseInteraction.SqlServer;
using Microsoft.Data.SqlClient;
using Npgsql;
using SqlParser;
using System.Data.SQLite;

namespace DatabaseInteraction
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
