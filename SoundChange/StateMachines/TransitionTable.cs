using SoundChange.Parser;
using SoundChange.Parser.Nodes;
using System.Collections.Generic;
using System.Linq;

namespace SoundChange.StateMachines
{
    class Transformation
    {
        public string Value { get; private set; }

        public Node From { get; private set; }

        public Node To { get; private set; }

        public string FromLiteral { get; set; }

        public Transformation(string value, Node from, Node to)
        {
            Value = value;
            From = from;
            To = to;
        }

        public override string ToString()
        {
            return $"{FromLiteral} ({From.ToString()}) → {Value} ({To.ToString()})";
        }
    }

    class TransitionTable
    {
        public Dictionary<(State from, char on), StateCollection> Table { get; set; } = new Dictionary<(State from, char on), StateCollection>();

        public Dictionary<(State from, char on), Transformation> Transforms { get; set; } = new Dictionary<(State from, char on), Transformation>();

        /// <summary>
        /// Gets or sets the set of transitions for which emission of an output symbol should be suppressed.
        /// </summary>
        /// <remarks>
        /// This set is used to suppress symbol emission so that multi-character segments can be treated as a single unit.
        /// </remarks>
        public HashSet<(State from, char on)> SuppressEmitTransitions { get; set; } = new HashSet<(State from, char on)>();

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

        public void SuppressSymbolEmission(State from, char on)
        {
            SuppressEmitTransitions.Add((from, on));
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
