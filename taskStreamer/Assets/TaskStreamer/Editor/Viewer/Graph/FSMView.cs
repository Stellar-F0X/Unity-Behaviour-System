using System.Collections.Generic;
using TaskStreamer.FSM;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Assertions;
using Edge = UnityEditor.Experimental.GraphView.Edge;

namespace TaskStreamer.Tool
{
    /// <summary>
    /// FSM(유한 상태 기계) 구조를 그래프 기반 인터페이스에서 시각화하고 관리하는
    /// 기능을 제공하는 클래스.
    /// </summary>
    /// <remarks>
    /// 이 클래스는 그래프 뷰 시스템 기반을 확장하여 FSM 작업에 특화된 기능을 제공합니다.
    /// 상태 간 연결, 전환 관리, 유효한 FSM 구성을 보장하는 작업을 지원합니다.
    /// 주로 동적 FSM 조작이 필요한 도구나 환경에서 사용됩니다.
    /// </remarks>
    internal class FSMView : GraphViewBase
    {
        protected internal FSMView() { }


        /// <summary>
        /// 두 노드를 그래프 뷰에서 엣지를 생성하여 연결을 시도합니다.
        /// </summary>
        /// <param name="graphView">노드들이 포함된 그래프 뷰입니다.</param>
        /// <param name="sourceView">연결의 소스 노드입니다.</param>
        /// <param name="targetView">연결의 타겟 노드입니다.</param>
        /// <returns>노드 연결이 성공적으로 이루어졌으면 true, 그렇지 않으면 false를 반환합니다.</returns>
        public override bool TryConnectNodesByEdge(TaskGraphView graphView, NodeViewBase sourceView, NodeViewBase targetView)
        {
            if (sourceView is null || targetView is null)
            {
                return false;
            }

            StateNodeView startView = this.TracePortConnection(sourceView, targetView, out var outputPort, out var inputPort, out var start, out var end);

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

            outputPort.Connect(transitionEdge);
            inputPort.Connect(transitionEdge);

            TaskStreamerEditor.Instance.taskGraphView.AddElement(transitionEdge);
            return true;
        }


        /// <summary>
        /// Creates nodes from the provided graph and connects them based on its structure.
        /// The resulting nodes are added to the specified graph view for display and interaction.
        /// </summary>
        /// <param name="graphView">The TaskGraphView instance where the created node views will be displayed.</param>
        /// <param name="graph">The source Graph instance containing nodes and edges to process and connect.</param>
        public override void CreateAndConnectNodes(TaskGraphView graphView, Graph graph)
        {
            foreach (NodeBase node in graph.GetIterator(GraphIteratorType.LS))
            {
                NodeViewBase nodeView = this.RecreateNodeViewOnLoad(node);
                Assert.IsNotNull(nodeView, "Failed creation node view");
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
                    Assert.IsTrue(sourceView is not null && targetView is not null, "source or target is null");

                    this.TryConnectNodesByEdge(graphView, sourceView, targetView);
                }
            }
        }


        /// <summary>
        /// 상태 머신 그래프를 위한 노드 생성 창을 생성한다.
        /// 이 창은 다양한 유형 및 유틸리티를 사용해 그래프 노드를 추가하고 관리할 수 있는 기능을 제공한다.
        /// </summary>
        /// <param name="graphView">노드 생성 창이 생성될 상태 머신과 연관된 그래프 뷰이다.</param>
        /// <returns>노드 생성 및 관리를 위한 구성이 완료된 <see cref="BindingWindow"/> 인스턴스를 반환한다.</returns>
        public override BindingWindow CreateGraphNodeCreationWindow(TaskGraphView graphView)
        {
            return BindingWindowBuilder.GetBuilder("State Machine", reuse: true)
                                       .AddFactoryModule(
                                           () => new NodeFactoryModule<ActionState>(graphView, "Action"),
                                           () => new TypeTreeProvider(true))
                                       .AddFactoryModule(
                                           () => new NodeFactoryModule<SubGraphState>(graphView, "Graph"),
                                           () => new TypeTreeProvider(true))
                                       .AddFactoryModule(
                                           () => new NodeGroupFactoryModule<NodeGroup>(graphView, "Utility"),
                                           () => new TypeTreeProvider(false))
                                       .Build();
        }


        /// <summary>
        /// Filters the selected elements based on specific conditions, removing nodes that do not meet the criteria from the selection.
        /// </summary>
        /// <param name="selection">The list of selected elements in the graph view to process.</param>
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


        /// <summary>
        /// 두 노드를 제공된 엣지를 사용하여 그래프에서 연결 해제합니다.
        /// 연결을 제거하고 그래프 계층 구조를 갱신합니다.
        /// </summary>
        /// <param name="graph">연결 해제를 수행할 엣지가 포함된 그래프 인스턴스입니다.</param>
        /// <param name="edge">노드 간 연결을 나타내는 엣지입니다.</param>
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


