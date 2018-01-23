using SoundChange.Parser;
using System.Collections;
using System.Collections.Generic;

namespace SoundChange.StateMachines
{
    class StateCollection : IEnumerable<State>
    {
        private HashSet<State> _collection;

        public StateCollection(List<State> collection)
        {
            _collection = new HashSet<State>(collection);
        }

        public void Add(State state)
        {
            _collection.Add(state);
        }

        public IEnumerator<State> GetEnumerator()
        {
            return _collection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _collection.GetEnumerator();
        }

        public static StateCollection From(State singleton)
        {
            return new StateCollection(new List<State> { singleton });
        }

        public override string ToString()
        {
            return string.Format("[{0}]", string.Join(", ", _collection));
        }
    }
}
