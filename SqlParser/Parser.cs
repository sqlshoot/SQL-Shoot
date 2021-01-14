using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SqlParser
{
    public class Parser
    {
        protected bool IsOneLineComment(char c0, char c1)
        {
            return c0 == '-' && c1 == '-';
        }

        protected bool IsMultiLineComment(char c0, char c1)
        {
            return c0 == '/' && c1 == '*';
        }

        protected bool IsEndMultiLineComment(char c0, char c1)
        {
            return c0 == '*' && c1 == '/';
        }
        
        protected bool IsDollarQuotedLiteral(ParsingContext parsingContext)
        {
            return parsingContext.currentCharacter == '$' && parsingContext.nextCharacters[0] == '$';
        }

        protected virtual bool IsAtDelimiter(ParsingContext parsingContext)
        {
            return false;
        }

        /// <summary>
        /// Called once we're at the end of a delimited batch.
        /// e.g. Given 'SELECT * FROM my_table;', this is called at the ';'
        /// The 'scriptStartIndex' is already set, and indicates the index at which the batch ought to start
        /// Therefore this method gets the batch by working backwards from the delimiter to
        /// the script start index, adjusting the new script start index as required
        /// </summary>
        protected virtual string GetDelimitedScript(string sql, ParsingContext parsingContext)
        {
            return string.Empty;
        }

        public string[] Parse(string sql)
        {
            var parsingContext = new ParsingContext();
            var scripts = new List<string>();
            parsingContext.sql = sql;

            for (parsingContext.index = 0; parsingContext.index < sql.Length; parsingContext.index++)
            {
                var i = parsingContext.index;
                parsingContext.previousCharacters.Clear();
                parsingContext.nextCharacters.Clear();
                
                // out of bounds chars are treated as whitespace

                for (var p = 0; p < 10; p++)
                {
                    var index = i - p - 1;

                    parsingContext.previousCharacters.Add(index < 0 ? ' ' : sql[index]);
                }
                
                for (var p = 0; p < 10; p++)
                {
                    var index = i + p + 1;

                    parsingContext.nextCharacters.Add(index >= sql.Length ? ' ' : sql[index]);
                }
                
                parsingContext.currentCharacter = sql[i];

                switch (parsingContext.primaryState)
                {
                    case PrimaryState.Searching:
                        Search(parsingContext);
                        break;

                    case PrimaryState.InOneLineComment:
                        ParseOneLineComment(parsingContext);
                        break;

                    case PrimaryState.InMultiLineComment:
                        ParseMultiLineComment(parsingContext);
                        break;

                    case PrimaryState.InBrackets:
                        ParseInBrackets(parsingContext);
                        break;

                    case PrimaryState.InQuotes:
                        ParseInQuotes(parsingContext);
                        break;

                    case PrimaryState.InDoubleQuotes:
                        ParseInDoubleQuotes(parsingContext);
                        break;
                }

                if (parsingContext.foundDelimiter)
                {
                    scripts.Add(GetDelimitedScript(sql, parsingContext));
                    parsingContext.foundDelimiter = false;
                    parsingContext.secondaryState = SecondaryState.None;
                }
                else
                {
                    if (parsingContext.index == sql.Length - 1)
                    {
                        // end of script
                        // set length +1 to include the current char
                        var length = parsingContext.index - parsingContext.scriptStartIndex + 1;
                        scripts.Add(sql.Substring(parsingContext.scriptStartIndex, length));
                    }
                }
            }

            // return scripts that contain non-whitespace
            return scripts.Where(s => Regex.Match(s, "\\S", RegexOptions.Multiline).Success)
                .ToArray();
        }

        private static void ParseInDoubleQuotes(ParsingContext parsingContext)
        {
            if (parsingContext.currentCharacter == '\"')
            {
                parsingContext.primaryState = PrimaryState.Searching;
            }
        }

        private static void ParseInQuotes(ParsingContext parsingContext)
        {
            if (parsingContext.currentCharacter == '\'')
            {
                parsingContext.primaryState = PrimaryState.Searching;
            }
        }

        private static void ParseInBrackets(ParsingContext parsingContext)
        {
            if (parsingContext.currentCharacter == ']')
            {
                parsingContext.primaryState = PrimaryState.Searching;
            }
        }

        private void ParseMultiLineComment(ParsingContext parsingContext)
        {
            if (IsEndMultiLineComment(parsingContext.previousCharacters[0], parsingContext.currentCharacter))
            {
                parsingContext.commentDepth--;
            }
            else if (IsMultiLineComment(parsingContext.previousCharacters[0], parsingContext.currentCharacter))
            {
                parsingContext.commentDepth++;
            }

            if (parsingContext.commentDepth < 0)
            {
                parsingContext.commentDepth = 0;
                parsingContext.primaryState = PrimaryState.Searching;
            }
        }

        private static void ParseOneLineComment(ParsingContext parsingContext)
        {
            if (parsingContext.currentCharacter == '\n')
            {
                parsingContext.primaryState = PrimaryState.Searching;
            }
        }

        private void Search(ParsingContext parsingContext)
        {
            if (IsDollarQuotedLiteral(parsingContext))
            {
                parsingContext.secondaryState = SecondaryState.InDollarQuotedLiteral;
            }
            
            if (IsMultiLineComment(parsingContext.previousCharacters[0], parsingContext.currentCharacter))
            {
                parsingContext.primaryState = PrimaryState.InMultiLineComment;
            }
            else if (IsOneLineComment(parsingContext.previousCharacters[0], parsingContext.currentCharacter))
            {
                parsingContext.primaryState = PrimaryState.InOneLineComment;
            }
            else if (parsingContext.currentCharacter == '[')
            {
                parsingContext.primaryState = PrimaryState.InBrackets;
            }
            else if (parsingContext.currentCharacter == '\'')
            {
                parsingContext.primaryState = PrimaryState.InQuotes;
            }
            else if (parsingContext.currentCharacter == '\"')
            {
                parsingContext.primaryState = PrimaryState.InDoubleQuotes;
            }
            else
            {
                parsingContext.foundDelimiter = IsAtDelimiter(parsingContext);
            }
        }
    }
}