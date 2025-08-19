using System.Collections.Generic;
using System.Linq;
using TaskStreamer.FSM;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Edge = UnityEditor.Experimental.GraphView.Edge;

namespace TaskStreamer.Tool
{
    /// <summary>
    /// Represents a specialized view for managing finite state machine (FSM) graphs in the TaskStreamer tool.
    /// </summary>
    /// <remarks>
    /// FSMView extends the functionality of the base class GraphViewBase to provide specific handling
    /// for FSM-related graph operations, such as node connection, disconnection, node recreation,
    /// and specialized graph behavior for finite state machine systems.
    /// </remarks>
    public class FSMView : GraphViewBase
    {
        /// <summary>
        /// Represents the base class for the Finite State Machine (FSM) graph view.
        /// This class provides methods for managing nodes, connections, and interactions within the FSM graph.
        /// </summary>
        protected internal FSMView() { }


        /// <summary> Tries to connect two nodes within a graph view by creating an edge between them. </summary>
        /// <param name="view"> The graph view that contains the nodes to be connected. </param>
        /// <param name="nodeA"> The first node to connect via an edge. </param>
        /// <param name="nodeB"> The second node to connect via an edge. </param>
        /// <returns> True if the nodes were successfully connected; false otherwise. </returns>
        public override bool TryConnectNodesByEdge(TaskGraphView view, NodeViewBase nodeA, NodeViewBase nodeB)
        {
            if (nodeA is null || nodeB is null)
            {
                return false;
            }

            StateNodeView startView = this.TracePortConnection(nodeA, nodeB, out var outputPort, out var inputPort, out var start, out var end);

            if (startView is null)
            {
                return false;
            }

            if (start.TryGetTransition(end.guid, out Transition transition) == false)
            {
                return false;
            }

            ArrowEdge transitionEdge = new ArrowEdge(transition)
            {
                isGhostEdgeMode = false,
                output = outputPort,
                input = inputPort
            };

            startView.connectionEdges.Add(end.guid, transitionEdge);

            this.RegisterTransitionEdgeEvents(transitionEdge, view);

            outputPort.Connect(transitionEdge);
            inputPort.Connect(transitionEdge);

            TaskStreamerEditor.Instance.view.AddElement(transitionEdge);
            return true;
        }


        /// <summary> 그래프를 불러올때, 주어진 그래프의 노드들을 생성 및 연결하고, TaskGraphView에 반영한다. </summary>
        /// <param name="graphView"> 노드 뷰를 생성 및 추가할 대상 TaskGraphView </param>
        /// <param name="graph"> 순회 및 처리에 사용할 소스 그래프 </param>
        public override void CreateAndConnectNodes(TaskGraphView graphView, Graph graph)
        {
            foreach (NodeBase node in graph.GetIterator(GraphIteratorType.LS))
            {
                NodeViewBase nodeView = this.RecreateNodeViewOnLoad(node);
                Debug.Assert(nodeView != null, "Failed creation node view");
                graphView.AddNewNodeView(nodeView);
            }

            foreach (NodeBase node in graph.GetIterator(GraphIteratorType.LS))
            {
                if (node is not StateBase stateNode || stateNode.transitions.Count == 0)
                {
                    continue;
                }

                foreach (Transition transition in stateNode.transitions)
                {
                    NodeViewBase sourceView = graphView.FindNodeView(stateNode);
                    NodeViewBase targetView = graphView.FindNodeView(transition.toNodeGuid.ToString());
                    Debug.Assert(sourceView is not null && targetView is not null, "source or target is null");

                    this.TryConnectNodesByEdge(graphView, sourceView, targetView);
                }
            }
        }


