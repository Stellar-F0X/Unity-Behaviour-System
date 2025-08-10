using System.Collections.Generic;
using System.Linq;
using TaskStreamer.BT;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Pool;

namespace TaskStreamer.Tool
{
    public class BTViewProcessor : GraphViewProcessor
    {
        protected internal BTViewProcessor() { }


        public override bool TryConnectNodesByEdge(TaskGraphView view, NodeView connectionSource, NodeView connectionTarget, out Edge linkedEdge)
        {
            if (connectionSource is null || connectionTarget is null || connectionSource.outputPort is null || connectionTarget.inputPort is null)
            {
                linkedEdge = null;
                return false;
            }

            linkedEdge = connectionSource.outputPort.ConnectTo(connectionTarget.inputPort);

            if (connectionTarget is BehaviorNodeView behaviorNodeView)
            {
                behaviorNodeView.parentConnectionEdge = linkedEdge;
            }
            else
            {
                Debug.LogError("The target node is not a BehaviorNodeView. Connection failed.");
                return false;
            }

            view.AddElement(linkedEdge);
            return true;
        }


        public override void CreateAndConnectNodes(TaskGraphView graphView, Graph graph)
        {
            foreach (NodeBase node in graph.GetGraphIterator())
            {
                NodeView recreatedNodeView = this.RecreateNodeViewOnLoad(node);
                graphView.AddNewNodeView(recreatedNodeView);
            }

            foreach (NodeBase parentNodeBase in graph.GetGraphIterator())
            {
                if (parentNodeBase is not IChildNodeProvider provider || provider.childCount == 0)
                {
                    continue;
                }

                foreach (NodeBase child in provider.GetChildren())
                {
                    NodeView parentView = graphView.FindNodeView(parentNodeBase);
                    NodeView childView = graphView.FindNodeView(child);

                    this.TryConnectNodesByEdge(graphView, parentView, childView, out _);
                }
            }
        }


        //Filter important nodes
        public override void OnDeleteSelectionElements(List<ISelectable> selection)
        {
            if (selection is null || selection.Count == 0)
            {
                return;
            }

            for (int i = 0; i < selection.Count; ++i)
            {
                if (selection[i] is not NodeView view)
                {
                    continue;
                }

                BehaviorNodeBase node = (BehaviorNodeBase)view.targetNode;

                if (node.nodeType == BehaviorNodeType.Root)
                {
                    view.selected = false;
                    selection.RemoveAt(i);
                    break;
                }
            }
        }


        public override void NotifyNodePositionChanged(TaskGraphView graphView, List<GraphElement> elements)
        {
            if (elements is null || elements.Count == 0)
            {
                return;
            }

            foreach (Node nodeElement in graphView.nodes)
            {
                if (nodeElement is BehaviorNodeView view)
                {
                    view.SortChildren();
                }
            }
        }


        /// <summary>로딩 시 노드 데이터로부터 NodeView를 재생성합니다.</summary>
        public override NodeView RecreateNodeViewOnLoad(NodeBase node)
        {
            if (node == null)
            {
                return null;
            }

            NodeView nodeView = new BehaviorNodeView(node, TaskStreamerEditor.settings.behaviorNodeViewXml);

            Debug.Assert(nodeView is not null, $"{nameof(TaskGraphView)}: NodeView is null");

            return nodeView;
        }


        /// <summary> 자식 노드에서 부모 노드와의 연결을 끊는다. </summary>
        public override void TryDisconnectChildToParent(NodeView childNodeView)
        {
            if (childNodeView.inputPort is null || childNodeView.inputPort.connected == false)
            {
                return;
            }
            
            if (childNodeView is not BehaviorNodeView behaviorNodeView)
            {
                Debug.LogError("The target node is not a BehaviorNodeView. Connection failed.");
                return;
            }

            if (behaviorNodeView.parentConnectionEdge?.output.node is NodeView view)
            {
                BehaviorTree.DisconnectNodes((BehaviorNodeBase)view.targetNode, (BehaviorNodeBase)childNodeView.targetNode);
                view.outputPort.Disconnect(behaviorNodeView.parentConnectionEdge);

                List<GraphElement> edges = ListPool<GraphElement>.Get();
                edges.Add(behaviorNodeView.parentConnectionEdge);
                TaskStreamerEditor.Instance.view.DeleteElements(edges);
                ListPool<GraphElement>.Release(edges);
            }
        }


        public override void DisconnectNodesByEdge(Graph graph, Edge edge)
        {
            BehaviorTree tree = graph as BehaviorTree;

            if (tree is null)
            {
                return;
            }

            NodeView parentView = edge.output.node as NodeView;
            NodeView childView = edge.input.node as NodeView;

            if (parentView is null || childView is null)
            {
                return;
            }

            BehaviorTree.DisconnectNodes((BehaviorNodeBase)parentView.targetNode, (BehaviorNodeBase)childView.targetNode);
            edge.RemoveFromHierarchy();
        }


        public override void ConnectNodesByEdges(TaskGraphView graphView, Graph graphCollection, List<Edge> edges)
        {
            BehaviorTree tree = graphCollection as BehaviorTree;

            if (tree is null || edges.Count == 0)
            {
                return;
            }

            foreach (Edge edge in edges)
            {
                BehaviorNodeView parentView = edge.output.node as BehaviorNodeView;
                BehaviorNodeView childView = edge.input.node as BehaviorNodeView;

                if (parentView is null || childView is null)
                {
                    continue;
                }

                childView.parentConnectionEdge = edge;

                BehaviorTree.ConnectNodes((BehaviorNodeBase)parentView.targetNode, (BehaviorNodeBase)childView.targetNode);
            }
        }


        protected override TaskCreationWindowBase CreateGraphNodeCreationWindow()
        {
            return ScriptableObject.CreateInstance<BehaviorCreationWindow>();
        }


        /// <summary> 부모 노드에서 자식 노드와의 연결을 끊는다. </summary>
        public override void TryDisconnectParentToChild(NodeView parentNodeView)
        {
            if (parentNodeView.outputPort is null || parentNodeView.outputPort.connected == false)
            {
                return;
            }

            BehaviorNodeBase node = (BehaviorNodeBase)parentNodeView.targetNode;

            bool isSingleChildNode = false;
            
            isSingleChildNode |= node.nodeType is BehaviorNodeType.Decorator;
            isSingleChildNode |= node.nodeType is BehaviorNodeType.Root;

            // 부모 노드에서 Edge 연결을 시작할 경우로, 부모 노드가 하나의 자식만 가질 수 있으며, 이미 자식으로 연결된 노드가 있다면 그 노드와의 연결을 해제한다.
            if (isSingleChildNode == false)
            {
                return;
            }

            if (parentNodeView.outputPort.connections.First()?.input.node is not BehaviorNodeView existingChildView)
            {
                return;
            }
            
            BehaviorTree.DisconnectNodes((BehaviorNodeBase)parentNodeView.targetNode, (BehaviorNodeBase)existingChildView.targetNode);

            parentNodeView.outputPort.Disconnect(existingChildView.parentConnectionEdge);

            List<GraphElement> edges = ListPool<GraphElement>.Get();
            edges.Add(existingChildView.parentConnectionEdge);
            TaskStreamerEditor.Instance.view.DeleteElements(edges);
            ListPool<GraphElement>.Release(edges);
        }
    }
}