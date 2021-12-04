using System;
using System.Collections.Generic;
using System.Text;

namespace LanguageProcessing.Expression
{
    public class Transition
    {
        public char? Character;
        public char? Character2;
        public TransitionType TransitionType { get; private set; }
        public Node NextNode;

        public Transition(TransitionType transitionType, Node nextNode)
        {
            this.TransitionType = transitionType;
            this.NextNode = nextNode;
        }
    }

    public enum TransitionType
    {
        Character,
        Null,
        Any,
        Range
    }
}
