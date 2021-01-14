using System.Collections.Generic;

namespace SqlParser
{
    public class ParsingContext
    {
        public string sql;
        public int index = 0;
        public List<char> previousCharacters = new List<char>();
        public List<char> nextCharacters = new List<char>();
        public char currentCharacter = ' ';
        public PrimaryState primaryState = PrimaryState.Searching;
        public SecondaryState secondaryState = SecondaryState.None;
        public int commentDepth = 0;
        public bool foundDelimiter = false;
        public int scriptStartIndex = 0;

        public bool NextNonWhitespaceCharacterIs(char c, int startingIndex)
        {
            for (var i = startingIndex; i < sql.Length; i++)
            {
                if (!char.IsWhiteSpace(sql[i]))
                {
                    return sql[i] == c;
                }
            }

            return false;
        }

        public int IndexOfNextNonWhitespaceCharacterFrom(int startingIndex)
        {
            for (var i = startingIndex; i < sql.Length; i++)
            {
                if (!char.IsWhiteSpace(sql[i]))
                {
                    return i;
                }
            }

            return 0;
        }
    }
}