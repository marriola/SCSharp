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
