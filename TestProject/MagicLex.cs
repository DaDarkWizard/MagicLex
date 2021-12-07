using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using Dasker.Generated.LanguageProcessing.LexerDependencies.Parser;
using Dasker.Generated.LanguageProcessing.LexerDependencies.Expression;

using TestProject;
namespace Dasker.Generated.LanguageProcessing.LexerDependencies.Expression
{
    public class DFA
    {
        private List<Node> nodes;
        public Node First { get; private set; }
        private Node current;

        public DFA(NFA nfa)
        {
            List<(HashSet<Node> States,
                Dictionary<char, HashSet<Node>> Transitions,
                HashSet<Node> AnyTransitions,
                Dictionary<(char, char), HashSet<Node>> PairTransitions)> dfaTable = 
                new List<(HashSet<Node>, Dictionary<char, HashSet<Node>>, HashSet<Node>, Dictionary<(char, char), HashSet<Node>>)>();

            HashSet<Node> current = nfa.First.LambdaClosure();
            dfaTable.Add((current, new Dictionary<char, HashSet<Node>>(), 
                new HashSet<Node>(), 
                new Dictionary<(char, char), HashSet<Node>>()));
            bool newSet = true;
            while(newSet)
            {
                newSet = false;
                int i;
                char? tranChar = null;
                char? tranChar2 = null;
                for(i = 0; i < dfaTable.Count; i++)
                {
                    foreach(var state in dfaTable[i].States)
                    {
                        foreach(var transition in state.Transitions)
                        {
                            if(transition.TransitionType == TransitionType.Character)
                            {
                                if(!dfaTable[i].Transitions.ContainsKey(transition.Character!.Value))
                                {
                                    newSet = true;
                                    tranChar = transition.Character.Value;
                                    break;
                                }
                            }
                            else if(transition.TransitionType == TransitionType.Any)
                            {
                                if(dfaTable[i].AnyTransitions.Count == 0)
                                {
                                    newSet = true;
                                    tranChar = null;
                                    break;
                                }
                            }
                            else if(transition.TransitionType == TransitionType.Range)
                            {
                                if(!dfaTable[i].PairTransitions
                                    .ContainsKey((transition.Character!.Value, transition.Character2!.Value)))
                                {
                                    newSet = true;
                                    tranChar = transition.Character.Value;
                                    tranChar2 = transition.Character2.Value;
                                    break;
                                }
                            }
                        }
                        if (newSet)
                        {
                            break;
                        }
                    }
                    if(newSet)
                    {
                        break;
                    }
                }
                if(newSet)
                {
                    if (tranChar is null)
                    {
                        foreach (var state in dfaTable[i].States)
                        {
                            foreach (var transition in state.Transitions)
                            {
                                if (transition.TransitionType == TransitionType.Any)
                                {
                                    dfaTable[i].AnyTransitions.UnionWith(transition.NextNode.LambdaClosure());
                                }
                            }
                        }
                        bool foundGroup = false;
                        for (int j = 0; j < dfaTable.Count; j++)
                        {
                            if (dfaTable[i].AnyTransitions.SetEquals(dfaTable[j].States))
                            {
                                foundGroup = true;
                                break;
                            }
                        }
                        if (!foundGroup)
                        {
                            dfaTable.Add((dfaTable[i].AnyTransitions,
                                new Dictionary<char, HashSet<Node>>(),
                                new HashSet<Node>(),
                                new Dictionary<(char, char), HashSet<Node>>()));
                        }
                    }
                    else if (tranChar2 != null)
                    {
                        dfaTable[i].PairTransitions.Add((tranChar.Value, tranChar2.Value), new HashSet<Node>());
                        foreach (var state in dfaTable[i].States)
                        {
                            foreach (var transition in state.Transitions)
                            {
                                if (transition.TransitionType == TransitionType.Range &&
                                   transition.Character!.Value == tranChar &&
                                   transition.Character2!.Value == tranChar2)
                                {
                                    dfaTable[i].PairTransitions[(tranChar.Value, tranChar2.Value)]
                                        .UnionWith(transition.NextNode.LambdaClosure());
                                }
                            }
                        }
                        bool foundGroup = false;
                        for (int j = 0; j < dfaTable.Count; j++)
                        {
                            if (dfaTable[i].PairTransitions[(tranChar.Value, tranChar2.Value)]
                                .SetEquals(dfaTable[j].States))
                            {
                                foundGroup = true;
                                break;
                            }
                        }
                        if (!foundGroup)
                        {
                            dfaTable.Add((dfaTable[i].PairTransitions[(tranChar.Value, tranChar2.Value)],
                                new Dictionary<char, HashSet<Node>>(),
                                new HashSet<Node>(),
                                new Dictionary<(char, char), HashSet<Node>>()));
                        }
                    }
                    else
                    {
                        dfaTable[i].Transitions.Add(tranChar.Value, new HashSet<Node>());
                        foreach (var state in dfaTable[i].States)
                        {
                            foreach (var transition in state.Transitions)
                            {
                                if (transition.TransitionType == TransitionType.Character &&
                                   transition.Character!.Value == tranChar)
                                {
                                    dfaTable[i].Transitions[tranChar.Value].UnionWith(transition.NextNode.LambdaClosure());
                                }
                                else if(transition.TransitionType == TransitionType.Range &&
                                    (int)tranChar >= (int)transition.Character!.Value &&
                                    (int)tranChar <= (int)transition.Character2!.Value)
                                {
                                    dfaTable[i].Transitions[tranChar.Value].UnionWith(transition.NextNode.LambdaClosure());
                                }
                            }
                        }
                        bool foundGroup = false;
                        for (int j = 0; j < dfaTable.Count; j++)
                        {
                            if (dfaTable[i].Transitions[tranChar.Value].SetEquals(dfaTable[j].States))
                            {
                                foundGroup = true;
                                break;
                            }
                        }
                        if (!foundGroup)
                        {
                            dfaTable.Add((dfaTable[i].Transitions[tranChar.Value],
                                new Dictionary<char, HashSet<Node>>(),
                                new HashSet<Node>(),
                                new Dictionary<(char, char), HashSet<Node>>()));
                        }
                    }
                }
            }

            List<Node> tableNodes = new List<Node>();
            for(int i = 0; i < dfaTable.Count; i++)
            {
                Node n = new Node();
                n.Id = i;
                if(dfaTable[i].States.Any(x => x.IsSuccess))
                {
                    n.IsSuccess = true;
                }
                tableNodes.Add(n);
            }
            for(int i = 0; i < dfaTable.Count; i++)
            {

                HashSet<Node> nodes;
                foreach (var key in dfaTable[i].Transitions.Keys)
                {
                    nodes = dfaTable[i].Transitions[key];
                    for (int j = 0; j < dfaTable.Count; j++)
                    {
                        if(nodes.SetEquals(dfaTable[j].States))
                        {
                            Transition transition = new Transition(TransitionType.Character, tableNodes[j]);
                            transition.Character = key;
                            tableNodes[i].Transitions.Add(transition);
                            break;
                        }
                    }
                }
                foreach (var key in dfaTable[i].PairTransitions.Keys)
                {
                    nodes = dfaTable[i].PairTransitions[key];
                    for (int j = 0; j < dfaTable.Count; j++)
                    {
                        if (nodes.SetEquals(dfaTable[j].States))
                        {
                            Transition transition = new Transition(TransitionType.Range, tableNodes[j]);
                            transition.Character = key.Item1;
                            transition.Character2 = key.Item2;
                            tableNodes[i].Transitions.Add(transition);
                            break;
                        }
                    }
                }
                nodes = dfaTable[i].AnyTransitions;
                for(int j = 0; j < dfaTable.Count; j++)
                {
                    if(nodes.SetEquals(dfaTable[j].States))
                    {
                        Transition transition = new Transition(TransitionType.Any, tableNodes[j]);
                        tableNodes[i].Transitions.Add(transition);
                        break;
                    }
                }
            }
            nodes = tableNodes;
            First = tableNodes[0];
            this.current = First;
            foreach(var node in nodes)
            {
                for(int i = 0; i < node.Transitions.Count; i++)
                {
                    if(node.Transitions[i].TransitionType == TransitionType.Character)
                    {
                        for(int j = 0; j < node.Transitions.Count; j++)
                        {
                            if(j == i)
                            {
                                continue;
                            }
                            if(node.Transitions[j].TransitionType == TransitionType.Range)
                            {
                                if((int)node.Transitions[i].Character!.Value >= (int)node.Transitions[j]!.Character!.Value &&
                                   (int)node.Transitions[i].Character!.Value <= (int)node.Transitions[j]!.Character2!.Value)
                                {
                                    if(node.Transitions[i].NextNode != node.Transitions[j].NextNode)
                                    {
                                        throw new Exception("Parser error! Consult developer.");
                                    }
                                    node.Transitions.RemoveAt(i);
                                    i--;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        public DFA()
        {
            First = null!;
            current = null!;
            nodes = new List<Node>();
        }

        public void Reset()
        {
            current = First;
        }

        public ParseResults Parse(char c)
        {
            foreach(Transition t in current.Transitions)
            {
                if(t.TransitionType == TransitionType.Character && t.Character == c)
                {
                    current = t.NextNode;
                    if(current.IsSuccess)
                    {
                        return ParseResults.SuccessState;
                    }
                    else
                    {
                        return ParseResults.FailState;
                    }
                }
                else if(t.TransitionType == TransitionType.Any)
                {
                    current = t.NextNode;
                    if (current.IsSuccess)
                    {
                        return ParseResults.SuccessState;
                    }
                    else
                    {
                        return ParseResults.FailState;
                    }
                }
                else if(t.TransitionType == TransitionType.Range &&
                    (int)c >= (int)t.Character! && (int)c <= (int)t.Character2!)
                {
                    current = t.NextNode;
                    if (current.IsSuccess)
                    {
                        return ParseResults.SuccessState;
                    }
                    else
                    {
                        return ParseResults.FailState;
                    }
                }
            }
            return ParseResults.AbsoluteFailure;
        }

        public bool Parse(string s)
        {
            Reset();
            ParseResults results = current.IsSuccess ? ParseResults.SuccessState : ParseResults.FailState;
            for(int i = 0; i < s.Length; i++)
            {
                results = Parse(s[i]);
                if(results == ParseResults.AbsoluteFailure)
                {
                    return false;
                }
            }
            if(results == ParseResults.SuccessState)
            {
                return true;
            }
            return false;
        }

        public static DFA FromString(string input)
        {
            var regex = new Regex(input);
            return FromRegex(regex);
        }

        public static DFA FromRegex(Regex regex)
        {
            var nfa = NFA.FromRegex(regex);
            return new DFA(nfa);
        }

        public enum ParseResults
        {
            AbsoluteFailure,
            FailState,
            SuccessState
        }

        public void Minimize()
        {
            // Minimize using standard algorithm.
            var t = new HashSet<HashSet<Node>>();
            HashSet<HashSet<Node>> p;
            t.Add(nodes.Where(x => x.IsSuccess).ToHashSet());
            t.Add(nodes.Where(x => !x.IsSuccess).ToHashSet());
            do
            {
                p = t;
                t = new HashSet<HashSet<Node>>();
                foreach(var set in p)
                {
                    HashSet<Node> newSet = new HashSet<Node>(set);
                    foreach(var s in t)
                    {
                        newSet.ExceptWith(s);
                    }
                    while(newSet.Count > 0)
                    {
                        Dictionary<Transition, HashSet<Node>> trans = new Dictionary<Transition, HashSet<Node>>();
                        HashSet<Node> finalSet = new HashSet<Node>();
                        Node initial = newSet.First();
                        newSet.Remove(initial);
                        finalSet.Add(initial);
                        foreach(Transition tran in initial.Transitions)
                        {
                            foreach(var tranSet in p)
                            {
                                if (tranSet.Contains(tran.NextNode))
                                {
                                    trans.Add(tran, tranSet);
                                    break;
                                }
                            }
                        }
                        foreach(var node in newSet)
                        {
                            if(TransitionsMatch(node.Transitions, trans))
                            {
                                finalSet.Add(node);
                            }
                        }
                        newSet.ExceptWith(finalSet);
                        t.Add(finalSet);
                    }
                }
            }
            while (!(t.Count() == p.Count()));

            // Link nodes to their set.
            Dictionary<Node, HashSet<Node>> tranToSet = new Dictionary<Node, HashSet<Node>>();
            foreach(var node in nodes)
            {
                tranToSet.Add(node, t.Single(x => x.Contains(node)));
            }
            // Create nodes for each set.
            Dictionary<HashSet<Node>, Node> setToNode = new Dictionary<HashSet<Node>, Node>();
            foreach(var set in t)
            {
                setToNode.Add(set, new Node());
                setToNode[set].IsSuccess = set.First().IsSuccess;
            }
            // Link transitions for each of the new nodes.
            foreach(var set in t)
            {
                foreach(var tran in set.First().Transitions)
                {
                    Transition newTran = new Transition(tran.TransitionType, setToNode[tranToSet[tran.NextNode]]);
                    newTran.Character = tran.Character;
                    newTran.Character2 = tran.Character2;
                    setToNode[set].Transitions.Add(newTran);
                }
            }
            // Set the class variables.
            First = setToNode[t.Single(x => x.Contains(First))];
            this.current = First;
            List<Node> newList = new List<Node>();
            foreach(var set in t)
            {
                newList.Add(setToNode[set]);
            }
            newList.Remove(First);
            newList = newList.Prepend(First).ToList();
            this.nodes = newList;
            for(int i = 0; i < nodes.Count; i++)
            {
                nodes[i].Id = i+1;
            }
        }

        private bool TransitionsMatch(IEnumerable<Transition> transA, Dictionary<Transition, HashSet<Node>> dictB)
        {
            // Check that there are the same number of transitions.
            if(dictB.Keys.Count != transA.Count())
            {
                return false;
            }

            // Check that the transitions match.
            foreach (var key in dictB.Keys)
            {
                Transition? match = transA.Where(x => x.TransitionType == key.TransitionType &&
                                                      x.Character == key.Character &&
                                                      x.Character2 == key.Character2).FirstOrDefault();
                if(match is null)
                {
                    return false;
                }
                if (!dictB[key].Contains(match.NextNode))
                {
                    return false;
                }
            }

            // All cases passed, return true.
            return true;
        }

        public string Encode()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append('[');
            for(int i = 0; i < nodes.Count; i++)
            {
                builder.Append(nodes[i].Id.ToString());
                builder.Append(':');
                builder.Append(nodes[i].IsSuccess ? 't' : 'f');
            }

            builder.Append("][");
            foreach(var node in nodes)
            {
                builder.Append(node.Id);
                builder.Append(":[");
                foreach(var transition in node.Transitions)
                {
                    builder.Append("{");
                    if(transition.TransitionType == TransitionType.Any)
                    {
                        builder.Append("0:");
                        builder.Append(transition.NextNode.Id);
                    }
                    else if(transition.TransitionType == TransitionType.Character)
                    {
                        builder.Append("1:");
                        builder.Append(transition.NextNode.Id);
                        builder.Append(":");
                        builder.Append(transition.Character);
                    }
                    else
                    {
                        builder.Append("2:");
                        builder.Append(transition.NextNode.Id);
                        builder.Append(":");
                        builder.Append(transition.Character);
                        builder.Append(transition.Character2);
                    }
                    builder.Append("}");
                }
                builder.Append("]");
            }
            builder.Append("]");

            return builder.ToString();
        }

        public void Decode(string input)
        {
            int i = 1;
            nodes = new List<Node>();
            while(input[i] != ']')
            {
                Node node = new Node();
                StringBuilder number = new StringBuilder();
                while(input[i] != ':')
                {
                    number.Append(input[i]);
                    i++;
                }
                node.Id = int.Parse(number.ToString());
                i++;
                node.IsSuccess = (input[i] == 't');
                i++;
                nodes.Add(node);
            }
            i+=2;
            while(input[i] != ']')
            {
                StringBuilder number = new StringBuilder();
                while(input[i] != ':')
                {
                    number.Append(input[i]);
                    i++;
                }
                Node n = nodes[int.Parse(number.ToString()) - 1];
                i+=2;
                while (input[i] != ']')
                {
                    i++;
                    char? type;
                    char? one;
                    char? two;
                    type = input[i];
                    i += 2;
                    number = new StringBuilder();
                    while (input[i] != ':')
                    {
                        number.Append(input[i]);
                        i++;
                    }
                    Node e = nodes[int.Parse(number.ToString()) - 1];
                    Transition t;
                    i++;
                    if(type == '0')
                    {
                        t = new Transition(TransitionType.Any, e);
                    }
                    else if (type == '1')
                    {
                        t = new Transition(TransitionType.Character, e);
                        t.Character = input[i];
                        i++;
                    }
                    else
                    {
                        t = new Transition(TransitionType.Range, e);
                        t.Character = input[i];
                        t.Character2 = input[i + 1];
                        i += 2;
                    }
                    n.Transitions.Add(t);
                    i++;
                }
                i++;
            }
            First = nodes[0];
            current = First;
        }
    }
}namespace Dasker.Generated.LanguageProcessing.LexerDependencies.Expression
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
namespace Dasker.Generated.LanguageProcessing.LexerDependencies.Expression
{
    public class NFA
    {
        /// <summary>
        /// First node in this NFA.
        /// </summary>
        public Node First;

        /// <summary>
        /// Last node in this NFA.
        /// </summary>
        public Node Last;

        /// <summary>
        /// Construct an NFA from a regex.
        /// </summary>
        /// <param name="regex">Regex to construct it from.</param>
        /// <returns>A constructed NFA.</returns>
        public static NFA FromRegex(Regex regex)
        {
            return FromString(regex.Expression);
        }

        /// <summary>
        /// Construct an NFA from the string in a regex.
        /// </summary>
        /// <param name="regex">The string from the regex to use.</param>
        /// <returns>A constructed NFA.</returns>
        private static NFA FromString(string regex)
        {
            // Current state of NFA building.
            NFA? current = null;

            // Iterate over the string.
            for(int i = 0; i < regex.Length; i++)
            {
                switch (regex[i])
                {
                    case '*':       // Take the Kuene Star of the NFA.
                        if(current is null)
                        {
                            throw new Exception("Invalid regex!");
                        }
                        current = current.Star();
                        break;
                    case '(':       // Recursively parse what's inside the parentheses.
                        {
                            int x;
                            int count = 1;
                            for (x = i + 1; x <= regex.Length; x++)
                            {
                                if (regex[x] == '(')
                                {
                                    count++;
                                }
                                else if (regex[x] == ')')
                                {
                                    count--;
                                }
                                if (count == 0)
                                {
                                    break;
                                }
                            }
                            if (count != 0)
                            {
                                throw new Exception("Invalid regex!");
                            }
                            if (current is null)
                            {
                                current = FromString(regex.Substring(i + 1, x - i - 1));
                            }
                            else
                            {
                                current = current.Append(FromString(regex.Substring(i + 1, x - i - 1)));
                            }
                            i = x;
                            break;
                        }
                    case '|':   // Generate Or.
                        {
                            if(current is null)
                            {
                                throw new Exception("Invalid regex!");
                            }
                            string next = regex.Substring(i + 1);
                            current = current.Or(FromString(next));
                            i = regex.Length - 1;
                            break;
                        }
                    case '%':
                        {
                            if (current is null)
                            {
                                Node first = new Node();
                                Node last = new Node();
                                Transition transition = new Transition(TransitionType.Any, last);
                                first.Transitions.Add(transition);
                                current = new NFA(first, last);
                                last.IsSuccess = true;
                            }
                            else
                            {
                                Node last = new Node();
                                Transition transition = new Transition(TransitionType.Any, last);
                                current.Last.Transitions.Add(transition);
                                current.Last.IsSuccess = false;
                                current.Last = last;
                                current.Last.IsSuccess = true;
                            }

                            break;
                        }
                    case '[':
                        {
                            int j;
                            List<(char, char)> pairs = new List<(char, char)>();
                            for(j = i + 1; j < regex.Length && regex[j] != ']'; j++)
                            {
                                pairs.Add((regex[j], regex[j + 2]));
                                j += 2;
                            }
                            i = j;
                            if (current is null)
                            {
                                Node first = new Node();
                                Node last = new Node();
                                for(int k = 0; k < pairs.Count; k++)
                                {
                                    Node first1 = new Node();
                                    Node last1 = new Node();
                                    Transition transition = new Transition(TransitionType.Range, last1);
                                    transition.Character = pairs[k].Item1;
                                    transition.Character2 = pairs[k].Item2;
                                    first1.Transitions.Add(transition);
                                    first.Transitions.Add(new Transition(TransitionType.Null, first1));
                                    last1.Transitions.Add(new Transition(TransitionType.Null, last));
                                }
                                current = new NFA(first, last);
                                last.IsSuccess = true;
                            }
                            else
                            {
                                Node last = new Node();
                                for (int k = 0; k < pairs.Count; k++)
                                {
                                    Node first1 = new Node();
                                    Node last1 = new Node();
                                    Transition transition = new Transition(TransitionType.Range, last1);
                                    transition.Character = pairs[k].Item1;
                                    transition.Character2 = pairs[k].Item2;
                                    first1.Transitions.Add(transition);
                                    current.Last.Transitions.Add(new Transition(TransitionType.Null, first1));
                                    last1.Transitions.Add(new Transition(TransitionType.Null, last));
                                }
                                current.Last.IsSuccess = false;
                                current.Last = last;
                                current.Last.IsSuccess = true;
                            }
                            break;
                        }
                    case '\\':
                        i++;
                        goto default;
                    default:
                        {
                            if(current is null)
                            {
                                Node first = new Node();
                                Node last = new Node();
                                Transition transition = new Transition(TransitionType.Character, last);
                                transition.Character = regex[i];
                                first.Transitions.Add(transition);
                                current = new NFA(first, last);
                                last.IsSuccess = true;
                            }
                            else
                            {
                                Node last = new Node();
                                Transition transition = new Transition(TransitionType.Character, last);
                                transition.Character = regex[i];
                                current.Last.Transitions.Add(transition);
                                current.Last.IsSuccess = false;
                                current.Last = last;
                                current.Last.IsSuccess = true;
                            }
                            
                            break;
                        }
                }
            }
            if(current is null)
            {
                throw new Exception("Invalid Regex!");
            }
            return current;
        }

        public NFA(Node first, Node last)
        {
            First = first;
            Last = last;
        }

        public NFA Append(NFA nfa2)
        {
            this.Last.Transitions.Add(new Transition(TransitionType.Null, nfa2.First));
            this.Last.IsSuccess = false;
            return new NFA(this.First, nfa2.Last);
        }

        public NFA Or(NFA nfa2)
        {
            Node firstNode = new Node();
            Node lastNode = new Node();
            firstNode.Transitions.Add(new Transition(TransitionType.Null, this.First));
            firstNode.Transitions.Add(new Transition(TransitionType.Null, nfa2.First));
            this.Last.Transitions.Add(new Transition(TransitionType.Null, lastNode));
            nfa2.Last.Transitions.Add(new Transition(TransitionType.Null, lastNode));
            this.Last.IsSuccess = false;
            nfa2.Last.IsSuccess = false;
            lastNode.IsSuccess = true;
            NFA newNFA = new NFA(firstNode, lastNode);
            return newNFA;
        }

        public NFA Star()
        {
            Node firstNode = new Node();
            Node lastNode = new Node();
            this.Last.IsSuccess = false;
            lastNode.IsSuccess = true;
            this.Last.Transitions.Add(new Transition(TransitionType.Null, this.First));
            firstNode.Transitions.Add(new Transition(TransitionType.Null, this.First));
            this.Last.Transitions.Add(new Transition(TransitionType.Null, lastNode));
            firstNode.Transitions.Add(new Transition(TransitionType.Null, lastNode));
            return new NFA(firstNode, lastNode);
        }
    }
}
namespace Dasker.Generated.LanguageProcessing.LexerDependencies.Expression
{
    public class Node
    {
        public int? Id { get; set; }
        public List<Transition> Transitions { get; private set; } = new List<Transition>();
        public bool IsSuccess { get; set; }
        public Node()
        {
            
        }

        public List<Node> GetNodes()
        {
            return GetNodes(new HashSet<Node>()).ToList();
        }

        private HashSet<Node> GetNodes(HashSet<Node> ignoreList)
        {
            HashSet<Node> returnList = new HashSet<Node>();
            returnList.UnionWith(ignoreList);
            returnList.Add(this);
            foreach(Transition t in Transitions)
            {
                if(!returnList.Contains(t.NextNode))
                {
                    returnList.UnionWith(t.NextNode.GetNodes(returnList));
                }
            }
            return returnList;
        }

        public HashSet<Node> LambdaClosure()
        {
            return LambdaClosure(new HashSet<Node>());
        }

        private HashSet<Node> LambdaClosure(HashSet<Node> ignoreList)
        {
            HashSet<Node> newList = new HashSet<Node>();
            newList.UnionWith(ignoreList);
            newList.Add(this);
            foreach(Transition t in Transitions)
            {
                if(t.TransitionType == TransitionType.Null &&
                   !newList.Contains(t.NextNode))
                {
                    newList.UnionWith(t.NextNode.LambdaClosure(newList));
                }
            }
            return newList;
        }
    }
}
namespace Dasker.Generated.LanguageProcessing.LexerDependencies.Expression
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
namespace Dasker.Generated.LanguageProcessing.LexerDependencies.Expression
{
    /// <summary>
    /// Transition for a parsing rule.
    /// </summary>
    public class Transition
    {
        /// <summary>
        /// Character this rule covers, or beginning character of this rule's range.
        /// </summary>
        public char? Character;

        /// <summary>
        /// End character of this rule's range.
        /// </summary>
        public char? Character2;

        /// <summary>
        /// The type of this transition.
        /// </summary>
        public TransitionType TransitionType { get; private set; }

        /// <summary>
        /// The node this transition moves to.
        /// </summary>
        public Node NextNode;

        /// <summary>
        /// Construct a transition with a type and node.
        /// </summary>
        /// <param name="transitionType"></param>
        /// <param name="nextNode"></param>
        public Transition(TransitionType transitionType, Node nextNode)
        {
            this.TransitionType = transitionType;
            this.NextNode = nextNode;
        }
    }

    /// <summary>
    /// Possible types for a transition.
    /// </summary>
    public enum TransitionType
    {
        Character,
        Null,
        Any,
        Range
    }
}
namespace Dasker.Generated.LanguageProcessing.LexerDependencies.Parser
{
    public class FileParser
    {
        public int Position 
        { 
            get { return position; }
            set 
            {
                Seek(value, SeekOrigin.Begin);
            } 
        }
        private string text;
        private int position;
        public int Length { get; private set; }

        public FileParser(FileStream file)
        {
            var reader = new StreamReader(file);
            text = reader.ReadToEnd();
            Length = text.Length;
        }

        public FileParser(string text)
        {
            this.text = text;
            this.Length = text.Length;
        }

        public int Peek()
        {
            return position < text.Length ? text[position] : -1;
        }

        public int ReadBlock(char[] buffer, int index, int count)
        {
            int i;
            for(i = 0; i + position < text.Length && i < count; i++)
            {
                buffer[i + index] = text[i + position];
            }
            position += i;
            return i;
        }

        public int Read()
        {
            int result = position < text.Length ? text[position] : -1;
            if(position <= text.Length) position++;
            return result;
        }

        public void Seek(int offset, SeekOrigin origin)
        {
            if(origin == SeekOrigin.Begin)
            {
                position = offset;
            }
            else if(origin == SeekOrigin.Current)
            {
                position += offset;
            }
            else if(origin == SeekOrigin.End)
            {
                position = text.Length - offset - 1;
            }
        }
    }
}
namespace Dasker.LanguageProcessing
{
    public class MagicLexer
    {
        private DFA[] magicdfas = new DFA[5];
        private Func<string, Types>[] magicfuncs = new Func<string, Types>[5];
        private FileParser magicinput;

        public MagicLexer(FileParser magicinput)
        {
            magicdfas[0] = new DFA();
            magicdfas[0].Decode(@"[1:f2:t3:f4:f][1:[{1:3:i}]2:[]3:[{1:4:n}]4:[{1:2:t}]]");
            magicdfas[1] = new DFA();
            magicdfas[1].Decode(@"[1:f2:t][1:[{2:2:19}]2:[{2:2:09}]]");
            magicdfas[2] = new DFA();
            magicdfas[2].Decode(@"[1:f2:t3:f][1:[{1:3:""}]2:[]3:[{1:1:\}{1:3:}{1:3:
}{1:3:	}{1:3:[}{1:3:]}{1:2:""}{2:3:#Z}{2:3:^~}]]");
            magicdfas[3] = new DFA();
            magicdfas[3].Decode(@"[1:f2:t][1:[{2:2:az}{2:2:AZ}]2:[{2:2:az}{2:2:AZ}{2:2:09}]]");
            magicdfas[4] = new DFA();
            magicdfas[4].Decode(@"[1:f2:t][1:[{1:2:}{1:2:
}{1:2: }{1:2:	}]2:[{1:2:}{1:2:
}{1:2: }{1:2:	}]]");
            magicfuncs[0] = (string magicText) => { return Types.INT; };
            magicfuncs[1] = (string magicText) => { return Types.Num; };
            magicfuncs[2] = (string magicText) => { return Types.STRING; };
            magicfuncs[3] = (string magicText) => { return Types.IDENTIFIER; };
            magicfuncs[4] = (string magicText) => { return Lex(); };
            this.magicinput = magicinput;
        }

        public Types Lex()
        {
            int pos = magicinput.Position;
            if ( pos >= magicinput.Length ) return (Types.End);
            int i;
            StringBuilder magicText = new StringBuilder();
            for ( i = 0; i < 5; i++ )
            {
                int lastSuccess = -1;
                magicdfas[i].Reset();
                magicinput.Position = pos;
                while ( magicinput.Position < magicinput.Length )
                {
                    int c = magicinput.Read();
                    if ( c >= 0 )
                    {
                        DFA.ParseResults t = magicdfas[i].Parse((char)c);
                        if ( t == DFA.ParseResults.AbsoluteFailure ) break;
                        else if ( t == DFA.ParseResults.SuccessState ) lastSuccess = magicinput.Position;
                    }
                }
                if ( lastSuccess > -1 )
                {
                    magicinput.Position = pos;
                    int c = magicinput.Read();
                    while ( magicinput.Position < lastSuccess )
                    {
                        if ( c >= 0 ) magicText.Append((char)c);
                        c = magicinput.Read();
                    }
                    break;
                }
            }

            if ( i < 5 )
            {
                return magicfuncs[i].Invoke(magicText.ToString());
            }
            return (Types.Error);
        }
    }
}
