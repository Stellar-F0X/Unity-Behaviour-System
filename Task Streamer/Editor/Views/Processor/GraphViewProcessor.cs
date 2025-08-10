using System.Collections.Generic;
using TaskStreamer.Utility;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace TaskStreamer.Tool
{
    public abstract class GraphViewProcessor
    {
        private static GraphViewProcessor[] _ProcessorInstances = new GraphViewProcessor[2];
        
        protected TaskCreationWindowBase _taskCreationWindow;
        
        
        
        [InitializeOnLoadMethod]
        private static void ResetProcessorInstancesOnScriptReload()
        {
            _ProcessorInstances[0] = null;
            _ProcessorInstances[1] = null;
        }


        public static GraphViewProcessor CreateGraphViewProcessor(Graph graph)
        {
            if (_ProcessorInstances[(int)graph.graphType] != null)
            {
                return _ProcessorInstances[(int)graph.graphType];
            }
            
            GraphViewProcessor resultProcessor = null;
            
            switch (graph.graphType)
            {
                case GraphType.BT: resultProcessor = new BTViewProcessor(); break;

                case GraphType.FSM: resultProcessor = new FSMViewProcessor(); break;
            }

            _ProcessorInstances[(int)graph.graphType] = resultProcessor;
            return resultProcessor;
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
        
        public abstract bool TryConnectNodesByEdge(TaskGraphView view, NodeView connectionTarget, NodeView nodeB, out Edge linkedEdge);
        
        public abstract void CreateAndConnectNodes(TaskGraphView graphView, Graph graph);

        public abstract void OnDeleteSelectionElements(List<ISelectable> selection);
        
        public abstract NodeView RecreateNodeViewOnLoad(NodeBase node);

        public abstract void TryDisconnectParentToChild(NodeView parentNodeView);

        public abstract void TryDisconnectChildToParent(NodeView childNodeView);
        
        public abstract void DisconnectNodesByEdge(Graph graph, Edge edge);
        
        public abstract void ConnectNodesByEdges(TaskGraphView view, Graph graphCollection, List<Edge> edges);
        
        protected abstract TaskCreationWindowBase CreateGraphNodeCreationWindow();
    }
}