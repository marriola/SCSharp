using System.Linq;

namespace SoundChange.Lexer
{
    class Token
    {
        public static char END = (char)0;

        public TokenType Type { get; private set; }

        public string Value { get; private set; }

        public string Name { get; private set; }

        public (int line, int column) Position { get; private set; }

        public Token(TokenType type, string value, (int line, int column)? position = null)
        {
            Type = type;
            Value = value;
            Position = position ?? (0, 0);

            var props = typeof(Tokens).GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            var token = props
                .Select(x => x.GetValue(null) as Token)
                .SingleOrDefault(x => x?.Type == type);

            Name = token != null
                ? "'" + token.Value + "'"
                : type.ToString();
        }

        public Token At((int line, int column) position)
        {
            return new Token(Type, Value, position);
        }

        public override string ToString()
        {
            return $"{Type.ToString()}, {Value}";
        }
    }
}
