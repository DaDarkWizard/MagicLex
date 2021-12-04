using System;
using System.Collections.Generic;
using System.Text;

namespace LanguageProcessing.Expression
{
    public class Regex
    {
        public string Expression { get; private set; }

        public Regex(string regex)
        {
            StringBuilder builder = new StringBuilder();
            for(int i = 0; i < regex.Length; i++)
            {
                switch(regex[i])
                {
                    case '\\':
                        {
                            char next = regex[++i];
                            switch (next)
                            {
                                case 'n':
                                    builder.Append('\n');
                                    break;
                                case 'r':
                                    builder.Append('\r');
                                    break;
                                case 's':
                                    builder.Append(' ');
                                    break;
                                case 'w':
                                    builder.Append("(\r|\n| |\t)");
                                    break;
                                case '0':
                                    builder.Append('\0');
                                    break;
                                case 't':
                                    builder.Append('\t');
                                    break;
                                case '\\':
                                    builder.Append("\\\\");
                                    break;
                                case '*':
                                    builder.Append("\\*");
                                    break;
                                case '|':
                                    builder.Append("\\|");
                                    break;
                                case '(':
                                    builder.Append("\\(");
                                    break;
                                case ')':
                                    builder.Append("\\)");
                                    break;
                                case '[':
                                    builder.Append("[");
                                    break;
                                case ']':
                                    builder.Append("]");
                                    break;
                                default:
                                    throw new Exception("Couldn't parse regex: \\" + next);
                            }
                            break;
                        }
                    
                    default:
                        builder.Append(regex[i]);
                        break;
                }
            }
            Expression = builder.ToString();
        }
    }
}
