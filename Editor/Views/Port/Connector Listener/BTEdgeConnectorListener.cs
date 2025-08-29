using TaskStreamer.BT;
using UnityEngine;

namespace TaskStreamer.Tool
{
    /// <summary>
    /// Represents a listener for handling edge connections specific to behavior trees
    /// within a graph-based editor environment.
    /// Provides functionality for creating and linking nodes during edge interactions
    /// and ensures the correct parent-child relationships within the behavior tree structure.
    /// </summary>
    public class BTEdgeConnectorListener : GraphEdgeConnectorListener
    {
        /// <summary>
        /// Creates a new node, establishes a connection from the original node to the newly created node,
        /// and links them within a graph at a specific position.
        /// </summary>
        /// <param name="sourceView">The original node view from which a connection is initiated.</param>
        /// <param name="newView">The newly created node view to which the connection is made.</param>
        /// <param name="position">The position where the new node will be created and linked to the original node.</param>
        protected override void CreationAndLinkAToB(NodeViewBase sourceView, NodeViewBase newView, Vector2 position)
        {
            ((BTView)_taskView.graphView).TryDisconnectParentToChild(sourceView);

            if (_taskView.graphView.TryConnectNodesByEdge(_taskView, sourceView, newView))
            {
                BehaviorTree behaviorTree = TaskStreamerEditor.Instance.currentGraph as BehaviorTree;
                Debug.Assert(behaviorTree is not null, "behaviorTree is null");
                behaviorTree.ConnectNodes((BehaviorNodeBase)sourceView.targetNode, (BehaviorNodeBase)newView.targetNode);
            }
        }


        /// <summary>
        /// Creates and links a new node to an original node in the graph, while ensuring the
        /// appropriate disconnection and reconnection logic for nodes in the behavior tree.
        /// </summary>
        /// <param name="newView">The node view representing the new node being connected.</param>
        /// <param name="sourceView">The node view representing the original source node.</param>
        /// <param name="position">The position in the graph where the new node is to be created and linked.</param>
        protected override void CreationAndLinkBToA(NodeViewBase newView, NodeViewBase sourceView, Vector2 position)
        {
            ((BTView)_taskView.graphView).TryDisconnectChildToParent(sourceView);

            if (_taskView.graphView.TryConnectNodesByEdge(_taskView, newView, sourceView))
            {
                BehaviorTree behaviorTree = TaskStreamerEditor.Instance.currentGraph as BehaviorTree;
                Debug.Assert(behaviorTree is not null, "behaviorTree is null");
                behaviorTree.ConnectNodes((BehaviorNodeBase)newView.targetNode, (BehaviorNodeBase)sourceView.targetNode);
            }
        }
    }
}