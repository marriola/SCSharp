using SoundChange.StateMachines;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace SoundChange
{
    class Lexer : IEnumerable<Token>
    {
        private static Regex RE_UTTERANCE = new Regex("[a-zɐ-˩æøØθ]+");

        private static Regex RE_IDENTIFIER = new Regex("\\$[a-zA-Z][a-zA-Z0-9-]*");

        private static Regex RE_NUMERIC = new Regex("[0-9]+");

        private readonly StreamReader _stream;

        private TokenMachine _stateMachine;

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
            return !_stream.EndOfStream;
        }

        private bool IsNextWhitespace
        {
            get
            {
                return _stream.EndOfStream || char.IsWhiteSpace((char)_stream.Peek());
            }
        }

        public Lexer(StreamReader stream)
        {
            _stream = stream;
            _stateMachine = new TokenMachine();
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
                    throw new ApplicationException("Unexpected token.");
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
            _stateMachine.Reset();

            while (char.IsWhiteSpace(peek()))
            {
                value += read();
            }

            var position = _filePosition;

            if (value.Length >= 1)
            {
                return Token.From(TokenType.WHITESPACE, value, position);
            }

            while (true)
            {
                if (_stateMachine.Current == TokenMachine.ERROR)
                {
                    // Detect end of token
                    if (char.IsWhiteSpace(peek()) || _stateMachine.PeekStart(peekNonWhitespace()) != TokenMachine.ERROR)
                    {
                        break;
                    }

                    // Might still be an identifier or utterance
                    value += read();
                    continue;
                }

                var c = read();
                _stateMachine.Step(c);
                value += c;

                var next = _stateMachine.Peek(Token.END);

                // Detect end of token
                if (next?.Token.Value.Length == 1 || IsNextWhitespace) // || IsNextSingleCharacterToken())
                {
                    break;
                }
            }

            if (_stateMachine.Current == TokenMachine.ERROR)
            {
                if (RE_IDENTIFIER.IsMatch(value))
                {
                    return Token.From(TokenType.IDENT, value, position);
                }
                else if (RE_UTTERANCE.IsMatch(value))
                {
                    return Token.From(TokenType.UTTERANCE, value, position);
                }

                return TokenMachine.ERROR.Token.At(position);
            }

            _stateMachine.Step(Token.END);

            return _stateMachine.Current.Token.At(position);
        }

        private void FlushWhitespace()
        {
            while (IsNextWhitespace)
            {
                if (_stream.EndOfStream)
                {
                    return;
                }

                read();
            }
        }

        //private bool IsNextSingleCharacterToken()
        //{
        //    return _stateMachine.Peek((char)_stream.Peek()).Token.Value.Length == 1;
        //}

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
            _stateMachine.AddToken(token);
        }

        private char peek()
        {
            return (char)_stream.Peek();
        }

        private char read()
        {
            var c = (char)_stream.Read();

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
            while (char.IsWhiteSpace(peek()))
            {
                read();
            }

            return peek();
        }

        public Token Peek()
        {
            var token = Next(true);
            Back();
            return token;
        }
    }
}
