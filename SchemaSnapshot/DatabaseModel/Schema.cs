using System.Collections.Generic;

namespace SchemaSnapshot.DatabaseModel
{
    public class Schema
    {
        public string Name { get; }
        public List<Table> Tables { get; }

        public Schema(string name, List<Table> tables)
        {
            Name = name;
            Tables = tables;
        }
    }
}