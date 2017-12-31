using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundChange
{
    class StateFactory
    {
        private int _id = 0;

        public State Next()
        {
            var state = new State("q" + _id);
            ++_id;
            return state;
        }
    }
}
