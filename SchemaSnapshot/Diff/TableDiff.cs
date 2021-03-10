namespace SchemaSnapshot.Diff
{
    public class TableDiff
    {
        public TableColumnDiff TableColumnDiff { get; }
        public TableConstraintDiff TableConstraint { get; }

        public TableDiff(TableColumnDiff tableColumnDiff, TableConstraintDiff tableConstraint)
        {
            TableColumnDiff = tableColumnDiff;
            TableConstraint = tableConstraint;
        }
    }
}