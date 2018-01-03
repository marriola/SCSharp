namespace SoundChange.Lexer
{
    static class Tokens
    {
        public static readonly Token LBRACK = new Token(TokenType.LBRACK, "[");
        public static readonly Token RBRACK = new Token(TokenType.RBRACK, "]");
        public static readonly Token LBRACE = new Token(TokenType.LBRACE, "{");
        public static readonly Token RBRACE = new Token(TokenType.RBRACE, "}");
        public static readonly Token LPAREN = new Token(TokenType.LPAREN, "(");
        public static readonly Token RPAREN = new Token(TokenType.RPAREN, ")");
        public static readonly Token SLASH = new Token(TokenType.SLASH, "/");
        public static readonly Token PLACEHOLDER = new Token(TokenType.PLACEHOLDER, "_");
        public static readonly Token BOUNDARY = new Token(TokenType.BOUNDARY, "#");
        public static readonly Token ARROW = new Token(TokenType.ARROW, "=>");
        public static readonly Token PLUS = new Token(TokenType.PLUS, "+");
        public static readonly Token MINUS = new Token(TokenType.MINUS, "-");
    }
}
