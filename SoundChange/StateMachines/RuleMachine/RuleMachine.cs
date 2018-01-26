using SoundChange.Factories;
using SoundChange.Parser;
using SoundChange.Parser.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SoundChange.StateMachines.RuleMachine
{
    using FeatureSetDictionary = Dictionary<string, FeatureSetNode>;
    using CategoryDictionary = Dictionary<string, CategoryNode>;

    delegate void MergedStateHandler(State mergingState, MergedState mergedState);

    static class Special
    {
        public static char LAMBDA = '\u2400';

        public static char START = '\u2402';

        public static char END = '\u2403';

        public static string CONTROL_CHARS = $"{LAMBDA}{START}{END}";
    }

    class RuleMachine
    {
        public static State START = new State("S");

        public RuleNode Rule { get; private set; }

        private static readonly HashSet<char> DIACRITICS_AND_MODIFIERS;

        private TransitionTable _transitions = new TransitionTable();

        private StateFactory _stateFactory;

        private MergedStateFactory _mergedStateFactory;

        static RuleMachine()
        {
            DIACRITICS_AND_MODIFIERS = new HashSet<char>();

            for (var c = '\u02b0'; c <= '\u0341'; c++)
            {
                DIACRITICS_AND_MODIFIERS.Add(c);
            }
        }

        public RuleMachine(RuleNode rule, List<FeatureSetNode> features, List<CategoryNode> categories)
        {
            Rule = rule;
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

            var placeholder = rule.Environment.OfType<PlaceholderNode>().Single();
            placeholder.Children = rule.Target;

            var dFeatures = features.ToDictionary(k => k.Name, v => v);
            var dCategories = categories.ToDictionary(k => k.Name, v => v);

            BuildSetNodes(rule.Target, dFeatures, dCategories);
            BuildSetNodes(rule.Result, dFeatures, dCategories);
            BuildSetNodes(rule.Environment, dFeatures, dCategories);

            _stateFactory = new StateFactory();
            _mergedStateFactory = new MergedStateFactory(HandleStatesMerged);

            BuildNFA(environment, rule.Result, dFeatures, dCategories);
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

            var defaultTransforms = _transitions.DefaultTransforms
                .Where(pair => pair.Key == state)
                .ToList();

            foreach (var pair in defaultTransforms)
            {
                _transitions.DefaultTransforms[mergedState] = pair.Value;
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
        public string ApplyTo(string word, out List<string> transformationsApplied)
        {
            var current = START;
            var nextWord = string.Empty;
            var builder = new StringBuilder();
            var undoBuilder = new StringBuilder();
            
            var isTransformationApplied = false;
            var transformationsInProgress = new List<string>();
            transformationsApplied = new List<string>();

            word = Special.START + word + Special.END;

            foreach (var c in word)
            {
                var transition = _transitions.GetFirst(current, c);
                var key = (current, c);

                // If the current state is a final state, flush the builder to nextWord and
                // go to START, but only if we are:
                //   * not about to transition to another final state, and
                //   * not about to transition on a diacritic or modifier.
                if (current.IsFinal && !DIACRITICS_AND_MODIFIERS.Contains(c) && transition?.IsFinal != true)
                {
                    current = START;

                    nextWord += builder.ToString();
                    builder.Clear();
                    transformationsApplied.AddRange(transformationsInProgress);
                    transformationsInProgress.Clear();
                    isTransformationApplied = false;
                }

                // If there is a transformation available, add the result to builder and save the input symbol to undoBuilder.
                var hasTransform = _transitions.Transforms.TryGetValue(key, out Transformation transform);
                var hasDefaultTransform = _transitions.DefaultTransforms.TryGetValue(key.current, out Transformation defaultTransform);

                if ((transform ?? defaultTransform) is Transformation t)
                {
                    if (builder.Length > 0)
                    {
                        nextWord += builder.ToString();
                        builder.Clear();
                    }

                    builder.Append(t.Result);
                    undoBuilder.Append(c);
                    t.FromLiteral = undoBuilder.ToString();

                    isTransformationApplied = true;
                    if (!(t is NullTransformation))
                        transformationsInProgress.Add(t.ToString());
                }

                // Add the character to the builder if no transformation was applied, or if a default
                // transformation was applied (in which case we want to keep the input symbol, since
                // it isn't part of the transformation).
                // If the latter is not the case, also add the input symbol to the undoBuilder.
                if ((!hasTransform || hasDefaultTransform) && !Special.CONTROL_CHARS.Contains(c))
                {
                    builder.Append(c);
                    if (!hasDefaultTransform && undoBuilder.Length > 0)
                        undoBuilder.Append(c);
                }

                // If no transition was possible, return to START. If a transformation was partially
                // applied:
                //   * undo it if it was a normal transformation, or
                //   * commit it if it was a default transformation.
                if (transition == null)
                {
                    current = START;

                    if (isTransformationApplied)
                    {
                        if (hasDefaultTransform)
                        {
                            nextWord += builder.ToString();
                        }
                        else
                        {
                            nextWord += undoBuilder.ToString();
                        }
                        builder.Clear();
                        undoBuilder.Clear();
                        transformationsInProgress.Clear();
                        isTransformationApplied = false;
                    }
                }
                else
                {
                    current = transition;
                }
            }

            return nextWord + builder.ToString();
        }

        private void ConvertToDFA()
        {
            var dfa = new TransitionTable();
            dfa.DefaultTransforms = _transitions.DefaultTransforms;

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
                    // Follow each lambda transition to the first non-lambda transition, and add each to possibleTransitions.
                    var travelStack = new Stack<State>();

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
                    // We'll handle lambda transitions in the next step.
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
                        groupedTransitions.Add((top, key, charStates.First()));
                    }
                }

                // Replace each transition to a state that has lambda transitions with a transition to
                // a merged set of states that has no lambda transitions.
                foreach (var transition in new List<(State from, char on, State to)>(groupedTransitions))
                {
                    var follow = _transitions.From(transition.to);

                    if (!follow.Any(x => x.Key.on == Special.LAMBDA))
                        continue;
                    
                    // Compute travel set of states that have transitions on lambda.
                    var travelSet = Travel(new List<State> { transition.to });

                    // Add back in any non-lambda transitions we can take from the origin state.
                    foreach (var t in follow.Where(x => x.Key.on != Special.LAMBDA))
                    {
                        travelSet.Add(t.Key.from);
                    }

                    // Merge the travel set into a new state and retarget the original transition to it.
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

            HashSet<State> Travel(IEnumerable<State> states)
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
        }

        private State BuildNFA(List<Node> nodes, List<Node> result, FeatureSetDictionary features, CategoryDictionary categories)
        {
            // Move the result window back one place. Just before we begin iterating over the
            // placeholder node, the window will be advanced to the beginning and begin moving.
            var resultWindow = result.ToWindow();
            resultWindow.MoveBack();

            return BuildNFAInternal(nodes.ToWindow(), resultWindow, START, features, categories);
        }

        State BuildNFAInternal(
            Window<Node> input,
            Window<Node> result,
            State startNode,
            FeatureSetDictionary features, 
            CategoryDictionary categories,
            Window<Node> outerNodes = null,
            bool inTargetSection = false)
        {
            var current = startNode;

            for (; !input.IsOutOfBounds; input.MoveNext())
            {
                var node = input.Current;
                var resultNode = result?.Current;

                switch (node)
                {
                    case BoundaryNode bNode:
                        MatchCharacter(input.AtBeginning
                            ? Special.START
                            : Special.END,
                            node);
                        break;

                    case UtteranceNode uNode:
                        TransformUtterance(uNode, resultNode);
                        break;

                    case SetIdentifierNode fiNode:
                        if (!features.ContainsKey(fiNode.Name))
                        {
                            throw new KeyNotFoundException($"Feature set '{fiNode.Name}' not defined.");
                        }

                        var feature = features[fiNode.Name];

                        current = TransformSet(fiNode.IsPresent
                            ? feature.PlusTree
                            : feature.MinusTree,
                            node,
                            resultNode);
                        break;

                    case CompoundSetIdentifierNode csiNode:
                        current = TransformSet(csiNode.Tree, node, resultNode);
                        break;

                    case IdentifierNode iNode:
                        if (!categories.ContainsKey(iNode.Name))
                        {
                            throw new KeyNotFoundException($"Category '{iNode.Name}' not defined.");
                        }

                        current = TransformSet(categories[iNode.Name].BuilderTree, node, resultNode);
                        break;

                    case OptionalNode oNode:
                        current = MatchOptional(oNode);
                        break;

                    case PlaceholderNode pNode:
                        // Advance the result window to the beginning.
                        result.MoveNext();
                        current = BuildNFAInternal(pNode.Children.ToWindow(), result, current, features, categories, input, inTargetSection: true);
                        break;
                }

                if ((outerNodes == null || outerNodes.AtEnd) && input.AtEnd)
                {
                    current.IsFinal = true;
                }

                if (result?.Index >= 0)
                {
                    result.MoveNext();
                }
            }

            return current;

            State MatchOptional(OptionalNode oNode)
            {
                var next = _stateFactory.Next(inTargetSection);
                _transitions.Add(current, Special.LAMBDA, next);

                var subtree = _stateFactory.Next(inTargetSection);
                _transitions.Add(current, Special.LAMBDA, subtree);

                var subtreeLast = BuildNFAInternal(oNode.Children.ToWindow(), result, subtree, features, categories, input);
                _transitions.Add(subtreeLast, Special.LAMBDA, next);

                var final = _stateFactory.Next(inTargetSection);
                _transitions.Add(next, Special.LAMBDA, final);

                return final;
            }

            void TransformUtterance(UtteranceNode targetNode, Node resultNode)
            {
                var uResult = CreateWindowOver(resultNode, targetNode);
                var original = string.Empty;
                var transformTo = string.Empty;

                for (int k = 0; k < targetNode.Value.Length; k++, uResult?.MoveNext())
                {
                    Transformation transformation = null;

                    if (uResult != null)
                    {
                        if (uResult == null || uResult.IsOutOfBounds)
                        {
                            result.MoveNext();
                            resultNode = result.Current;
                            uResult = CreateWindowOver(resultNode, targetNode);
                        }

                        transformTo += uResult?.Current.ToString();

                        // Apply a transformation if we're done matching the target utterance and there's more result
                        // utterance left, insert a new UtteranceNode to contain the remainder.

                        if (k == targetNode.Value.Length - 1)
                        {
                            if (uResult.HasNext)
                            {
                                var remainder = string.Join(string.Empty, uResult.Contents).Substring(uResult.Index + 1);

                                if (input.HasNext)
                                {
                                    result.Insert(result.Index + 1, new UtteranceNode(remainder));
                                }
                                else
                                {
                                    transformTo += remainder;
                                }
                            }

                            transformation = new Transformation(transformTo, targetNode, new UtteranceNode(transformTo));
                            transformTo = string.Empty;
                        }
                    }
                    else if (inTargetSection)
                    {
                        // Still in placeholder, but either have no result or have run out of result.
                        transformation = new NullTransformation();
                    }
                    else
                    {
                        transformTo += targetNode.Value[k];

                        if (k == targetNode.Value.Length - 1)
                        {
                            transformation = new Transformation(transformTo, targetNode, new UtteranceNode(transformTo));
                            transformTo = string.Empty;
                        }
                    }

                    MatchCharacter(targetNode.Value[k], targetNode, transformation);
                }
            }

            Window<char> CreateWindowOver(Node node, UtteranceNode targetNode)
            {
                if (node == null)
                {
                    return null;
                }

                switch (node)
                {
                    case UtteranceNode uNode_result:
                        return uNode_result.Value.ToWindow();

                    case FeatureSetIdentifierNode fsiNode_result:
                        var featureSet = features[fsiNode_result.Name];

                        var subset = fsiNode_result.IsPresent
                            ? featureSet.Additions
                            : featureSet.Removals;

                        subset.TryGetValue(targetNode.Value, out string transformResult);
                        return transformResult.ToWindow();

                    default:
                        throw new ApplicationException(node.GetType().Name + " is not valid in result section.");
                }
            }

            void MatchCharacter(char c, Node targetNode, Transformation transformation = null)
            {
                var next = _stateFactory.Next(inTargetSection);

                _transitions.Add(current, c, next);

                if (transformation != null)
                {
                    _transitions.Transforms[(current, c)] = transformation;
                }

                current = next;
            }

            State TransformSet(BuilderNode builder, Node targetNode, Node transformation = null)
            {
                var final = _stateFactory.Next(inTargetSection);

                var stack = new Stack<(State State, BuilderNode Node)>();
                stack.Push((current, builder));

                while (stack.Count > 0)
                {
                    var top = stack.Pop();

                    foreach (var child in top.Node.Children)
                    {
                        if (child.Children.Any())
                        {
                            var next = _stateFactory.Next(inTargetSection);

                            _transitions.Add(top.State, child.Character.Value, next);

                            if (inTargetSection)
                                _transitions.Transforms[(top.State, child.Character.Value)] = new NullTransformation();

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
                                if (child.Character.Value == Special.LAMBDA)
                                {
                                    _transitions.DefaultTransforms[top.State] = new Transformation(transformTo, targetNode, transformation);
                                }
                                else
                                {
                                    _transitions.Transforms[(top.State, on)] = new Transformation(transformTo, targetNode, transformation);
                                }
                            }
                        }
                    }
                }

                return final;
            }
        }
    }
}
