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
namespace SqlShootEngine
{
    internal static class DatabaseEngineUtils
    {
        public const string SqlServer = "SQL Server";
        public const string SQLite = "SQLite";
        public const string PostgreSQL = "PostgreSQL";

        public static bool IsValidDatabaseEngine(string name)
        {
            return DoesEngineNameMatch(name, SqlServer) ||
                DoesEngineNameMatch(name, SQLite) ||
                DoesEngineNameMatch(name, PostgreSQL);
        }

        public static bool DoesEngineNameMatch(string test, string name)
        {
            return test.Equals(name, System.StringComparison.OrdinalIgnoreCase);
        }

        public static bool DoesDatabaseEngineHaveConceptOfSchemas(string name)
        {
            return DoesEngineNameMatch(name, SqlServer) || DoesEngineNameMatch(name, PostgreSQL);
        }
    }
}
