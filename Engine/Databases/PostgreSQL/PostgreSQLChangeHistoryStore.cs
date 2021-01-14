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
using Engine.DatabaseInteraction;
using Engine.DatabaseInteraction.ChangeHistory;
using Engine.Databases.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Engine.Databases.PostgreSQL
{
    internal class PostgreSQLChangeHistoryStore : IChangeHistoryStore
    {
        private readonly ISqlExecutor _sqlExecutor;
        private readonly ITimestampProvider _timestampProvider;
        private readonly string _hostSchemaName;
        private readonly string _hostDatabaseName;
        private readonly ScriptTemplateProvider _scriptTemplateProvider;

        private const string TableName = "SqlShootChangeHistory";

        public PostgreSQLChangeHistoryStore(
            ISqlExecutor sqlExecutor,
            ITimestampProvider timestampProvider,
            string hostSchemaName,
            string hostDatabaseName)
        {
            _sqlExecutor = sqlExecutor;
            _timestampProvider = timestampProvider;
            _hostSchemaName = hostSchemaName;
            _hostDatabaseName = hostDatabaseName;

            var scriptDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Databases", "PostgreSQL");
            _scriptTemplateProvider = new ScriptTemplateProvider(scriptDirectory);
        }

        public bool Exists()
        {
            var script = _scriptTemplateProvider.Get(
                "Exists",
                new Dictionary<string, string>
                {
                    { "schemaName", _hostSchemaName },
                    { "tableName", TableName }
                });

            var result = false;
            
            _sqlExecutor.Execute(script, rowReader =>
            {
                result = rowReader.ReadBoolean(0);
            });

            return result;
        }

        public void Create()
        {
            var script = _scriptTemplateProvider.Get(
                "Create",
                new Dictionary<string, string>
                {
                    { "schemaName", _hostSchemaName },
                    { "tableName", TableName }
                });

            _sqlExecutor.Execute(script);
        }

        public ChangeHistory Read()
        {
            var list = new List<Change>();

            _sqlExecutor.Execute($"SELECT * FROM \"{_hostSchemaName}\".\"{TableName}\"", reader =>
            {
                var name = reader.ReadString("name");
                var checksum = reader.ReadString("checksum");
                var source = reader.ReadString("source");
                var type = reader.ReadString("type");
                var state = reader.ReadString("state");
                var timestamp = reader.ReadString("timestamp");

                list.Add(new Change(name, checksum, source, type, state, timestamp));
            });

            return new ChangeHistory(list);
        }

        public void Write(Change change)
        {
            var timestamp = _timestampProvider.GetTimestampForCurrentMoment();

            var script = _scriptTemplateProvider.Get(
                "Write",
                new Dictionary<string, string>
                {
                    { "schemaName", _hostSchemaName },
                    { "tableName", TableName },
                    { "name", change.Name },
                    { "checksum", change.Checksum },
                    { "source", change.Source },
                    { "type", change.Type },
                    { "state", change.State },
                    { "timestamp", timestamp },
                });

            _sqlExecutor.Execute(script);
        }

        public void Delete(Change change)
        {
            var script = _scriptTemplateProvider.Get(
                "Delete",
                new Dictionary<string, string>
                {
                    { "schemaName", _hostSchemaName },
                    { "tableName", TableName },
                    { "name", change.Name },
                    { "checksum", change.Checksum },
                    { "source", change.Source },
                    { "type", change.Type },
                    { "state", change.State },
                });

            _sqlExecutor.Execute(script);
        }

        public void UpdateChecksum(string changeName, string newChecksum)
        {
            var script = _scriptTemplateProvider.Get(
                "UpdateChecksum",
                new Dictionary<string, string>
                {
                    { "schemaName", _hostSchemaName },
                    { "tableName", TableName },
                    { "name", changeName },
                    { "newChecksum", newChecksum },
                });

            _sqlExecutor.Execute(script);
        }
    }
}
