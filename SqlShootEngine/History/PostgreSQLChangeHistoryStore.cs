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
using DatabaseInteraction;
using SqlShootEngine.History;
using System.Collections.Generic;

namespace SqlShootEngine.Databases.PostgreSQL
{
    internal class PostgreSQLChangeHistoryStore : IChangeHistoryStore
    {
        private readonly IDatabaseInteractor _databaseInteractor;
        private readonly ITimestampProvider _timestampProvider;
        private readonly string _hostSchemaName;
        private readonly string _hostDatabaseName;

        private const string TableName = "SqlShootChangeHistory";

        public PostgreSQLChangeHistoryStore(
            IDatabaseInteractor databaseInteractor,
            ITimestampProvider timestampProvider,
            string hostSchemaName,
            string hostDatabaseName)
        {
            _databaseInteractor = databaseInteractor;
            _timestampProvider = timestampProvider;
            _hostSchemaName = hostSchemaName;
            _hostDatabaseName = hostDatabaseName;
        }

        public bool Exists()
        {
            var script = @$"SELECT EXISTS (
                           SELECT FROM pg_catalog.pg_class c
                           JOIN   pg_catalog.pg_namespace n ON n.oid = c.relnamespace
                           WHERE  n.nspname = '{_hostSchemaName}'
                           AND    c.relname = '{TableName}'
                           );";

            var result = false;
            
            _databaseInteractor.GetSqlExecutor().Execute(script, rowReader =>
            {
                result = rowReader.ReadBoolean(0);
            });

            return result;
        }

        public void Create()
        {
            var script = $@"CREATE TABLE ""{_hostSchemaName}"".""{TableName}"" (
	                        name varchar NOT NULL,
                            checksum varchar NOT NULL,
	                        source varchar NOT NULL,
                            type varchar NOT NULL,
	                        state varchar NOT NULL,
                            timestamp varchar NOT NULL
                        )";

            _databaseInteractor.GetSqlExecutor().Execute(script);
        }

        public ChangeHistory Read()
        {
            var list = new List<Change>();

            _databaseInteractor.GetSqlExecutor().Execute($"SELECT * FROM \"{_hostSchemaName}\".\"{TableName}\"", reader =>
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

            var script = $@"INSERT INTO ""{_hostSchemaName}"".""{TableName}""
                            (name
                            ,checksum
                            ,source
                            ,type
                            ,state
                            ,timestamp)
                        VALUES
                            ('{change.Name}'
                            ,'{change.Checksum}'
                            ,'{change.Source}'
                            ,'{change.Type}'
                            ,'{change.State}'
                            ,'{timestamp}')";

            _databaseInteractor.GetSqlExecutor().Execute(script);
        }

        public void Delete(Change change)
        {
            var script = $@"DELETE FROM ""{_hostSchemaName}"".""{TableName}""
                            WHERE
                                name = '{change.Name}' AND
                                checksum = '{change.Checksum}' AND
                                source = '{change.Source}' AND
                                type = '{change.Type}' AND
                                state = '{change.State}';";

            _databaseInteractor.GetSqlExecutor().Execute(script);
        }

        public void UpdateChecksum(string changeName, string newChecksum)
        {
            var script = $@"UPDATE ""{_hostSchemaName}"".""{TableName}""
                            SET
                                checksum = '{newChecksum}'
                            WHERE
                                name = '{changeName}'";

            _databaseInteractor.GetSqlExecutor().Execute(script);
        }
    }
}
