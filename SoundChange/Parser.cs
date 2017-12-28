using SoundChange.Nodes;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace SoundChange
{
    class Parser
    {
        private readonly Lexer _lexer;

        public bool HasNext
        {
            get
            {
                return _lexer.HasNext;
            }
        }

        public Parser(StreamReader stream)
        {
            _lexer = new Lexer(stream)
            {
                Tokens.LBRACE,
                Tokens.RBRACE,
                Tokens.LBRACK,
                Tokens.RBRACK,
                Tokens.LPAREN,
                Tokens.RPAREN,
                Tokens.ARROW,
                Tokens.BOUNDARY,
                Tokens.PLACEHOLDER,
                Tokens.SLASH,
                Tokens.PLUS,
                Tokens.MINUS
            };

            _lexer.Buffer();
        }

        /// <summary>
        /// Parses the next node from the stream.
        /// </summary>
        /// <returns></returns>
        public Node Next()
        {
            if (_lexer.Current?.Type == TokenType.WHITESPACE)
            {
                _lexer.Next();
            }

            if (_lexer.Current?.Type == TokenType.IDENT)
            {
                return Category();
            }
            else if (_lexer.Current?.Type == TokenType.LBRACK)
            {
                return FeatureSet_Rule();
            }
            else if (_lexer.Current != null)
            {
                return Rule();
            }

            return null;
        }

        private CategoryNode Category()
        {
            var ident = Match(TokenType.IDENT, "Category");
            Match(TokenType.LBRACE, "Category");

            var members = new HashSet<string>();

            while (true)
            {
                var next = _lexer.Next(true);

                if (next.Type == TokenType.UTTERANCE)
                {
                    members.Add(next.Value);
                }
                else if (next.Type == TokenType.RBRACE)
                {
                    break;
                }
                else
                {
                    throw new ParseException("Category", "an utterance or ']'", next);
                }
            }

            return new CategoryNode(ident.Value, members);
        }

        /// <summary>
        /// Parses tokens that may parse to either a RuleNode or a FeatureSetNode
        /// Rule: { UTTERANCE | '[' ( '+' | '+' ) IDENT ']' } ...
        /// FeatureSet: '[' IDENT ']' '{' ...
        /// </summary>
        /// <returns>A RuleNode if the following tokens describe a rule, a FeatureSetNode if they represent a feature set.</returns>
        private Node FeatureSet_Rule()
        {
            Match(TokenType.LBRACK, "FeatureSet_Rule");
            var next = _lexer.Next(true);

            if (next.Type == TokenType.IDENT)
            {
                Match(TokenType.RBRACK, "FeatureSet_Rule");
                return FeatureSet(next.Value);
            }
            else
            {
                _lexer.Back(2);
                return Rule();
            }
        }

        private FeatureSetNode FeatureSet(string name)
        {
            Match(TokenType.LBRACE, "FeatureSet");

            var members = new HashSet<string>();
            var transformations = new List<(string from, string to)>();

            while (Member_Transform(members, transformations)) ;

            Match(TokenType.RBRACE, "FeatureSet");

            return new FeatureSetNode(name, members, transformations);
        }

        /// <summary>
        /// Matches tokens that may parse to an UtteranceNode or a TransformationNode.
        /// </summary>
        /// <param name="members"></param>
        /// <param name="transformations"></param>
        /// <returns>True if more child nodes remain; otherwise, false.</returns>
        private bool Member_Transform(HashSet<string> members, List<(string from, string to)> transformations)
        {
            var utterance = Match(TokenType.UTTERANCE, "Member_Transform");
            var next = _lexer.Next(true);

            switch (next.Type)
            {
                case TokenType.ARROW:
                    var to = Match(TokenType.UTTERANCE, "Member_Transform");
                    transformations.Add((utterance.Value, to.Value));
                    members.Add(to.Value);

                    return _lexer.Peek().Type != TokenType.RBRACE;

                case TokenType.UTTERANCE:
                    // Ran into the next utterance; back up.
                    _lexer.Back();
                    members.Add(utterance.Value);
                    return true;

                case TokenType.RBRACE:
                    // Ran into the end; back up.
                    _lexer.Back();
                    members.Add(utterance.Value);
                    return false;

                default:
                    throw new ParseException("Member_Transform", "an identifier, '=>' or '}'", _lexer.Current);
            }
        }

        private Node Rule()
        {
            var target = new List<Node>();
            var result = new List<Node>();
            var environment = new List<Node>();

            var targetPosition = _lexer.Current.Position;
            RuleTarget(target);
            RuleResult(result);

            var envPosition = _lexer.Current.Position;
            var node = RuleEnvironment(target, result, environment);

            if (node.Target.Count == 0)
            {
                throw new SyntaxException("Rule", "Target segment may not be empty.", targetPosition);
            }

            for (var i = 1; i < environment.Count - 1; i++)
            {
                if (environment[i] is BoundaryNode)
                {
                    throw new SyntaxException("Rule", "Boundary token may only appear at each end of the environment segment.", envPosition);
                }
            }

            return node;
        }

        private void RuleTarget(List<Node> target)
        {
            while (true)
            {
                var next = _lexer.Next();

                switch (next.Type)
                {
                    case TokenType.LBRACK:
                        target.Add(FeatureSetIdentifier());
                        break;

                    case TokenType.IDENT:
                        target.Add(new IdentifierNode(next.Value));
                        break;

                    case TokenType.UTTERANCE:
                        target.Add(new UtteranceNode(next.Value));
                        break;

                    case TokenType.SLASH:
                        return;

                    default:
                        throw new ParseException("RuleTarget", "an identifier, '[' or '/'", next);
                }
            }
        }

        private void RuleResult(List<Node> result)
        {
            while (true)
            {
                var next = _lexer.Next();

                switch (next.Type)
                {
                    case TokenType.LBRACK:
                        result.Add(FeatureSetIdentifier());
                        break;

                    case TokenType.IDENT:
                        result.Add(new IdentifierNode(next.Value));
                        break;

                    case TokenType.UTTERANCE:
                        result.Add(new UtteranceNode(next.Value));
                        break;

                    case TokenType.SLASH:
                        return;

                    default:
                        throw new ParseException("RuleResult", "an identifier, '[' or '/'", next);
                }
            }
        }

        private RuleNode RuleEnvironment(List<Node> target, List<Node> result, List<Node> environment)
        {
            OptionalNode optional = null;
            List<Node> parent = environment;

            while (true)
            {
                var next = _lexer.Next();

                switch (next.Type)
                {
                    case TokenType.LBRACK:
                        parent.Add(FeatureSetIdentifier());
                        break;

                    case TokenType.PLACEHOLDER:
                        if (optional != null)
                        {
                            throw new SyntaxException("RuleEnvironment", "Placeholder cannot be optional.", next.Position);
                        }
                        parent.Add(new PlaceholderNode());
                        break;

                    case TokenType.IDENT:
                        parent.Add(new IdentifierNode(next.Value));
                        break;

                    case TokenType.UTTERANCE:
                        parent.Add(new UtteranceNode(next.Value));
                        break;

                    case TokenType.BOUNDARY:
                        if (optional != null)
                        {
                            throw new SyntaxException("RuleEnvironment", "Boundary cannot be optional.", next.Position);
                        }
                        parent.Add(new BoundaryNode());
                        break;

                    case TokenType.LPAREN:
                        if (optional != null)
                        {
                            throw new ParseException("RuleEnvironment", "utterance, identifier, placeholder, category identifier, feature identifier, or ')'", next);
                        }
                        optional = new OptionalNode();
                        parent = optional.Children;
                        environment.Add(optional);
                        break;

                    case TokenType.RPAREN:
                        if (optional == null)
                        {
                            throw new ParseException("RuleEnvironment", "utterance, identifier, placeholder, category identifier, feature identifier, or '('", next);
                        }
                        optional = null;
                        parent = environment;
                        break;

                    default:
                        // Done; back up.
                        _lexer.Back();
                        return new RuleNode(target, result, environment);
                }
            }
        }

        private FeatureIdentifierNode FeatureSetIdentifier(bool optional = false)
        {
            var presenceToken = _lexer.Next();
            if (presenceToken.Type != TokenType.PLUS && presenceToken.Type != TokenType.MINUS)
            {
                throw new ParseException("FeatureSetIdentifier", "'+' or '-'", presenceToken);
            }

            var isPresent = presenceToken.Type == TokenType.PLUS;
            var ident = _lexer.Next();
            Match(TokenType.RBRACK, "FeatureSetIdentifier");

            return new FeatureIdentifierNode(isPresent, ident.Value);
        }

        private Token Match(TokenType type, string rule)
        {
            var token = _lexer.Next(type != TokenType.WHITESPACE);

            if (token.Type != type)
            {
                throw new ParseException(rule, GetTokenValue(type), token);
            }

            return token;
        }

        private string GetTokenValue(TokenType type)
        {
            var props = typeof(Tokens).GetFields(BindingFlags.Static | BindingFlags.Public);

            foreach (var prop in props)
            {
                var token = (Token)prop.GetValue(null);
                if (token.Type == type)
                {
                    return "'" + token.Value + "'";
                }
            }

            return type.ToString();
        }
    }
}
