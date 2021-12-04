using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LanguageProcessing.Expression
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
