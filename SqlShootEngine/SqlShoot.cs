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
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using SqlShootEngine.History;
using SqlShootEngine.Databases.PostgreSQL;
using SqlShootEngine.Databases.SQLite;
using SqlShootEngine.Databases.SqlServer;
using SqlShootEngine.Exceptions;
using SqlShootEngine.Resources;
using SchemaSnapshot;
using SchemaSnapshot.DatabaseModel;
using DatabaseInteraction;

namespace SqlShootEngine
{
    /// <summary>
    /// SQL Shoot - Change control for the database
    /// See https://sqlshoot.com/documentation/nuget for usage instructions
    /// </summary>
    public class SqlShoot : ISqlShoot
    {
        private readonly IResourceResolver _resourceResolver;

        private Configuration _configuration;
        
        private IDatabaseInteractor _databaseInteractor;
        private IChangeHistoryStore _changeHistoryStore;
        private SchemaSnapshotCreator _schemaSnapshotCreator = new SchemaSnapshotCreator();

        /// <summary>
        /// Construct SQL Shoot. Call SetConfiguration to configure before executing commands
        /// </summary>
        public SqlShoot()
        {
            _resourceResolver = new ResourceResolver();
        }

        /// <summary>
        /// Configure SQL Shoot before executing commands
        /// </summary>
        public void SetConfiguration(Configuration configuration)
        {
            ValidateConfiguration(configuration);

            _configuration = configuration;
            var connectionStringWithCredentials = GetConnectionStringWithCredentials(configuration);

            var timestampProvider = new TimestampProvider();

            var databaseInteractorFactory = new DatabaseInteractorFactory();

            if (DatabaseEngineUtils.DoesEngineNameMatch(configuration.DatabaseEngine, DatabaseEngineUtils.SqlServer))
            {
                _databaseInteractor = databaseInteractorFactory
                    .CreateSQLServerDatabaseInteractor(
                    connectionStringWithCredentials,
                    configuration.RunScriptsInTransactions);

                _changeHistoryStore = new SqlServerChangeHistoryStore(
                    _databaseInteractor,
                    timestampProvider,
                    configuration.PrimarySchema,
                    configuration.DatabaseName);
            }

            if (DatabaseEngineUtils.DoesEngineNameMatch(configuration.DatabaseEngine, DatabaseEngineUtils.SQLite))
            {
                _databaseInteractor = databaseInteractorFactory
                    .CreateSQLiteDatabaseInteractor(
                    connectionStringWithCredentials,
                    configuration.RunScriptsInTransactions);

                _changeHistoryStore = new SQLiteChangeHistoryStore(_databaseInteractor, timestampProvider);
            }

            if (DatabaseEngineUtils.DoesEngineNameMatch(configuration.DatabaseEngine, DatabaseEngineUtils.PostgreSQL))
            {
                _databaseInteractor = databaseInteractorFactory
                    .CreatePostgreSQLDatabaseInteractor(
                    connectionStringWithCredentials,
                    configuration.RunScriptsInTransactions);

                _changeHistoryStore = new PostgreSQLChangeHistoryStore(
                    _databaseInteractor,
                    timestampProvider,
                    configuration.PrimarySchema,
                    configuration.DatabaseName);
            }

            ValidateDatabaseVersion();
        }

        private static string GetConnectionStringWithCredentials(Configuration configuration)
        {
            var connectionStringWithCredentials =
                ConnectionStringUtils.AppendCredentialsToConnectionString(
                configuration.DatabaseEngine,
                configuration.ConnectionString,
                configuration.Username,
                configuration.Password);

            ConnectionStringUtils.ValidateConnectionString(configuration.DatabaseEngine, connectionStringWithCredentials);
            return connectionStringWithCredentials;
        }

        /// <summary>
        /// Set the logger action to use
        /// </summary>
        /// <param name="writeLineAction">An Action which defines where to log messages to</param>
        public void SetLoggerWriteLine(Action<string> writeLineAction)
        {
            Logger.SetWriteLineAction(writeLineAction);
        }

