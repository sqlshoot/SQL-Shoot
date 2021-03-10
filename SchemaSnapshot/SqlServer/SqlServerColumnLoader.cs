using System.Collections.Generic;
using System.Data;
using SchemaSnapshot.DatabaseModel;

namespace SchemaSnapshot.SqlServer
{
    internal class SqlServerColumnLoader : IColumnLoader
    {
        public List<Column> LoadColumnsForTablesInSchema(string schemaName, IDbCommand dbCommand)
        {
            var columns = new List<Column>();

            dbCommand.CommandText = $@"SELECT
                    TABLE_NAME,
                    COLUMN_NAME,
                    ORDINAL_POSITION,
                    IS_NULLABLE,
                    DATA_TYPE,
                    NUMERIC_PRECISION,
                    NUMERIC_SCALE
                FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_SCHEMA = '{schemaName}'";

            using (var dr = dbCommand.ExecuteReader())
            {
                while (dr.Read())
                {
                    var tableName = (string) dr["TABLE_NAME"];
                    var columnName = (string) dr["COLUMN_NAME"];
                    var ordinalPosition = (int) dr["ORDINAL_POSITION"];
                    var isNullable = (string) dr["IS_NULLABLE"];
                    var dataType = (string) dr["DATA_TYPE"];
                    var column = new SqlServerColumn(
                        tableName,
                        columnName,
                        ordinalPosition,
                        isNullable == "YES",
                        dataType);

                    columns.Add(column);
                }
            }

            return columns;
        }
    }
}