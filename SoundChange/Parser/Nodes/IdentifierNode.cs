namespace SoundChange.Parser.Nodes
{
    class IdentifierNode : Node
    {
        public string Name { get; protected set; }

        public IdentifierNode(string name)
        {
            Name = name;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
