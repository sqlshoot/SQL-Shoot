using System;
using System.Collections.Generic;
using SchemaSnapshot.DatabaseModel;

namespace SchemaSnapshot.Diff
{
    public class SchemaDiff
    {
        public SchemaDiff(List<Table> tablesOnlyInSource, List<Table> tablesOnlyInTarget, List<Tuple<Table, Table>> tablesInCommon, List<TableDiff> tableDiffs)
        {
            TablesOnlyInSource = tablesOnlyInSource;
            TablesOnlyInTarget = tablesOnlyInTarget;
            TablesInCommon = tablesInCommon;
            TableDiffs = tableDiffs;
        }

        public List<Table> TablesOnlyInSource { get; }
        public List<Table> TablesOnlyInTarget { get; }
        public List<Tuple<Table, Table>> TablesInCommon { get; }
        public List<TableDiff>  TableDiffs { get; }
    }
}