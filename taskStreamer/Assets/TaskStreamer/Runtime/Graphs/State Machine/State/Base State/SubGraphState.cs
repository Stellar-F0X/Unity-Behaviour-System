using System;
using TaskStreamer.Utility;
using Unity.Properties;
using UnityEngine;

namespace TaskStreamer.FSM
{
    /// <summary>
    /// Represents a specialized type of state node in a finite state machine (FSM)
    /// that encapsulates the functionality of a subgraph.
    /// This abstract base class is designed specifically for handling operations
    /// related to subgraphs within a broader graph structure.
    /// </summary>
    /// <remarks>
    /// Each instance of <see cref="SubGraphState"/> corresponds to an individual subgraph,
    /// defined by a unique identifier. Subclasses of this class are required to implement
    /// the behavior and type of the subgraph being used.
    /// </remarks>
    [Serializable]
    internal abstract class SubGraphState : StateBase, ISubGraphProvider
    {
        /// <summary>
        /// Represents a private instance of a subgraph associated with the current state.
        /// </summary>
        /// <remarks>
        /// This variable holds the reference to a subgraph that belongs to the state.
        /// It is initialized during the state awakening process and is critical for
        /// managing the subgraph's lifecycle, including resetting and updating its state
        /// during transitions and updates in the finite state machine.
        /// </remarks>
        [DontCreateProperty]
        private Graph _subGraph;

        /// <summary>
        /// Represents the unique identifier (GUID) of a sub-graph associated with this state.
        /// </summary>
        /// <remarks>
        /// The identifier is used to reference a specific sub-graph within the system, enabling
        /// interactions and transitions within the finite state machine framework.
        /// </remarks>
        [SerializeField, DontCreateProperty]
        private UGUID _subGraphGuid;


        /// <summary>
        /// Represents the type of the current state node in the finite state machine (FSM).
        /// </summary>
        /// <remarks>
        /// This property is used to distinguish the category of state nodes within a state machine,
        /// such as regular action states, entry/exit points, or composite states that contain sub-graphs.
        /// In the context of <see cref="SubGraphState"/>, this property always returns <see cref="StateNodeType.SubGraph"/>.
        /// </remarks>
        public override StateNodeType nodeType
        {
            get { return StateNodeType.SubGraph; }
        }


        /// <summary>
        /// Gets or sets the unique identifier (GUID) for the subgraph associated with this state.
        /// </summary>
        /// <remarks>
        /// The <c>subGraphGuid</c> property is used to identify and reference a specific subgraph within
        /// the system. It is of type <see cref="UGUID"/>, which is a serializable Unity-specific GUID
        /// implementation. This property is crucial for linking and managing subgraph-related functionality.
        /// The subgraph identified by this GUID must exist and be retrievable during the lifecycle of the state.
        /// During state awakening, the subgraph is fetched using this GUID. An assertion ensures that the
        /// subgraph is valid and correctly associated with this state.
        /// </remarks>
        public UGUID subGraphGuid
        {
            get { return _subGraphGuid; } 
            
            set { _subGraphGuid = value; } 
        }


        /// <summary>
        /// Represents the type of the sub-graph associated with the state.
        /// </summary>
        /// <remarks>
        /// This property is used to define the type of the sub-graph (e.g., Behavior Tree, Finite State Machine, or Goal-Oriented Action Planning)
        /// as specified in the <see cref="TaskStreamer.GraphType"/> enumeration.
        /// It is implemented in derived classes to distinguish the specific sub-graph type associated with the state.
        /// </remarks>
        /// <value>
        /// A <see cref="TaskStreamer.GraphType"/> value that indicates the type of the sub-graph.
        /// </value>
        public abstract GraphType subGraphType
        {
            get;
        }


        /// <summary>
        /// Called when the state is awakened. This method is responsible for assigning
        /// the subGraph by retrieving it from the graph asset using the subGraphGuid.
        /// Ensures that the subGraph is properly initialized and asserts its existence
        /// for error handling.
        /// </summary>
        public override void OnAwake()
        {
            _subGraph = streamer.graphAsset.GetGraph(subGraphGuid); 

            Debug.Assert(_subGraph != null, "SubGraph not found"); 
        }


        /// <summary>
        /// Executes when the state is entered and ensures the associated sub-graph is properly initialized.
        /// </summary>
        /// <remarks>
        /// This method resets the sub-graph when the state is entered. If the sub-graph is null, an error message is logged.
        /// Additionally, this method sets the blockTransition property to true, potentially preventing state transitions
        /// until a specific condition is met or additional logic is executed.
        /// </remarks>
        /// <exception cref="System.NullReferenceException">
        /// Throws an exception if the sub-graph is not initialized before entering the state.
        /// </exception>
        protected override void OnEnter()
        {
            if (_subGraph is null)
            {
                Debug.LogError($"{name} {nameof(OnEnter)}: SubGraph is null");
                return;
            }

            _subGraph.ResetGraph();

            this.blockTransition = true;
        }


        /// Handles the update logic specific to the state.
        /// This method is called during the update cycle of the state machine.
        /// If the `_subGraph` is null, it logs an error. Otherwise, it updates the graph
        /// using the `UpdateGraph()` method. If the graph's status is not `Status.Running`,
        /// the `blockTransition` property is set to false.
        /// This method is used to control the internal behavior of a state's graph
        /// and manage its transition blocking status dynamically.
        protected override void OnUpdate()
        {
            if (_subGraph is null)
            {
                Debug.LogError($"{name} {nameof(OnUpdate)}: SubGraph is null");
                return;
            }

            if (_subGraph.UpdateGraph() != Status.Running)
            {
                this.blockTransition = false;
            }
        }


        /// <summary>
        /// Executes any necessary cleanup or termination logic when exiting the state.
        /// </summary>
        /// <remarks>
        /// This method ensures that the associated subgraph is properly stopped during the exit process.
        /// If the subgraph is null, an error message will be logged.
        /// </remarks>
        protected override void OnExit()
        {
            if (_subGraph is null)
            {
                Debug.LogError($"{name} {nameof(OnExit)}: SubGraph is null");
                return;
            }

            _subGraph.StopGraph();
        }
    }
}