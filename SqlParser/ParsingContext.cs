#region original-work-license
/*
 * This file is part of SQL Shoot.
 *
 * SQL Shoot is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * SQL Shoot is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser General Public License
 * along with SQL Shoot. If not, see <https://www.gnu.org/licenses/>.
 */
#endregion
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