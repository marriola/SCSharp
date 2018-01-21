using System.Collections.Generic;
using System.Linq;

namespace SoundChange.Parser
{
    class MergedState : State
    {
        public HashSet<State> States { get; private set; }

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

            IsFinal = States.Any(x => x.IsFinal);

            var stateNames = States
                .Select(x => x.Name)
                .OrderBy(x => x[0] == 'q'
                    ? int.Parse(x.Substring(1))
                    : -1);

            Name = string.Join("+", stateNames);
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
}
