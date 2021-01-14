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
using System.Collections.Generic;

namespace Engine
{
    /// <summary>
    /// Configuration options for SQL Shoot
    /// </summary>
    public class Configuration
    {
        /// <summary>
        /// The connection string to your database
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// The name of the database to connect to
        /// </summary>
        public string DatabaseName { get; set; }

        /// <summary>
        /// Schema under SQL Shoot's control. Stores the Change History table, and is cleared by the Nuke command
        /// Some databases don't have a concept of a 'schema' per se (such as SQLite). In such cases this option is ignored
        /// </summary>
        public string PrimarySchema { get; set; }

        /// <summary>
        /// Paths where SQL Shoot will look for scripts on your filesystem. Paths are searched in the order specified
        /// Script paths can be either a directory, or a direct filepath
        /// Directory script paths are suffixed with a *. Such paths will recursively search for scripts, ordering according to Ordering & Dependencies
        /// Direct script paths point directly to a.sql file
        /// Script paths can either be absolute, or relative. Relative script paths are resolved from the executing directory in the command line
        /// Script paths must resolve files with unique names
        /// Multiple script paths cannot resolve the same files
        /// For instance, C:/release-1/* and C:/release-1/createTable.sql is not allowed because they both resolve to createTable.sql
        /// </summary>
        public List<string> ScriptPaths { get; set; }

        /// <summary>
        /// The database engine
        /// Supported values:
        ///   - SQLite
        ///   - SQL Server
        ///   - PostgreSQL
        /// </summary>
        public string DatabaseEngine { get; set; }

        /// <summary>
        /// Whether to avoid executing scripts when running commands
        /// The change history table is updated without the script actually having been run
        /// </summary>
        public bool Fake { get; set; }

        /// <summary>
        /// Whether to execute scripts in transactions.
        /// By default, SQL Shoot wraps all scripts in Run and Revert in a transaction.
        /// This option allows you to manually override this behavior.It may be desirable to execute non-transactional statements.
        /// </summary>
        public bool RunScriptsInTransactions { get; set; }

        /// <summary>
        /// Username for authentication. If an equivalent is supplied in the connection string, this is ignored.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Password for authentication. If an equivalent is supplied in the connection string, this is ignored.
        /// </summary>
        public string Password { get; set; }
    }
}