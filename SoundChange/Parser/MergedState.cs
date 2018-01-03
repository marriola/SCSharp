using System.Collections.Generic;
using System.Linq;

namespace SoundChange.Parser
{
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

            IsFinal = States.Any(x => x.IsFinal);
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
