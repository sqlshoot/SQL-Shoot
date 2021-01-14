using SqlParser;

namespace Engine.Databases.PostgreSQL
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