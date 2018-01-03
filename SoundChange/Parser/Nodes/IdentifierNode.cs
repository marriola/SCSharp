namespace SoundChange.Parser.Nodes
{
    class IdentifierNode : Node
    {
        public string Value { get; private set; }

        public IdentifierNode(string value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return Value;
        }
    }
}
