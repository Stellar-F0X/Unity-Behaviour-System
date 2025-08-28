using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Pool;

namespace TaskStreamer.Tool
{
    /// <summary>
    /// Represents a base class implementing the IEdgeConnectorListener interface for managing edge connection behaviors
    /// within a graph view in the TaskStreamer Editor. Classes derived from this listener handle
    /// specific functionality such as creating, linking, and managing connections between nodes.
    /// </summary>
    public abstract class GraphEdgeConnectorListener : IEdgeConnectorListener
    {
        /// Represents a listener for edge connection events in a graph view. This class provides the
        /// functionality to handle the creation and manipulation of edges, including connecting nodes
        /// within a graph structure and managing changes during graph editing.
        /// This abstract class implements the `IEdgeConnectorListener` interface, enabling it to
        /// interact with a `GraphView` and respond to edge-related events.
        /// Derive from this class to implement specific behaviors when edges are created or manipulated.
        /// Fields:
        /// - `_graphViewChange`: Represents the changes occurring in the graph view, including edges to create.
        /// - `_taskView`: Represents the `TaskGraphView` associated with the current editor instance.
        /// - `_pendingEdgesToCreate`: Maintains a list of edges pending creation.
        /// Responsibilities:
        /// - Handle edge drops on a port and in empty space.
        /// - Manage edge connections between nodes.
        /// - Allow node relationships creation and linking.
        /// - Define abstract methods for linking between new and original nodes.
        protected GraphEdgeConnectorListener()
        {
            _pendingEdgesToCreate = new List<Edge>();
            _taskView = TaskStreamerEditor.Instance.taskGraphView;
            _graphViewChange.edgesToCreate = this._pendingEdgesToCreate;
        }

        /// <summary>
        /// Represents a private instance of the <see cref="GraphViewChange"/> struct, which is used
        /// to keep track of graph view changes, such as edges to be created. It is configured with a
        /// reference to the current list of edges awaiting creation within the graph view.
        /// </summary>
        /// <remarks>
        /// This variable is primarily utilized during graph interactions, such as connecting nodes
        /// by edges, where it aids in managing and processing pending graph modifications.
        /// </remarks>
        private readonly GraphViewChange _graphViewChange;

        /// <summary>
        /// Reference to the <see cref="TaskGraphView"/> instance associated with the graph edge connector listener.
        /// This field is used to facilitate operations like node creation, connection, and linking within the task graph.
        /// </summary>
        protected readonly TaskGraphView _taskView;


        /// <summary>
        /// A private list that holds the edges to be created in the graph view.
        /// This list is utilized during drag-and-drop operations within the editor to
        /// temporarily store new edges that need to be added during graph modification.
        /// </summary>
        private List<Edge> _pendingEdgesToCreate;


        /// Handles the drop action when an edge is dropped outside a valid port in the graph.
        /// Creates a new node and links it to either the source or target node depending
        /// on the drop context.
        /// <param name="edge">The edge that was dropped.</param>
        /// <param name="pos">The position in the graph where the edge was dropped.</param>
        public virtual void OnDropOutsidePort(Edge edge, Vector2 pos)
        {
            edge.isGhostEdge = false;

            if (_taskView is null || TaskStreamerEditor.canEditGraph == false)
            {
                return;
            }

            if (edge.input is not null && edge.input.node is NodeViewBase childView) //Create and link new parent node
            {
                _taskView.OpenContextualMenuWindow(pos, newParentView => this.CreationAndLinkBToA(newParentView, childView, pos));
            }
            else if (edge.output is not null && edge.output.node is NodeViewBase parentView) //Create and link new child node
            {
                _taskView.OpenContextualMenuWindow(pos, newChildView => this.CreationAndLinkAToB(parentView, newChildView, pos));
            }
        }


        // Referenced: https://github.com/thekiwicoder0/UnityBehaviourTreeEditor/blob/main/Editor/NodePort.cs
        // Copyright (c) 2021 Original Author
        // Licensed under the MIT License. See LICENSE file in the root for details.
        /// Handles the logic when an edge is dropped onto a graph view.
        /// <param name="graphView">The graph view where the edge drop occurred.</param>
        /// <param name="edge">The edge being dropped onto the graph view.</param>
        public virtual void OnDrop(GraphView graphView, Edge edge)
        {
            List<GraphElement> edgesToDelete = ListPool<GraphElement>.Get();

            edge.isGhostEdge = false;

            _pendingEdgesToCreate.Clear();
            _pendingEdgesToCreate.Add(edge);

            if (edge.input.capacity == Port.Capacity.Single)
            {
                this.AddToDeleteList(edgesToDelete, edge.input.connections, edge);
            }

            if (edge.output.capacity == Port.Capacity.Single)
            {
                this.AddToDeleteList(edgesToDelete, edge.output.connections, edge);
            }

            if (edgesToDelete.Count > 0)
            {
                graphView.DeleteElements(edgesToDelete);
            }

            ListPool<GraphElement>.Release(edgesToDelete);

            if (graphView.graphViewChanged is not null)
            {
                _pendingEdgesToCreate = graphView.graphViewChanged.Invoke(_graphViewChange).edgesToCreate;
            }

            foreach (Edge e in _pendingEdgesToCreate)
            {
                graphView.AddElement(e);
                edge.input.Connect(e);
                edge.output.Connect(e); 
            }
        }


        /// Adds a list of edges to be deleted, excluding the specified target edge.
        /// <param name="edgeToDeleteList">The list where the edges to be removed are added.</param>
        /// <param name="edges">A collection of edges to evaluate for deletion.</param>
        /// <param name="targetEdge">The edge that should be excluded from the deletion.</param>
        protected virtual void AddToDeleteList(List<GraphElement> edgeToDeleteList, IEnumerable<Edge> edges, Edge targetEdge)
        {
            edgeToDeleteList.AddRange(edges.Where(edge => edge is not null && edge != targetEdge));
        }

        
        /// Creates and links the original node to the newly created node.
        /// For Behavior Tree (BT), it establishes a parent-child relationship.
        /// For Finite State Machine (FSM), it establishes a state transition relationship.
        /// </summary>
        /// <param name="sourceView">The original node that serves as the source of the link.</param>
        /// <param name="newView">The newly created node to be linked to the original node.</param>
        /// <param name="position">The position in the graph view where the new node is placed.</param>
        protected abstract void CreationAndLinkAToB(NodeViewBase sourceView, NodeViewBase newView, Vector2 position);


        /// <summary>
        /// Creates and links a relationship from the new node to the original/source node in a graph structure.
        /// </summary>
        /// <param name="newView">The view representing the new node that is being added to the graph.</param>
        /// <param name="sourceView">The view representing the original/source node that is already present in the graph.</param>
        /// <param name="position">The position at which the new node is being created and linked.</param>
        protected abstract void CreationAndLinkBToA(NodeViewBase newView, NodeViewBase sourceView, Vector2 position);
    }
}