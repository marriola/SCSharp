using System.Collections.Generic;
using System.Linq;

namespace SoundChange.Parser.Nodes
{
    class FeatureSetNode : Node
    {
        public string Name { get; private set; }

        public HashSet<string> Members { get; private set; }

        public Dictionary<string, string> Additions { get; private set; } = new Dictionary<string, string>();

        public Dictionary<string, string> Removals { get; private set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the builder tree for segments that have this feature.
        /// </summary>
        public BuilderNode PlusTree { get; private set; }

        /// <summary>
        /// Gets or sets the builder tree for segments that do not have this feature.
        /// </summary>
        public BuilderNode MinusTree { get; private set; }

        public FeatureSetNode(string name, HashSet<string> members = null, List<(string from, string to)> transitions = null)
        {
            Name = name;
            Members = members;

            foreach (var transition in transitions)
            {
                Additions[transition.from] = transition.to;
                Removals[transition.to] = transition.from;
                Members.Add(transition.to);
            }

            PlusTree = BuilderNode.TreeFrom(Members);
            MinusTree = BuilderNode.TreeFrom(Additions.Keys);
        }

        public override string ToString()
        {
            return $"[{Name}] {{ {string.Join(string.Empty, Members.ToList())} }}";
        }
    }
}
