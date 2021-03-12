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
using Npgsql;
using System.Data;

namespace DatabaseInteraction.PostgreSQL
{
    internal class PostgreSQLInteractor : IDatabaseInteractor
    {
        private readonly ISqlExecutor _sqlExecutor;
        private readonly ISchemaNuker _schemaNuker;
        private readonly IDatabaseVersionProvider _databaseVersionProvider;
        private readonly ScriptExecutor _scriptExecutor;
        private readonly IDbConnection _dbConnection;

        public PostgreSQLInteractor(
            ISqlExecutor sqlExecutor,
            ISchemaNuker schemaNuker,
            IDatabaseVersionProvider databaseVersionProvider,
            ScriptExecutor scriptExecutor,
            IDbConnection dbConnection)
        {
            _sqlExecutor = sqlExecutor;
            _schemaNuker = schemaNuker;
            _databaseVersionProvider = databaseVersionProvider;
            _scriptExecutor = scriptExecutor;
            _dbConnection = dbConnection;
        }

        public void CreateDatabase(string databaseName)
        {
            _sqlExecutor.Execute($"CREATE DATABASE \"{databaseName}\"");
        }

        public IDbConnection GetDatabaseConnection()
        {
            return _dbConnection;
        }

        public void CreateSchema(string databaseName, string schemaName)
        {
            _sqlExecutor.SetDatabaseContext(databaseName);
            _sqlExecutor.Execute($"CREATE SCHEMA IF NOT EXISTS \"{schemaName}\"");
        }

        public void DeleteDatabase(string databaseName)
        {
            _sqlExecutor.Execute($"DROP DATABASE IF EXISTS \"{databaseName}\"");
        }

        public void DeleteSchema(string databaseName, string schemaName)
        {
            _sqlExecutor.SetDatabaseContext(databaseName);
            _sqlExecutor.Execute($"DROP SCHEMA IF EXISTS \"{schemaName}\"");
        }

        public void ExecuteScript(IResource resource)
        {
            _scriptExecutor.ExecuteScript(resource);
        }

        public ISqlExecutor GetSqlExecutor()
        {
            return _sqlExecutor;
        }

        public DatabaseVersion GetVersion()
        {
            return _databaseVersionProvider.QueryForDatabaseVersion(_sqlExecutor);
        }

        public void NukeSchema(string databaseName, string schemaName)
        {
            _schemaNuker.Nuke(databaseName, schemaName, GetVersion());
        }

        public void SetDatabaseContext(string databaseName)
        {
            _sqlExecutor.SetDatabaseContext(databaseName);
        }
    }
}
