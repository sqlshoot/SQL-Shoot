#region original-work-license
/*
 * This file is part of SQL Shoot.
 *
 * SQL Shoot is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * SQL Shoot is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with SQL Shoot. If not, see <https://www.gnu.org/licenses/>.
 */
#endregion
using System.Collections.Generic;
using CommandLine;

namespace CLI
{
    public class CommandLineConfiguration
    {
        [Option("commandName", Required = false, HelpText = "The command to execute")]
        public string CommandName { get; set; }

        [Option("connectionString", Required = false, HelpText = "Database connection string")]
        public string ConnectionString { get; set; }

        [Option("databaseName", Required = false, HelpText = "Database name")]
        public string DatabaseName { get; set; }

        [Option("primarySchema", Required = false, HelpText = "Schema to contain the change history table")]
        public string PrimarySchema { get; set; }

        [Option("scriptPaths", Required = false, HelpText = "Filepaths for scripts. Supports direct filepaths (C:\\scripts\\script.sql) and wildcards (C:\\scripts\\*)")]
        public IEnumerable<string> ScriptPaths { get; set; }

        [Option("databaseEngine", Required = false, HelpText = "Database engine")]
        public string DatabaseEngine { get; set; }

        [Option("fake", Required = false, HelpText = "Whether to fake script execution")]
        public bool Fake { get; set; }

        [Option("runScriptsInTransactions", Required = false, HelpText = "Whether to run scripts in transactions if supported by the database engine")]
        public bool RunScriptsInTransactions { get; set; }

        [Option("username", Required = false, HelpText = "Username for the connection")]
        public string Username { get; set; }

        [Option("password", Required = false, HelpText = "Password for the connection")]
        public string Password { get; set; }

        [Option("debug", Required = false, HelpText = "Whether to output debug information")]
        public bool Debug { get; set; }

        [Option("configFilepath", Required = false, HelpText = "Where to find the config file")]
        public string ConfigFilePath { get; set; }
    }
}
