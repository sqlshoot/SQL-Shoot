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
using Engine.DatabaseInteraction;
using System.Collections.Generic;

namespace Engine.Databases.PostgreSQL
{
    internal class PostgreSQLSchemaNuker : ISchemaNuker
    {
        private ISqlExecutor _sqlExecutor;

        public PostgreSQLSchemaNuker(ISqlExecutor sqlExecutor)
        {
            _sqlExecutor = sqlExecutor;
        }

        public void Nuke(string databaseName, string schemaName, DatabaseVersion databaseVersion)
        {
            if (databaseVersion.Major > 9 || (databaseVersion.Major == 9 && databaseVersion.Minor >= 3))
            {
                DropMaterializedViews(schemaName); // PostgreSQL >= 9.3
            }

            DropViews(schemaName);
            DropTables(schemaName);
            DropBaseTypes(schemaName, true);
            DropRoutines(schemaName, databaseVersion);
            DropEnums(schemaName);
            DropDomains(schemaName);
            DropSequences(schemaName);

            if (databaseVersion.Major < 11)
            {
                DropBaseAggregates(schemaName);
            }

            DropBaseTypes(schemaName, false);
            DropExtensions(schemaName);
        }

        protected void DropExtensions(string schemaName)
        {
            if (!DoesExtensionsTableExist())
            {
                return;
            }

            var currentUser = _sqlExecutor.ExecuteWithStringResult("SELECT current_user");

            var sql = $@"SELECT e.extname, n.nspname
                        FROM pg_extension e
                        LEFT JOIN pg_namespace n ON n.oid = e.extnamespace
                        LEFT JOIN pg_roles r ON r.oid = e.extowner
                        WHERE n.nspname ILIKE '{schemaName}' AND r.rolname ILIKE '{currentUser}'";

            var extensionNames = _sqlExecutor.ExecuteWithListResult(sql);

            foreach (var extensionName in extensionNames)
            {
                _sqlExecutor.Execute($"DROP EXTENSION IF EXISTS \"{extensionName}\" CASCADE");
            }
        }

        private bool DoesExtensionsTableExist()
        {
            return _sqlExecutor.ExecuteWithBooleanResult(
                        "SELECT EXISTS ( \n" +
                        "SELECT 1 \n" +
                        "FROM pg_tables \n" +
                        "WHERE tablename = 'pg_extension');");
        }


    protected void DropMaterializedViews(string schemaName)
        {
            var sql = $"SELECT relname FROM pg_catalog.pg_class c JOIN pg_namespace n ON n.oid = c.relnamespace WHERE c.relkind = 'm' AND n.nspname = '{schemaName}'";
            var lst = new List<string>();

            _sqlExecutor.Execute(sql, rowReader =>
            {
                lst.Add($"DROP MATERIALIZED VIEW IF EXISTS \"{schemaName}\".\"{Quote(rowReader.ReadString(0))}\" CASCADE");
            });

            ExecuteSqlList(lst);
        }

        protected void DropSequences(string schemaName)
        {
            var sql = $"SELECT sequence_name FROM information_schema.sequences WHERE sequence_schema = '{schemaName}'";
            var lst = new List<string>();

            _sqlExecutor.Execute(sql, rowReader =>
            {
                lst.Add($"DROP SEQUENCE IF EXISTS \"{schemaName}\".\"{Quote(rowReader.ReadString(0))}\"");
            });

            ExecuteSqlList(lst);
        }

        protected void DropBaseTypes(string schemaName, bool recreate)
        {
            var sql = "SELECT typname, typcategory " +
                         "FROM pg_catalog.pg_type t " +
                         "WHERE (t.typrelid = 0 OR (SELECT c.relkind = 'c' FROM pg_catalog.pg_class c WHERE c.oid = t.typrelid)) " +
                         "AND NOT EXISTS(SELECT 1 FROM pg_catalog.pg_type el WHERE el.oid = t.typelem AND el.typarray = t.oid) " +
                        $"AND t.typnamespace in (SELECT oid FROM pg_catalog.pg_namespace WHERE nspname = '{schemaName}')";
            var lst = new List<string>();

            _sqlExecutor.Execute(sql, rowReader =>
            {
               lst.Add($"DROP TYPE IF EXISTS \"{schemaName}\".\"{Quote(rowReader.ReadString(0))}\" CASCADE");
            });

            ExecuteSqlList(lst);

            if (recreate)
            {
                lst.Clear();

                _sqlExecutor.Execute(sql, rowReader =>
                {
                    var typeName = rowReader.ReadString(0);
                    var typeCategory = rowReader.ReadString(1);

                    // Only recreate Pseudo-types (P) and User-defined types (U)
                    if (typeCategory == "P" || typeCategory == "U")
                    {
                        lst.Add($"CREATE TYPE \"{schemaName}\".\"{Quote(typeName)}\"");
                    }
                });

                ExecuteSqlList(lst);
            }
        }

        protected void DropBaseAggregates(string schemaName)
        {
            var sql = "SELECT proname, oidvectortypes(proargtypes) AS args " +
                         "FROM pg_proc INNER JOIN pg_namespace ns ON (pg_proc.pronamespace = ns.oid) " +
                        $"WHERE pg_proc.proisagg = true AND ns.nspname = '{schemaName}'";

            var lst = new List<string>();

            _sqlExecutor.Execute(sql, rowReader =>
            {
                var proName = rowReader.ReadString(0);
                var args = rowReader.ReadString(1);

                lst.Add($"DROP AGGREGATE IF EXISTS \"{schemaName}\".\"{Quote(proName)}\" ({args}) CASCADE");
            });

            ExecuteSqlList(lst);
        }


