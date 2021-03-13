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
namespace DatabaseInteraction.SQLite
{
    internal class SQLiteTable
    {
        private readonly bool _undroppable;
        private readonly bool _foreignKeysEnabled;
        public string FullyQualifiedName { get; set; }

        public SQLiteTable(string fullyQualifiedName, bool foreignKeysEnabled)
        {
            FullyQualifiedName = fullyQualifiedName;
            _foreignKeysEnabled = foreignKeysEnabled;
            _undroppable = fullyQualifiedName.Contains("sqlite_sequence");
        }

        public void Drop(ISqlExecutor sqlExecutor)
        {
            if (_undroppable)
            {
                // Cannot drop this table
                return;
            }

            var query = $"DROP TABLE {FullyQualifiedName}";
            if (_foreignKeysEnabled)
            {
                query = $"PRAGMA foreign_keys = OFF; {query}; PRAGMA foreign_keys = ON";
            }

            sqlExecutor.Execute(query);
        }
    }
}