using System.Collections.Generic;
using System.Linq;
using TaskStreamer.Runtime;
using TaskStreamer.Runtime.BT;
using TaskStreamer.Runtime.Utility;
using UnityEditor.Experimental.GraphView;
using UnityEngine.Assertions;
using UnityEngine.Pool;

namespace TaskStreamer.Tool
{
    /// <summary>
    /// Represents a custom behavior tree view derived from the base class <see cref="GraphViewBase"/>.
    /// Provides functionality for node management, connection, and other specialized tasks related to behavior trees.
    /// </summary>
    internal class BTView : GraphViewBase
    {
        /// <summary>
        /// Represents a specialized graph view for managing Behavior Tree (BT) structures, providing node-based manipulation functionalities.
        /// </summary>
        protected internal BTView() { }


        /// <summary>
        /// Attempts to connect two nodes in the task graph by creating an edge between them.
        /// </summary>
        /// <param name="graphView">The task graph view where the connection is being made.</param>
        /// <param name="sourceView">The sourceView node from which the connection originates.</param>
        /// <param name="targetView">The target node to which the connection is directed.</param>
        /// <returns>
        /// True if the connection was successfully established; otherwise, false.
        /// </returns>
        public override bool TryConnectNodesByEdge(TaskGraphView graphView, NodeViewBase sourceView, NodeViewBase targetView)
        {
            Assert.IsNotNull(graphView, $"{nameof(TaskGraphView)}: TaskGraphView is null");
            Assert.IsNotNull(sourceView, $"{nameof(TaskGraphView)}: sourceView is null");
            Assert.IsNotNull(targetView, $"{nameof(TaskGraphView)}: targetView is null");
            
            Assert.IsNotNull(sourceView.outputPort, $"{nameof(TaskGraphView)}: sourceView's outputPort is null");
            Assert.IsNotNull(targetView.inputPort, $"{nameof(TaskGraphView)}: targetView's inputPort is null");

            BTEdge linkedEdge = sourceView.outputPort.ConnectTo<BTEdge>(targetView.inputPort);
            BehaviorNodeView connectionTargetView = targetView as BehaviorNodeView;
            Assert.IsNotNull(connectionTargetView, "The target node is not a BehaviorNodeView. Connection failed.");
            
            connectionTargetView.connectionEdges[UGUID.Empty] = linkedEdge;
            graphView.AddElement(linkedEdge);
            return true;
        }


        /// <summary>
        /// Creates node views for all nodes in the given graph and establishes connections between them
        /// based on the parent-child relationships defined in the graph.
        /// </summary>
        /// <param name="graphView">The TaskGraphView instance where the nodes and connections will be added.</param>
        /// <param name="graph">The graph containing the nodes and parent-child relationships to be processed.</param>
        public override void CreateAndConnectNodes(TaskGraphView graphView, Graph graph)
        {
            Assert.IsNotNull(graphView, $"{nameof(TaskGraphView)}: TaskGraphView is null");
            
            // 모든 노드뷰 생성
            foreach (NodeBase node in graph.GetIterator(GraphIteratorType.LS))
            {
                NodeViewBase recreatedNodeView = this.RecreateNodeViewOnLoad(node);
                graphView.AddNewNodeView(recreatedNodeView);
            }

            // 부모-자식 관계에 따른 노드 연결
            foreach (NodeBase parentNodeBase in graph.GetIterator(GraphIteratorType.LS))
            {
                if (parentNodeBase is IChildNode provider && provider.childCount != 0)
                {
                    foreach (NodeBase child in provider.GetChildren())
                    {
                        NodeViewBase parentView = graphView.FindNodeView(parentNodeBase);
                        NodeViewBase childView = graphView.FindNodeView(child);

                        this.TryConnectNodesByEdge(graphView, parentView, childView);
                    }
                }
            }
        }


