using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundChange.Nodes
{
    class FeatureSetIdentifierNode : Node
    {
        public bool IsPresent { get; private set; }

        public string Name { get; private set; }

        public FeatureSetIdentifierNode(bool isPresent, string name)
        {
            IsPresent = isPresent;
            Name = name;
        }

        public override string ToString()
        {
            return $"[{( IsPresent ? "+" : "-" )}{Name}]";
        }
    }
}
