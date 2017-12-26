using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundChange
{
    class State
    {
        public Token Token { get; private set; }

        public State()
        {
        }

        public State(Token token)
        {
            Token = token;
        }
    }
}