        /// <summary>
        /// Excludes important nodes from the selection (e.g., Root nodes).
        /// </summary>
        /// <param name="selection">A list of selectable elements to be filtered. Elements representing certain nodes, such as Root nodes, are deselected and removed from this list.</param>
        public override void FilterSelectionElements(List<ISelectable> selection)
        {
            Assert.IsNotNull(selection, $"{nameof(TaskGraphView)}: selection is null");
            
            if (selection.Count == 0)
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


        /// <summary>
        /// Notifies that the position of certain nodes has changed and updates their order accordingly.
        /// </summary>
        /// <param name="graphView">The graph view containing the nodes whose positions have changed.</param>
        /// <param name="elements">The list of graph elements whose positions have changed. If null or empty, the operation will be skipped.</param>
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


        /// <summary>Recreates a NodeView from the given NodeBase data when loading.</summary>
        /// <param name="node">The NodeBase instance containing the data required to create the NodeView.</param>
        /// <returns>A new NodeViewBase instance representing the recreated NodeView, or null if the input node is null.</returns>
        public override NodeViewBase RecreateNodeViewOnLoad(NodeBase node)
        {
            Assert.IsNotNull(node, $"{nameof(TaskGraphView)}: NodeBase is null");

            NodeViewBase nodeView = BehaviorNodeView.Create(node, TSUIElementSettings.instance.BTNode);
            Assert.IsNotNull(nodeView, $"{nameof(TaskGraphView)}: NodeViewBase is null");
            return nodeView;
        }


        /// <summary>Disconnects two nodes in the behavior tree graph using the provided edge and removes the edge from the hierarchy.</summary>
        /// <param name="graph">The graph where the disconnection will occur. Must be of type <see cref="BehaviorTree"/>.</param>
        /// <param name="edge">The edge connecting the nodes to be disconnected.</param>
        public override void DisconnectNodesByEdge(Graph graph, Edge edge)
        {
            BehaviorTree tree = graph as BehaviorTree;
            Assert.IsNotNull(tree, $"{nameof(TaskGraphView)}: BehaviorTree is null");

            BehaviorNodeBase parentNode = edge.output.node.GetNodeByView<BehaviorNodeBase>();
            BehaviorNodeBase childNode = edge.input.node.GetNodeByView<BehaviorNodeBase>();

            ((BehaviorTree)graph).DisconnectNodes(parentNode, childNode);
            edge.RemoveFromHierarchy();
        }


        /// <summary>
        /// Connects nodes within a graph using specified edges.
        /// </summary>
        /// <param name="graphView">The graph view that contains the nodes.</param>
        /// <param name="graph">The graph instance where the connections are applied.</param>
        /// <param name="edges">A list of edges that define the connections between nodes.</param>
        public override void ConnectNodesByEdges(TaskGraphView graphView, Graph graph, List<Edge> edges)
        {
            Assert.IsNotNull(graphView, $"{nameof(TaskGraphView)}: TaskGraphView is null");
            
            if (edges.Count == 0)
            {
                return;
            }
            
            BehaviorTree tree = graph as BehaviorTree;
            Assert.IsNotNull(tree, $"{nameof(TaskGraphView)}: behaviorTree is null");

            foreach (Edge edge in edges)
            {
                BehaviorNodeBase parentNode = edge.output.node.GetNodeByView<BehaviorNodeBase>();
                BehaviorNodeBase childNode = edge.input.node.GetNodeByView<BehaviorNodeBase>();

                if (parentNode is not null && childNode is not null && edge.output.node is NodeViewBase view)
                {
                    view.connectionEdges[UGUID.Empty] = edge;
                    tree.ConnectNodes(parentNode, childNode);
                }
            }
        }


        /// <summary>
        /// Creates a graph node creation window for adding various types of nodes into the given task graph view.
        /// </summary>
        /// <param name="graphView">The task graph view for which the node creation window will be created.</param>
        /// <returns>A creation window instance populated with factory modules for creating different node types.</returns>
        public override BindingWindow CreateGraphNodeCreationWindow(TaskGraphView graphView)
        {
            return BindingWindowBuilder.GetBuilder("Behavior Tree", reuse: true)
                                       .AddFactoryModule(
                                           () => new NodeFactoryModule<ActionNode>(graphView, "Action"),
                                           () => new RelatedTypeTreeProvider(true))
                                       .AddFactoryModule(
                                           () => new NodeFactoryModule<DecoratorNode>(graphView, "Decorator"),
                                           () => new RelatedTypeTreeProvider(true))
                                       .AddFactoryModule(
                                           () => new NodeFactoryModule<CompositeNode>(graphView, "Composite"),
                                           () => new RelatedTypeTreeProvider(true))
                                       .AddFactoryModule(
                                           () => new NodeFactoryModule<SubGraphNode>(graphView, "Graph"),
                                           () => new RelatedTypeTreeProvider(true))
                                       .AddFactoryModule(
                                           () => new NodeGroupFactoryModule<NodeGroup>(graphView, "Utility"),
                                           () => new RelatedTypeTreeProvider(false))
                                       .AddFactoryModule(
                                           () => new ScriptCreationFactoryModule<CreateNewBTNodeScriptCommandBase>(graphView, "New Node"),
                                           () => new RelatedTypeTreeProvider(true))
                                       .Build();
        }


        /// <summary>Disconnects a child node from its parent node.</summary>
        /// <param name="childNodeView">The child node to disconnect from its parent node.</param>
        public void TryDisconnectChildToParent(NodeViewBase childNodeView)
        {
            if (this.IsValidConnectionForDisconnect(childNodeView, true) == false)
            {
                return;
            }

            Edge parentConnectionEdge = childNodeView.connectionEdges[UGUID.Empty];

            if (parentConnectionEdge.output.node is NodeViewBase view)
            {
                this.DisconnectAndDeleteEdge(view, childNodeView, parentConnectionEdge, view.outputPort);
            }
        }


        /// <summary>
        /// Disconnects the connection between a parent node and its child node.
        /// </summary>
        /// <param name="parentNodeView">The parent node from which the connection to its child will be removed.</param>
        public void TryDisconnectParentToChild(NodeViewBase parentNodeView)
        {
            if (this.IsValidConnectionForDisconnect(parentNodeView, false) == false)
            {
                return;
            }

            // 단일 자식만 가질 수 있는 노드 타입 확인
            if (this.IsSingleChildNode((BehaviorNodeBase)parentNodeView.targetNode) == false)
            {
                return;
            }

            if (parentNodeView.outputPort.connections.First()?.input.node is not BehaviorNodeView existingChildView)
            {
                return;
            }

            Edge parentConnectionEdge = existingChildView.connectionEdges[UGUID.Empty];
            this.DisconnectAndDeleteEdge(parentNodeView, existingChildView, parentConnectionEdge, parentNodeView.outputPort);
        }


        /// <summary>Determines whether the connection can be safely disconnected based on the specified conditions.</summary>
        /// <param name="nodeView">The node view that represents the connection point to evaluate.</param>
        /// <param name="checkInputPort">Indicates whether the check should be performed on the input port (true) or the output port (false) of the node.</param>
        /// <returns>True if the connection is valid for disconnection; otherwise, false.</returns>
        private bool IsValidConnectionForDisconnect(NodeViewBase nodeView, bool checkInputPort)
        {
            Assert.IsNotNull(nodeView, $"{nameof(TaskGraphView)}: NodeViewBase is null");

            if (checkInputPort)
            {
                return nodeView.inputPort is not null && nodeView.inputPort.connected;
            }
            else
            {
                return nodeView.outputPort is not null && nodeView.outputPort.connected;
            }
        }


        /// <summary>
        /// Checks if a node is of a type that can only have a single child (e.g., Decorator or Root).
        /// </summary>
        /// <param name="node">The node to be checked.</param>
        /// <returns>Returns true if the node can only have a single child; otherwise, false.</returns>
        private bool IsSingleChildNode(BehaviorNodeBase node)
        {
            return node.nodeType is BehaviorNodeType.Decorator or BehaviorNodeType.Root;
        }


        /// <summary>
        /// Disconnects a parent node from a child node and deletes the specified edge.
        /// </summary>
        /// <param name="parentView">The parent node view from which the connection will be removed.</param>
        /// <param name="childView">The child node view to be disconnected.</param>
        /// <param name="edge">The edge representing the connection to be deleted.</param>
        /// <param name="port">The port on the parent node associated with the connection.</param>
        private void DisconnectAndDeleteEdge(NodeViewBase parentView, NodeViewBase childView, Edge edge, Port port)
        {
            BehaviorTree behaviorTree = TSEditor.Instance.currentGraph as BehaviorTree;
            Assert.IsNotNull(behaviorTree, $"{nameof(TaskGraphView)}: BehaviorTree is null");

            behaviorTree.DisconnectNodes((BehaviorNodeBase)parentView.targetNode, (BehaviorNodeBase)childView.targetNode);
            port.Disconnect(edge);

            List<GraphElement> edges = ListPool<GraphElement>.Get();
            edges.Add(edge);
            TSEditor.Instance.taskGraphView.DeleteElements(edges);
            ListPool<GraphElement>.Release(edges);
        }
    }
}