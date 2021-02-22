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
using SqlShootEngine;
using System.Collections.Generic;
using System.Linq;

namespace CLI
{
    internal class ClientConfiguration
    {
        public CommandLineConfiguration CommandLineConfiguration { get; }

        public Configuration FileConfiguration { get; }

        private readonly Configuration _configuration;

        private ClientConfiguration(CommandLineConfiguration cliConfig, Configuration fileConfig)
        {
            CommandLineConfiguration = cliConfig;
            FileConfiguration = fileConfig;

            _configuration = new Configuration()
            {
                ConnectionString = Override(cliConfig.ConnectionString, fileConfig.ConnectionString),
                DatabaseName = Override(cliConfig.DatabaseName, fileConfig.DatabaseName),
                PrimarySchema = Override(cliConfig.PrimarySchema, fileConfig.PrimarySchema),
                ScriptPaths = Override(cliConfig.ScriptPaths, fileConfig.ScriptPaths),
                DatabaseEngine = Override(cliConfig.DatabaseEngine, fileConfig.DatabaseEngine),
                RunScriptsInTransactions = Override(cliConfig.RunScriptsInTransactions, fileConfig.RunScriptsInTransactions, true),
                Fake = Override(cliConfig.Fake, fileConfig.Fake, false),
                Username = Override(cliConfig.Username, fileConfig.Username),
                Password = Override(cliConfig.Password, fileConfig.Password)
            };
        }

        public static ClientConfiguration FromConfigurationSources(CommandLineConfiguration commandLineConfiguration, Configuration configurationFromFile)
        {
            return new ClientConfiguration(commandLineConfiguration, configurationFromFile);
        }

        private bool Override(bool primary, bool secondary, bool defaultValue)
        {
            return primary || secondary || defaultValue;
        }

        private List<string> Override(IEnumerable<string> primary, List<string> secondary)
        {
            if (primary == null && secondary == null)
            {
                return new List<string>();
            }

            return (primary != null && primary.Any()) ? primary.ToList() : secondary;
        }

        private string Override(string primary, string secondary)
        {
            if (string.IsNullOrEmpty(primary) && string.IsNullOrEmpty(secondary))
            {
                return string.Empty;
            }

            return string.IsNullOrEmpty(primary) ? secondary : primary;
        }

        public Configuration GetOverallConfiguration()
        {
            return _configuration;
        }
    }
}
