using System.Collections.Generic;
using System.Linq;

namespace SoundChange.Parser.Nodes
{
    class DisjunctNode : Node
    {
        public List<List<Node>> Branches { get; private set; } = new List<List<Node>>();

        public void AddNode(Node node)
        {
            Branches.LastOrDefault().Add(node);
        }

        public void AddBranch()
        {
            Branches.Add(new List<Node>());
        }

        public override string ToString()
        {
            var lists = Branches
                .Select(c => string.Join(string.Empty, c.Select(x => x.ToString())));

            return string.Format("({0})", string.Join("|", lists));
        }
    }
}
