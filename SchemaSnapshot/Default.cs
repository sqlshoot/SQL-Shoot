namespace SchemaSnapshot
{
    internal class Default
    {
        public string Name;
        public string Value;
        public bool IsSystemNamed;

        public Default(string name, string value, bool isSystemNamed)
        {
            Name = name;
            Value = value;
            IsSystemNamed = isSystemNamed;
        }
    }
}