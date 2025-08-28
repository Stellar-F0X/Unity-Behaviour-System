using System;
using System.Collections.Generic;
using System.Linq;
using TaskStreamer.Utility;
using Unity.Properties;
using UnityEditor;
using UnityEngine;
using ObjectFactory = TaskStreamer.Utility.ObjectFactory;

namespace TaskStreamer.FSM
{
    [Serializable]
    public partial class StateMachine : Graph
    {
        private StateMachine(string graphName, GraphAsset graphAsset) : base(graphName, graphAsset) { }

        [SerializeReference, DontCreateProperty]
        private StateBase _current;

        [SerializeReference, DontCreateProperty]
        private StateBase _any;

        [SerializeReference, DontCreateProperty]
        private StateBase _exit;

        private bool _blockAllTransition;


        public override GraphType graphType
        {
            get { return GraphType.FSM; }
        }

        public bool blockAllTransition
        {
            get { return _blockAllTransition; }

            set { _blockAllTransition = value; }
        }


#if UNITY_EDITOR
        public static StateMachine CreateGraph(string graphName, GraphAsset graphAsset)
        {
            StateMachine graph = new StateMachine(graphName, graphAsset);

            graph.entry = graph.CreateAndAddNodeToList("Enter", typeof(EnterState), new Vector2Int(0, 0)) as StateBase;
            graph._exit = graph.CreateAndAddNodeToList("Exit", typeof(ExitState), new Vector2Int(0, 200)) as StateBase;
            graph._any = graph.CreateAndAddNodeToList("Any", typeof(AnyState), new Vector2Int(-230, 0)) as StateBase;
            graph._current = graph.entry as StateBase;

            return graph;
        }
#endif


        public override IGraphIterator GetIterator(GraphIteratorType iteratorType)
        {
            switch (iteratorType)
            {
                case GraphIteratorType.LS: return new Graph.CommonLSIterator(this);

                case GraphIteratorType.BFS: return new StateMachine.BFSIterator(this);
            }

            throw new NotImplementedException("BreadthFirstSearch iterator is not implemented for StateMachine.");
        }


        internal override void InitializeOnEnterRuntime(TaskStreamer streamer)
        {
            foreach (StateBase node in this.GetIterator(GraphIteratorType.LS))
            {
                node.streamer = streamer;
                node.machine = this;
            }

            _current = _nodeLookup[_current.guid] as StateBase;
            Debug.Assert(_current != null, "current node is null.");

            _exit = _nodeLookup[_exit.guid] as StateBase;
            Debug.Assert(_exit != null, "exit node is null.");

            _any = _nodeLookup[_any.guid] as StateBase;
            Debug.Assert(_any != null, "any node is null.");
        }
        

        internal void ChangeState(NodeBase nextNode)
        {
            if (_current == nextNode)
            {
                return;
            }
            
            if (_current != null)
            {
                _current.ExitNode();
            }
            
            _current = (StateBase)nextNode;
            _current.EnterNode();
        }


        internal override Status UpdateGraph()
        {
            if (_current == null)
            {
                return Status.Failure;
            }

            _current.UpdateNode();

            if (_current.guid == _exit.guid)
            {
                return Status.Success;
            }

            if (this.TryGetNextState(out NodeBase nextState))
            {
                this.ChangeState(nextState);
            }

            return Status.Running;
        }


        internal override void ResetGraph()
        {
            if (entry.guid.IsEmpty())
            {
                Debug.LogError($"{typeof(StateMachine)}: Entry node's guid is empty");
                return;
            }

            if (this._nodeLookup[entry.guid] is not EnterState entryState)
            {
                Debug.LogError($"{typeof(StateMachine)}: Cannot cast the prepared entry node to an Entry node.");
                return;
            }

            _current = entryState;
            _current.EnterNode();
            _any.EnterNode();
        }


        internal override void StopGraph()
        {
            if (_current.callState == NodeCallState.Updating)
            {
                _current.ExitNode();
            }

            if (_any.callState == NodeCallState.Updating)
            {
                _any.ExitNode();
            }
        }


        private bool TryGetNextState(out NodeBase nextState)
        {
            if (this.blockAllTransition == false)
            {
                if (_current.CheckTransition(out nextState))
                {
                    return true;
                }

                if (_any.CheckTransition(out nextState))
                {
                    return true;
                }
            }

            nextState = null;
            return false;
        }


#if UNITY_EDITOR
        internal override void OnRemoveGraph()
        {
            List<NodeBase> nodes = this._nodeLookup.Values.ToList();

            foreach (StateBase node in nodes)
            {
                if (node.transitions is null || node.transitions.Count > 0)
                {
                    continue;
                }

                this.DeleteAndRemoveNodeFromList(node, false);
            }
        }


        public Transition ConnectStates(StateBase from, StateBase to)
        {
            if (from.TryGetTransition(to.guid, out _))
            {
                return null; //이미 연결되어 있음.
            }

            // GraphAsset과 from 노드 모두 기록
            Undo.RecordObject(_graphAsset, "State Machine (Connect)");

            Transition newTransition = ObjectFactory.CreateTransition(from, to);
            from.AddTransition(newTransition);
            EditorUtility.SetDirty(_graphAsset);
            return newTransition;
        }


        public Transition DisconnectStates(StateBase from, StateBase to)
        {
            if (from.TryGetTransition(to.guid, out Transition transition) == false)
            {
                return null;
            }

            // GraphAsset과 from 노드 모두 기록
            Undo.RecordObject(_graphAsset, "State Machine (Disconnect)");
            from.RemoveTransition(transition);
            EditorUtility.SetDirty(_graphAsset);
            return transition;
        }
    }
#endif
}