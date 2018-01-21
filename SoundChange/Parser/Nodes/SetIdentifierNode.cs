using SoundChange.Lexer;

namespace SoundChange.Parser.Nodes
{
    class SetIdentifierNode : IdentifierNode
    {
        public SetType SetType { get; private set; }

        public bool IsPresent { get; private set; }

        public SetIdentifierNode(bool isPresent, string name, SetType setType)
            : base(name)
        {
            IsPresent = isPresent;
            SetType = setType;
        }

        public override string ToString()
        {
            if (SetType == SetType.Feature)
            {
                var sign = IsPresent ? "+" : "-";
                return $"[{sign}{Name}]";
            }
            else
            {
                return Name;
            }
        }
    }
}
