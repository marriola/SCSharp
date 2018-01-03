using System.Collections.Generic;
using System.Linq;

namespace SoundChange.Parser.Nodes
{
    class OptionalNode : Node
    {
        public List<Node> Children { get; private set; }

        public OptionalNode(List<Node> children = null)
        {
            Children = children ?? new List<Node>();
        }

        public override string ToString()
        {
            return string.Format("({0})", string.Join(string.Empty, Children.Select(x => x.ToString())));
        }
    }
}