        protected void DropRoutines(string schemaName, DatabaseVersion databaseVersion)
        {
            if (databaseVersion.Major < 11)
            {
                var sql = "SELECT proname, oidvectortypes(proargtypes) AS args " +
                             "FROM pg_proc INNER JOIN pg_namespace ns ON (pg_proc.pronamespace = ns.oid) " +
                             "LEFT JOIN pg_depend dep ON dep.objid = pg_proc.oid AND dep.deptype = 'e' " +
                            $"WHERE pg_proc.proisagg = false AND ns.nspname = '{schemaName}' AND dep.objid IS NULL";

                var lst = new List<string>();


                _sqlExecutor.Execute(sql, rowReader =>
                {
                    var proName = rowReader.ReadString(0);
                    var args = rowReader.ReadString(1);

                    lst.Add($"DROP FUNCTION IF EXISTS \"{schemaName}\".\"{Quote(proName)}\" ({args}) CASCADE");
                });

                ExecuteSqlList(lst);
            }
            else
            {
                var sql = "SELECT proname, oidvectortypes(proargtypes) AS args, pg_proc.prokind AS kind " +
                             "FROM pg_proc INNER JOIN pg_namespace ns ON (pg_proc.pronamespace = ns.oid) " +
                             "LEFT JOIN pg_depend dep ON dep.objid = pg_proc.oid AND dep.deptype = 'e' " +
                            $"WHERE ns.nspname = '{schemaName}' AND dep.objid IS NULL";

                var lst = new List<string>();


                _sqlExecutor.Execute(sql, rowReader =>
                {
                    var proName = rowReader.ReadString(0);
                    var args = rowReader.ReadString(1);
                    var kind = rowReader.ReadChar(2);

                    string objectType = null;
                    switch (kind)
                    {
                        case 'a': objectType = "AGGREGATE"; break;
                        case 'f': objectType = "FUNCTION"; break;
                        case 'p': objectType = "PROCEDURE"; break;
                        default: break;
                    }

                    if (objectType is null)
                    {
                        return;
                    }

                    lst.Add($"DROP {objectType} IF EXISTS \"{schemaName}\".\"{Quote(proName)}\" ({args}) CASCADE");
                });

                ExecuteSqlList(lst);
            }
        }

        protected void DropEnums(string schemaName)
        {
            var sql = $"SELECT t.typname FROM pg_catalog.pg_type t INNER JOIN pg_catalog.pg_namespace n ON n.oid = t.typnamespace WHERE n.nspname = '{schemaName}' and t.typtype = 'e'";
            var lst = new List<string>();

            _sqlExecutor.Execute(sql, rowReader =>
            {
                lst.Add($"DROP TYPE \"{schemaName}\".\"{Quote(rowReader.ReadString(0))}\"");
            });

            ExecuteSqlList(lst);
        }

        protected void DropDomains(string schemaName)
        {
            var sql = "SELECT t.typname " +
                         "FROM pg_catalog.pg_type t " +
                         "LEFT JOIN pg_catalog.pg_namespace n ON n.oid = t.typnamespace " +
                         "LEFT JOIN pg_depend dep ON dep.objid = t.oid AND dep.deptype = 'e' " +
                         "WHERE t.typtype = 'd' " +
                        $"AND n.nspname = '{schemaName}' " +
                         "AND dep.objid IS NULL";
            var lst = new List<string>();

            _sqlExecutor.Execute(sql, rowReader =>
            {
                lst.Add($"DROP DOMAIN \"{schemaName}\".\"{Quote(rowReader.ReadString(0))}\"");
            });

            ExecuteSqlList(lst);
        }

        protected void DropViews(string schemaName)
        {
            var sql = "SELECT relname " +
                         "FROM pg_catalog.pg_class c " +
                         "JOIN pg_namespace n ON n.oid = c.relnamespace " +
                         "LEFT JOIN pg_depend dep ON dep.objid = c.oid AND dep.deptype = 'e' " +
                        $"WHERE c.relkind = 'v' AND  n.nspname = '{schemaName}' AND dep.objid IS NULL";
            var lst = new List<string>();

            _sqlExecutor.Execute(sql, rowReader =>
            {
                lst.Add($"DROP VIEW IF EXISTS \"{schemaName}\".\"{Quote(rowReader.ReadString(0))}\" CASCADE");
            });

            ExecuteSqlList(lst);
        }

        protected void DropTables(string schemaName)
        {
            var sql = "SELECT t.table_name " +
                         "FROM information_schema.tables t " +
                         "LEFT JOIN pg_depend dep ON dep.objid = (quote_ident(t.table_schema)||'.'||quote_ident(t.table_name))::regclass::oid AND dep.deptype = 'e' " +
                        $"WHERE table_schema = '{schemaName}' " +
                         "AND table_type='BASE TABLE' " +
                         "AND dep.objid IS NULL " +
                         "AND NOT (SELECT EXISTS (SELECT inhrelid FROM pg_catalog.pg_inherits WHERE inhrelid = (quote_ident(t.table_schema)||'.'||quote_ident(t.table_name))::regclass::oid))";
            var lst = new List<string>();

            _sqlExecutor.Execute(sql, rowReader =>
            {
                lst.Add($"DROP TABLE IF EXISTS \"{schemaName}\".\"{Quote(rowReader.ReadString(0))}\" CASCADE");
            });

            ExecuteSqlList(lst);
        }

        private static string Quote(string dbObject) => dbObject.Replace("\"", "\"\"");

        private void ExecuteSqlList(List<string> lst)
        {
            foreach (var item in lst)
            {
                _sqlExecutor.Execute(item);
            }
        }
    }
}
