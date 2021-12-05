using System;
using System.Collections.Generic;
using System.Text;

namespace LanguageProcessing.Expression
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
