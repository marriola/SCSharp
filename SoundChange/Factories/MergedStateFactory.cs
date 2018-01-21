using SoundChange.Parser;
using SoundChange.StateMachines;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoundChange.Factories
{
    class MergedStateFactory
    {
        private List<MergedState> _merged = new List<MergedState>();

        private MergedStateHandler _statesMerged;

        public MergedStateFactory(MergedStateHandler statesMerged)
        {
            _statesMerged = statesMerged;
        }

        public State Merge(IEnumerable<State> states)
        {
            switch (states.Count())
            {
                case 0:
                    throw new ArgumentException("Cannot merge an empty list of states.");

                case 1:
                    return states.First();
            }

            states = states.OrderBy(x => x.Name).ToList();
            var merged = _merged.SingleOrDefault(x => x.States.OrderBy(y => y.Name).SequenceEqual(states));

            if (merged == null)
            {
                merged = new MergedState(states);
                _merged.Add(merged);

                if (_statesMerged != null)
                {
                    foreach (var state in states)
                    {
                        _statesMerged(state, merged);
                    }
                }
            }

            return merged;
        }
    }
}
