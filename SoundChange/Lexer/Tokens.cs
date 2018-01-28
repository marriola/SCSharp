namespace SoundChange.Lexer
{
    static class Tokens
    {
        public static readonly Token EOF = new Token(TokenType.ERROR, "EOF");
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
        public static readonly Token PIPE = new Token(TokenType.PIPE, "|");
        public static readonly RegexToken COMMENT = new RegexToken(TokenType.COMMENT, "^;(?<text>.+)\r?\n");
        public static readonly RegexToken UTTERANCE = new RegexToken(TokenType.UTTERANCE, "^[a-zɐ-́æœŒøØθçðβχʄħŋⱱǀǁǂǃ]+");
        public static readonly RegexToken IDENTIFIER = new RegexToken(TokenType.IDENT, "^\\$[a-zA-Z]([a-zA-Z0-9-]*[a-zA-Z0-9])?\\.?");
    }
}
