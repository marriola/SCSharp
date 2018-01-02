using System.Collections.Generic;
using System.Linq;

namespace SoundChange
{
    class State
    {
        public virtual string Name { get; private set; }

        public bool IsFinal { get; set; }

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

    class MergedState : State
    {
        public HashSet<State> States { get; private set; }

        public override string Name
        {
            get
            {
                return string.Join(",", States.Select(x => x.Name));
            }
        }

        public MergedState(IEnumerable<State> states)
        {
            States = new HashSet<State>();

            foreach (var state in states)
            {
                if (state is MergedState ms)
                {
                    foreach (var substate in ms.States)
                    {
                        States.Add(substate);
                    }
                }
                else
                {
                    States.Add(state);
                }
            }
        }

        public List<State> Closure(char c, Dictionary<(State, char), State> transitions)
        {
            var closure = new List<State>();

            foreach (var state in States)
            {
                var key = (state, c);
                if (transitions.ContainsKey(key))
                {
                    closure.Add(transitions[key]);
                }
            }

            return closure;
        }
    }

    class MergedStateFactory
    {
        private List<MergedState> _merged = new List<MergedState>();

        public State Merge(IEnumerable<State> states)
        {
            states = states.OrderBy(x => x.Name).ToList();
            var merged = _merged.SingleOrDefault(x => x.States.OrderBy(y => y.Name).SequenceEqual(states));

            if (merged == null)
            {
                merged = new MergedState(states);
                _merged.Add(merged);
            }

            return merged;
        }
    }
}
