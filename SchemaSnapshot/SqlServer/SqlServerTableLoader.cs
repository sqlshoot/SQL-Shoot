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
using SchemaSnapshot.DatabaseModel;

namespace SchemaSnapshot.SqlServer
{
    internal class SqlServerTableLoader : ITableLoader
    {
        public List<Table> LoadTablesInSchema(string schemaName, ISqlExecutor sqlExecutor)
        {
            var tables = new List<Table>();

            var sql = $@"
                SELECT name FROM sys.objects
                    WHERE schema_id = SCHEMA_ID('{schemaName}')
                    and type = 'U'";

            sqlExecutor.Execute(sql, reader =>
            {
                var name = reader.ReadString("name");

                tables.Add(new SqlServerTable(schemaName, name));
            });

            return tables;
        }
    }
}
