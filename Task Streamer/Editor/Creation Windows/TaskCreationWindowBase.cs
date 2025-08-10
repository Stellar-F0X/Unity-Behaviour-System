using System;
using System.Collections.Generic;
using TaskStreamer.BT;
using TaskStreamer.FSM;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace TaskStreamer.Tool
{
    public abstract class TaskCreationWindowBase : CreationWindowBase
    {
        private readonly Vector2 _nodeOffset = new Vector2(-75, -20);
        private event Action<NodeView> _createCallback;


        public void RegisterNodeCreationCallbackOnce(Action<NodeView> callback)
        {
            _createCallback = null;
            _createCallback = callback;
        }


        public override List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            List<SearchTreeEntry> searchTree = new List<SearchTreeEntry>();
            searchTree.Add(new SearchTreeGroupEntry(new GUIContent("Create Node"), 0));

            this.RegisterSubSearchTrees(searchTree, context);

            searchTree.Add(new SearchTreeGroupEntry(new GUIContent("Group"), 1));
            searchTree.Add(this.CreateNodeViewGroupSearchTreeEntry(context));
            return searchTree;
        }
        

        protected virtual NodeView CreateNode(Type type, SearchWindowContext context)
        {
            Vector2 nodePosition = _nodeOffset + this.CalculateMousePosition(context);
            NodeView nodeView = graphView.CreateNewNodeAndView(type, nodePosition);

            _createCallback?.Invoke(nodeView);
            _createCallback = null;
            graphView.SelectNode(nodeView);
            return nodeView;
        }


        private SearchTreeEntry CreateNodeViewGroupSearchTreeEntry(SearchWindowContext context, int layerLevel = 2)
        {
            SearchTreeEntry nodeViewGroupEntry = new SearchTreeEntry(new GUIContent("Node Group"));
            Vector2 graphMousePosition = this.CalculateMousePosition(context);

            nodeViewGroupEntry.content.text = "Group";
            nodeViewGroupEntry.userData = (Action)(() => graphView.CreateNewNodeGroupView("Node Group", graphMousePosition));
            nodeViewGroupEntry.level = layerLevel;

            return nodeViewGroupEntry;
        }


        protected virtual void CreateAndInjectSubGraph(NodeView newSubGraphNodeView)
        {
            if (newSubGraphNodeView.targetNode is not ISubGraph graphNode)
            {
                return;
            }

            GraphAsset graphAsset = TaskStreamerEditor.Instance.graphAsset;

            Debug.Assert(graphAsset is not null, $"{nameof(TaskCreationWindowBase)}: GraphAsset is null");

            string graphName = newSubGraphNodeView.title;
            Graph baseGraph = TaskStreamerEditor.Instance.view.focusGraph;
            Graph newGraph = null;

            switch (graphNode.subGraphType)
            {
                case GraphType.BT: newGraph = BehaviorTree.CreateGraph(graphName, graphAsset); break;
                case GraphType.FSM: newGraph = StateMachine.CreateGraph(graphName, graphAsset); break;
            }

            Debug.Assert(newGraph is not null, $"{nameof(TaskCreationWindowBase)}: NewGraph is null");
            
            baseGraph.AddSubGraph(newGraph); 
            graphNode.subGraphGuid = newGraph.guid;
            EditorUtility.SetDirty(graphNode as NodeBase);
        }


        protected abstract void RegisterSubSearchTrees(List<SearchTreeEntry> searchTree, SearchWindowContext context);
    }
}