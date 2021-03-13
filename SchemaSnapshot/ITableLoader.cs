using System.Collections.Generic;
using DatabaseInteraction;
using SchemaSnapshot.DatabaseModel;

namespace SchemaSnapshot
{
    internal interface ITableLoader
    {
        List<Table> LoadTablesInSchema(string schemaName, ISqlExecutor sqlExecutor);
    }
}