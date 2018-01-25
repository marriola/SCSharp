using SoundChange.StateMachines;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SoundChange.Lexer
{
    class Lexer : IEnumerable<Token>
    {
        private readonly Window<char> _contents;

        private TokenMachine _tokenMachine;

        private List<Token> _tokens;

        private (int line, int column) _filePosition;

        private int _it;

        public Token Current
        {
            get
            {
                return _it < _tokens.Count
                    ? _tokens[_it]
                    : null;
            }
        }

        private bool hasNext()
        {
            return !_contents.IsOutOfBounds;
        }

        private bool IsNextWhitespace
        {
            get
            {
                return _contents.IsOutOfBounds || char.IsWhiteSpace(_contents.Current);
            }
        }

        public Lexer(StreamReader stream)
        {
            _contents = stream.ReadToEnd().ToWindow();
            _tokenMachine = new TokenMachine();
            _tokens = new List<Token>();
        }

        public void Buffer()
        {
            _filePosition = (1, 1);

            while (hasNext())
            {
                var next = this.next();

                if (next.Type == TokenType.ERROR)
                {
                    throw new UnexpectedTokenException(next);
                }

                _tokens.Add(next);
            }

            _it = 0;
        }

        public bool HasNext
        {
            get
            {
                return _it < _tokens.Count;
            }
        }

        public Token Next(bool consumeWhitespace = false)
        {
            if (consumeWhitespace)
            {
                while (_tokens[_it].Type == TokenType.WHITESPACE)
                {
                    _it++;
                }
            }

            return _tokens[_it++];
        }

        public void Back(int count = 1)
        {
            _it -= count;
        }

        private Token next()
        {
            var value = string.Empty;
            _tokenMachine.Reset();

            while (char.IsWhiteSpace(_contents.Current))
            {
                value += read();
            }

            var position = _filePosition;

            if (value.Length >= 1)
            {
                return new Token(TokenType.WHITESPACE, value, position);
            }

            while (true)
            {
                if (_tokenMachine.Current == TokenMachine.ERROR)
                {
                    // Detect end of token
                    if (char.IsWhiteSpace(_contents.Current) || _tokenMachine.PeekStart(peekNonWhitespace()) != TokenMachine.ERROR)
                    {
                        break;
                    }

                    // Might still be an identifier or utterance
                    value += read();
                    continue;
                }

                var c = read();
                _tokenMachine.Step(c);
                value += c;

                var next = _tokenMachine.Peek(Token.END);

                // Detect end of token
                if (next?.Token.Value.Length == 1 || IsNextWhitespace) // || IsNextSingleCharacterToken())
                {
                    break;
                }
            }

            if (_tokenMachine.Current == TokenMachine.ERROR)
            {
                // TODO be less lazy
                foreach (var reToken in _tokenMachine.RegexTokens)
                {
                    var match = reToken.Regex.Match(value);

                    if (match.Success)
                    {
                        var token = new Token(reToken.Type, value.Substring(0, match.Length), position);
                        if (match.Length < value.Length)
                        {
                            _contents.MoveBack(value.Length - match.Length);
                        }

                        return token;
                    }
                }

                return TokenMachine.ERROR.Token.At(position);
            }

            _tokenMachine.Step(Token.END);

            return _tokenMachine.Current.Token.At(position);
        }

        private void FlushWhitespace()
        {
            while (IsNextWhitespace)
            {
                if (_contents.IsOutOfBounds)
                {
                    return;
                }

                read();
            }
        }

        public IEnumerator<Token> GetEnumerator()
        {
            return null;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return null;
        }

        public void Add(Token token)
        {
            _tokenMachine.AddToken(token);
        }

        private char read()
        {
            var c = _contents.Read();

            if (c == '\n')
            {
                _filePosition = (_filePosition.line + 1, 1);
            }
            else
            {
                _filePosition.column++;
            }

            return c;
        }

        private char peekNonWhitespace()
        {
            while (char.IsWhiteSpace(_contents.Current))
            {
                read();
            }

            return _contents.Current;
        }

        public Token Peek()
        {
            var token = Next(true);
            Back();
            return token;
        }
    }
}
