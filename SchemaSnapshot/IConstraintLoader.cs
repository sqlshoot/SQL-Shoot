using DatabaseInteraction;
using System.Collections.Generic;
using Constraint = SchemaSnapshot.DatabaseModel.Constraint;

namespace SchemaSnapshot
{
    internal interface IConstraintLoader
    {
        List<Constraint> LoadConstraintsAndIndexesInSchema(string schemaName, ISqlExecutor sqlExecutor);
    }
}