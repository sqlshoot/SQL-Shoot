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
using SqlShootEngine.DatabaseInteraction;
using SqlShootEngine.Databases.Shared;

namespace SqlShootEngine.Databases.SQLite
{
    internal class SQLiteInteractor : IDatabaseInteractor
    {
        private readonly ISqlExecutor _sqlExecutor;
        private readonly ISchemaNuker _schemaNuker;
        private readonly IDatabaseVersionProvider _databaseVersionProvider;
        private readonly ScriptExecutor _scriptExecutor;

        public SQLiteInteractor(
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
            // No op
        }

        public void CreateSchema(string databaseName, string schemaName)
        {
            // No op
        }

        public void DeleteDatabase(string databaseName)
        {
            // No op
        }

        public void DeleteSchema(string databaseName, string schemaName)
        {
            // No op
        }

        public void ExecuteScript(IResource resource)
        {
            _scriptExecutor.ExecuteScript(resource);
        }

        public DatabaseVersion GetVersion()
        {
            return _databaseVersionProvider.QueryForDatabaseVersion(_sqlExecutor);
        }

        public void NukeSchema(string databaseName, string schemaName)
        {
            _schemaNuker.Nuke(databaseName, schemaName, GetVersion());
        }
    }
}
