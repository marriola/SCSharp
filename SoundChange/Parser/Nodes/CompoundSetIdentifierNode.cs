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

        public CompoundSetIdentifierNode()
        {
        }

        /// <summary>
        /// Creates the builder tree from the available feature sets and categories.
        /// </summary>
        /// <param name="features"></param>
        /// <param name="categories"></param>
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

                        if (Members.Count == 0)
                        {
                            Members = new HashSet<string>(
                                child.IsPresent
                                    ? featureSet.Members.ToList()
                                    : featureSet.Removals.Keys.ToList());
                        }
                        else
                        {
                            Members = new HashSet<string>(
                                child.IsPresent
                                    ? Members.Intersect(featureSet.Removals.Keys)
                                    : Members.Except(featureSet.Members));
                        }

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
