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
using DatabaseInteraction;
using SqlShootEngine.History;

namespace SqlShootEngine.Databases.SQLite
{
    internal class SQLiteChangeHistoryStore : IChangeHistoryStore
    {
        private readonly IDatabaseInteractor _databaseInteractor;
        private readonly ITimestampProvider _timestampProvider;

        private const string TableName = "SqlShootChangeHistory";

        public SQLiteChangeHistoryStore(
            IDatabaseInteractor databaseInteractor,
            ITimestampProvider timeStampProvider)
        {
            _databaseInteractor = databaseInteractor;
            _timestampProvider = timeStampProvider;
        }

        public bool Exists()
        {
            var script = $"SELECT name FROM sqlite_master WHERE type = 'table' AND name = '{TableName}'";

            var result = _databaseInteractor.GetSqlExecutor().ExecuteWithResult(script);

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

            _databaseInteractor.GetSqlExecutor().Execute(script);
        }

        public ChangeHistory Read()
        {
            var list = new List<Change>();

            _databaseInteractor.GetSqlExecutor().Execute($"SELECT * FROM {TableName}", reader =>
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

            _databaseInteractor.GetSqlExecutor().Execute(script);
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

            _databaseInteractor.GetSqlExecutor().Execute(script);
        }

        public void UpdateChecksum(string changeName, string newChecksum)
        {
            var script = $@"UPDATE {TableName}
                            SET
                                checksum = '{newChecksum}'
                            WHERE
                                name = '{changeName}";

            _databaseInteractor.GetSqlExecutor().Execute(script);
        }
    }
}