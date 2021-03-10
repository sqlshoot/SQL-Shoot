using System.Collections.Generic;
using System.Data;
using Constraint = SchemaSnapshot.DatabaseModel.Constraint;

namespace SchemaSnapshot
{
    internal interface IConstraintLoader
    {
        List<Constraint> LoadConstraintsAndIndexesInSchema(string schemaName, IDbCommand dbCommand);
    }
}