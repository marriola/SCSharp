using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundChange.Nodes
{
    class FeatureSetNode : Node
    {
        public string Name { get; private set; }

        public HashSet<string> Members { get; private set; }

        public Dictionary<string, string> Additions { get; private set; } = new Dictionary<string, string>();

        public Dictionary<string, string> Removals { get; private set; } = new Dictionary<string, string>();

        public FeatureSetNode(string name, HashSet<string> members = null, List<(string from, string to)> transitions = null)
        {
            Name = name;
            Members = members;

            foreach (var transition in transitions)
            {
                Additions[transition.from] = transition.to;
                Removals[transition.to] = transition.from;
            }
        }

        public override string ToString()
        {
            return $"[{Name}] {{ {string.Join(string.Empty, Members.ToList())} }}";
        }
    }
}
