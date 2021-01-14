#region derivation-license
/* This file contains work derived from another open source tool (https://github.com/flyway/flyway).
 * Note this file has been modified after derivation by the author of SQL Shoot.
 * To comply with the original license, the notice as presented in the original work
 * at the time of derivation, is reproduced here:
 *
 * Copyright © Red Gate Software Ltd 2010-2020
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *         http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
#endregion
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
using System.Linq;
using Engine.DatabaseInteraction;

namespace Engine.Databases.SQLite
{
    internal class SQLiteSchemaNuker : ISchemaNuker
    {
        private readonly ISqlExecutor _sqlExecutor;

        public SQLiteSchemaNuker(ISqlExecutor sqlExecutor)
        {
            _sqlExecutor = sqlExecutor;
        }

        public void Nuke(string databaseName, string schemaName, DatabaseVersion databaseVersion)
        {
            var viewNames = _sqlExecutor.ExecuteWithListResult($"SELECT tbl_name FROM sqlite_master WHERE type='view'");

            foreach (var viewName in viewNames)
            {
                _sqlExecutor.Execute($"DROP VIEW {Quote(viewName)}");
            }

            var tables = GetAllTables();

            foreach (var table in tables)
            {
                table.Drop(_sqlExecutor);
            }

            if (tables.Any(t => t.FullyQualifiedName.Contains("sqlite_sequence")))
            {
                _sqlExecutor.Execute($"DELETE FROM sqlite_sequence");
            }
        }

        protected SQLiteTable[] GetAllTables()
        {
            var foreignKeysEnabled = _sqlExecutor.ExecuteWithBooleanResult("PRAGMA foreign_keys");

            var tableNames = _sqlExecutor.ExecuteWithListResult(
                $"SELECT tbl_name FROM sqlite_master WHERE type='table'");

            var tables = new SQLiteTable[tableNames.Count];

            for (var i = 0; i < tableNames.Count; i++)
            {
                tables[i] = new SQLiteTable(tableNames[i], foreignKeysEnabled);
            }

            return tables;
        }

        public string Quote(string identifier)
        {
            return $"\"{identifier}\"";
        }
    }
}
