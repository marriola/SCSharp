namespace SoundChange
{
    class Token
    {
        public static char END = (char)0;

        public TokenType Type { get; private set; }

        public string Value { get; private set; }

        public (int line, int column) Position { get; private set; }

        public static Token From(TokenType type, string value, (int line, int column)? position = null)
        {
            return new Token
            {
                Type = type,
                Value = value,
                Position = position ?? (0, 0)
            };
        }

        public Token At((int line, int column) position)
        {
            return new Token
            {
                Type = Type,
                Value = Value,
                Position = position
            };
        }

        public override string ToString()
        {
            return $"{Type.ToString()}, {Value}";
        }


    }
}
