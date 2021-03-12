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

namespace SqlShootEngine.DatabaseInteraction.SqlServer
{
    internal class SqlServerDatabaseVersionProvider : IDatabaseVersionProvider
    {
        public DatabaseVersion QueryForDatabaseVersion(ISqlExecutor sqlExecutor)
        {
            var result = sqlExecutor.ExecuteWithResult("SELECT @@VERSION");

            if (result is string versionInfo)
            {
                if (versionInfo.Contains("Microsoft SQL Server 2019"))
                {
                    return new DatabaseVersion("2019", 2019, 0, 0, true);
                }

                if (versionInfo.Contains("Microsoft SQL Server 2017"))
                {
                    return new DatabaseVersion("2017", 2017, 0, 0, true);
                }

                if (versionInfo.Contains("Microsoft SQL Server 2016"))
                {
                    return new DatabaseVersion("2016", 2016, 0, 0, true);
                }

                return new DatabaseVersion(versionInfo, 0, 0, 0, false);
            }

            return new DatabaseVersion(string.Empty, 0, 0, 0, false);
        }
    }
}
