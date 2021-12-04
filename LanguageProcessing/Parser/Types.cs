using System;
using System.Collections.Generic;
using System.Text;

namespace LanguageProcessing.Parser
{
    public enum Types
    {
        Error,
        End,
        Func,
        STRING,
        Num,
        LBrace,
        RBrace,
        Semicolin,
        Name,
        Equal,
        LParen,
        RParen,
        INT
    }
}
