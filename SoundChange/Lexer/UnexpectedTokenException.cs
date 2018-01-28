using System;

namespace SoundChange.Lexer
{
    class UnexpectedTokenException : ApplicationException
    {
        public UnexpectedTokenException(Token token)
            : base($"Unexpected token '{token.Value}' at {token.Position}.")
        {
        }
    }
}
