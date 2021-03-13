using System.Collections.Generic;
using DatabaseInteraction;
using SchemaSnapshot.DatabaseModel;

namespace SchemaSnapshot
{
    internal interface IColumnLoader
    {
        List<Column> LoadColumnsForTablesInSchema(string schemaName, ISqlExecutor sqlExecutor);
    }
}