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
using System.Data;

namespace SqlShootEngine.DatabaseInteraction
{
    internal class QueryResultRowReader : IQueryResultRowReader
    {
        private readonly IDataReader _dataReader;

        public QueryResultRowReader(IDataReader dataReader)
        {
            _dataReader = dataReader;
        }

        public int GetIndexOfColumn(string columnName)
        {
            return _dataReader.GetOrdinal(columnName);
        }

        public string ReadString(string columnName)
        {
            return ReadString(GetIndexOfColumn(columnName));
        }

        public int ReadInt(int index)
        {
            return _dataReader.GetInt32(index);
        }

        public int ReadInt(string columnName)
        {
            return ReadInt(GetIndexOfColumn(columnName));
        }

        public string ReadString(int index)
        {
            return _dataReader.GetString(index);
        }

        public bool ReadBoolean(int index)
        {
            return _dataReader.GetBoolean(index);
        }

        public char ReadChar(int index)
        {
            return _dataReader.GetChar(index);
        }
    }
}