        private void ValidateConfiguration(Configuration configuration)
        {
            var sb = new StringBuilder();

            if (string.IsNullOrEmpty(configuration.ConnectionString))
            {
                sb.AppendLine("connectionString is null or empty");
            }

            if (string.IsNullOrEmpty(configuration.DatabaseEngine))
            {
                sb.AppendLine("databaseEngine is null or empty");
            }
            else
            {
                if (!DatabaseEngineUtils.IsValidDatabaseEngine(configuration.DatabaseEngine))
                {
                    sb.AppendLine($"databaseEngine {configuration.DatabaseEngine} is not valid");
                }
            }

            if (string.IsNullOrEmpty(configuration.DatabaseName) &&
                DatabaseEngineUtils.DoesEngineNameMatch(configuration.DatabaseEngine, DatabaseEngineUtils.SqlServer))
            {
                sb.AppendLine("databaseName is null or empty");
            }

            if (string.IsNullOrEmpty(configuration.PrimarySchema) && DatabaseEngineUtils.DoesDatabaseEngineHaveConceptOfSchemas(configuration.DatabaseEngine))
            {
                sb.AppendLine("primarySchema is null or empty");
            }

            if (configuration.ScriptPaths == null)
            {
                sb.AppendLine("scriptPaths is null");
            }

            if (sb.Length > 0)
            {
                throw new SqlShootInvalidConfigurationException(sb.ToString());
            }
        }

        /// <summary>
        /// The Run command executes scripts against your database
        /// </summary>
        public void Run()
        {
            ValidateProject();

            Run(ResolveResourcesForRun());
        }

        private bool ShouldRunChange(ChangeHistory changeHistory, Change change)
        {
            if (change.Type.Equals(ResourceTypes.OnChangeScript))
            {
                return !changeHistory.Changes.Any(c => c.State == ChangeStates.Applied && change.IsTheSameAs(c));
            }

            return !changeHistory.Changes.Any(c => c.State == ChangeStates.Applied && change.IsTheSameAs(c));
        }

        private void Run(List<IResource> resources)
        {
            try
            {
                InitializeChangeHistoryStore();

                var changeHistory = GetChangeHistory();

                foreach (var resource in resources)
                {
                    var type = resource.GetName().EndsWith(ResourceTypes.OnChangeScriptSuffix, StringComparison.OrdinalIgnoreCase) ? ResourceTypes.OnChangeScript : ResourceTypes.Script;
                    var inProgressChange = CreateChangeFromResource(resource, type, "In Progress");

                    if (ShouldRunChange(changeHistory, inProgressChange))
                    {
                        Logger.WriteLine($"Running script {resource.GetName()}");

                        _databaseInteractor.GetDatabaseConnection().Open();
                        _changeHistoryStore.Write(inProgressChange);
                        _databaseInteractor.GetDatabaseConnection().Close();

                        _databaseInteractor.GetDatabaseConnection().Open();

                        try
                        {
                            ExecuteScript(resource);
                        }
                        catch (ScriptExecutionException scriptExecutionException)
                        {
                            RecordFailedChangeInChangeHistoryStore(resource, inProgressChange);

                            throw new SqlShootException(
                                $"Failed to execute script '{resource.GetSource()}':\n{scriptExecutionException.Message}",
                                scriptExecutionException);
                        }

                        _databaseInteractor.GetDatabaseConnection().Close();

                        var appliedChange = CreateChangeFromResource(resource, type, ChangeStates.Applied);

                        _databaseInteractor.GetDatabaseConnection().Open();
                        _changeHistoryStore.Delete(inProgressChange);
                        _changeHistoryStore.Write(appliedChange);
                        _databaseInteractor.GetDatabaseConnection().Close();
                    }
                }
            }
            catch (Exception e)
            {
                if (_databaseInteractor.GetDatabaseConnection().State != ConnectionState.Closed)
                {
                    _databaseInteractor.GetDatabaseConnection().Close();
                }

                throw new SqlShootException("SQL Shoot failed!", e);
            }
        }

        private void ExecuteScript(IResource resource)
        {
            if (_configuration.Fake)
            {
                Logger.WriteLine("Skipping execution of " + resource.GetName());
            }
            else
            {
                _databaseInteractor.SetDatabaseContext(_configuration.DatabaseName);
                _databaseInteractor.ExecuteScript(resource);
            }
        }

        private void InitializeChangeHistoryStore()
        {
            CreatePrimarySchema();

            _databaseInteractor.GetDatabaseConnection().Open();

            if (_changeHistoryStore.Exists())
            {
                Logger.WriteLine("Change history already exists, skipping");
            }
            else
            {
                if (DatabaseEngineUtils.DoesDatabaseEngineHaveConceptOfSchemas(_configuration.DatabaseEngine))
                {
                    Logger.WriteLine($"Creating change history store in primary schema {_configuration.PrimarySchema}");
                }
                else
                {
                    Logger.WriteLine($"Creating change history store");
                }

                _changeHistoryStore.Create();
            }

            _databaseInteractor.GetDatabaseConnection().Close();
        }

