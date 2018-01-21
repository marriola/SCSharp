using SoundChange.Factories;
using SoundChange.Parser;
using SoundChange.Parser.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SoundChange.StateMachines
{
    using FeatureSetDictionary = Dictionary<string, FeatureSetNode>;
    using CategoryDictionary = Dictionary<string, CategoryNode>;

    delegate void MergedStateHandler(State mergingState, MergedState mergedState);

    static class Special
    {
        public static char LAMBDA = '\u2400';

        public static char START = '\u2402';

        public static char END = '\u2403';
    }

    class RuleMachine
    {
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

            BuildSetNodes(rule.Target, dFeatures, dCategories);
            BuildSetNodes(rule.Result, dFeatures, dCategories);
            BuildSetNodes(rule.Environment, dFeatures, dCategories);

            _stateFactory = new StateFactory();
            _mergedStateFactory = new MergedStateFactory(HandleStatesMerged);

            var resultWindow = rule.Result.ToWindow();
            resultWindow.MoveBack();
            BuildNFA(environment.ToWindow(), dFeatures, dCategories, result: resultWindow);
            ConvertToDFA();
        }

        /// <summary>
        /// When the MergedStateFactory merges states, this method is called for each state merged to retarget
        /// any transformations that may have originated from each state to the merged state.
        /// </summary>
        /// <param name="state">The state being merged.</param>
        /// <param name="mergedState">The newly merged state.</param>
        private void HandleStatesMerged(State state, MergedState mergedState)
        {
            var transforms = _transitions.Transforms
                .Where(pair => pair.Key.from == state)
                .ToList();

            foreach (var pair in transforms)
            {
                //_transitions.Transforms.Remove(pair.Key);
                _transitions.Transforms[(mergedState, pair.Key.on)] = pair.Value;
            }
        }

        private void BuildSetNodes(List<Node> nodes, FeatureSetDictionary features, CategoryDictionary categories)
        {
            foreach (var node in nodes)
            {
                if (node is CompoundSetIdentifierNode csiNode)
                {
                    csiNode.Build(features, categories);
                }
            }
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
            var builder = new StringBuilder();

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
                        ? _transitions.Table.Where(x => ms.States.Contains(x.Key.from))
                        : _transitions.Table.Where(x => x.Key.from == top))
                    .GroupBy(x => x.Key.on)
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
                            if (transition.Key.on == Special.LAMBDA)
                            {
                                foreach (var next in _transitions.From(state).Select(x => x.Value).SelectMany(x => x))
                                {
                                    travelStack.Push(next);
                                }
                            }
                            else
                            {
                                if (possibleTransitions.ContainsKey(transition.Key.on))
                                {
                                    foreach (var s in transition.Value)
                                    {
                                        possibleTransitions[transition.Key.on].Add(s);
                                    }
                                }
                                else
                                {
                                    possibleTransitions[transition.Key.on] = new HashSet<State>(transition.Value);
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
                    var follow = _transitions.From(transition.to);

                    if (!follow.Any(x => x.Key.on == Special.LAMBDA))
                        continue;

                    var travelSet = Travel(new List<State> { transition.to });

                    foreach (var t in follow.Where(x => x.Key.on != Special.LAMBDA))
                    {
                        //t.Value.ToList().ForEach(s => travelSet.Add(s));
                        travelSet.Add(t.Key.from);
                    }

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

            dfa.Transforms = _transitions.Transforms;
            _transitions = dfa;
        }

        private HashSet<State> Travel(IEnumerable<State> states)
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
                    if (pair.Key.on == Special.LAMBDA)
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

            return result;
        }

        private State BuildNFA(
            Window<Node> nodes, 
            FeatureSetDictionary features, 
            CategoryDictionary categories, 
            State startNode = null,
            Window<Node> result = null,
            bool transform = false)
        {
            var current = startNode ?? START;

            for (; !nodes.IsOutOfBounds; nodes.MoveNext())
            {
                var node = nodes.Current;
                var resultNode = result?.Current;

                switch (node)
                {
                    case BoundaryNode bNode:
                        MatchCharacter(nodes.AtBeginning
                            ? Special.START
                            : Special.END);
                        break;

                    case UtteranceNode uNode:
                        var uNode_result = resultNode as UtteranceNode;
                        var uResult = uNode_result.Value.ToList().ToWindow();
                        var transformTo = string.Empty;

                        for (int k = 0; k < uNode.Value.Length; k++, uResult.MoveNext())
                        {
                            if (uNode_result == null || uResult.IsOutOfBounds)
                            {
                                result.MoveNext();
                                resultNode = result.Current; //resultNodes[++i_result];
                                uNode_result = resultNode as UtteranceNode;
                                uResult = uNode_result.Value.ToList().ToWindow();
                            }

                            if (uNode_result == null)
                            {
                                throw new ApplicationException("Not enough ");
                            }

                            var c = uNode.Value[k];
                            transformTo = uResult.Current.ToString();

                            //var transformation = k == uNode.Value.Length - 1
                            //    ? new TransformationNode(uNode.Value, transformTo)
                            //    : null;

                            // Apply a transformation if we're done matching the target utterance and there's more result
                            // utterance left, insert a new UtteranceNode to contain the remainder.
                            if (k == uNode.Value.Length - 1 && uResult.Index < uNode_result.Value.Length - 1)
                            {
                                if (uNode_result != null)
                                {
                                    //result.Contents.RemoveAt(result.Index);
                                    //result.Contents.Insert(result.Index, new UtteranceNode(uNode_result.Value.Substring(uResult.Index + 1)));
                                    //result.MoveBack();
                                    result.Contents.Insert(result.Index + 1, new UtteranceNode(uNode_result.Value.Substring(uResult.Index + 1)));
                                }
                            }

                            MatchCharacter(c, transformTo);
                        }
                        break;

                    case SetIdentifierNode fiNode:
                        if (!features.ContainsKey(fiNode.Name))
                        {
                            throw new KeyNotFoundException($"Feature set '{fiNode.Name}' not defined.");
                        }

                        var feature = features[fiNode.Name];

                        MatchSet(fiNode.IsPresent
                            ? feature.PlusTree
                            : feature.MinusTree,
                            resultNode);
                        break;

                    case CompoundSetIdentifierNode csiNode:
                        MatchSet(csiNode.Tree, resultNode);
                        break;

                    case IdentifierNode iNode:
                        if (!categories.ContainsKey(iNode.Name))
                        {
                            throw new KeyNotFoundException($"Category '{iNode.Name}' not defined.");
                        }

                        MatchSet(categories[iNode.Name].BuilderTree, resultNode);
                        break;

                    case OptionalNode oNode:
                        var next = _stateFactory.Next();
                        _transitions.Add(current, Special.LAMBDA, next);

                        var subtree = _stateFactory.Next();
                        _transitions.Add(current, Special.LAMBDA, subtree);

                        var subtreeLast = BuildNFA(oNode.Children.ToWindow(), features, categories, subtree);
                        _transitions.Add(subtreeLast, Special.LAMBDA, next);

                        var final = _stateFactory.Next();
                        _transitions.Add(next, Special.LAMBDA, final);
                        current = final;
                        break;

                    case PlaceholderNode pNode:
                        result.MoveNext();
                        current = BuildNFA(pNode.Children.ToWindow(), features, categories, startNode: current, result: result, transform: true);
                        break;
                }

                if (startNode == null && nodes.AtEnd)
                {
                    current.IsFinal = true;
                }

                if (result?.Index >= 0)
                {
                    result.MoveNext();
                }
            }

            return current;

            TransformationNode Transform(string utterance, Node resultNode, bool add)
            {
                switch (resultNode)
                {
                    case UtteranceNode uResult:
                        return new TransformationNode(utterance, uResult.Value);

                    case CompoundSetIdentifierNode csResult:
                        // TODO implement
                        return null;

                    case SetIdentifierNode sResult:
                        if (sResult.SetType == Lexer.SetType.Category)
                        {
                            // TODO move this check to parser
                            throw new ApplicationException("Category identifier cannot appear in rule result.");
                        }

                        var dic = add
                            ? features[sResult.Name].Additions
                            : features[sResult.Name].Removals;

                        return new TransformationNode(utterance, dic[utterance]);

                    default:
                        throw new ApplicationException($"Cannot transform to {resultNode.GetType().Name}");
                }
            }

            void MatchCharacter(char c, string transformation = null)
            {
                var next = _stateFactory.Next();

                _transitions.Add(current, c, next);

                if (transformation != null)
                {
                    _transitions.Transforms[(current, c)] = transformation;
                }

                current = next;
            }

            void MatchSet(BuilderNode builder, Node transformation = null)
            {
                var final = _stateFactory.Next();
                
                //if (optional && !builder.Character.HasValue)
                //{
                //    _transitions.Add(current, Special.LAMBDA, final);
                //}

                var stack = new Stack<(State State, BuilderNode Node)>();
                stack.Push((current, builder));

                while (stack.Count > 0)
                {
                    var top = stack.Pop();

                    foreach (var child in top.Node.Children)
                    {
                        if (child.Children.Any())
                        {
                            // If a node has no siblings, do not add a transition for its NUL child.
                            //if (child.Children.Count > 1 || child.Children[0].Character != Special.LAMBDA)
                            //{
                            //}
                            var next = _stateFactory.Next();
                            _transitions.Add(top.State, child.Character.Value, next);
                            if (child.IsFinal)
                            {
                                _transitions.Add(top.State, child.Character.Value, final);
                            }
                            stack.Push((next, child));
                        }
                        else
                        {
                            // Leaf node - add transformation to transition table.
                            string transformTo = null;
                            var on = child.Character.Value == Special.LAMBDA
                                ? child.Parent.Character.Value
                                : child.Character.Value;

                            if (transformation is UtteranceNode uNode)
                            {
                                transformTo = uNode.Value;
                            }
                            else if (transformation is SetIdentifierNode siNode)
                            {
                                // Parser guarantees only feature identifiers here.
                                var set = siNode.IsPresent
                                    ? features[siNode.Name].Additions
                                    : features[siNode.Name].Removals;

                                set.TryGetValue(child.Value, out transformTo);
                            }

                            if (child.Character.Value != Special.LAMBDA)
                            {
                                _transitions.Add(top.State, on, final);
                            }

                            if (transformTo != null)
                            {
                                // Apply transformation if one was found.
                                _transitions.Transforms[(top.State, on)] = transformTo;
                            }
                            //else
                            //{
                                //_transitions.Transforms[(top.State, on)] = child.Value;
                            //}
                        }
                    }
                }

                //optional = false;
                current = final;
            }
        }
    }
}
