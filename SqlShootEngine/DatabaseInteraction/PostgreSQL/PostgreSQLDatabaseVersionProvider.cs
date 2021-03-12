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

namespace SqlShootEngine.DatabaseInteraction.PostgreSQL
{
    internal class PostgreSQLDatabaseVersionProvider : IDatabaseVersionProvider
    {
        public DatabaseVersion QueryForDatabaseVersion(ISqlExecutor sqlExecutor)
        {

            var result = sqlExecutor.ExecuteWithResult("SELECT version();");

            if (result is string versionInfo)
            {
                if (versionInfo.Contains("PostgreSQL 13", System.StringComparison.OrdinalIgnoreCase))
                {
                    return new DatabaseVersion(versionInfo, 13, 0, 0, true);
                }

                if (versionInfo.Contains("PostgreSQL 12", System.StringComparison.OrdinalIgnoreCase))
                {
                    return new DatabaseVersion(versionInfo, 12, 0, 0, true);
                }

                if (versionInfo.Contains("PostgreSQL 11", System.StringComparison.OrdinalIgnoreCase))
                {
                    return new DatabaseVersion(versionInfo, 11, 0, 0, true);
                }

                if (versionInfo.Contains("PostgreSQL 10", System.StringComparison.OrdinalIgnoreCase))
                {
                    return new DatabaseVersion(versionInfo, 10, 0, 0, true);
                }

                if (versionInfo.Contains("PostgreSQL 9.6", System.StringComparison.OrdinalIgnoreCase))
                {
                    return new DatabaseVersion(versionInfo, 9, 6, 0, true);
                }

                if (versionInfo.Contains("PostgreSQL 9.5", System.StringComparison.OrdinalIgnoreCase))
                {
                    return new DatabaseVersion(versionInfo, 9, 5, 0, true);
                }

                return new DatabaseVersion(versionInfo, 0, 0, 0, false);
            }

            return new DatabaseVersion(string.Empty, 0, 0, 0, false);
        }
    }
}
