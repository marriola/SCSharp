using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundChange.Nodes
{
    class UtteranceNode : Node
    {
        public string Value { get; private set; }

        public UtteranceNode(string value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return Value;
        }
    }
}
