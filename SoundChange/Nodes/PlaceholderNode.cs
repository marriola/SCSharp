using System.Collections.Generic;

namespace SoundChange.Nodes
{
    class PlaceholderNode : Node
    {
        public List<Node> Children { get; set; }

        public override string ToString()
        {
            return "_";
        }
    }
}
