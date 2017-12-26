namespace SoundChange
{
    class State
    {
        public Token Token { get; private set; }

        public State()
        {
        }

        public State(Token token)
        {
            Token = token;
        }
    }
}
