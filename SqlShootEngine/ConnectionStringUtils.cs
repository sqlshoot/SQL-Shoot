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
using SqlShootEngine.Exceptions;
using System;

namespace SqlShootEngine
{
    internal static class ConnectionStringUtils
    {
        public static void ValidateConnectionString(string databaseEngineName, string connectionString)
        {
            if (DatabaseEngineUtils.DoesEngineNameMatch(DatabaseEngineUtils.SQLite, databaseEngineName))
            {
                if (connectionString.Contains(":Memory:", StringComparison.OrdinalIgnoreCase))
                {
                    throw new SqlShootException("In memory SQLite databases are not supported");
                }
            }   
        }

        public static string AppendCredentialsToConnectionString(string databaseEngineName, string connectionString, string username, string password)
        {
            var newConnectionString = connectionString;

            if (DatabaseEngineUtils.DoesEngineNameMatch(databaseEngineName, DatabaseEngineUtils.SqlServer))
            {
                var containsUserId = connectionString.Contains("User Id=", StringComparison.OrdinalIgnoreCase);
                var containsUid = connectionString.Contains("UID=", StringComparison.OrdinalIgnoreCase);
                var containsUser = connectionString.Contains("User=", StringComparison.OrdinalIgnoreCase);
                var containsPassword = connectionString.Contains("Password=", StringComparison.OrdinalIgnoreCase);
                var containsPwd = connectionString.Contains("Pwd=", StringComparison.OrdinalIgnoreCase);

                if (!containsUserId && !containsUid && !containsUser)
                {
                    if (!string.IsNullOrEmpty(username))
                    {
                        newConnectionString += $"User={username};";
                    }
                }

                if (!containsPassword && !containsPwd)
                {
                    if (!string.IsNullOrEmpty(password))
                    {
                        newConnectionString += $"Password={password};";
                    }
                }
            }

            if (DatabaseEngineUtils.DoesEngineNameMatch(databaseEngineName, DatabaseEngineUtils.PostgreSQL))
            {
                var containsUsername = connectionString.Contains("Username=", StringComparison.OrdinalIgnoreCase);
                var containsPassword = connectionString.Contains("Password=", StringComparison.OrdinalIgnoreCase);

                if (containsUsername)
                {
                    if (!string.IsNullOrEmpty(username))
                    {
                        newConnectionString += $"Username={username};";
                    }
                }

                if (!containsPassword)
                {
                    if (!string.IsNullOrEmpty(password))
                    {
                        newConnectionString += $"Password={password};";
                    }
                }
            }

            return newConnectionString;
        }
    }
}
