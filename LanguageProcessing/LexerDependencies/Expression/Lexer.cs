using System;
using System.Collections.Generic;
using System.Text;

namespace Dasker.LanguageProcessing.LexerDependencies.Expression
{
    public class Lexer<T>
    {
        public List<DFA> ExpressionList = new List<DFA>();
        public List<Func<string, T>> FunctionList = new List<Func<string, T>>();
        private int index;
        private string text;
        private T end;
        private T error;

        public Lexer(string text, T end, T error)
        {
            this.text = text;
            this.end = end;
            this.error = error;
        }

        public T Lex()
        {
            int i;
            if(index >= text.Length)
            {
                return end;
            }
            for(i = 0; i < ExpressionList.Count; i++)
            {
                int successIndex = -1;
                var dfa = ExpressionList[i];
                dfa.Reset();
                for(int j = index; j < text.Length; j++)
                {
                    var result = dfa.Parse(text[j]);
                    if(result == DFA.ParseResults.SuccessState)
                    {
                        successIndex = j;
                    }
                    else if(result == DFA.ParseResults.AbsoluteFailure)
                    {
                        break;
                    }
                }
                if(successIndex > -1)
                {
                    string input = text.Substring(index, index - successIndex + 1);
                    index = successIndex + 1;
                    return FunctionList[i].Invoke(input);
                }
            }
            Error();
            return error;
        }

        public virtual void Error()
        {

        }
    }
}
