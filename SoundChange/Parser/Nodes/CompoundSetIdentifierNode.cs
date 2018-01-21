using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundChange.Parser.Nodes
{
    class CompoundSetIdentifierNode : Node
    {
        public List<SetIdentifierNode> Children { get; private set; } = new List<SetIdentifierNode>();

        public HashSet<string> Members { get; private set; }

        public Dictionary<string, string> Additions { get; private set; } = new Dictionary<string, string>();

        public Dictionary<string, string> Removals { get; private set; } = new Dictionary<string, string>();

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

                if (child.SetType == Lexer.SetType.Feature)
                {
                    var featureSet = features[child.Name];

                    foreach (var pair in featureSet.Removals)
                    {
                        if (memberCount > 0 && !Members.Contains(pair.Key))
                            continue;

                        Removals[pair.Key] = pair.Value;
                        Members.Add(pair.Key);
                    }

                    foreach (var pair in featureSet.Additions)
                    {
                        if (memberCount > 0 && !Members.Contains(pair.Key))
                            continue;

                        Additions[pair.Key] = pair.Value;
                        Members.Add(pair.Key);
                    }
                }
                else if (child.SetType == Lexer.SetType.Category)
                {
                    foreach (var key in categories[child.Name].Members)
                    {
                        if (memberCount > 0 && !Members.Contains(key))
                            continue;

                        Members.Add(key);
                    }
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
