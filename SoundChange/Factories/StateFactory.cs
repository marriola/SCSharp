using SoundChange.Parser;

namespace SoundChange.Factories
{
    class StateFactory
    {
        private int _id = 0;

        public State Next(bool isTarget = false)
        {
            var state = new State("q" + _id);
            state.IsTarget = isTarget;
            ++_id;
            return state;
        }
    }
}
