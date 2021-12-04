using LanguageProcessing.Expression;
using System;

namespace LanguageProcessing
{
    class Program
    {
        static void Main(string[] args)
        {
            var dfa = DFA.FromString(@"([a-zA-Z])([a-zA-Z0-9]*)");
            Console.WriteLine(dfa.Parse("f9*"));
        }
    }
}
