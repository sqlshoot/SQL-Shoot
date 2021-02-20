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
using System;
using System.IO;
using System.Linq;
using CommandLine;
using Engine;
using Engine.Exceptions;
using Spectre.Console;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace CLI
{
    public class Program
    {
        private static SqlShoot _sqlShoot;

        private static void Main(string[] args)
        {
            var debug = false;

            try
            {
                PrintProductInformation();
                var commandLineConfiguration = ParseCommandLineArguments(args);

                debug = commandLineConfiguration.Debug;
                var configFilepath = string.IsNullOrEmpty(commandLineConfiguration.ConfigFilePath) ? "config.yaml" : commandLineConfiguration.ConfigFilePath;

                var fileConfiguration = ReadConfigurationFiles(configFilepath);
                var clientConfiguration = ClientConfiguration.FromConfigurationSources(commandLineConfiguration, fileConfiguration);

                SqlShoot(clientConfiguration);
            } 
            catch (SqlShootInvalidConfigurationException e)
            {
                WriteLineError("Configuration options with invalid values detected:");
                WriteLineError(e.Message);
                WriteLineError("Check your configuration and ensure these are set to valid values");
                WriteErrorReportRequest();
            }
            catch (SqlShootInvalidProjectException e)
            {
                WriteLineError(e.Message);
                WriteErrorReportRequest();
            } 
            catch (SqlShootException e)
            {
                WriteLineError(e.Message);
                WriteErrorReportRequest();

                if (e.InnerException != null)
                {
                    WriteLineError(e.InnerException.Message);
                }
            }
            catch (Exception e)
            {
                WriteLineError($"Unexpected exception occurred: {e.GetType().Name}");
                WriteLineError(e.Message);
                WriteLineError($"Retry with the --debug flag to see the full stack trace.");
                WriteErrorReportRequest();

                if (debug)
                {
                    AnsiConsole.WriteException(e, ExceptionFormats.ShortenEverything);
                }
            }
        }

        private static void PrintProductInformation()
        {
            AnsiConsole.WriteLine($"SQL Shoot version: {GetProductVersion()}");
        }

        private static string GetProductVersion()
        {
            return "0.2.0";
        }

        private static CommandLineConfiguration ParseCommandLineArguments(string[] args)
        {
            if (args.Length == 0)
            {
                return new CommandLineConfiguration();
            }

            var argsToParse = args.Skip(1);
            var commandName = args.First();

            var commandLineConfiguration = CommandLine.Parser.Default.ParseArguments<CommandLineConfiguration>(argsToParse);
            CommandLineConfiguration c = null;
            
            // TODO find less stupid API
            commandLineConfiguration.WithParsed(options => c = options);

            c.CommandName = commandName;

            return c;
        }

        private static Configuration ReadConfigurationFiles(string configFilepath)
        {
            if (!File.Exists(configFilepath))
            {
                return new Configuration();
            }

            var deserializer = new DeserializerBuilder()
             .WithNamingConvention(new CamelCaseNamingConvention())
             .Build();

            try
            {
                return deserializer.Deserialize<Configuration>(File.OpenText(configFilepath));
            }
            catch (YamlException e)
            {
                if (e.InnerException != null)
                {
                    WriteLineError($"Failed to parse config file: {e.InnerException.Message}");
                }

                throw new Exception("Error running SQL Shoot", e);
            }
        }


        private static void SqlShoot(ClientConfiguration clientConfiguration)
        {
            _sqlShoot = new SqlShoot();
            _sqlShoot.SetLoggerWriteLine(text => AnsiConsole.WriteLine(text));

            var configuration = clientConfiguration.GetOverallConfiguration();

            _sqlShoot.SetConfiguration(configuration);

            ExecuteCommand(clientConfiguration.CommandLineConfiguration.CommandName, configuration);
        }

        private static void ExecuteCommand(string commandName, Configuration configuration)
        {
            switch (commandName)
            {
                case "run":
                    _sqlShoot.Run();
                    break;

                case "recover":
                    _sqlShoot.Recover();
                    break;

                case "revert":
                    _sqlShoot.Revert();
                    break;

                case "nuke":
                    _sqlShoot.Nuke();
                    break;

                case "overview":
                    ExecuteOverview();
                    break;

                default:
                    PrintHelp();
                    break;
            }
        }

        private static void PrintHelp()
        {
            AnsiConsole.WriteLine("SQL Shoot - change control for the database");
            AnsiConsole.WriteLine("Usage: sqlshoot [COMMAND NAME] [CONFIGURATION OPTIONS]");
            AnsiConsole.WriteLine($"See {Links.Website} for documentation, troubleshooting, error reporting, and latest news.");
        }

        private static void ExecuteOverview()
        {
            var changeOverview = _sqlShoot.GetChangeOverview();

            var table = new Table();
            table.AddColumn("Name");
            table.AddColumn("Type");
            table.AddColumn("State");
            table.AddColumn("Revertable");
            table.AddColumn("timestamp");

            foreach (var change in changeOverview.ChangeHistory.Changes)
            {
                var revertable = changeOverview.PendingChanges.Any(c => c.Reverts(change));

                if (change.Type.Equals(ResourceTypes.Script))
                {
                    table.AddRow(change.Name, change.Type, change.State, revertable ? "Yes" : "No", Convert.ToDateTime(change.Timestamp).ToString("dd-MM-yyyy HH:mm:ss"));
                }
                else
                {
                    table.AddRow(change.Name, change.Type, change.State, "n/a", change.Timestamp);
                }
            }

            foreach (var change in changeOverview.PendingChanges)
            {
                if (change.Type.Equals(ResourceTypes.Script))
                {
                    table.AddRow(change.Name, change.Type, change.State, "n/a");
                }

                if (change.Type.Equals(ResourceTypes.OnChangeScript))
                {
                    table.AddRow(change.Name, change.Type, change.State, "n/a");
                }

                if (change.Type.Equals(ResourceTypes.RevertScript))
                {
                    var hasCorrespondingRevert = changeOverview.ChangeHistory.HasCorrespondingScriptChangeForRevertChange(change);

                    if (hasCorrespondingRevert)
                    {
                        table.AddRow(change.Name, change.Type, change.State, "n/a");
                    }
                }
            }

            AnsiConsole.Render(table);

            PrintErrors(changeOverview.Errors);
        }

        private static void PrintErrors(Errors errors)
        {
            if (!errors.HasAnyErrors())
            {
                return;
            }

            WriteLineError("Problems detected!");
            WriteLineError(string.Empty);

            if (errors.DuplicateScriptPaths.Any())
            {
                WriteLineError("Found the same files when resolving different paths specified in the 'scriptPaths' configuration option:");

                foreach (var duplicateScriptPath in errors.DuplicateScriptPaths)
                {
                    WriteLineError($"\t{duplicateScriptPath}");
                }

                WriteLineError(string.Empty);
                WriteLineError("\tPossible fixes:");
                WriteLineError("\t - You may have specified the script path multiple times. Look for any duplicate entires and delete them");
                WriteLineError("\t - Adjust your 'scriptPaths' so they do not resolve the same files multiple times");
                WriteLineError(string.Empty);
            }

            if (errors.DuplicateResources.Any())
            {
                WriteLineError("Found multiple scripts with the same name:");

                foreach (var duplicateResource in errors.DuplicateResources)
                {
                    WriteLineError($"\t{duplicateResource.GetName()}");
                }

                WriteLineError(string.Empty);
                WriteLineError("\tPossible fixes:");
                WriteLineError("\t - Rename the duplicate scripts");
                WriteLineError("\t - Delete the duplicate scripts");
                WriteLineError(string.Empty);
            }

            if (errors.MissingChanges.Any())
            {
                WriteLineError("Found applied changes without corresponding scripts:");

                foreach (var missingChange in errors.MissingChanges)
                {
                    WriteLineError($"\t{missingChange.Name}");
                }

                WriteLineError(string.Empty);
                WriteLineError("\tPossible fixes:");
                WriteLineError("\t - Re-introduce the scripts that created these changes in your 'scriptPaths' configuration option");
                WriteLineError(string.Empty);
            }

            if (errors.ModifiedAppliedChanges.Any())
            {
                WriteLineError("Found applied changes with corresponding scripts that have been modified sicne they were applied:");

                foreach (var modifiedAppliedChange in errors.ModifiedAppliedChanges)
                {
                    WriteLineError($"\t{modifiedAppliedChange.Name}");
                }

                WriteLineError(string.Empty);
                WriteLineError("\tPossible fixes:");
                WriteLineError("\t - Revert the scripts back to their original contents");
                WriteLineError("\t - Run the 'Recover' command to adjust the checksums in the change history table");
                WriteLineError(string.Empty);
            }

            WriteErrorReportRequest();
        }

        private static void WriteLineError(string text)
        {
            AnsiConsole.MarkupLine($"[red]{text}[/]");
        }

        private static void WriteErrorReportRequest()
        {
            WriteLineError($"See {Links.Troubleshooting} for help with this problem, or if you suspect a bug, how to send an bug report.");
        }
    }
}
