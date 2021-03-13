namespace SchemaSnapshot.DatabaseModel
{
    public class Constraint
    {
        public string TableName { get; }
        public string Name { get; }
        public string ColumnName { get; }
        public string Type { get; }

        public Constraint(
            string tableName,
            string name,
            string columnName,
            string type)
        {
            TableName = tableName;
            Name = name;
            ColumnName = columnName;
            Type = type;
        }
    }
}