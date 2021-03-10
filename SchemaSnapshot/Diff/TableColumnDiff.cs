using System;
using System.Collections.Generic;
using SchemaSnapshot.DatabaseModel;

namespace SchemaSnapshot.Diff
{
    public class TableColumnDiff
    {
        public TableColumnDiff(List<Column> columnsOnlyInSource, List<Column> columnsOnlyInTarget, List<Tuple<Column, Column>> columnsInCommon)
        {
            ColumnsOnlyInSource = columnsOnlyInSource;
            ColumnsOnlyInTarget = columnsOnlyInTarget;
            ColumnsInCommon = columnsInCommon;
        }

        public List<Column> ColumnsOnlyInSource { get; }
        public List<Column> ColumnsOnlyInTarget { get; }
        public List<Tuple<Column, Column>> ColumnsInCommon { get; }
    }
}