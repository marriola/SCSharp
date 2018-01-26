using SoundChange.Factories;
using SoundChange.Lexer;
using SoundChange.Parser;
using System.Collections.Generic;

namespace SoundChange.StateMachines
{
    class TokenMachine
    {
        public static State START = new State("S");

        public static State ERROR = new State(new Token(TokenType.ERROR, string.Empty));

        private State _current;

        private Dictionary<(State, char), State> _transitions = new Dictionary<(State, char), State>();

        private List<RegexToken> _regexTokens = new List<RegexToken>();

        private StateFactory _stateFactory = new StateFactory();

        public IReadOnlyList<RegexToken> RegexTokens
        {
            get
            {
                return _regexTokens.AsReadOnly();
            }
        }

        public bool IsFinalState
        {
            get
            {
                return _current.Token != null;
            }
        }

        public State Current
        {
            get
            {
                return _current;
            }
        }

        public void AddToken(Token token)
        {
            if (token is RegexToken reToken)
            {
                _regexTokens.Add(reToken);
                return;
            }

            var currentState = START;

            for (var i = 0; i < token.Value.Length; i++)
            {
                var c = token.Value[i];

                if (!_transitions.ContainsKey((currentState, c)))
                {
                    _transitions[(currentState, c)] = _stateFactory.Next();
                }

                currentState = _transitions[(currentState, c)];
            }

            _transitions[(currentState, Token.END)] = new State(token);
        }

        public void Reset()
        {
            _current = START;
        }

        public State Step(char c)
        {
            _current = Peek(c);

            return _current;
        }

        public State Peek(char c)
        {
            return _transitions.ContainsKey((_current, c))
                ? _transitions[(_current, c)]
                : ERROR;
        }

        public State PeekStart(char c)
        {
            return _transitions.ContainsKey((START, c))
                ? _transitions[(START, c)]
                : ERROR;
        }
    }
}