        private List<IResource> ResolveResourcesForRun()
        {
            return _resourceResolver.ResolveForRun(_configuration.ScriptPaths).ResolvedResources;
        }

        /// <summary>
        /// Get the change history for your database
        /// </summary>
        public ChangeHistory GetChangeHistory()
        {
            var changeHistory = new ChangeHistory(new List<Change>());

            _databaseInteractor.GetDatabaseConnection().Open();

            if (_changeHistoryStore.Exists())
            {
                changeHistory = _changeHistoryStore.Read();
            }

            _databaseInteractor.GetDatabaseConnection().Close();

            return changeHistory;
        }

        private void ValidateDatabaseVersion()
        {
            _databaseInteractor.GetDatabaseConnection().Open();

            var databaseVersion = _databaseInteractor.GetVersion();
            Logger.WriteLine($"{_configuration.DatabaseEngine} database version {databaseVersion.VersionText}");

            if (!databaseVersion.Supported)
            {
                Logger.WriteLine($"This version of the database has not been tested, and may not work correctly with SQL Shoot.");
            }

            _databaseInteractor.GetDatabaseConnection().Close();

        }

        /// <summary>
        /// Get the change overview for your database
        /// </summary>
        public ChangeOverview GetChangeOverview()
        {
            var changeHistory = GetChangeHistory();
            var resourcesForRun = _resourceResolver.ResolveForRun(_configuration.ScriptPaths);
            var resourcesForRevert = _resourceResolver.ResolveForRevert(_configuration.ScriptPaths);

            var pendingChanges = new List<Change>();

            foreach (var resource in resourcesForRun.ResolvedResources)
            {
                if (resource.GetName().EndsWith(ResourceTypes.OnChangeScriptSuffix, StringComparison.OrdinalIgnoreCase))
                {
                    if (!changeHistory.HasAppliedOrReAppliedChangeWithThisName(resource.GetName()) || 
                        changeHistory.HasAppliedChangeWithThisNameButWithoutMatchingChecksum(resource.GetName(), resource.GetChecksum()))
                    {
                        pendingChanges.Add(CreateChangeFromResource(resource, ResourceTypes.OnChangeScript, "Pending"));
                    }
                }
                else
                {
                    if (!changeHistory.HasAppliedOrReAppliedChangeWithThisName(resource.GetName()))
                    {
                        pendingChanges.Add(CreateChangeFromResource(resource, ResourceTypes.Script, "Pending"));
                    }
                }
            }

            foreach (var resource in resourcesForRevert.ResolvedResources)
            {
                var revertChange = CreateChangeFromResource(resource, ResourceTypes.RevertScript, "Pending");
                var shouldApply = false;

                foreach (var change in changeHistory.Changes)
                {
                    if (revertChange.Reverts(change))
                    {
                        shouldApply = true;
                    }

                    if (shouldApply && change.Name.Equals(revertChange.Name))
                    {
                        shouldApply = false;
                    }
                }

                if (shouldApply)
                {
                    pendingChanges.Add(revertChange);
                }
            }

            var duplicateResources = resourcesForRevert.DuplicateResources.Concat(resourcesForRun.DuplicateResources).ToList();
            var duplicateScriptPaths = resourcesForRevert.DuplicateScriptPaths.Concat(resourcesForRun.DuplicateScriptPaths).ToList();

            var modifiedAppliedChanges = new List<Change>();
            var missingChanges = new List<Change>();
            var allResolvedResources = resourcesForRun.ResolvedResources.Concat(resourcesForRevert.ResolvedResources).ToList();

            foreach (var change in changeHistory.Changes)
            {
                var modifiedAppliedChange = allResolvedResources.FirstOrDefault(resource => resource.GetName().Equals(change.Name) && !resource.GetChecksum().Equals(change.Checksum));

                if (modifiedAppliedChange != null && !change.Type.Equals(ResourceTypes.OnChangeScript))
                {
                    modifiedAppliedChanges.Add(CreateChangeFromResource(modifiedAppliedChange, change.Type, "Modified"));
                }

                if (!allResolvedResources.Any(resource => resource.GetName().Equals(change.Name)))
                {
                    missingChanges.Add(change);
                }
            }

            if (!allResolvedResources.Any())
            {
                Logger.WriteLine("Did not resolve any resources. Are your scriptPaths configured correctly?");
            }

            var changeErrors = new Errors(duplicateResources, duplicateScriptPaths, modifiedAppliedChanges, missingChanges);

            return new ChangeOverview(changeHistory, pendingChanges, changeErrors);
        }

