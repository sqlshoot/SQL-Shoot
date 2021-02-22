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
using SqlShootEngine.DatabaseInteraction;
using SqlParser;

namespace SqlShootEngine.Databases.Shared
{
    internal class ScriptExecutor
    {
        private ISqlExecutor _sqlExecutor;
        private Parser _parser;
        private INonTransactionalSqlDetector _nonTransactionalSqlDetector;
        private bool _useTransactions;

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
                if (_useTransactions)
                {
                    if (_nonTransactionalSqlDetector.IsSqlNonTransactional(sql))
                    {
                        Logger.WriteLine($"Non transactional script detected. Executing {resource.GetName()} in non transactional context");
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
            }
        }
    }
}
