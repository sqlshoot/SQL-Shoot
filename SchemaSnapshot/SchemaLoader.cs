using System.Collections.Generic;
using System.Linq;
using DatabaseInteraction;
using SchemaSnapshot.DatabaseModel;

namespace SchemaSnapshot
{
    internal class SchemaLoader
    {
        private readonly ITableLoader _tableLoader;
        private readonly IColumnLoader _columnLoader;
        private readonly IConstraintLoader _constraintLoader;
        private readonly string _schemaName;
        private readonly string _databaseName;

        public SchemaLoader(
            ITableLoader tableLoader,
            IColumnLoader columnLoader,
            IConstraintLoader constraintLoader,
            string schemaName,
            string databaseName)
        {
            _tableLoader = tableLoader;
            _columnLoader = columnLoader;
            _schemaName = schemaName;
            _constraintLoader = constraintLoader;
            _databaseName = databaseName;
        }

        public Schema Load(IDatabaseInteractor databaseInteractor)
        {
            databaseInteractor.GetDatabaseConnection().Open();
            databaseInteractor.SetDatabaseContext(_databaseName);

            var sqlExecutor = databaseInteractor.GetSqlExecutor();
            var tables = _tableLoader.LoadTablesInSchema(_schemaName, sqlExecutor);
            var columns = _columnLoader.LoadColumnsForTablesInSchema(_schemaName, sqlExecutor);
            var constraints = _constraintLoader.LoadConstraintsAndIndexesInSchema(_schemaName, sqlExecutor);

            databaseInteractor.GetDatabaseConnection().Close();

            foreach (var column in columns)
            {
                var parentTable = tables.FirstOrDefault(table => table.Name.Equals(column.ParentTableName));
                parentTable?.Columns.Add(column);
            }

            foreach (var constraint in constraints)
            {
                var parentTable = tables.FirstOrDefault(table => table.Name.Equals(constraint.TableName));
                parentTable?.Constraints.Add(constraint);
            }

            SortTables(tables);

            return new Schema(_schemaName, tables);
        }

        // Sort tables so the resultant object can be easily diffed
        private static void SortTables(List<Table> tables)
        {
            tables.Sort((x, y) => x.Name.CompareTo(y.Name));

            foreach (var table in tables)
            {
                table.Columns.Sort((x, y) => x.OrdinalPosition.CompareTo(y.OrdinalPosition));
                table.Constraints.Sort((x, y) => x.ColumnName.CompareTo(y.ColumnName));
            }
        }
    }
}