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

namespace DatabaseInteraction
{
    internal class SqlExecutor : ISqlExecutor
    {
        private readonly IDbConnection _dbConnection;

        public SqlExecutor(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public void Execute(string sql)
        {
            var command = _dbConnection.CreateCommand();
            command.CommandText = sql;
            command.ExecuteNonQuery();
        }

        public void Execute(string sql, Action<IQueryResultRowReader> onRowRead)
        {
            var command = _dbConnection.CreateCommand();
            command.CommandText = sql;
            var reader = command.ExecuteReader();

            while (reader.Read())
            {
                onRowRead(new QueryResultRowReader(reader));
            }

            reader.Close();
        }


        public object ExecuteWithResult(string sql)
        {
            var command = _dbConnection.CreateCommand();
            command.CommandText = sql;
            return command.ExecuteScalar();
        }

        public void SetDatabaseContext(string databaseName)
        {
            Execute("USE " + databaseName);
        }

        public bool ExecuteWithBooleanResult(string sql)
        {
            var result = ExecuteWithResult(sql);

            if (result is bool boolResult)
            {
                return boolResult;
            }

            return (long)result == 1;
        }

        public long ExecuteWithLongResult(string sql)
        {
            var command = _dbConnection.CreateCommand();
            command.CommandText = sql;
            var result = command.ExecuteScalar();

            if (result is long longResult)
            {
                return longResult;
            }

            return 0;
        }

        public List<string> ExecuteWithListResult(string sql)
        {
            var lst = new List<string>();

            Execute(sql, reader =>
            {
                lst.Add(reader.ReadString(0));
            });

            return lst;
        }

        public void ExecuteInTransaction(string sql)
        {
            var transaction = _dbConnection.BeginTransaction();
            var command = _dbConnection.CreateCommand();
            
            command.CommandText = sql;
            command.Transaction = transaction;
            try
            {
                command.ExecuteNonQuery();
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public string ExecuteWithStringResult(string sql)
        {
            var command = _dbConnection.CreateCommand();
            command.CommandText = sql;
            var result = command.ExecuteScalar();

            if (result is string stringResult)
            {
                return stringResult;
            }

            return string.Empty;
        }
    }
}
