using Dasker.LanguageProcessing.LexerDependencies.Expression;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Dasker.LanguageProcessing.ScannerMaker
{
    public class Lex
    {
        public string? Usings { get; set; }

        public string className { get; set; } = "MagicLexer";
        public string nameSpace { get; set; } = "Dasker.LanguageProcessing";
        public string lexType { get; set; } = "int";
        public string errorValue { get; set; } = "-1";
        public string endValue { get; set; } = "-2";
        public List<DFA> DFAS { get; set; } = new List<DFA>();
        public Dictionary<DFA, string> DFAActions { get; set; } = new Dictionary<DFA, string>();

        private StreamWriter? output = null;

        public void WriteToFile(string file)
        {
            FileStream outputFileStream = File.Create(file);
            output = new StreamWriter(outputFileStream);
            PrintUsings();
            output.Write(Usings);
            CreateDependencies();
            CreateClassHead();
            output.WriteLine();
            CreateClassConstructor();
            output.WriteLine();
            CreateLexFunction();
            CreateClassTail();
            output.Dispose();
            outputFileStream.Close();
            
        }

        private void CreateDependencies()
        {
            if(output is null)
            {
                throw new Exception();
            }

            List<string> allFiles = new List<string>();
            allFiles.AddRange(Directory.GetFiles("../../../LexerDependencies/Expression/"));
            allFiles.AddRange(Directory.GetFiles("../../../LexerDependencies/Parser/"));

            foreach(string filename in allFiles)
            {
                FileStream file = File.OpenRead(filename);
                StreamReader fileReader = new StreamReader(file);
                string check = "namespace Dasker.";
                while(!fileReader.EndOfStream)
                {
                    int i = fileReader.Read();
                    if(i >= 0)
                    {
                        if((char)i == check[0])
                        {
                            var builder = new StringBuilder();
                            builder.Append((char)i);
                            bool passed = false;
                            for(int j = 1; j < check.Length; j++)
                            {
                                i = fileReader.Read();
                                if(i >= 0 && check[j] == (char)i)
                                {
                                    builder.Append((char)i);
                                    if(j == check.Length - 1)
                                    {
                                        passed = true;
                                    }
                                }
                                else if(i >= 0)
                                {
                                    break;
                                }
                            }
                            if(passed)
                            {
                                output.Write(builder.ToString());
                                output.Write("Generated.");
                                i = fileReader.Read();
                                while(i >= 0)
                                {
                                    output.Write((char)i);
                                    i = fileReader.Read();
                                }
                            }
                        }
                    }
                }
            }
        }

        private void PrintUsings()
        {
            if (output is null)
            {
                throw new Exception();
            }
            output.WriteLine("using System;");
            output.WriteLine("using System.IO;");
            output.WriteLine("using System.Text;");
            output.WriteLine("using System.Linq;");
            output.WriteLine("using System.Collections.Generic;");
            output.WriteLine("using Dasker.Generated.LanguageProcessing.LexerDependencies.Parser;");
            output.WriteLine("using Dasker.Generated.LanguageProcessing.LexerDependencies.Expression;");
        }

        private void CreateClassHead()
        {
            if (output is null)
            {
                throw new Exception();
            }
            output.WriteLine($"namespace {nameSpace}\n{{\n    public class {className}\n    {{");
            string space = "        ";
            output.WriteLine($"{space}private DFA[] magicdfas = new DFA[{DFAS.Count}];");
            output.WriteLine($"{space}private Func<string, {lexType}>[] magicfuncs = new Func<string, {lexType}>[{DFAS.Count}];");
            output.WriteLine($"{space}private FileParser magicinput;");
        }

        private void CreateClassConstructor()
        {
            if(output is null)
            {
                throw new Exception();
            }
            string space = "        ";
            output.WriteLine($"{space}public {className}(FileParser magicinput)");
            output.WriteLine($"{space}{{");
            space = "            ";
            for (int i = 0; i < DFAS.Count; i++)
            {
                output.WriteLine($"{space}magicdfas[{i}] = new DFA();");
                output.WriteLine($"{space}magicdfas[{i}].Decode(@\"{DFAS[i].Encode().Replace("\"", "\"\"")}\");");
            }
            for (int i = 0; i < DFAS.Count; i++)
            {
                output.WriteLine($"{space}magicfuncs[{i}] = (string magicText) => {DFAActions[DFAS[i]]};");
            }
            output.WriteLine($"{space}this.magicinput = magicinput;");
            space = "        ";
            output.WriteLine($"{space}}}");
        }

        private void CreateLexFunction()
        {
            if(output is null)
            {
                throw new Exception();
            }
            string space = "        ";
            output.WriteLine($"{space}public {lexType} Lex()\n{space}{{");
            space = "            ";
            output.WriteLine($"{space}int pos = magicinput.Position;");
            output.WriteLine($"{space}if ( pos >= magicinput.Length ) return ({endValue});");
            output.WriteLine($"{space}int i;");
            output.WriteLine($"{space}StringBuilder magicText = new StringBuilder();");
            output.WriteLine($"{space}for ( i = 0; i < {DFAS.Count}; i++ )");
            output.WriteLine($"{space}{{");
            space = "                ";
            
            output.WriteLine($"{space}int lastSuccess = -1;");
            output.WriteLine($"{space}magicdfas[i].Reset();");
            output.WriteLine($"{space}magicinput.Position = pos;");
            output.WriteLine($"{space}while ( magicinput.Position < magicinput.Length )");
            output.WriteLine($"{space}{{");
            space = space + "    ";
            output.WriteLine($"{space}int c = magicinput.Read();");
            output.WriteLine($"{space}if ( c >= 0 )");
            output.WriteLine($"{space}{{");
            space += "    ";
            output.WriteLine($"{space}DFA.ParseResults t = magicdfas[i].Parse((char)c);");
            output.WriteLine($"{space}if ( t == DFA.ParseResults.AbsoluteFailure ) break;");
            output.WriteLine($"{space}else if ( t == DFA.ParseResults.SuccessState ) lastSuccess = magicinput.Position;");
            space = space.Substring(4);
            output.WriteLine($"{space}}}");
            space = space.Substring(4);
            output.WriteLine($"{space}}}");
            output.WriteLine($"{space}if ( lastSuccess > -1 )");
            output.WriteLine($"{space}{{");
            space += "    ";
            output.WriteLine($"{space}magicinput.Position = pos;");
            output.WriteLine($"{space}int c = magicinput.Read();");
            output.WriteLine($"{space}while ( magicinput.Position < lastSuccess )");
            output.WriteLine($"{space}{{");
            space += "    ";
            output.WriteLine($"{space}if ( c >= 0 ) magicText.Append((char)c);");
            output.WriteLine($"{space}c = magicinput.Read();");
            space = space.Substring(4);
            output.WriteLine($"{space}}}");
            output.WriteLine($"{space}break;");
            space = space.Substring(4);
            output.WriteLine($"{space}}}");
            space = "            ";
            output.WriteLine($"{space}}}");
            output.WriteLine();
            output.WriteLine($"{space}if ( i < {DFAS.Count} )");
            output.WriteLine($"{space}{{");
            space = "                ";
            output.WriteLine($"{space}return magicfuncs[i].Invoke(magicText.ToString());");
            space = "            ";
            output.WriteLine($"{space}}}");
            //output.WriteLine
            output.WriteLine($"{space}return ({errorValue});");
            space = "        ";
            output.WriteLine($"{space}}}");
        }

        private void CreateClassTail()
        {
            if(output is null)
            {
                throw new Exception();
            }
            output.WriteLine("    }");
            output.WriteLine("}");
        }
    }
}
