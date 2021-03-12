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

namespace SqlShootEngine.DatabaseInteraction.PostgreSQL
{
    public class PostgreSQLParser : Parser
    {
        protected override bool IsAtDelimiter(ParsingContext parsingContext)
        {
            if (parsingContext.secondaryState == SecondaryState.InDollarQuotedLiteral)
            {
                return parsingContext.currentCharacter == '$' &&
                       parsingContext.nextCharacters[0] == '$' &&
                       parsingContext.NextNonWhitespaceCharacterIs(';', parsingContext.index + 2);
            }

            return parsingContext.currentCharacter == ';';
        }

        protected override string GetDelimitedScript(string sql, ParsingContext parsingContext)
        {
            if (parsingContext.secondaryState == SecondaryState.InDollarQuotedLiteral)
            {
                return GetDelimitedScriptInDollarQuotedLiteral(sql, parsingContext);
            }

            var length = parsingContext.index - parsingContext.scriptStartIndex + 1;
            var script = sql.Substring(parsingContext.scriptStartIndex, length);

            parsingContext.scriptStartIndex += length + 1;
            parsingContext.index = parsingContext.scriptStartIndex;

            return script;
        }

        private static string GetDelimitedScriptInDollarQuotedLiteral(string sql, ParsingContext parsingContext)
        {
            // Current character should be '$'
            // So we're looking for the next '$', then any number of whitespace, then ending with ';'
            var endIndex = parsingContext.IndexOfNextNonWhitespaceCharacterFrom(parsingContext.index + 2);
            var length = endIndex - parsingContext.scriptStartIndex;
            var script = sql.Substring(parsingContext.scriptStartIndex, length);

            // Start the next script after the '$$;' sequence
            parsingContext.scriptStartIndex = endIndex + 1;
            // Move up the index to the end of the '$$;' sequence
            parsingContext.index = parsingContext.scriptStartIndex;

            return script;
        }
    }
}