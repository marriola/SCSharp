using System.Collections.Generic;
using System.Linq;

namespace SoundChange.Parser.Nodes
{
    /// <summary>
    /// Represents the intersection of a list of feature sets and categories.
    /// </summary>
    class CompoundSetIdentifierNode : Node
    {
        /// <summary>
        /// Gets or sets the list of feature set and category identifiers.
        /// </summary>
        public List<SetIdentifierNode> Children { get; private set; } = new List<SetIdentifierNode>();

        /// <summary>
        /// Gets or sets the set of segments present in the intersection of sets.
        /// </summary>
        public HashSet<string> Members { get; private set; }

        /// <summary>
        /// Gets or sets the builder tree for segments that have this feature.
        /// </summary>
        public BuilderNode Tree { get; private set; }

        /// <summary>
        /// Gets or sets the builder tree for segments that do not have this feature.
        /// </summary>
        //public BuilderNode MinusTree { get; private set; }

        public CompoundSetIdentifierNode()
        {
        }

        public void Build(Dictionary<string, FeatureSetNode> features = null, Dictionary<string, CategoryNode> categories = null)
        {
            if (Members != null)
            {
                return;
            }

            Members = new HashSet<string>();

            foreach (var child in Children)
            {
                var memberCount = Members.Count;

                switch (child)
                {
                    case FeatureSetIdentifierNode fsiNode:
                        var featureSet = features[child.Name];
                        var subset = child.IsPresent
                            ? featureSet.Removals
                            : featureSet.Additions;

                        if (Members.Count == 0)
                        {
                            Members = new HashSet<string>(subset.Keys);
                        }
                        else
                        {
                            Members = new HashSet<string>(Members.Intersect(subset.Keys));
                        }

                        //foreach (var pair in subset)
                        //{
                        //    if (memberCount > 0 && !Members.Contains(pair.Key))
                        //        continue;

                        //    Members.Add(pair.Key);
                        //}

                        break;

                    case CategoryIdentifierNode ciNode:
                        var category = categories[child.Name].Members;

                        if (Members.Count == 0)
                        {
                            Members = new HashSet<string>(category);
                        }
                        else
                        {
                            Members = new HashSet<string>(Members.Intersect(category));
                        }

                        //foreach (var key in categories[child.Name].Members)
                        //{
                        //    if (memberCount > 0 && !Members.Contains(key))
                        //        continue;

                        //    Members.Add(key);
                        //}
                        break;
                }
            }

            Tree = BuilderNode.TreeFrom(Members);
        }

        public override string ToString()
        {
            return "[" + string.Join(string.Empty, Children.Select(x => RemoveBrackets(x.ToString()))) + "]";
        }

        private string RemoveBrackets(string s)
        {
            return s[0] == '['
                ? s.Substring(1, s.Length - 2)
                : s;
        }
    }
}
