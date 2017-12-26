namespace SoundChange
{
    static class Tokens
    {
        public static readonly Token LBRACK = Token.From(TokenType.LBRACK, "[");
        public static readonly Token RBRACK = Token.From(TokenType.RBRACK, "]");
        public static readonly Token LBRACE = Token.From(TokenType.LBRACE, "{");
        public static readonly Token RBRACE = Token.From(TokenType.RBRACE, "}");
        public static readonly Token SLASH = Token.From(TokenType.SLASH, "/");
        public static readonly Token PLACEHOLDER = Token.From(TokenType.PLACEHOLDER, "_");
        public static readonly Token BOUNDARY = Token.From(TokenType.BOUNDARY, "#");
        public static readonly Token ARROW = Token.From(TokenType.ARROW, "=>");
        public static readonly Token PLUS = Token.From(TokenType.PLUS, "+");
        public static readonly Token MINUS = Token.From(TokenType.MINUS, "-");
    }
}
