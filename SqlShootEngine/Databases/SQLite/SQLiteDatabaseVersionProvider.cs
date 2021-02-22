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

namespace SqlShootEngine.Databases.SQLite
{
    internal class SQLiteDatabaseVersionProvider : IDatabaseVersionProvider
    {
        public DatabaseVersion QueryForDatabaseVersion(ISqlExecutor sqlExecutor)
        {
            var result = sqlExecutor.ExecuteWithResult("select sqlite_version();");

            if (result is string versionInfo)
            {
                var versionParts = versionInfo.Split(".");

                if (versionParts.Length >= 2)
                {
                    var major = int.Parse(versionParts[0]);
                    var minor = int.Parse(versionParts[1]);

                    if (major == 3 && minor > 9)
                    {
                        return new DatabaseVersion(versionInfo, major, minor, 0, true);
                    }
                }

                return new DatabaseVersion(versionInfo, 0, 0, 0, false);
            }

            return new DatabaseVersion(string.Empty, 0, 0, 0, false);
        }
    }
}
