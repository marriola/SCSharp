using SoundChange.Parser;
using System.Collections.Generic;
using System.Linq;

namespace SoundChange.Factories
{
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
