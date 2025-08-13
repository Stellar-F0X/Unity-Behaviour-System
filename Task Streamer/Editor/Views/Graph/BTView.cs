using System.Collections.Generic;
using System.Linq;
using TaskStreamer.BT;
using TaskStreamer.Utility;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Pool;

namespace TaskStreamer.Tool
{
    public class BTView : GraphViewBase
    {
        protected internal BTView() { }


        public override bool TryConnectNodesByEdge(TaskGraphView view, NodeViewBase connectionSource, NodeViewBase connectionTarget)
        {
            if (connectionSource is null || connectionTarget is null || connectionSource.outputPort is null || connectionTarget.inputPort is null)
            {
                return false;
            }

            Edge linkedEdge = connectionSource.outputPort.ConnectTo(connectionTarget.inputPort);

            if (connectionTarget is BehaviorNodeView behaviorNodeView)
            {
                behaviorNodeView.connectionEdge[UGUID.Empty] = linkedEdge;
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
            // 모든 노드뷰 생성
            foreach (NodeBase node in graph.GetIterator(GraphIteratorType.LS))
            {
                NodeViewBase recreatedNodeView = this.RecreateNodeViewOnLoad(node);
                graphView.AddNewNodeView(recreatedNodeView);
            }

            // 부모-자식 관계에 따른 노드 연결
            foreach (NodeBase parentNodeBase in graph.GetIterator(GraphIteratorType.LS))
            {
                if (parentNodeBase is not IChildProvider provider || provider.childCount == 0)
                {
                    continue;
                }

                foreach (NodeBase child in provider.GetChildren())
                {
                    NodeViewBase parentView = graphView.FindNodeView(parentNodeBase);
                    NodeViewBase childView = graphView.FindNodeView(child);

                    this.TryConnectNodesByEdge(graphView, parentView, childView);
                }
            }
        }


        /// <summary>중요한 노드들을 선택에서 제외합니다 (Root 노드 등)</summary>
        public override void FilterSelectionElements(List<ISelectable> selection)
        {
            if (selection is null || selection.Count == 0)
            {
                return;
            }

            for (int i = 0; i < selection.Count; ++i)
            {
                if (selection[i] is not NodeViewBase view)
                {
                    continue;
                }

                BehaviorNodeBase node = (BehaviorNodeBase)view.targetNode;

                if (node.nodeType != BehaviorNodeType.Root)
                {
                    continue;
                }

                view.selected = false;
                selection.RemoveAt(i);
                break;
            }
        }


        public override void NotifyNodePositionChanged(TaskGraphView graphView, List<GraphElement> elements)
        {
            if (elements is null || elements.Count == 0)
            {
                return;
            }

            foreach (BehaviorNodeView nodeElement in graphView.nodes)
            {
                nodeElement.SortChildren();
            }
        }


        /// <summary>로딩 시 노드 데이터로부터 NodeView를 재생성합니다</summary>
        public override NodeViewBase RecreateNodeViewOnLoad(NodeBase node)
        {
            if (node == null)
            {
                return null;
            }

            NodeViewBase nodeView = new BehaviorNodeView(node, TaskStreamerEditor.settings.behaviorNodeViewXml);

            Debug.Assert(nodeView is not null, $"{nameof(TaskGraphView)}: NodeViewBase is null");

            return nodeView;
        }


        /// <summary>자식 노드에서 부모 노드와의 연결을 해제합니다</summary>
        public override void TryDisconnectChildToParent(NodeViewBase childNodeView)
        {
            if (this.IsValidConnectionForDisconnect(childNodeView, true) == false)
            {
                return;
            }

            Edge parentConnectionEdge = childNodeView.connectionEdge[UGUID.Empty];

            if (parentConnectionEdge.output.node is NodeViewBase view)
            {
                this.DisconnectAndDeleteEdge(view, childNodeView, parentConnectionEdge, view.outputPort);
            }
        }


        public override void DisconnectNodesByEdge(Graph graph, Edge edge)
        {
            BehaviorTree tree = graph as BehaviorTree;

            if (tree is null)
            {
                return;
            }

            NodeViewBase parentView = edge.output.node as NodeViewBase;
            NodeViewBase childView = edge.input.node as NodeViewBase;

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
                if (edge.output.node is not BehaviorNodeView parentView || edge.input.node is not BehaviorNodeView childView)
                {
                    continue;
                }

                childView.connectionEdge[UGUID.Empty] = edge;

                BehaviorTree.ConnectNodes((BehaviorNodeBase)parentView.targetNode, (BehaviorNodeBase)childView.targetNode);
            }
        }


        protected override CreationWindow CreateGraphNodeCreationWindow(TaskGraphView graphView)
        {
            ICreationWindow window = CreationWindow.GetCreationWindow("Behavior Tree");

            window.AddFactoryModule(new NodeFactoryModule(graphView, typeof(ActionNode), "Action"))
                  .AddFactoryModule(new NodeFactoryModule(graphView, typeof(DecoratorNode), "Decorator"))
                  .AddFactoryModule(new NodeFactoryModule(graphView, typeof(CompositeNode), "Composite"))
                  .AddFactoryModule(new NodeFactoryModule(graphView, typeof(SubGraphNode), "Graph"))
                  .AddFactoryModule(new NodeGroupFactoryModule(graphView, typeof(NodeGroup), "Utility"));

            return window as CreationWindow;
        }


        /// <summary>부모 노드에서 자식 노드와의 연결을 해제합니다</summary>
        public override void TryDisconnectParentToChild(NodeViewBase parentNodeView)
        {
            if (this.IsValidConnectionForDisconnect(parentNodeView, false) == false)
            {
                return;
            }

            BehaviorNodeBase node = (BehaviorNodeBase)parentNodeView.targetNode;

            // 단일 자식만 가질 수 있는 노드 타입 확인
            if (this.IsSingleChildNode(node) == false)
            {
                return;
            }

            if (parentNodeView.outputPort.connections.First()?.input.node is not BehaviorNodeView existingChildView)
            {
                return;
            }

            Edge parentConnectionEdge = parentNodeView.connectionEdge[UGUID.Empty];
            this.DisconnectAndDeleteEdge(parentNodeView, existingChildView, parentConnectionEdge, parentNodeView.outputPort);
        }


        /// <summary>연결 해제가 가능한 상태인지 확인합니다</summary>
        private bool IsValidConnectionForDisconnect(NodeViewBase nodeView, bool checkInputPort)
        {
            if (nodeView is null)
            {
                return false;
            }

            if (checkInputPort)
            {
                return nodeView.inputPort is not null && nodeView.inputPort.connected;
            }
            else
            {
                return nodeView.outputPort is not null && nodeView.outputPort.connected;
            }
        }


        /// <summary>단일 자식만 가질 수 있는 노드 타입인지 확인합니다</summary>
        private bool IsSingleChildNode(BehaviorNodeBase node)
        {
            return node.nodeType is BehaviorNodeType.Decorator or BehaviorNodeType.Root;
        }


        /// <summary>노드 연결을 해제하고 에지를 삭제합니다</summary>
        private void DisconnectAndDeleteEdge(NodeViewBase parentView, NodeViewBase childView, Edge edge, Port port)
        {
            BehaviorTree.DisconnectNodes((BehaviorNodeBase)parentView.targetNode, (BehaviorNodeBase)childView.targetNode);
            port.Disconnect(edge);

            List<GraphElement> edges = ListPool<GraphElement>.Get();
            edges.Add(edge);
            TaskStreamerEditor.Instance.view.DeleteElements(edges);
            ListPool<GraphElement>.Release(edges);
        }
    }
}