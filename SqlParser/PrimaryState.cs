namespace SqlParser
{
    public enum PrimaryState
    {
        Searching,
        InOneLineComment,
        InMultiLineComment,
        InBrackets,
        InQuotes,
        InDoubleQuotes
    }
}