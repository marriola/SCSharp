using SoundChange.Parser;
using SoundChange.StateMachines.RuleMachine;
using System.Collections.Generic;
using System.Linq;

namespace SoundChange.StateMachines
{
    class TransitionTable
    {
        public Dictionary<(State from, char on), StateCollection> Table { get; set; } = new Dictionary<(State from, char on), StateCollection>();

        public Dictionary<(State from, char on), Transformation> Transforms { get; set; } = new Dictionary<(State from, char on), Transformation>();

        /// <summary>
        /// Gets or sets the dictionary of transformations taken when no other transformation applies for a given state.
        /// </summary>
        /// <remarks>
        /// The purpose of these transformations is to produce output when transforming a segment that is a subset of a larger segment. Such a
        /// segment would otherwise be omitted instead of transformed.
        /// </remarks>
        public Dictionary<State, Transformation> DefaultTransforms { get; set; } = new Dictionary<State, Transformation>();

        public bool Contains((State from, char on) key)
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
                Table[key] = StateCollection.From(to);
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

        public IEnumerable<KeyValuePair<(State from, char on), StateCollection>> From(State state)
        {
            if (state is MergedState ms)
            {
                return Table.Where(x => ms.States.Contains(x.Key.from));
            }
            else
            {
                return Table.Where(x => x.Key.from == state);
            }
        }

        public IEnumerable<(char, StateCollection)> GetAll(State state)
        {
            return Table
                .Where(x => x.Key.from == state)
                .Select(x => (x.Key.on, Table[x.Key]))
                .ToList();
        }

        public State GetFirst(State state, char c)
        {
            return GetAll(state, c)?.FirstOrDefault();
        }
    }
}
