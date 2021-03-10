using System;
using System.Collections.Generic;
using SchemaSnapshot.DatabaseModel;

namespace SchemaSnapshot.Diff
{
    public class TableConstraintDiff
    {
        public TableConstraintDiff(List<Constraint> constraintsOnlyInSource, List<Constraint> constraintsOnlyInTarget, List<Tuple<Constraint, Constraint>> constraintsInCommon)
        {
            ConstraintsOnlyInSource = constraintsOnlyInSource;
            ConstraintsOnlyInTarget = constraintsOnlyInTarget;
            ConstraintsInCommon = constraintsInCommon;
        }

        public List<Constraint> ConstraintsOnlyInSource { get; }
        public List<Constraint> ConstraintsOnlyInTarget { get; }
        public List<Tuple<Constraint, Constraint>> ConstraintsInCommon { get; }
    }
}