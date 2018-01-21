namespace SoundChange.Parser.Nodes
{
    class SetIdentifierNode : IdentifierNode
    {
        public bool IsPresent { get; protected set; }

        public SetIdentifierNode(bool isPresent, string name)
            : base(name)
        {
            IsPresent = isPresent;
        }
    }

    class FeatureSetIdentifierNode : SetIdentifierNode
    {
        public FeatureSetIdentifierNode(bool isPresent, string name)
            : base(isPresent, name)
        {
        }

        public override string ToString()
        {
            var sign = IsPresent ? "+" : "-";
            return $"[{sign}{Name}]";
        }
    }

    class CategoryIdentifierNode : SetIdentifierNode
    {
        public CategoryIdentifierNode(string name)
            : base(true, name)
        {
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
