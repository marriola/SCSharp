namespace SoundChange.Nodes
{
    class FeatureIdentifierNode : Node
    {
        public bool IsPresent { get; private set; }

        public string Name { get; private set; }

        public FeatureIdentifierNode(bool isPresent, string name)
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
