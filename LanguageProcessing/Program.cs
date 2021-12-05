using LanguageProcessing.Expression;
using Newtonsoft.Json;
using System;

namespace LanguageProcessing
{
    class Program
    {
        static void Main(string[] args)
        {
            var dfa = DFA.FromString(@"([a-zA-Z])*");
            dfa.Minimize();
            Console.WriteLine(dfa.Parse("a"));
            Console.WriteLine(JsonConvert.SerializeObject(dfa));
        }
    }
}
