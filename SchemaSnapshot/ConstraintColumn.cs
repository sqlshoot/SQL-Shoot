namespace SchemaSnapshot
{
    internal class ConstraintColumn
    {
        public string ColumnName { get; }
        public bool Descending { get; }

        public ConstraintColumn(string columnName, bool descending)
        {
            ColumnName = columnName;
            Descending = descending;
        }
    }
}