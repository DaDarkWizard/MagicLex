using LanguageProcessing.Expression;
using LanguageProcessing.Parser;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LanguageProcessing
{
    class Program
    {
        static void Main(string[] args)
        {
            //var dfa = DFA.FromString(@"([a-zA-Z])*");
            //dfa.Minimize();
            //Console.WriteLine(dfa.Parse("a"));
            FileStream inputFile = File.OpenRead(args[0]);
            //FileStream output = File.Create("MagicLex.cs");
            FileParser input = new FileParser(inputFile);
            Lex lex = new Lex();

            lex.Usings = WriteAllSections("usings", input);
            input.Seek(0, SeekOrigin.Begin);
            ScanUntil("begin", input);
            lex.DFAS = new List<DFA>();
            lex.DFAActions = new Dictionary<DFA, string>();

            int beginIndex = input.Position;
            ScanUntil("end", input);
            int endIndex = input.Position - 4;
            input.Position = beginIndex;
            SkipWhiteSpace(input);

            while (input.Position < endIndex)
            {
                
                string regex = ReadUntilWhitespace(input);
                var dfa = DFA.FromString(regex.Substring(1, regex.Length - 2));
                dfa.Minimize();
                SkipWhiteSpace(input);
                input.Read();
                SkipWhiteSpace(input);
                string function = ReadFunctionBlock(input);
                lex.DFAS.Add(dfa);
                lex.DFAActions.Add(dfa, function);
                SkipWhiteSpace(input);
            }

            lex.WriteToFile("MagicLex.cs");
            //output.Dispose();*
        }

        public static void ScanUntil(string delimiter, FileParser inputFile)
        {
            while (inputFile.Position < inputFile.Length)
            {
                int i = inputFile.Read();
                if (i < 0) break;
                char c = (char)i;
                if (c != '%') continue;

                while(c == '%')
                {
                    i = inputFile.Read();
                    if (i < 0) break;
                    c = (char)i;
                    if(c == '%')
                    {
                        break;
                    }
                }
                if(c == '%')
                {
                    continue;
                }
                else
                {
                    inputFile.Seek(-1, SeekOrigin.Current);
                }


                char[] chars = new char[delimiter.Length];
                int read = inputFile.ReadBlock(chars, 0, delimiter.Length);
                if (read < delimiter.Length) break;
                string text = new string(chars);
                if (text == delimiter)
                {
                    return;
                }
            }
        }

        public static void SkipWhiteSpace(FileParser inputFile)
        {
            HashSet<char> whiteSpace = new HashSet<char>(new char[]{ ' ', '\t', '\r', '\n' });
            while(inputFile.Position < inputFile.Length)
            {
                int i = inputFile.Read();
                if (i < 0) break;
                char c = (char)i;
                if(!whiteSpace.Contains(c))
                {
                    inputFile.Seek(-1, SeekOrigin.Current);
                    break;
                }
            }
        }

        public static string ReadUntilWhitespace(FileParser inputFile)
        {
            HashSet<char> whiteSpace = new HashSet<char>(new char[] { ' ', '\t', '\r', '\n' });
            StringBuilder builder = new StringBuilder();
            while (inputFile.Position < inputFile.Length)
            {
                int i = inputFile.Read();
                if (i < 0) break;
                char c = (char)i;
                if (c == '%')
                {
                    inputFile.Read();
                }
                if (whiteSpace.Contains(c))
                {
                    inputFile.Seek(-1, SeekOrigin.Current);
                    break;
                }
                else
                {
                    builder.Append(c);
                }
            }
            return builder.ToString();
        }

        public static string ReadFunctionBlock(FileParser inputFile)
        {
            int count = 1;
            StringBuilder builder = new StringBuilder();
            builder.Append('{');
            inputFile.Read();
            while (inputFile.Position < inputFile.Length && count >= 1)
            {
                int i = inputFile.Read();
                if (i < 0) break;
                char c = (char)i;

                if (c == '{') count++;
                else if (c == '}') count--;

                builder.Append(c);
            }
            return builder.ToString();
        }

        public static string WriteAllSections(string delimiter, FileParser inputFile)
        {
            StringBuilder builder = new StringBuilder();
            while (inputFile.Position < inputFile.Length)
            {
                int i = inputFile.Read();
                if (i < 0) break;
                char c = (char)i;
                if (c != '%') continue;
                char[] chars = new char[delimiter.Length];
                int read = inputFile.ReadBlock(chars, 0, delimiter.Length);
                if (read < delimiter.Length) break;
                string text = new string(chars);
                if (text == delimiter)
                {
                    i = inputFile.Read();
                    while (i >= 0 && (char)i != '%')
                    {
                        i = inputFile.Read();
                    }
                    if (i < 0) break;
                    inputFile.Read();
                    i = inputFile.Read();
                    while (i >= 0 && (char)i != '%')
                    {
                        if (i > 0)
                        {
                            c = (char)i;
                            //byte[] bytes = BitConverter.GetBytes(c);
                            //output.Write(bytes, 0, bytes.Length);
                            builder.Append(c);
                        }
                        i = inputFile.Read();
                        while (i >= 0 && i == '%')
                        {
                            i = inputFile.Read();
                            if (i >= 0 && (char)i == '%')
                            {
                                c = (char)i;
                                //byte[] bytes = BitConverter.GetBytes(c);
                                builder.Append(c);
                                //output.Write(bytes, 0, bytes.Length);
                                i = inputFile.Read();
                            }
                            else if(i >= 0)
                            {
                                inputFile.Seek(-1, SeekOrigin.Current);
                                i = (int)'%';
                                break;
                            }
                        }
                    }
                }
                else if(inputFile.Position >= inputFile.Length)
                {
                    break;
                }
                else
                {
                    inputFile.Seek(delimiter.Length * -1, SeekOrigin.Current);
                }
            }
            return builder.ToString();
        }
    }
}
