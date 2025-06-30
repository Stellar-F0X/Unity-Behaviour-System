using System.Collections.Generic;
using TaskStreamer.FSM;
using UnityEditor.Experimental.GraphView;

namespace TaskStreamer.Tool
{
    public class StateCreationWindow : TaskCreationWindowBase
    {
        protected override void RegisterSubSearchTrees(List<SearchTreeEntry> searchTree, SearchWindowContext context)
        {
            searchTree.AddRange(this.CreateSearchTreeEntry<ActionState>("State", type => this.CreateNode(type, context)));

            searchTree.AddRange(this.CreateSearchTreeEntry<SubGraphState>("Graph", type => this.CreateAndInjectSubGraph(this.CreateNode(type, context))));
        }
    }
}