        /// <summary>
        /// Connects nodes within the graph by traversing the provided edges.
        /// This method modifies the graph structure based on the given edges.
        /// </summary>
        /// <param name="view">The instance of the task graph view where the operation is performed.</param>
        /// <param name="graph">The graph containing nodes to connect.</param>
        /// <param name="edges">The list of edges used to define connections between nodes.</param>
        public override void ConnectNodesByEdges(TaskGraphView view, Graph graph, List<Edge> edges)
        {
            Assert.IsTrue(edges.Count != 0, "Graph edge's element count is 0");
            StateMachine fsm = graph as StateMachine;
            Assert.IsNotNull(fsm, "fsm graph is null referenced");

            //Edge Connector Listener에서 만들어진 커스텀 Edge들이 IEnumerable로 반환된다.
            for (int index = edges.Count - 1; index >= 0; index--)
            {
                Edge edge = edges[index];

                StateBase sourceNode = edge.output.node.GetNodeByView<StateBase>();
                StateBase targetNode = edge.input.node.GetNodeByView<StateBase>();
                Assert.IsTrue(sourceNode != null && targetNode != null, "sourceNode or targetNode is null");

                Transition transition = fsm.ConnectStates(sourceNode, targetNode);

                if (transition is null || edge is not ArrowEdge transitionView)
                {
                    //graphViewChange는 Graph의 변경사항을 담은 컨테이너.
                    //그 안에 Edges가 이 함수의 매개변수로 전달되는데,
                    //이미 연결되어있는 경우라면 앞으로 반영될 사항 중, Edge를 제외시킨다.
                    edges.RemoveAt(index);
                    continue;
                }

                ((StateNodeView)edge.output.node).connectionEdges[sourceNode.guid] = transitionView;
                transitionView.targetTransition = transition; //이미 만들어진거라 대입할 수 밖에 없다.
            }
        }


        /// <summary>
        /// 주어진 NodeBase 객체를 기반으로 NodeView를 재생성합니다.
        /// 그래프 로드 시 대응되는 NodeView가 올바르게 생성되도록 보장합니다.
        /// </summary>
        /// <param name="node">NodeView를 재생성해야 하는 NodeBase 객체입니다.</param>
        /// <returns>주어진 NodeBase에 대응하는 새롭게 생성된 NodeViewBase 인스턴스를 반환합니다. 입력이 null인 경우 null을 반환합니다.</returns>
        public override NodeViewBase RecreateNodeViewOnLoad(NodeBase node)
        {
            if (node == null)
            {
                return null;
            }

            NodeViewBase nodeView = StateNodeView.Create(node, TaskStreamerResourceLoader.stateNode);
            Assert.IsNotNull(nodeView, $"{nameof(TaskGraphView)}: NodeViewBase is null");

            return nodeView;
        }



        /// <summary>
        /// 소스 노드와 원래 연결된 노드 간의 연결을 끊으려고 시도합니다.
        /// 이 메서드는 드래그 앤 드롭을 통해 새 노드를 생성하는 작업 중에 사용됩니다.
        /// </summary>
        /// <param name="sourceState">연결을 끊으려는 소스 노드를 나타내는 노드입니다.</param>
        public void TryDisconnectSourceToOriginal(NodeViewBase sourceState)
        {
            StateMachine fsm = TaskStreamerEditor.Instance.currentGraph as StateMachine;
            Assert.IsNotNull(fsm, "fsm graph is null referenced");

            //enter 노드만이 하나의 아웃풋 포트를 가지므로, enter 노드인지만 확인한다.
            //exit 또는 any 노드는 output 포트 없이, input 포트만 가지므로 검사하지 않아도 된다.
            if (sourceState.targetNode is not EnterState enter || enter.transitions.Count == 0)
            {
                return;
            }

            fsm.DisconnectStates(enter, enter.transitions[0].destinationNode as StateBase);
            TaskStreamerEditor.Instance.taskGraphView.DeleteElements(sourceState.outputPort.connections);
        }


        /// <summary>
        /// 노드 간의 연결 정보를 결정합니다.
        /// 유효한 방향(A->B 또는 B->A)을 선택하고, 연결에 필요한 출력/입력 포트,
        /// 시작/종료 상태 및 시작 노드 뷰를 반환합니다.
        /// </summary>
        /// <param name="a">연결 과정에 포함된 첫 번째 노드입니다.</param>
        /// <param name="b">연결 과정에 포함된 두 번째 노드입니다.</param>
        /// <param name="outPort">결정된 출력 포트를 반환합니다.</param>
        /// <param name="inputPort">결정된 입력 포트를 반환합니다.</param>
        /// <param name="start">연결의 시작 상태를 반환합니다.</param>
        /// <param name="end">연결의 종료 상태를 반환합니다.</param>
        /// <returns>유효한 연결이 결정되면 시작 StateNodeView를 반환하며, 그렇지 않으면 null을 반환합니다.</returns>
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


        /// <summary>
        /// 지정된 노드 유형(StateNodeType)이 선택에서 제외되어야 하는지 여부를 결정합니다.
        /// 이 메서드는 제외 기준을 평가하여 선택에 포함되지 않아야 할 요소를 식별합니다.
        /// </summary>
        /// <param name="type">선택 제외 기준을 평가할 노드 유형(StateNodeType).</param>
        /// <returns>노드 유형이 제외 기준을 충족하면 true를 반환하고, 그렇지 않으면 false를 반환합니다.</returns>
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