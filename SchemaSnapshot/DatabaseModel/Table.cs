using System.Collections.Generic;

namespace SchemaSnapshot.DatabaseModel
{
    public abstract class Table
    {
        public string ParentSchema { get; }
        public string Name { get; }
        public List<Column> Columns { get; } = new List<Column>();
        public List<Constraint> Constraints { get; } = new List<Constraint>();

        protected Table(string parentSchema, string name)
        {
            ParentSchema = parentSchema;
            Name = name;
        }
    }
}