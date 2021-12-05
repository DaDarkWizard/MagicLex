using System;
using System.Collections.Generic;
using System.Text;

namespace LanguageProcessing.Expression
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
