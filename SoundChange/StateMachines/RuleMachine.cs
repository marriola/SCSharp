using SoundChange.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundChange.StateMachines
{
    class RuleMachine
    {
        static class Special
        {
            public static char LAMBDA = '\u2400';

            public static char START = '\u2402';

            public static char END = '\u2403';
        }

        public static State START = new State("S");

        private TransitionTable _transitions = new TransitionTable();

        //private Dictionary<(State, char), State> _transitions = new Dictionary<(State, char), State>();

        //private Dictionary<State, Dictionary<char, List<State>>> _transitionsByState;

        private StateFactory _stateFactory;

        public RuleMachine(RuleNode rule, List<FeatureSetNode> features, List<CategoryNode> categories)
        {
            var placeholder = rule.Environment.Single(x => x is PlaceholderNode) as PlaceholderNode;
            placeholder.Children = rule.Target;

            var dFeatures = features.ToDictionary(k => k.Name, v => v);
            var dCategories = categories.ToDictionary(k => k.Name, v => v);

            _stateFactory = new StateFactory();
            BuildNFA(rule.Environment, dFeatures, dCategories);
        }

        public string ApplyTo(string word)
        {
            var current = START;
            var nextWord = string.Empty;

            word = Special.START + word + Special.END;

            foreach (var c in word)
            {
                var transition = _transitions.GetFirst(current, c);

                if (transition == null)
                {
                    current = TransitionFromStart(c);
                }
                else
                {
                    current = transition;
                }

                if (current.IsFinal)
                {
                    current = TransitionFromStart(c);
                }
            }

            return nextWord;

            State TransitionFromStart(char c)
            {
                return _transitions.GetFirst(START, c) ?? START;
            }
        }

        private State BuildNFA(
            List<Node> nodes, 
            Dictionary<string, FeatureSetNode> features, 
            Dictionary<string, CategoryNode> categories, 
            State startNode = null, 
            bool optional = false)
        {
            var current = startNode ?? START;
            var len = nodes.Count;
            var last = len - 1;

            for (var i = 0; i < len; i++)
            {
                var node = nodes[i];

                if (node is BoundaryNode)
                {
                    MatchCharacter(i == 0
                        ? Special.START
                        : Special.END);
                }
                else if (node is UtteranceNode uNode)
                {
                    foreach (var c in uNode.Value)
                    {
                        MatchCharacter(c);
                    }
                }
                else if (node is FeatureIdentifierNode fiNode)
                {
                    if (!features.ContainsKey(fiNode.Name))
                    {
                        throw new KeyNotFoundException($"Feature set '{fiNode.Name}' not defined.");
                    }

                    var feature = features[fiNode.Name];

                    MatchSet(fiNode.IsPresent
                        ? feature.PlusTree
                        : feature.MinusTree);
                }
                else if (node is IdentifierNode iNode)
                {
                    if (!categories.ContainsKey(iNode.Value))
                    {
                        throw new KeyNotFoundException($"Category '{iNode.Value}' not defined.");
                    }

                    MatchSet(categories[iNode.Value].BuilderTree);
                }
                else if (node is OptionalNode oNode)
                {
                    current = BuildNFA(oNode.Children, features, categories, current, true);
                }
                else if (node is PlaceholderNode pNode)
                {
                    current = BuildNFA(pNode.Children, features, categories, current);
                }

                if (startNode == null && i == last)
                {
                    current.IsFinal = true;
                }
            }

            return current;

            void MatchCharacter(char c)
            {
                var next = _stateFactory.Next();

                //_transitions[(current, c)] = next;
                _transitions.Add(current, c, next);
                if (optional)
                {
                    //_transitions[(current, Special.LAMBDA)] = next;
                    _transitions.Add(current, Special.LAMBDA, next);
                    optional = false;
                }

                current = next;
            }

            void MatchSet(BuilderNode builder)
            {
                var final = _stateFactory.Next();

                if (optional && !builder.Character.HasValue)
                {
                    //_transitions[(current, Special.LAMBDA)] = final;
                    _transitions.Add(current, Special.LAMBDA, final);
                }

                var stack = new Stack<(State State, BuilderNode Node)>();
                stack.Push((current, builder));

                while (stack.Count > 0)
                {
                    var top = stack.Pop();

                    foreach (var child in top.Node.Children)
                    {
                        if (child.Children.Any())
                        {
                            var next = _stateFactory.Next();
                            //_transitions[(top.State, child.Character.Value)] = next;
                            _transitions.Add(top.State, child.Character.Value, next);
                            stack.Push((next, child));
                        }
                        else
                        {
                            //_transitions[(top.State, child.Character.Value)] = final;
                            _transitions.Add(top.State, child.Character.Value, final);
                        }
                    }
                }

                optional = false;
                current = final;
            }
        }
    }
}
