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
        public List<State> States { get; private set; }

        public override string Name
        {
            get
            {
                return string.Join(string.Empty, States.Select(x => x.Name));
            }
        }

        public MergedState(State state1, State state2)
        {
            States = new List<State> { state1, state2 };
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

        public static MergedState Find(State state1, State state2, Dictionary<State, MergedState> mergers)
        {
            return mergers[state1] ?? mergers[state2] ?? new MergedState(state1, state2);
        }
    }
}
