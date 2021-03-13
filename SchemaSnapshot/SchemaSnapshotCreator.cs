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
