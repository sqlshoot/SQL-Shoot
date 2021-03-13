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
using SqlParser;
using System;

namespace DatabaseInteraction
{
    internal class ScriptExecutor
    {
        private readonly ISqlExecutor _sqlExecutor;
        private readonly Parser _parser;
        private readonly INonTransactionalSqlDetector _nonTransactionalSqlDetector;
        private readonly bool _useTransactions;

        public ScriptExecutor(ISqlExecutor sqlExecutor, Parser parser, INonTransactionalSqlDetector nonTransactionalSqlDetector, bool useTransactions)
        {
            _sqlExecutor = sqlExecutor;
            _parser = parser;
            _nonTransactionalSqlDetector = nonTransactionalSqlDetector;
            _useTransactions = useTransactions;
        }

        public void ExecuteScript(IResource resource)
        {
            var sqlBatches = _parser.Parse(resource.Read());

            foreach (var sql in sqlBatches)
            {
                try
                {
                    if (_useTransactions)
                    {
                        if (_nonTransactionalSqlDetector.IsSqlNonTransactional(sql))
                        {
                            _sqlExecutor.Execute(sql);
                        }
                        else
                        {
                            _sqlExecutor.ExecuteInTransaction(sql);
                        }
                    }
                    else
                    {
                        _sqlExecutor.Execute(sql);
                    }
                } catch (Exception e)
                {
                    throw new ScriptExecutionException($"Failed to execute SQL batch\n\n{sql}\n\n{e.Message}", e);
                }
            }
        }
    }
}
