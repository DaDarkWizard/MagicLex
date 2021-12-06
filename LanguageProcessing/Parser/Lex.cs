using LanguageProcessing.Expression;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LanguageProcessing.Parser
{
    public class Lex
    {
        public string? Usings { get; set; }

        public string className { get; set; } = "MagicLexer";
        public string nameSpace { get; set; } = "Dasker.LanguageProcessing";
        public string lexType { get; set; } = "int";
        public List<DFA> DFAS { get; set; } = new List<DFA>();
        public Dictionary<DFA, string> DFAActions { get; set; } = new Dictionary<DFA, string>();

        private StreamWriter? output = null;

        public void WriteToFile(string file)
        {
            FileStream outputFileStream = File.Create(file);
            output = new StreamWriter(outputFileStream);
            output.Write(Usings);
            CreateClassHead();
            output.Dispose();
            outputFileStream.Close();
            
        }

        private void CreateClassHead()
        {
            if (output is null)
            {
                throw new Exception();
            }
            output.WriteLine($"namespace {nameSpace}\n{{\n    public class {className}\n    {{");
            string space = "        ";
            output.WriteLine($"{space}private DFA[] dfas = new DFA[{DFAS.Count}];");
            output.WriteLine($"{space}public {className}()");
            output.WriteLine($"{space}{{");
            space = "            ";
            for (int i = 0; i < DFAS.Count; i++)
            {
                output.WriteLine($"{space}dfas[{i}] = new DFA();");
                output.WriteLine($"{space}dfas[{i}].Decode(@\"{DFAS[i].Encode().Replace("\"", "\"\"")}\")");
            }
            space = "        ";
            output.WriteLine($"{space}");
            output.WriteLine();
            output.WriteLine($"{space}public {lexType} Lex()\n{space}{{");
            space = "            ";
            //output.WriteLine
            space = "        ";
            output.WriteLine($"{space}}}");
            space = "    ";
            output.WriteLine($"{space}}}");
            output.WriteLine("}");

        }
    }
}
