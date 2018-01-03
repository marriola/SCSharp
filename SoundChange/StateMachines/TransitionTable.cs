using SoundChange.Parser;
using System.Collections.Generic;
using System.Linq;

namespace SoundChange.StateMachines
{
    class TransitionTable
    {
        public Dictionary<(State, char), List<State>> Table { get; set; } = new Dictionary<(State, char), List<State>>();

        public bool Contains((State, char) key)
        {
            return Table.ContainsKey(key);
        }

        public void Add(State from, char c, State to)
        {
            var key = (from, c);

            if (Table.ContainsKey(key))
            {
                Table[key].Add(to);
            }
            else
            {
                Table[key] = new List<State> { to };
            }
        }

        public void Add(State from, char c, IEnumerable<State> to)
        {
            if (to.Count() == 0)
            {
                return;
            }

            foreach (var state in to)
            {
                Add(from, c, state);
            }
        }

        public IEnumerable<State> GetAll(State state, char c)
        {
            var key = (state, c);

            return Table.ContainsKey(key)
                ? Table[key]
                : null;
        }

        public IEnumerable<KeyValuePair<(State, char), List<State>>> From(State state)
        {
            if (state is MergedState ms)
            {
                return Table.Where(x => ms.States.Contains(x.Key.Item1));
            }
            else
            {
                return Table.Where(x => x.Key.Item1 == state);
            }
        }

        public IEnumerable<(char, List<State>)> GetAll(State state)
        {
            return Table
                .Where(x => x.Key.Item1 == state)
                .Select(x => (x.Key.Item2, Table[x.Key]))
                .ToList();
        }

        public State GetFirst(State state, char c)
        {
            return GetAll(state, c)?.FirstOrDefault();
        }
    }
}
