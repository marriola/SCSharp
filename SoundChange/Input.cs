using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundChange
{
    abstract class InputBase
    {
    }

    class PhoneInput : InputBase
    {
        public char Phone { get; private set; }

        public PhoneInput(char c)
        {
            Phone = c;
        }
    }

    class FeatureInput : InputBase
    {
        public bool IsPresent { get; private set; }

        public string Name { get; private set; }

        public FeatureInput(bool isPresent, string name)
        {
            IsPresent = isPresent;
            Name = name;
        }
    }
}
