using System.Collections.Generic;
using DatabaseInteraction;
using SchemaSnapshot.DatabaseModel;

namespace SchemaSnapshot.Postgres
{
    internal class PostgresColumnLoader : IColumnLoader
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
                var column = new PostgresColumn(
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