using SoundChange.Parser;

namespace SoundChange.Factories
{
    class StateFactory
    {
        private int _id = 0;

        public State Next()
        {
            var state = new State("q" + _id);
            ++_id;
            return state;
        }
    }
}
