namespace SchemaSnapshot.DatabaseModel
{
    public abstract class Column
    {
        protected Column(
            string parentTableName,
            string name,
            int ordinalPosition,
            bool isNullable,
            string dataType)
        {
            ParentTableName = parentTableName;
            Name = name;
            OrdinalPosition = ordinalPosition;
            DataType = dataType;
            IsNullable = isNullable;
        }

        public string ParentTableName { get; }
        public string Name { get; }
        public int OrdinalPosition { get; }
        public bool IsNullable { get; }
        public string DataType { get; }
    }
}