using System.Collections.Generic;
using TaskStreamer.BT;
using UnityEditor.Experimental.GraphView;

namespace TaskStreamer.Tool
{
    public class BehaviorCreationWindow : TaskCreationWindowBase
    {
        protected override void RegisterSubSearchTrees(List<SearchTreeEntry> searchTree, SearchWindowContext context)
        {
            searchTree.AddRange(this.CreateSearchTreeEntry<ActionNode>("Action", type => this.CreateNode(type, context)));

            searchTree.AddRange(this.CreateSearchTreeEntry<CompositeNode>("Composite", type => this.CreateNode(type, context)));

            searchTree.AddRange(this.CreateSearchTreeEntry<DecoratorNode>("Decorator", type => this.CreateNode(type, context)));

            searchTree.AddRange(this.CreateSearchTreeEntry<SubGraphNode>("Graph", type => this.CreateAndInjectSubGraph(this.CreateNode(type, context))));
        }
    }
}