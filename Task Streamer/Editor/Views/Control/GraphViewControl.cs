using System.Collections.Generic;
using TaskStreamer.Utility;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace TaskStreamer.Tool
{
    public abstract class GraphViewControl
    {
        private static GraphViewControl[] _ProcessorInstances = new GraphViewControl[2];
        
        protected TaskCreationWindowBase _taskCreationWindow;
        
        
        
        [InitializeOnLoadMethod]
        private static void ResetProcessorInstancesOnScriptReload()
        {
            _ProcessorInstances[0] = null;
            _ProcessorInstances[1] = null;
        }


        public static GraphViewControl CreateGraphViewProcessor(Graph graph)
        {
            if (_ProcessorInstances[(int)graph.graphType] != null)
            {
                return _ProcessorInstances[(int)graph.graphType];
            }
            
            GraphViewControl resultControl = null;
            
            switch (graph.graphType)
            {
                case GraphType.BT: resultControl = new BTViewControl(); break;

                case GraphType.FSM: resultControl = new FSMViewControl(); break;
            }

            _ProcessorInstances[(int)graph.graphType] = resultControl;
            return resultControl;
        }
        
        
        public TaskCreationWindowBase GetGraphNodeCreationWindow()
        {
            if (_taskCreationWindow is null)
            {
                _taskCreationWindow = this.CreateGraphNodeCreationWindow();
            }
            
            Debug.Assert(_taskCreationWindow != null, "CreationWindow is null");

            return _taskCreationWindow;
        }
        
        
        public void DeleteNodeFromGraph(Graph graph, NodeBase targetNode)
        {
            if (targetNode is ISubGraph subGraphNode)
            {
                UGUID targetGuid = subGraphNode.subGraphGuid;
                Graph foundSubGraph = TaskStreamerEditor.Instance.graphAsset.GetGraph(targetGuid);
                Debug.Assert(foundSubGraph != null, $"Graph is null. guid : {targetGuid}");
                
                foundSubGraph.RemoveSelfAndSubGraphs(); //서브 그래프를 삭제.
            }
            
            //노드도 삭제.
            graph.DeleteNode(targetNode);
        }
        
        
        public virtual void NotifyNodePositionChanged(TaskGraphView graphView, List<GraphElement> elements) { }
        
        public abstract bool TryConnectNodesByEdge(TaskGraphView view, NodeViewBase connectionTarget, NodeViewBase nodeB);
        
        public abstract void CreateAndConnectNodes(TaskGraphView graphView, Graph graph);

        public abstract void FilterSelectionElements(List<ISelectable> selection);
        
        public abstract NodeViewBase RecreateNodeViewOnLoad(NodeBase node);

        public abstract void TryDisconnectParentToChild(NodeViewBase parentNodeView);

        public abstract void TryDisconnectChildToParent(NodeViewBase childNodeView);
        
        public abstract void DisconnectNodesByEdge(Graph graph, Edge edge);
        
        public abstract void ConnectNodesByEdges(TaskGraphView view, Graph graphCollection, List<Edge> edges);
        
        protected abstract TaskCreationWindowBase CreateGraphNodeCreationWindow();
    }
}