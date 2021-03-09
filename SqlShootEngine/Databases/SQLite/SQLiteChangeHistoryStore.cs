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
using System.IO;
using System.Reflection;
using SqlShootEngine.DatabaseInteraction;
using SqlShootEngine.DatabaseInteraction.ChangeHistory;
using SqlShootEngine.Databases.Shared;

namespace SqlShootEngine.Databases.SQLite
{
    internal class SQLiteChangeHistoryStore : IChangeHistoryStore
    {
        private readonly ISqlExecutor _sqlExecutor;
        private readonly ITimestampProvider _timestampProvider;

        private const string TableName = "SqlShootChangeHistory";

        public SQLiteChangeHistoryStore(
            ISqlExecutor sqlExecutor,
            ITimestampProvider timeStampProvider)
        {
            _sqlExecutor = sqlExecutor;
            _timestampProvider = timeStampProvider;

            var scriptDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Databases", "SQLite");
        }

        public bool Exists()
        {
            var script = $"SELECT name FROM sqlite_master WHERE type = 'table' AND name = '{TableName}'";

            var result = _sqlExecutor.ExecuteWithResult(script);

            return result != null;
        }

        public void Create()
        {
            var script = $@"CREATE TABLE {TableName} (
                            [name][TEXT],
                            [checksum] [TEXT],
	                        [source] [TEXT],
	                        [type] [TEXT],
	                        [state] [TEXT],
	                        [timestamp] [TEXT]
                        )";

            _sqlExecutor.Execute(script);
        }

        public ChangeHistory Read()
        {
            var list = new List<Change>();

            _sqlExecutor.Execute($"SELECT * FROM {TableName}", reader =>
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

            var script = $@"INSERT INTO {TableName}
                            (name
                            , checksum
                            , source
                            , type
                            , state
                            , timestamp)
                        VALUES
                            ('{change.Name}'
                            , '{change.Checksum}'
                            , '{change.Source}'
                            , '{change.Type}'
                            , '{change.State}'
                            , '{timestamp}')";

            _sqlExecutor.Execute(script);
        }

        public void Delete(Change change)
        {
            var script = @$"DELETE FROM {TableName}
                            WHERE
                                name LIKE '%{change.Name}%' AND
                                checksum LIKE '%{change.Checksum}%' AND
                                source LIKE '%{change.Source}%' AND
                                type LIKE '%{change.Type}%' AND
                                state LIKE '%{change.State}%'";

            _sqlExecutor.Execute(script);
        }

        public void UpdateChecksum(string changeName, string newChecksum)
        {
            var script = $@"UPDATE {TableName}
                            SET
                                checksum = '{newChecksum}'
                            WHERE
                                name = '{changeName}";
            _sqlExecutor.Execute(script);
        }
    }
}