        /// <summary>
        /// The Nuke command deletes all objects under the schema specified in Primary Schema
        /// This is a destructive process and should only be used when you know it's safe to do so
        /// </summary>
        public void Nuke()
        {
            if (DatabaseEngineUtils.DoesDatabaseEngineHaveConceptOfSchemas(_configuration.DatabaseEngine))
            {
                Logger.WriteLine($"Nuking schema {_configuration.PrimarySchema}");
            }
            else
            {
                Logger.WriteLine($"Nuking database {_configuration.DatabaseName}");
            }

            _databaseInteractor.GetDatabaseConnection().Open();
            _databaseInteractor.NukeSchema(_configuration.DatabaseName, _configuration.PrimarySchema);
            _databaseInteractor.GetDatabaseConnection().Close();
        }

        /// <summary>
        /// Create the database specified in Database Name
        /// </summary>
        public void CreateDatabase()
        {
            _databaseInteractor.GetDatabaseConnection().Open();
            _databaseInteractor.CreateDatabase(_configuration.DatabaseName);
            _databaseInteractor.GetDatabaseConnection().Close();
        }

        internal void CreatePrimarySchema()
        {
            if (DatabaseEngineUtils.DoesDatabaseEngineHaveConceptOfSchemas(_configuration.DatabaseEngine))
            {
                Logger.WriteLine($"Attempting to create schema '{_configuration.PrimarySchema}', if it doesn't already exist.");
            }

            _databaseInteractor.GetDatabaseConnection().Open();
            _databaseInteractor.CreateSchema(_configuration.DatabaseName, _configuration.PrimarySchema);
            _databaseInteractor.GetDatabaseConnection().Close();
        }

        /// <summary>
        /// Delete the database specified in Database Name
        /// </summary>
        public void DeleteDatabase()
        {
            _databaseInteractor.GetDatabaseConnection().Open();
            _databaseInteractor.DeleteDatabase(_configuration.DatabaseName);
            _databaseInteractor.GetDatabaseConnection().Close();
        }

        /// <summary>
        /// Delete the Primary Schema
        /// </summary>
        public void DeleteSchema()
        {
            _databaseInteractor.GetDatabaseConnection().Open();
            _databaseInteractor.DeleteSchema(_configuration.DatabaseName, _configuration.PrimarySchema);
            _databaseInteractor.GetDatabaseConnection().Close();
        }

        /// <summary>
        /// The Recover command attempts to put your deployment into a valid state after a failed Run, or errors in your project
        /// Recover will:
        ///   Delete 'Failed' entries in the change history table
        ///   Update checksums in the change history table to match your scripts on disk
        /// </summary>
        public void Recover()
        {
            var changeOverview = GetChangeOverview();
            var changeHistory = changeOverview.ChangeHistory;

            _databaseInteractor.GetDatabaseConnection().Open();

            var deletedChangeNames = new List<string>();

            foreach (var change in changeHistory.Changes)
            {
                if (change.State.Equals("Failed"))
                {
                    Logger.WriteLine($"Deleting failed change from change history store {change.Name}");

                    _changeHistoryStore.Delete(change);

                    deletedChangeNames.Add(change.Name);
                }
            }

            foreach (var change in changeOverview.Errors.ModifiedAppliedChanges)
            {
                if (!deletedChangeNames.Contains(change.Name))
                {
                    Logger.WriteLine($"Amending checksum for change {change.Name}");

                    _changeHistoryStore.UpdateChecksum(change.Name, change.Checksum);
                }
            }

            _databaseInteractor.GetDatabaseConnection().Close();
        }

