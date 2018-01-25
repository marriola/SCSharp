namespace SoundChange.Parser.Nodes
{
    class IdentifierNode : Node
    {
        public string Name { get; protected set; }

        public IdentifierNode(string name)
        {
            // Cut off trailing disambiguation dot
            if (name.EndsWith("."))
            {
                name = name.Substring(0, name.Length - 1);
            }

            Name = name;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
