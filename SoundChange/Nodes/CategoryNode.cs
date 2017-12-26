using System.Collections.Generic;
using System.Linq;

namespace SoundChange.Nodes
{
    class CategoryNode : Node
    {
        public string Name { get; private set; }

        public HashSet<string> Members { get; private set; }

        public CategoryNode(string name, HashSet<string> members)
        {
            Name = name;
            Members = members;
        }

        public override string ToString()
        {
            return $"{Name} {{ {string.Join(string.Empty, Members.ToList())} }}";
        }
    }
}