        /// <summary>
        /// The Revert command executes revert scripts
        /// Revert will execute the relevant revert script for the latest applied Script (not On Change Scripts or other Revert Scripts)
        /// It only reverts one script at a time
        /// </summary>
        public void Revert()
        {
            try
            {
                ValidateProject();

                var changeOverview = GetChangeOverview();
                var anyPendingReverts = changeOverview.PendingChanges.Any(change => change.Type.Equals(ResourceTypes.RevertScript));

                if (!anyPendingReverts)
                {
                    Logger.WriteLine("No pending revert scripts");
                    return;
                }

                var resources = _resourceResolver.ResolveForRevert(_configuration.ScriptPaths).ResolvedResources;
                var pendingRevert = changeOverview.PendingChanges.First(change => change.Type.Equals(ResourceTypes.RevertScript));
                var resource = resources.First(r => r.GetName().Equals(pendingRevert.Name));

                if (!changeOverview.ChangeHistory.HasCorrespondingScriptChangeForRevertChange(pendingRevert))
                {
                    throw new SqlShootException($"No corresponding script for {resource.GetName()}");
                }

                var inProgressChange = CreateChangeFromResource(resource, ResourceTypes.RevertScript, "In Progress");

                if (changeOverview.ChangeHistory.HasEquivalentChange(inProgressChange))
                {
                    throw new SqlShootException("Script already applied!");
                }

                WriteToChangeHistoryStore(inProgressChange);

                _databaseInteractor.GetDatabaseConnection().Open();

                try
                {
                    Logger.WriteLine($"Running script {resource.GetName()}");
                    ExecuteScript(resource);
                }
                catch
                {
                    RecordFailedChangeInChangeHistoryStore(resource, inProgressChange);
                    throw;
                }

                _databaseInteractor.GetDatabaseConnection().Close();

                var appliedChange = CreateChangeFromResource(resource, ResourceTypes.RevertScript, ChangeStates.Applied);

                UpdateChangeInChangeHistoryStore(inProgressChange, appliedChange);
            }
            catch (Exception e)
            {
                if (_databaseInteractor.GetDatabaseConnection().State != ConnectionState.Closed)
                {
                    _databaseInteractor.GetDatabaseConnection().Close();
                }

                throw new SqlShootException("SQL Shoot failed!", e);
            }
        }

        /// <summary>
        /// Create a lightweight snapshot of the configured Primary Schema. See documentation for which objects are captured by the snapshot.
        /// </summary>
        public Schema Snapshot()
        {
            var connectionStringWithCredentials = GetConnectionStringWithCredentials(_configuration);

            if (DatabaseEngineUtils.DoesEngineNameMatch(_configuration.DatabaseEngine, DatabaseEngineUtils.PostgreSQL))
            {
                return _schemaSnapshotCreator.CreatePostgreSQLSchemaSnapshot(
                    connectionStringWithCredentials,
                    _configuration.PrimarySchema,
                    _configuration.DatabaseName);
            }
            else if (DatabaseEngineUtils.DoesEngineNameMatch(_configuration.DatabaseEngine, DatabaseEngineUtils.SqlServer))
            {
                return _schemaSnapshotCreator.CreateSqlServerSchemaSnapshot(
                    connectionStringWithCredentials,
                    _configuration.PrimarySchema,
                    _configuration.DatabaseName);
            }
            else
            {
                throw new SqlShootException($"Schema snapshots for database engine '{_configuration.DatabaseEngine}' are not supported.");
            }

        }

        private void WriteToChangeHistoryStore(Change change)
        {
            _databaseInteractor.GetDatabaseConnection().Open();
            _changeHistoryStore.Write(change);
            _databaseInteractor.GetDatabaseConnection().Close();
        }

        private void UpdateChangeInChangeHistoryStore(Change oldChange, Change newChange)
        {
            _databaseInteractor.GetDatabaseConnection().Open();
            _changeHistoryStore.Delete(oldChange);
            _changeHistoryStore.Write(newChange);
            _databaseInteractor.GetDatabaseConnection().Close();
        }

        private void RecordFailedChangeInChangeHistoryStore(IResource resource, Change inProgressChange)
        {
            var failedChange = CreateChangeFromResource(resource, inProgressChange.Type, "Failed");
            _changeHistoryStore.Delete(inProgressChange);
            _changeHistoryStore.Write(failedChange);
        }

        private static Change CreateChangeFromResource(IResource resource, string type, string state)
        {
            return new Change(
                resource.GetName(),
                resource.GetChecksum(),
                resource.GetSource(),
                type,
                state,
                string.Empty);
        }

        private void ValidateProject()
        {
            var changeOverview = GetChangeOverview();

            if (changeOverview.Errors.HasAnyErrors())
            {
                throw new SqlShootInvalidProjectException("Project is in an invalid state. Run the 'overview' command to troubleshoot problems.");
            }

            if (changeOverview.ChangeHistory.Changes.Any(c => c.State.Equals("Failed")))
            {
                var failedChange = changeOverview.ChangeHistory.Changes.First(c => c.State.Equals("Failed"));

                throw new SqlShootException($"Change history store contains a failed change '{failedChange.Name}'.\n" +
                    "Use the Recover command to remove the failed entry from the change history store and try again.");
            }
        }
    }
}
