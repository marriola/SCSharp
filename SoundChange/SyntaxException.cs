using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundChange
{
    class SyntaxException : ApplicationException
    {
        public SyntaxException(string rule, string message, (int line, int column) position)
            : base($"Line {position.line}, column {position.column} in '{rule}': {message}")
        {
        }
    }
}
