using Dasker.LanguageProcessing;
using Dasker.Generated.LanguageProcessing.LexerDependencies.Parser;
using LanguageProcessing.Parser;
using System;

namespace TestProject
{
    class Program
    {
        static void Main(string[] args)
        {
            string program = "asdfg 1234 as45 ";
            FileParser parser = new FileParser(program);
            MagicLexer x = new MagicLexer(parser);
            Types y = Types.Equal;
            while(y != Types.End && y != Types.Error)
            {
                y = x.Lex();
                Console.WriteLine(y.ToString());
            }
        }
    }
}
