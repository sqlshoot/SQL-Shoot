using System.Collections.Generic;
using System.Data;
using SchemaSnapshot.DatabaseModel;

namespace SchemaSnapshot
{
    internal interface IColumnLoader
    {
        List<Column> LoadColumnsForTablesInSchema(string schemaName, IDbCommand dbCommand);
    }
}