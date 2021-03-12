#region derivation-license
/* This file contains work derived from another open source tool (https://github.com/flyway/flyway).
 * Note this file has been modified after derivation by the author of SQL Shoot.
 * To comply with the original license, the notice as presented in the original work
 * at the time of derivation, is reproduced here:
 *
 * Copyright © Red Gate Software Ltd 2010-2020
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *         http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
#endregion
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

namespace DatabaseInteraction.SqlServer
{
    internal class SqlServerSchemaNuker : ISchemaNuker
    {
        private readonly ISqlExecutor _sqlExecutor;

        public SqlServerSchemaNuker(ISqlExecutor sqlExecutor)
        {
            _sqlExecutor = sqlExecutor;
        }

        public void Nuke(string databaseName, string schemaName, DatabaseVersion databaseVersion)
        {
            DropForeignKeys(schemaName);
            DropDefaultConstraints(schemaName);
            DropProcedures(schemaName);
            DropViews(schemaName);

            // This only works for 2016 and onwards
            // So if we aren't on a supported DB we must be old, so don't bother
            if (databaseVersion.Supported)
            {
                DropSystemVersioning(schemaName);
            }

            DropTables(schemaName);
            DropFunctions(schemaName);
            DropTypes(schemaName);
            DropSynonyms(schemaName);

            // This only works for 2012 and onwards
            // So if we aren't on a supported DB we must be old, so don't bother
            if (databaseVersion.Supported)
            {
                DropSequences(schemaName);
            }
        }

        protected void DropForeignKeys(string schemaName)
        {
            var sql = "SELECT TABLE_NAME, CONSTRAINT_NAME " +
                         "FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS " +
                         "WHERE CONSTRAINT_TYPE IN ('FOREIGN KEY','CHECK') " +
                        $"AND TABLE_SCHEMA = '{schemaName}'";
            var lst = new List<string>();

            _sqlExecutor.Execute(sql, rowReader =>
            {
                var routineName = rowReader.ReadString(0);
                var routineType = rowReader.ReadString(1);

                lst.Add($"ALTER TABLE [{schemaName}].[{routineName}] DROP CONSTRAINT [{routineType}]");
            });

            ExecuteSqlList(lst);
        }

        protected void DropDefaultConstraints(string schemaName)
        {
            string sql = "SELECT t.name as TABLE_NAME, d.name as CONSTRAINT_NAME " +
                         "FROM sys.tables t " +
                         "INNER JOIN sys.default_constraints d ON d.parent_object_id = t.object_id " +
                         "INNER JOIN sys.schemas s ON s.schema_id = t.schema_id " +
                        $"WHERE s.name = '{schemaName}'";
            var lst = new List<string>();

            _sqlExecutor.Execute(sql, rowReader =>
            {
                var routineName = rowReader.ReadString(0);
                var routineType = rowReader.ReadString(1);

                lst.Add($"ALTER TABLE [{schemaName}].[{routineName}] DROP CONSTRAINT [{routineType}]");
            });

            ExecuteSqlList(lst);
        }

        protected void DropProcedures(string schemaName)
        {
            string sql = $"SELECT routine_name FROM INFORMATION_SCHEMA.ROUTINES WHERE routine_schema = '{schemaName}' AND routine_type = 'PROCEDURE' ORDER BY created DESC";
            var lst = new List<string>();

            _sqlExecutor.Execute(sql, rowReader =>
            {
                var procedureName = rowReader.ReadString(0);

                lst.Add($"DROP PROCEDURE [{schemaName}].[{procedureName}]");
            });

            ExecuteSqlList(lst);
        }

        protected void DropViews(string schemaName)
        {
            var sql = $"SELECT table_name FROM INFORMATION_SCHEMA.VIEWS WHERE table_schema = '{schemaName}'";
            var lst = new List<string>();

            _sqlExecutor.Execute(sql, rowReader =>
            {
                var viewName = rowReader.ReadString(0);

                lst.Add($"DROP VIEW [{schemaName}].[{viewName}]");
            });

            ExecuteSqlList(lst);
        }

        private void DropSystemVersioning(string schemaName)
        {
            var sql = "SELECT t.name as TABLE_NAME " +
                         "FROM sys.tables t " +
                         "INNER JOIN sys.schemas s ON s.schema_id = t.schema_id " +
                        $"WHERE s.name = '{schemaName}' " +
                         "AND t.temporal_type = 2";
            var lst = new List<string>();

            _sqlExecutor.Execute(sql, rowReader =>
            {
                var tableName = rowReader.ReadString(0);

                lst.Add($"ALTER TABLE [{schemaName}].[{tableName}] SET (SYSTEM_VERSIONING = OFF)");
            });

            ExecuteSqlList(lst);
        }

        protected void DropTables(string schemaName)
        {
            var sql = $"SELECT table_name FROM INFORMATION_SCHEMA.TABLES WHERE table_type='BASE TABLE' AND table_schema = '{schemaName}'";
            var lst = new List<string>();

            _sqlExecutor.Execute(sql, rowReader =>
            {
                var tableName = rowReader.ReadString(0);

                lst.Add($"DROP TABLE [{schemaName}].[{tableName}]");
            });

            ExecuteSqlList(lst);
        }

        protected void DropFunctions(string schemaName)
        {
            var sql = $"SELECT routine_name FROM INFORMATION_SCHEMA.ROUTINES WHERE routine_schema = '{schemaName}' AND routine_type = 'FUNCTION' ORDER BY created DESC";
            var lst = new List<string>();

            _sqlExecutor.Execute(sql, rowReader =>
            {
                var functionName = rowReader.ReadString(0);

                lst.Add($"DROP FUNCTION [{schemaName}].[{functionName}]");
            });

            ExecuteSqlList(lst);
        }

        protected void DropTypes(string schemaName)
        {
            var sql = $"SELECT t.name FROM sys.types t INNER JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE t.is_user_defined = 1 AND s.name = '{schemaName}'";
            var lst = new List<string>();

            _sqlExecutor.Execute(sql, rowReader =>
            {
                var typeName = rowReader.ReadString(0);

                lst.Add($"DROP Type [{schemaName}].[{typeName}]");
            });

            ExecuteSqlList(lst);
        }

        protected void DropSynonyms(string schemaName)
        {
            var sql = $"SELECT sn.name FROM sys.synonyms sn INNER JOIN sys.schemas s ON sn.schema_id = s.schema_id WHERE s.name = '{schemaName}'";
            var lst = new List<string>();

            _sqlExecutor.Execute(sql, rowReader =>
            {
                var synonymName = rowReader.ReadString(0);

                lst.Add($"DROP SYNONYM [{schemaName}].[{synonymName}]");
            });

            ExecuteSqlList(lst);
        }

        protected void DropSequences(string schemaName)
        {
            var sql = $"SELECT sequence_name FROM INFORMATION_SCHEMA.SEQUENCES WHERE sequence_schema = '{schemaName}'";
            var lst = new List<string>();

            _sqlExecutor.Execute(sql, rowReader =>
            {
                var sequenceName = rowReader.ReadString(0);

                lst.Add($"DROP SEQUENCE [{schemaName}].[{sequenceName}]");
            });

            ExecuteSqlList(lst);
        }

        private void ExecuteSqlList(List<string> lst)
        {
            foreach (var item in lst)
            {
                _sqlExecutor.Execute(item);
            }
        }
    }
}
