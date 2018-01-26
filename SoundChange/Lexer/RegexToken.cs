using System.Text.RegularExpressions;

namespace SoundChange.Lexer
{
    class RegexToken : Token
    {
        public Regex Regex { get; private set; }

        public RegexToken(TokenType type, string value, (int line, int column)? position = null)
            : base(type, value, position)
        {
            Regex = new Regex(value);
        }

        public Match Match(string input)
        {
            return Regex.Match(input);
        }
    }
}
