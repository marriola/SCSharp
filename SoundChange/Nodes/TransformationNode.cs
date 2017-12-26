using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundChange.Nodes
{
    class TransformationNode : Node
    {
        public string From { get; private set; }

        public string To { get; private set; }

        public TransformationNode(string from, string to)
        {
            From = from;
            To = to;
        }

        public override string ToString()
        {
            return $"{From} => {To}";
        }
    }
}
