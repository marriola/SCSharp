using SoundChange.Lexer;

namespace SoundChange.Parser
{
    class State
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public virtual string Name { get; protected set; }

        /// <summary>
        /// Gets or sets a value indicating whether this state is a final state.
        /// </summary>
        public bool IsFinal { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this state matches part of the result section.
        /// </summary>
        public bool IsTarget { get; set; }

        /// <summary>
        /// Gets or sets the token.
        /// </summary>
        public Token Token { get; private set; }

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
