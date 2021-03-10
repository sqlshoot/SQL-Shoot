using System.Collections.Generic;
using System.Data;
using SchemaSnapshot.DatabaseModel;

namespace SchemaSnapshot
{
    internal interface ITableLoader
    {
        List<Table> LoadTablesInSchema(string schemaName, IDbCommand dbCommand);
    }
}