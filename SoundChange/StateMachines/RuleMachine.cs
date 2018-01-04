using SoundChange.Factories;
using SoundChange.Parser;
using SoundChange.Parser.Nodes;
using System.Collections.Generic;
using System.Linq;

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

        private StateFactory _stateFactory;

        private MergedStateFactory _mergedStateFactory;

        public RuleMachine(RuleNode rule, List<FeatureSetNode> features, List<CategoryNode> categories)
        {
            var environment = rule.Environment;

            // Remove optional nodes from the end
            for (var i = environment.Count - 1; i >= 0; --i)
            {
                if (!(environment[i] is OptionalNode))
                {
                    environment.RemoveRange(i + 1, environment.Count - i - 1);
                    break;
                }
            }

            var placeholder = rule.Environment.Single(x => x is PlaceholderNode) as PlaceholderNode;
            placeholder.Children = rule.Target;

            var dFeatures = features.ToDictionary(k => k.Name, v => v);
            var dCategories = categories.ToDictionary(k => k.Name, v => v);

            _stateFactory = new StateFactory();
            _mergedStateFactory = new MergedStateFactory();
            BuildNFA(environment, dFeatures, dCategories);
            ConvertToDFA();
        }

        /// <summary>
        /// Applies the rule to a word.
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        public string ApplyTo(string word)
        {
            // TODO currently only recognizes matching words; must also apply transformation.
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

        private void ConvertToDFA()
        {
            var dfa = new TransitionTable();
            var stack = new Stack<State>();
            stack.Push(START);

            while (stack.Count > 0)
            {
                var top = stack.Pop();

                // Gather all possible transitions, creating merged states grouped by input symbol if necessary.
                var possibleTransitions =
                    (top is MergedState ms
                        ? _transitions.Table.Where(x => ms.States.Contains(x.Key.Item1))
                        : _transitions.Table.Where(x => x.Key.Item1 == top))
                    .GroupBy(x => x.Key.Item2)
                    .ToDictionary(k => k.Key, v => new HashSet<State>(v.SelectMany(x => x.Value).Distinct()));

                if (possibleTransitions.ContainsKey(Special.LAMBDA))
                {
                    var travelStack = new Stack<State>();

                    // Follow each lambda transition to the first non-lambda transition, and add each to possibleTransitions.
                    foreach (var state in Travel(possibleTransitions[Special.LAMBDA]))
                    {
                        travelStack.Push(state);
                    }

                    while (travelStack.Count > 0)
                    {
                        var state = travelStack.Pop();

                        var transitions = _transitions.From(state);
                        foreach (var transition in transitions)
                        {
                            // TODO verify handling of states with mixed lambda and non-lambda transitions works.
                            if (transition.Key.Item2 == Special.LAMBDA)
                            {
                                foreach (var next in _transitions.From(state).Select(x => x.Value).SelectMany(x => x))
                                {
                                    travelStack.Push(next);
                                }
                            }
                            else
                            {
                                if (possibleTransitions.ContainsKey(transition.Key.Item2))
                                {
                                    foreach (var s in transition.Value)
                                    {
                                        possibleTransitions[transition.Key.Item2].Add(s);
                                    }
                                }
                                else
                                {
                                    possibleTransitions[transition.Key.Item2] = new HashSet<State>(transition.Value);
                                }
                            }
                        }

                    }

                    possibleTransitions.Remove(Special.LAMBDA);
                }

                // Group transitions by input symbol and merge states that can be reached by the same symbol.
                var groupedTransitions = new List<(State from, char on, State to)>();

                foreach (var key in possibleTransitions.Keys)
                {
                    if (key == Special.LAMBDA)
                        continue;

                    var charStates = possibleTransitions[key];

                    if (charStates.Count > 1)
                    {
                        var mergedState = _mergedStateFactory.Merge(charStates);
                        groupedTransitions.Add((top, key, mergedState));
                    }
                    else
                    {
                        groupedTransitions.Add((top, key, charStates.FirstOrDefault()));
                    }
                }

                // Follow states with lambda transitions to the end and replace them with non-lambdas.
                foreach (var transition in new List<(State from, char on, State to)>(groupedTransitions))
                {
                    if (!_transitions.From(transition.to).Any(x => x.Key.Item2 == Special.LAMBDA))
                        continue;

                    var travelSet = Travel(new List<State> { transition.to });
                    groupedTransitions.Remove(transition);
                    groupedTransitions.Add((transition.from, transition.on, _mergedStateFactory.Merge(travelSet)));
                }

                // Add states to the DFA
                foreach (var transition in groupedTransitions)
                {
                    dfa.Add(transition.from, transition.on, transition.to);
                    if (!stack.Contains(transition.to))
                    {
                        stack.Push(transition.to);
                    }
                }
            }

            _transitions = dfa;
        }

        private List<State> Travel(IEnumerable<State> states)
        {
            var result = new HashSet<State>(states);
            var travelStack = new Stack<State>();

            foreach (var state in states)
            {
                travelStack.Push(state);
            }

            // Follow lambda transitions up to states that have non-lambda transitions.
            while (travelStack.Count > 0)
            {
                var top = travelStack.Pop();

                foreach (var pair in _transitions.From(top))
                {
                    if (pair.Key.Item2 == Special.LAMBDA)
                    {
                        result.Remove(top);
                        foreach (var state in pair.Value)
                        {
                            travelStack.Push(state);
                        }
                    }
                    else
                    {
                        result.Add(top);
                        continue;
                    }
                }
            }

            return result.ToList();
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
                    var next = _stateFactory.Next();
                    _transitions.Add(current, Special.LAMBDA, next);

                    var subtree = _stateFactory.Next();
                    _transitions.Add(current, Special.LAMBDA, subtree);

                    var subtreeLast = BuildNFA(oNode.Children, features, categories, subtree);
                    _transitions.Add(subtreeLast, Special.LAMBDA, next);

                    var final = _stateFactory.Next();
                    _transitions.Add(next, Special.LAMBDA, final);
                    current = final;
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

                _transitions.Add(current, c, next);
                if (optional)
                {
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
                            _transitions.Add(top.State, child.Character.Value, next);
                            stack.Push((next, child));
                        }
                        else
                        {
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
