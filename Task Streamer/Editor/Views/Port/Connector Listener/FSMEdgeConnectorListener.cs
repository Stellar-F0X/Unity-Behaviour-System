using TaskStreamer.FSM;
using UnityEngine;

namespace TaskStreamer.Tool
{
    /// <summary>
    /// FSMEdgeConnectorListener is a class that provides logic for handling edge
    /// connections specific to Finite State Machines (FSM) within a graph visualizer.
    /// It extends the functionality of the abstract GraphEdgeConnectorListener class.
    /// </summary>
    /// <remarks>
    /// FSMEdgeConnectorListener implements methods to create and link graph nodes
    /// and ensures that the connection logic adheres to the rules of a Finite State Machine.
    /// It utilizes the FSMView and StateMachine classes to manage the relationships between states.
    /// </remarks>
    /// <inheritDoc cref="GraphEdgeConnectorListener"/>
    public class FSMEdgeConnectorListener : GraphEdgeConnectorListener
    {
        /// <summary>
        /// Disconnects the source node from the original connection, connects the source node to a newly created node,
        /// and then establishes the connection through a graph edge.
        /// </summary>
        /// <param name="sourceView">The source node to be disconnected and linked to the new node.</param>
        /// <param name="newView">The newly created node to be linked with the source node.</param>
        /// <param name="position">The position where the new node is created in the graph view.</param>
        protected override void CreationAndLinkOriginalToNew(NodeViewBase sourceView, NodeViewBase newView, Vector2 position)
        {
            ((FSMView)_taskView.graphView).TryDisconnectSourceToOriginal(sourceView);

            StateMachine stateMachine = TaskStreamerEditor.Instance.currentGraph as StateMachine;
            Debug.Assert(stateMachine is not null, "stateMachine is null");

            stateMachine.ConnectStates((StateBase)sourceView.targetNode, (StateBase)newView.targetNode);

            _taskView.graphView.TryConnectNodesByEdge(_taskView, sourceView, newView);
        }


        /// <summary>
        /// Creates a new connection between two nodes by linking the new node to the original source node.
        /// </summary>
        /// <param name="newView">The view representation of the newly created node.</param>
        /// <param name="sourceView">The view representation of the source node to be linked.</param>
        /// <param name="position">The position in the graph where the connection is established.</param>
        protected override void CreationAndLinkNewToOriginal(NodeViewBase newView, NodeViewBase sourceView, Vector2 position)
        {
            ((FSMView)_taskView.graphView).TryDisconnectSourceToOriginal(sourceView);
            
            StateMachine stateMachine = TaskStreamerEditor.Instance.currentGraph as StateMachine;
            Debug.Assert(stateMachine is not null, "stateMachine is null");
            
            stateMachine.ConnectStates((StateBase)newView.targetNode, (StateBase)sourceView.targetNode);

            _taskView.graphView.TryConnectNodesByEdge(_taskView, newView, sourceView);
        }
    }
}