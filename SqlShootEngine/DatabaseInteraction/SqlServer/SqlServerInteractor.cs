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

namespace SqlShootEngine.DatabaseInteraction.SqlServer
{
    internal class SqlServerInteractor : IDatabaseInteractor
    {
        private readonly ISqlExecutor _sqlExecutor;
        private readonly ISchemaNuker _schemaNuker;
        private readonly IDatabaseVersionProvider _databaseVersionProvider;
        private readonly ScriptExecutor _scriptExecutor;

        public SqlServerInteractor(
            ISqlExecutor sqlExecutor,
            ISchemaNuker schemaNuker,
            IDatabaseVersionProvider databaseVersionProvider,
            ScriptExecutor scriptExecutor)
        {
            _sqlExecutor = sqlExecutor;
            _schemaNuker = schemaNuker;
            _databaseVersionProvider = databaseVersionProvider;
            _scriptExecutor = scriptExecutor;
        }

        public void CreateDatabase(string databaseName)
        {
            _sqlExecutor.Execute("CREATE DATABASE " + databaseName);
        }

        public void DeleteDatabase(string databaseName)
        {
            _sqlExecutor.Execute("DROP DATABASE IF EXISTS " + databaseName);
        }

        public void ExecuteScript(IResource resource)
        {
            _scriptExecutor.ExecuteScript(resource);
        }

        public void CreateSchema(string databaseName, string schemaName)
        {
            _sqlExecutor.SetDatabaseContext(databaseName);
            
            var result = _sqlExecutor.ExecuteWithResult($"IF EXISTS (SELECT name FROM sys.schemas WHERE name = N'mySchema')" +
                $"\nBEGIN" +
                $"\nSELECT 1" +
                $"\nEND" +
                $"\nELSE" +
                $"\nBEGIN" +
                $"\nSELECT 0" +
                $"\nEND");

            if ((int)result == 0)
            {
                _sqlExecutor.Execute($"CREATE SCHEMA [{schemaName}]");
            }
        }

        public void DeleteSchema(string databaseName, string schemaName)
        {
            _sqlExecutor.SetDatabaseContext(databaseName);
            _sqlExecutor.Execute("DROP SCHEMA " + schemaName);
        }

        public void NukeSchema(string databaseName, string schemaName)
        {
            _sqlExecutor.SetDatabaseContext(databaseName);
            _schemaNuker.Nuke(databaseName, schemaName, GetVersion());
        }

        public DatabaseVersion GetVersion()
        {
            return _databaseVersionProvider.QueryForDatabaseVersion(_sqlExecutor);
        }

        public void SetDatabaseContext(string databaseName)
        {
            _sqlExecutor.SetDatabaseContext(databaseName);
        }
    }
}
