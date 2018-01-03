using SoundChange.Lexer;
using System;

namespace SoundChange.Parser
{
    class ParseException : ApplicationException
    {
        public ParseException(string rule, string expected, Token got)
            : base($"At line {got.Position.line}, column {got.Position.column} in '{rule}': Expected {expected}, got '{got.Value}'.")
        {
        }
    }
}