        /// <summary> 상태 머신 그래프 작업을 위한 노드 생성 창을 생성하고 반환한다. </summary>
        /// <param name="graphView"> 상태 머신 그래프 작업을 위한 그래프 뷰 </param>
        /// <returns> 노드 생성 및 추가를 지원하는 CreationWindow 인스턴스 </returns>
        protected override CreationWindow CreateGraphNodeCreationWindow(TaskGraphView graphView)
        {
            ICreationWindow window = CreationWindow.GetCreationWindow("State Machine");

            window.AddFactoryModule(new NodeFactoryModule(graphView, typeof(ActionState), "Action"))
                  .AddFactoryModule(new NodeFactoryModule(graphView, typeof(SubGraphState), "Graph"))
                  .AddFactoryModule(new NodeGroupFactoryModule(graphView, typeof(NodeGroup), "Utility"));

            return window as CreationWindow;
        }


        /// <summary> 선택된 노드 중 특정 조건에 따라 제외해야 할 노드를 필터링한다. </summary>
        /// <param name="selection"> 선택된 요소들의 리스트 </param>
        public override void FilterSelectionElements(List<ISelectable> selection)
        {
            for (int i = 0; i < selection.Count; ++i)
            {
                NodeViewBase view = (selection[i] as NodeViewBase);
                StateBase target = view?.GetNodeByView<StateBase>();

                if (target is null || this.ShouldExcludeFromSelection(target.nodeType) == false)
                {
                    continue;
                }

                view.selected = false;
                selection.RemoveAt(i--);
            }
        }


        /// <summary> 지정된 그래프와 관련된 Edge를 이용해 노드 간의 연결을 해제한다. </summary>
        /// <param name="graph"> Edge와 연결된 그래프 객체 </param>
        /// <param name="edge"> 연결 해제를 수행할 대상 Edge 객체 </param>
        public override void DisconnectNodesByEdge(Graph graph, Edge edge)
        {
            if (graph is not StateMachine fsm)
            {
                return;
            }

            StateBase sourceNode = edge.output.node.GetNodeByView<StateBase>();
            StateBase targetNode = edge.input.node.GetNodeByView<StateBase>();

            if (sourceNode is not null && targetNode is not null)
            {
                fsm.DisconnectStates(sourceNode, targetNode);
                edge.RemoveFromHierarchy();
            }
        }


        /// <summary> 그래프를 수정했을 때 호출되며, 엣지를 통해 노드들을 연결한다. </summary>
        /// <param name="view"> 작업 그래프 뷰 인스턴스 </param>
        /// <param name="graph"> 연결을 처리할 대상 그래프 </param>
        /// <param name="edges"> 연결을 처리하기 위한 엣지 리스트 </param>
        public override void ConnectNodesByEdges(TaskGraphView view, Graph graph, List<Edge> edges)
        {
            Debug.Assert(edges.Count != 0, "Graph edge's element count is 0");
            StateMachine fsm = graph as StateMachine;

            //Edge Connector Listener에서 만들어진 커스텀 Edge들이 IEnumerable로 반환된다.
            foreach (Edge edge in edges)
            {
                StateBase sourceNode = edge.output.node.GetNodeByView<StateBase>();
                StateBase targetNode = edge.input.node.GetNodeByView<StateBase>();
                Debug.Assert(sourceNode != null && targetNode != null, "sourceNode or targetNode is null");

                Transition transition = fsm.ConnectStates(sourceNode, targetNode);
                ArrowEdge transitionView = edge as ArrowEdge;
                Debug.Assert(transition != null && transitionView != null, "transition or transitionView is null");
                ((StateNodeView)edge.output.node).connectionEdges[sourceNode.guid] = transitionView;
                
                transitionView.targetTransition = transition; //이미 만들어진거라 대입할 수 밖에 없다.
                transitionView.RefreshTransitionData(transition);
                this.RegisterTransitionEdgeEvents(transitionView, view);
            }
        }


