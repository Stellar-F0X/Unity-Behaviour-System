using System.Collections.Generic;
using TaskStreamer.FSM;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Edge = UnityEditor.Experimental.GraphView.Edge;

namespace TaskStreamer.Tool
{
    public class FSMViewProcessor : GraphViewProcessor
    {
        protected internal FSMViewProcessor() { }
        
        
        public override bool TryConnectNodesByEdge(TaskGraphView view, NodeView nodeA, NodeView nodeB, out Edge linkedEdge)
        {
            if (nodeA is null || nodeB is null)
            {
                linkedEdge = null;
                return false;
            }
    
            Port outputPort = null;
            Port inputPort = null;

            StateBase start = null;
            StateBase end = null;

            StateNodeView startView = null;
    
            // 포트 방향에 따라 올바른 연결 결정
            if (nodeA.outputPort is not null && nodeB.inputPort is not null)
            {
                outputPort = nodeA.outputPort;
                inputPort = nodeB.inputPort;
                start = nodeA.targetNode as StateBase;
                end = nodeB.targetNode as StateBase;
                startView = nodeA as StateNodeView;
            }
            else if (nodeB.outputPort is not null && nodeA.inputPort is not null)
            {
                outputPort = nodeB.outputPort;
                inputPort = nodeA.inputPort;
                start = nodeB.targetNode as StateBase;
                end = nodeA.targetNode as StateBase;
                startView = nodeB as StateNodeView;
            }
            else
            {
                linkedEdge = null;
                return false;
            }
            
            if (start.TryGetTransition(end.guid, out Transition transition) == false)
            {
                linkedEdge = null;
                return false;
            }
            
            TransitionEdgeView transitionEdge = new TransitionEdgeView()
            {
                targetTransition = transition,
                output = outputPort,
                input = inputPort
            };
            
            startView.connectionEdge.Add(end.guid, transitionEdge);
            
            transitionEdge.onTransitionSelected -= view.onElementSelected;
            transitionEdge.onTransitionSelected += view.onElementSelected;
            
            outputPort.Connect(transitionEdge);
            inputPort.Connect(transitionEdge);
    
            linkedEdge = transitionEdge;
    
            TaskStreamerEditor.Instance.view.AddElement(linkedEdge);
            return true;
        }
        

        public override void CreateAndConnectNodes(TaskGraphView graphView, Graph graph)
        {
            foreach (NodeBase node in graph.GetGraphIterator())
            {
                graphView.AddNewNodeView(this.RecreateNodeViewOnLoad(node));
            }

            foreach (NodeBase node in graph.GetGraphIterator())
            {
                if (node is not StateBase parentNodeBase || parentNodeBase.transitions.Count == 0)
                {
                    continue;
                }

                foreach (Transition child in parentNodeBase.transitions)
                {
                    NodeView sourceView = graphView.FindNodeView(parentNodeBase);
                    NodeView targetView = graphView.FindNodeView(child.toStateGuid.ToString());

                    this.TryConnectNodesByEdge(graphView, sourceView, targetView, out _);
                }
            }
        }


        protected override TaskCreationWindowBase CreateGraphNodeCreationWindow()
        {
            return ScriptableObject.CreateInstance<StateCreationWindow>();
        }


        public override void OnDeleteSelectionElements(List<ISelectable> selection)
        {
            for (int i = 0; i < selection.Count; ++i)
            {
                if (selection[i] is not NodeView view || view.targetNode is not StateBase targetNode)
                {
                    continue;
                }

                StateNodeType type = targetNode.nodeType;

                bool exclude = false;
                
                exclude |= type == StateNodeType.Any;
                exclude |= type == StateNodeType.Enter;
                exclude |= type == StateNodeType.Exit;

                if (exclude == false)
                {
                    continue;
                }

                view.selected = false;
                selection.RemoveAt(i--);
            }
        }


        public override void DisconnectNodesByEdge(Graph graph, Edge edge)
        {
            if (graph is not StateMachine fsm)
            {
                return;
            }

            NodeView sourceStateView = edge.output.node as NodeView;
            NodeView targetStateView = edge.input.node as NodeView;

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

                if (sourceStateView.connectionEdge.TryAdd(targetNode.guid, transitionView) == false)
                {
                    continue;
                }

                Debug.Assert(transition != null && transitionView != null, "transition or transitionView is null");
                
                
                transitionView.targetTransition = transition;
                
                transitionView.onTransitionSelected -= view.onElementSelected;
                transitionView.onTransitionSelected += view.onElementSelected;
            }
        }


        public override NodeView RecreateNodeViewOnLoad(NodeBase node)
        {
            if (node == null)
            {
                return null;
            }

            NodeView nodeView = new StateNodeView(node, TaskStreamerEditor.settings.stateNodeViewXml);

            Debug.Assert(nodeView is not null, $"{nameof(TaskGraphView)}: NodeView is null");
            return nodeView;
        }


        //TODO: OnExit나 OnEnter 대상으로만 
        public override void TryDisconnectParentToChild(NodeView parentNodeView) { }


        //TODO: 마찬가지로 OnExit나 OnEnter 대상으로만 
        public override void TryDisconnectChildToParent(NodeView childNodeView) { }
    }
}