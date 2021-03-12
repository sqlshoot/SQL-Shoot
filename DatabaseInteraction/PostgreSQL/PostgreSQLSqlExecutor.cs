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

namespace DatabaseInteraction.PostgreSQL
{
    internal class PostgreSQLSqlExecutor : ISqlExecutor
    {
        private SqlExecutor _sqlExecutor;

        public PostgreSQLSqlExecutor(SqlExecutor sqlExecutor)
        {
            _sqlExecutor = sqlExecutor;
        }

        public void Execute(string sql)
        {
            _sqlExecutor.Execute(sql);
        }

        public void Execute(string sql, Action<IQueryResultRowReader> onRowRead)
        {
            _sqlExecutor.Execute(sql, onRowRead);
        }

        public void ExecuteInTransaction(string sql)
        {
            _sqlExecutor.ExecuteInTransaction(sql);
        }

        public bool ExecuteWithBooleanResult(string sql)
        {
            return _sqlExecutor.ExecuteWithBooleanResult(sql);
        }

        public List<string> ExecuteWithListResult(string sql)
        {
            return _sqlExecutor.ExecuteWithListResult(sql);
        }

        public long ExecuteWithLongResult(string sql)
        {
            return _sqlExecutor.ExecuteWithLongResult(sql);
        }

        public object ExecuteWithResult(string sql)
        {
            return _sqlExecutor.ExecuteWithResult(sql);
        }

        public string ExecuteWithStringResult(string sql)
        {
            return _sqlExecutor.ExecuteWithStringResult(sql);
        }

        public void SetDatabaseContext(string databaseName)
        {
            // Postgres connections are always to a single DB - therefore no op
        }
    }
}
