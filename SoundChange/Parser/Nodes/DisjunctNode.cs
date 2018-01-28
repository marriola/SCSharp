using System.Collections.Generic;
using System.Linq;

namespace SoundChange.Parser.Nodes
{
    class DisjunctNode : Node
    {
        public List<List<Node>> Children { get; private set; } = new List<List<Node>>();

        public void AddNode(Node node)
        {
            Children.LastOrDefault().Add(node);
        }

        public void AddChild()
        {
            Children.Add(new List<Node>());
        }

        public override string ToString()
        {
            var lists = Children
                .Select(c => string.Join(string.Empty, c.Select(x => x.ToString())));

            return string.Format("({0})", string.Join("|", lists));
        }
    }
}
