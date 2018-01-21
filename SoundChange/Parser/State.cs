using SoundChange.Lexer;
using SoundChange.Parser.Nodes;

namespace SoundChange.Parser
{
    class State
    {
        public virtual string Name { get; protected set; }

        public bool IsFinal { get; set; }

        public Token Token { get; private set; }

        public TransformationNode Transformation { get; set; }

        public State(string name = null)
        {
            Name = name;
        }

        public State(Token token)
        {
            Name = token.Type.ToString();
            Token = token;
        }

        public override string ToString()
        {
            return IsFinal
                ? "(" + Name + ")"
                : Name;
        }
    }
}
