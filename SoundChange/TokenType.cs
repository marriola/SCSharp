using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundChange
{
    enum TokenType
    {
        ERROR,
        WHITESPACE,
        LBRACK,
        RBRACK,
        LBRACE,
        RBRACE,
        SLASH,
        PLACEHOLDER,
        BOUNDARY,
        ARROW,
        PLUS,
        MINUS,
        PHONE,
        IDENT,
        UTTERANCE
    }
}
