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
using SqlParser;
using System.Text.RegularExpressions;

namespace SqlShootEngine.Databases.SqlServer
{
    public class SqlServerParser : Parser
    {
        protected override bool IsAtDelimiter(ParsingContext parsingContext)
        {
            return IsGO(parsingContext);
        }
        
        protected override string GetDelimitedScript(string sql, ParsingContext parsingContext)
        {
            // Assumes delimiter is 'GO'
            // store the current script and continue searching
            // set length -1 so 'G' is not included in the script
            var length = parsingContext.index - parsingContext.scriptStartIndex - 1;
            var script = sql.Substring(parsingContext.scriptStartIndex, length);
            // start the next script after the 'O' in "GO"
            parsingContext.scriptStartIndex = parsingContext.index + 1;

            return script;
        }
        
        private bool IsGO(ParsingContext parsingContext)
        {
            /* valid GO is preceded by whitespace or the end of a multi-line
             * comment, and followed by whitespace or the beginning of a single
             * line or multi line comment. */
            if (char.ToUpper(parsingContext.previousCharacters[0]) != 'G' ||
                char.ToUpper(parsingContext.currentCharacter) != 'O')
            {
                return false;
            }

            if (!IsWhitespace(parsingContext.previousCharacters[1]) &&
                !IsEndMultiLineComment(parsingContext.previousCharacters[2], parsingContext.previousCharacters[1]))
            {
                return false;
            }

            if (!IsWhitespace(parsingContext.nextCharacters[0]) &&
                !IsOneLineComment(parsingContext.nextCharacters[0], parsingContext.nextCharacters[1]) 
                && !IsMultiLineComment(parsingContext.nextCharacters[0], parsingContext.nextCharacters[1]))
            {
                return false;
            }

            return true;
        }
        
        private bool IsWhitespace(char c)
        {
            return Regex.Match(c.ToString(), "\\s", RegexOptions.Multiline).Success;
        }
    }
}