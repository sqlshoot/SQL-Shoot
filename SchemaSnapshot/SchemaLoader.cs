using System.Data;
using System.Linq;
using SchemaSnapshot.DatabaseModel;

namespace SchemaSnapshot
{
    internal class SchemaLoader
    {
        private readonly ITableLoader _tableLoader;
        private readonly IColumnLoader _columnLoader;
        private readonly IConstraintLoader _constraintLoader;
        private readonly string _schemaName;

        public SchemaLoader(
            ITableLoader tableLoader,
            IColumnLoader columnLoader,
            IConstraintLoader constraintLoader,
            string schemaName)
        {
            _tableLoader = tableLoader;
            _columnLoader = columnLoader;
            _schemaName = schemaName;
            _constraintLoader = constraintLoader;
        }

        public Schema Load(IDbCommand dbCommand)
        {
            var tables = _tableLoader.LoadTablesInSchema(_schemaName, dbCommand);
            var columns = _columnLoader.LoadColumnsForTablesInSchema(_schemaName, dbCommand);
            var constraints = _constraintLoader.LoadConstraintsAndIndexesInSchema(_schemaName, dbCommand);

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

            return new Schema(_schemaName, tables);
        }
    }
}