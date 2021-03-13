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
    internal class SqlServerColumnLoader : IColumnLoader
    {
        public List<Column> LoadColumnsForTablesInSchema(string schemaName, ISqlExecutor sqlExecutor)
        {
            var columns = new List<Column>();

            var sql = $@"SELECT
                    TABLE_NAME,
                    COLUMN_NAME,
                    ORDINAL_POSITION,
                    IS_NULLABLE,
                    DATA_TYPE,
                    NUMERIC_PRECISION,
                    NUMERIC_SCALE
                FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_SCHEMA = '{schemaName}'";

            sqlExecutor.Execute(sql, reader =>
            {
                var tableName = reader.ReadString("TABLE_NAME");
                var columnName = reader.ReadString("COLUMN_NAME");
                var ordinalPosition = reader.ReadInt("ORDINAL_POSITION");
                var isNullable = reader.ReadString("IS_NULLABLE");
                var dataType = reader.ReadString("DATA_TYPE");
                var column = new SqlServerColumn(
                    tableName,
                    columnName,
                    ordinalPosition,
                    isNullable == "YES",
                    dataType);

                columns.Add(column);
            });

            return columns;
        }
    }
}