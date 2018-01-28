using SoundChange.StateMachines;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
                    ++_it;
                }
            }

            return _tokens[_it++];
        }

        public (int count, Token token) NextCountWhitespace(int count = 1)
        {
            var incremented = 0;
            var lastIt = _it;

            for (var i = 0; i < count; i++)
            {
                while (_tokens[_it].Type == TokenType.WHITESPACE)
                {
                    ++incremented;
                    ++_it;
                }

                lastIt = _it;
                ++_it;
            }

            return (incremented + count, _tokens[lastIt]);
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
                // Detect end of token
                if (_tokenMachine.Current == TokenMachine.ERROR &&
                    (char.IsWhiteSpace(_contents.Current) ||
                        _tokenMachine.PeekStart(peekNonWhitespace()) != TokenMachine.ERROR))
                {
                    break;
                }

                var c = read();
                _tokenMachine.Step(c);
                value += c;

                if (_tokenMachine.Current == TokenMachine.ERROR)
                {
                    // Check to see if we can match on one of the regex tokens before failing.
                    // TODO be less lazy and write a proper state machine
                    var contents = new string(_contents.Contents.Skip(_contents.Index - 1).ToArray());

                    foreach (var reToken in _tokenMachine.RegexTokens)
                    {
                        var match = reToken.Match(contents);

                        if (match.Success)
                        {
                            var token = new Token(reToken.Type, match.Value, position);
                            _contents.MoveNext(match.Length - 1);
                            UpdateFilePosition(match.Value);

                            return token;
                        }
                    }
                }

                var next = _tokenMachine.Peek(Token.END);

                // Detect end of token
                if (next?.Token.Value.Length == 1 || IsNextWhitespace)
                {
                    break;
                }
            }

            if (_tokenMachine.Current == TokenMachine.ERROR)
            {
                return TokenMachine.ERROR.Token.At(position);
            }

            _tokenMachine.Step(Token.END);

            return _tokenMachine.Current.Token.At(position);
        }

        private void UpdateFilePosition(string match)
        {
            int numLines = 0;
            int numColumns = 0;

            foreach (var c in match)
            {
                if (c == '\n')
                {
                    numColumns = 1;
                    ++numLines;
                }
                else
                {
                    ++numColumns;
                }
            }

            _filePosition.line += numLines;
            _filePosition.column = numLines == 0
                ? _filePosition.column + numColumns
                : numColumns;
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

        public Token Peek(int count = 1)
        {
            var (numIncremented, token) = NextCountWhitespace(count);
            Back(numIncremented);
            return token;
        }
    }
}
