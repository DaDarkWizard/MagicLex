using System;
using System.Collections.Generic;
using System.Text;

namespace LanguageProcessing.Expression
{
    /// <summary>
    /// Class for parsing a regular expression.
    /// </summary>
    public class Regex
    {
        /// <summary>
        /// Expression after it's been simplified.
        /// </summary>
        public string Expression { get; private set; }

        /// <summary>
        /// Construct the Regex from a string.
        /// </summary>
        /// <param name="regex">The regular expression string to parse.</param>
        public Regex(string regex)
        {
            // Use a stringbuilder to efficiently build the new string.
            StringBuilder builder = new StringBuilder();

            // Parse the given regex for 
            for(int i = 0; i < regex.Length; i++)
            {
                switch(regex[i])
                {
                    // Replace escaped characters with their correct values.
                    case '\\':
                        {
                            char next = regex[++i];
                            switch (next)
                            {
                                // Newline character.
                                case 'n':
                                    builder.Append('\n');
                                    break;
                                // Carrage return.
                                case 'r':
                                    builder.Append('\r');
                                    break;
                                // Space.
                                case 's':
                                    builder.Append(' ');
                                    break;
                                // Whitespace.
                                case 'w':
                                    builder.Append("(\r|\n| |\t)");
                                    break;
                                // Null character.
                                case '0':
                                    builder.Append('\0');
                                    break;
                                // Tab character.
                                case 't':
                                    builder.Append('\t');
                                    break;
                                // Escaped backslash.
                                case '\\':
                                    builder.Append("\\\\");
                                    break;
                                // Escaped *.
                                case '*':
                                    builder.Append("\\*");
                                    break;
                                // Escaped |.
                                case '|':
                                    builder.Append("\\|");
                                    break;
                                // Escaped parenthese.
                                case '(':
                                    builder.Append("\\(");
                                    break;
                                // Escaped parenthese.
                                case ')':
                                    builder.Append("\\)");
                                    break;
                                // Escaped bracket.
                                case '[':
                                    builder.Append("\\[");
                                    break;
                                // Escaped bracket.
                                case ']':
                                    builder.Append("\\]");
                                    break;
                                // Unknown escape character.
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
            // Set the expression.
            Expression = builder.ToString();
        }
    }
}
