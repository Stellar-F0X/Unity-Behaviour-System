using System.Collections.Generic;
using TaskStreamer.FSM;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Edge = UnityEditor.Experimental.GraphView.Edge;

namespace TaskStreamer.Tool
{
    public class FSMView : GraphViewBase
    {
        protected internal FSMView() { }


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

            TransitionEdgeView transitionEdge = new TransitionEdgeView()
            {
                targetTransition = transition,
                output = outputPort,
                input = inputPort
            };

            startView.connectionEdge.Add(end.guid, transitionEdge);

            this.RegisterTransitionEdgeEvents(transitionEdge, view);

            outputPort.Connect(transitionEdge);
            inputPort.Connect(transitionEdge);

            TaskStreamerEditor.Instance.view.AddElement(transitionEdge);
            return true;
        }


        public override void CreateAndConnectNodes(TaskGraphView graphView, Graph graph)
        {
            foreach (NodeBase node in graph.GetIterator(GraphIteratorType.LS))
            {
                graphView.AddNewNodeView(this.RecreateNodeViewOnLoad(node));
            }

            foreach (NodeBase node in graph.GetIterator(GraphIteratorType.LS))
            {
                if (node is not StateBase parentNodeBase || parentNodeBase.transitions.Count == 0)
                {
                    continue;
                }

                foreach (Transition child in parentNodeBase.transitions)
                {
                    NodeViewBase sourceView = graphView.FindNodeView(parentNodeBase);
                    NodeViewBase targetView = graphView.FindNodeView(child.toStateGuid.ToString());

                    this.TryConnectNodesByEdge(graphView, sourceView, targetView);
                }
            }
        }


        protected override TaskCreationWindowBase CreateGraphNodeCreationWindow()
        {
            return ScriptableObject.CreateInstance<StateCreationWindow>();
        }


        public override void FilterSelectionElements(List<ISelectable> selection)
        {
            for (int i = 0; i < selection.Count; ++i)
            {
                if (selection[i] is not NodeViewBase view || view.targetNode is not StateBase targetNode)
                {
                    continue;
                }

                if (this.ShouldExcludeFromSelection(targetNode.nodeType))
                {
                    view.selected = false;
                    selection.RemoveAt(i--);
                }
            }
        }


        public override void DisconnectNodesByEdge(Graph graph, Edge edge)
        {
            if (graph is not StateMachine fsm)
            {
                return;
            }

            NodeViewBase sourceStateView = edge.output.node as NodeViewBase;
            NodeViewBase targetStateView = edge.input.node as NodeViewBase;

            if (sourceStateView?.targetNode is StateBase sourceNode && targetStateView?.targetNode is StateBase targetNode)
            {
                fsm.DisconnectStates(sourceNode, targetNode);
                edge.RemoveFromHierarchy();
            }
        }


        public override void ConnectNodesByEdges(TaskGraphView view, Graph graphCollection, List<Edge> edges)
        {
            if (graphCollection is not StateMachine fsm || edges.Count == 0)
            {
                return;
            }

            foreach (Edge edge in edges)
            {
                StateNodeView sourceStateView = edge.output.node as StateNodeView;
                StateNodeView destinationStateView = edge.input.node as StateNodeView;
                Debug.Assert(sourceStateView is not null && destinationStateView is not null, "sourceStateView or targetStateView is null");

                StateBase sourceNode = sourceStateView.targetNode as StateBase;
                StateBase targetNode = destinationStateView.targetNode as StateBase;
                Debug.Assert(sourceNode != null || targetNode != null, "targetNode is not null");

                Transition transition = fsm.ConnectStates(sourceNode, targetNode);
                TransitionEdgeView transitionView = edge as TransitionEdgeView;
                Debug.Assert(transition != null && transitionView != null, "transition or transitionView is null");

                if (sourceStateView.connectionEdge.TryAdd(targetNode.guid, transitionView))
                {
                    transitionView.targetTransition = transition;
                    this.RegisterTransitionEdgeEvents(transitionView, view);
                }
            }
        }


        public override NodeViewBase RecreateNodeViewOnLoad(NodeBase node)
        {
            if (node == null)
            {
                return null;
            }

            NodeViewBase nodeView = new StateNodeView(node, TaskStreamerEditor.settings.stateNodeViewXml);
            Debug.Assert(nodeView is not null, $"{nameof(TaskGraphView)}: NodeViewBase is null");

            return nodeView;
        }


        //TODO: OnExit나 OnEnter 대상으로만 
        public override void TryDisconnectParentToChild(NodeViewBase parentNodeView) { }


        //TODO: 마찬가지로 OnExit나 OnEnter 대상으로만 
        public override void TryDisconnectChildToParent(NodeViewBase childNodeView) { }



        /// <summary> 전이 에지의 선택/해제 이벤트를 그래프 뷰에 바인딩합니다. </summary>
        private void RegisterTransitionEdgeEvents(TransitionEdgeView transitionEdge, TaskGraphView view)
        {
            transitionEdge.onTransitionSelected -= view.onElementSelected;
            transitionEdge.onTransitionSelected += view.onElementSelected;

            transitionEdge.onTransitionUnselected -= view.onElementUnselected;
            transitionEdge.onTransitionUnselected += view.onElementUnselected;
        }


        /// <summary>
        /// 두 노드 간의 연결 포트 정보를 결정합니다.
        /// - A->B 또는 B->A 방향 중 가능한 방향을 선택
        /// - output/inputPort 포트와 시작/종료 상태, 시작 뷰를 함께 반환
        /// </summary>
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