        /// <summary> 로드 시 노드의 NodeView를 재생성한다. </summary>
        /// <param name="node"> 재생성할 타겟 NodeBase </param>
        /// <return> 재생성된 NodeViewBase 개체를 반환하거나, 타겟 노드가 null일 경우 null을 반환한다. </return>
        public override NodeViewBase RecreateNodeViewOnLoad(NodeBase node)
        {
            if (node == null)
            {
                return null;
            }

            NodeViewBase nodeView = new StateNodeView(node, TaskStreamerEditor.settings.stateNodeXml);
            Debug.Assert(nodeView is not null, $"{nameof(TaskGraphView)}: NodeViewBase is null");

            return nodeView;
        }



        /// <summary> Attempts to disconnect the source node from its originally connected node when creating a new node through drag-and-drop. </summary>
        /// <param name="sourceState"> The source node where the drag-and-drop operation begins. </param>
        public void TryDisconnectSourceToOriginal(NodeViewBase sourceState)
        {
            StateMachine fsm = TaskStreamerEditor.Instance.currentGraph as StateMachine;

            EnterState enter = sourceState.targetNode as EnterState;

            if (fsm is not null && enter is not null && enter.transitions.Count > 0)
            {
                fsm.DisconnectStates(enter, enter.transitions[0].destinationNode as StateBase);
                TaskStreamerEditor.Instance.view.DeleteElements(sourceState.outputPort.connections);
            }
        }



        /// <summary> 전이 에지의 선택/해제 이벤트를 그래프 뷰에 바인딩합니다. </summary>
        /// <param name="transitionEdge"> 선택/해제 이벤트를 등록할 전이 에지 </param>
        /// <param name="view"> 이벤트를 바인딩할 그래프 뷰 </param>
        private void RegisterTransitionEdgeEvents(ArrowEdge transitionEdge, TaskGraphView view)
        {
            transitionEdge.onTransitionSelected -= view.onElementSelected;
            transitionEdge.onTransitionSelected += view.onElementSelected;

            transitionEdge.onTransitionUnselected -= view.onElementUnselected;
            transitionEdge.onTransitionUnselected += view.onElementUnselected;
        }


        /// <summary>
        /// Determines the connection port information between two nodes.
        /// Selects a valid direction (A->B or B->A) and returns the corresponding output/input ports,
        /// start/end states, and the starting node view.
        /// </summary>
        /// <param name="a">The first node involved in the connection process.</param>
        /// <param name="b">The second node involved in the connection process.</param>
        /// <param name="outPort">The output port determined for the connection.</param>
        /// <param name="inputPort">The input port determined for the connection.</param>
        /// <param name="start">The starting state for the connection.</param>
        /// <param name="end">The ending state for the connection.</param>
        /// <returns>The starting StateNodeView if a valid connection can be determined; otherwise, null.</returns>
        public StateNodeView TracePortConnection(NodeViewBase a, NodeViewBase b, out Port outPort, out Port inputPort, out StateBase start, out StateBase end)
        {
            // A -> B 연결 시도
            if (a.outputPort is not null && b.inputPort is not null)
            {
                outPort = a.outputPort;
                inputPort = b.inputPort;
                start = a.targetNode as StateBase;
                end = b.targetNode as StateBase;
                return a as StateNodeView;
            }

            // B -> A 연결 시도
            if (b.outputPort is not null && a.inputPort is not null)
            {
                outPort = b.outputPort;
                inputPort = a.inputPort;
                start = b.targetNode as StateBase;
                end = a.targetNode as StateBase;
                return b as StateNodeView;
            }

            outPort = null;
            inputPort = null;
            start = null;
            end = null;
            return null;
        }


        /// <summary> Determines if a specific node type should be excluded from selection in the FSM view. </summary>
        /// <param name="type"> The type of the node being evaluated for exclusion. </param>
        /// <returns> True if the node type should be excluded from selection; otherwise, false. </returns>
        private bool ShouldExcludeFromSelection(StateNodeType type)
        {
            if (type == StateNodeType.Any || type == StateNodeType.Enter || type == StateNodeType.Exit)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}