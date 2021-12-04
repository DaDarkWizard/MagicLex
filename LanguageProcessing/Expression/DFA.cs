using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LanguageProcessing.Expression
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
                                   (int)node.Transitions[j].Character!.Value <= (int)node.Transitions[j]!.Character2!.Value)
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
    }
}
