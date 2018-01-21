using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SoundChange.Parser.Nodes
{
    class RuleNode : Node
    {
        public List<Node> Target { get; private set; }

        public List<Node> Result { get; private set; }

        public List<Node> Environment { get; private set; }

        public RuleNode(List<Node> target, List<Node> result, List<Node> environment)
        {
            Target = target;
            Result = result;
            Environment = environment;
        }

        /// <summary>
        /// Breaks up and collapses UtteranceNodes where possible to fit them to members of the given feature sets and categories.
        /// </summary>
        public void FitUtterancesToKeys(List<FeatureSetNode> features, List<CategoryNode> categories)
        {
            var dFeatures = features.ToDictionary(k => k.Name, v => v);
            var dCategories = categories.ToDictionary(k => k.Name, v => v);

            var keys = dFeatures.SelectMany(x => x.Value.Additions.Keys)
                .Concat(dFeatures.SelectMany(x => x.Value.Removals.Keys))
                .Concat(dCategories.SelectMany(x => x.Value.Members))
                .Distinct()
                .OrderByDescending(x => x.Length)
                .ToList();

            Result = MaximizeUtterances_Internal(Result);
            Target = MaximizeUtterances_Internal(Target);

            List<Node> MaximizeUtterances_Internal(List<Node> nodes)
            {
                var result = new List<Node>();
                var builder = string.Empty;

                while (nodes.Count > 0)
                {
                    var top = nodes[0];
                    nodes.RemoveAt(0);

                    //if (result.Count == 0)
                    //{
                    //    result.Add(top);
                    //}
                    if (top is OptionalNode oNode)
                    {
                        //result.AddRange(MaximizeUtterances_Internal(oNode.Children));
                        result.Add(new OptionalNode(MaximizeUtterances_Internal(oNode.Children)));
                    }
                    else if (top is UtteranceNode uNode_target)
                    {
                        // Accumulate utterance node contents and match the longest keys possible.
                        while (true)
                        {
                            builder += uNode_target.Value;
                            var segment = CanBreak(builder);
                            if (segment != null)
                            {
                                if (segment.Value.at > 0)
                                {
                                    // If the match is in the middle of the builder, add non-matching part to the result.
                                    result.Add(new UtteranceNode(builder.Substring(0, segment.Value.at)));
                                }
                                result.Add(new UtteranceNode(segment.Value.segment));
                                builder = builder.Substring(segment.Value.segment.Length);
                            }

                            // Keep going until the next node isn't an utterance node.
                            if (nodes.Count == 0 || !(nodes[0] is UtteranceNode))
                                break;

                            uNode_target = nodes[0] as UtteranceNode;
                            nodes.RemoveAt(0);
                        }

                        if (builder.Length > 0)
                        {
                            result.Add(new UtteranceNode(builder));
                            builder = string.Empty;
                        }
                    }
                    //else if (top is CompoundSetIdentifierNode csiNode)
                    //{

                    //}
                    //else if (top is SetIdentifierNode siNode)
                    //{

                    //}
                    else
                    {
                        result.Add(top);
                    }
                }

                return result;
            }

            /// <summary>
            /// Determines the longest key that can be matched to the builder.
            /// </summary>
            (int at, string segment)? CanBreak(string builder)
            {
                int? lowestIndex = int.MaxValue;
                string matchingKey = null;

                foreach (var key in keys)
                {
                    var index = builder.IndexOf(key);
                    if (index != -1 && index < lowestIndex)
                    {
                        lowestIndex = index;
                        matchingKey = key;
                        //return (index, key);
                    }
                }

                if (matchingKey == null)
                {
                    return null;
                }
                else
                {
                    return (lowestIndex.Value, matchingKey);
                }
            }
        }

        //private List<Node> CollapseUtteranceNodes(List<Node> nodes)
        //{
        //    var result = new List<Node>();
        //    var builder = new StringBuilder();

        //    while (nodes.Count > 0)
        //    {
        //        var top = nodes[0];
        //        nodes.RemoveAt(0);

        //        if (result.Count == 0)
        //        {
        //            result.Add(top);
        //        }
        //        else if (top is OptionalNode oNode)
        //        {
        //            result.AddRange(CollapseUtteranceNodes(oNode.Children));
        //        }
        //        else if (result.Last() is UtteranceNode uNode_result && top is UtteranceNode uNode_target)
        //        {
        //            do
        //            {
        //                builder.Append(uNode_target.Value);
        //                uNode_target = nodes[0] as UtteranceNode;
        //                nodes.RemoveAt(0);
        //            } while (uNode_target != null);

        //            result.RemoveAt(result.Count - 1);
        //            result.Add(new UtteranceNode(builder.ToString()));
        //            builder.Clear();
        //        }
        //        else
        //        {
        //            result.Add(top);
        //        }
        //    }

        //    return result;
        //}

        public override string ToString()
        {
            var target = string.Join(string.Empty, Target.Select(x => x.ToString()));
            var result = string.Join(string.Empty, Result.Select(x => x.ToString()));
            var environment = string.Join(string.Empty, Environment.Select(x => x.ToString()));

            return string.Format("{0}/{1}/{2}", target, result, environment);
        }
    }
}
