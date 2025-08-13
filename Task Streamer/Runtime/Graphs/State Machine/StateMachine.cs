using System;
using System.Collections.Generic;
using System.Linq;
using TaskStreamer.Utility;
using Unity.Properties;
using UnityEditor;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

namespace TaskStreamer.FSM
{
    [Serializable]
    public partial class StateMachine : Graph
    {
        private StateMachine(string graphName, GraphAsset graphAsset) : base(graphName, graphAsset) { }

        [SerializeField, DontCreateProperty]
        private StateBase _current;

        [SerializeField, DontCreateProperty]
        private StateBase _any;

        [SerializeField, DontCreateProperty]
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

            graph.entry = graph.CreateNode("Enter", typeof(EnterState), new Vector2Int(0, 0)) as StateBase;
            graph._exit = graph.CreateNode("Exit", typeof(ExitState), new Vector2Int(0, 200)) as StateBase;
            graph._any = graph.CreateNode("Any", typeof(AnyState), new Vector2Int(-230, 0)) as StateBase;
            graph._current = graph.entry as StateBase;

            return graph;
        }
#endif


        public override IGraphIterator GetIterator(GraphIteratorType iteratorType)
        {
            switch (iteratorType)
            {
                case GraphIteratorType.LS: return new StateMachine.LSIterator(this);

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
                //현재 상태에서 전이가 발생하면 || 기준 왼쪽 함수에서 얻어온 guid를 토대로 전이할 것이고 anyState에서 발생하면 그 반대.
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

                foreach (Transition transition in node.transitions)
                {
                    if (AssetDatabase.Contains(transition))
                    {
                        AssetDatabase.RemoveObjectFromAsset(transition);
                    }

                    Object.DestroyImmediate(transition);
                }

                this.DeleteNode(node);
            }
        }


        public Transition ConnectStates(StateBase from, StateBase to)
        {
            if (from.TryGetTransition(to.guid, out _))
            {
                return null;
            }

            // GraphAsset과 from 노드 모두 기록
            Undo.RecordObject(_graphAsset, "State Machine (Connect)");
            Undo.RecordObject(from, "State Machine (Connect)");

            Transition newTransition = TaskStreamerUtility.CreateTransition(from, to);
            from.AddTransition(newTransition);

            // Transition을 GraphAsset의 sub-asset으로 추가
            if (AssetDatabase.Contains(_graphAsset))
            {
                AssetDatabase.AddObjectToAsset(newTransition, _graphAsset);
            }

            // Undo 등록 및 저장
            Undo.RegisterCreatedObjectUndo(newTransition, "State Machine (Connect)");
            EditorUtility.SetDirty(from);
            EditorUtility.SetDirty(to);
            EditorUtility.SetDirty(_graphAsset);
            AssetDatabase.SaveAssets();
            return newTransition;
        }


        public void DisconnectStates(StateBase from, StateBase to)
        {
            if (from.TryGetTransition(to.guid, out Transition transition) == false)
            {
                return;
            }

            // GraphAsset과 from 노드 모두 기록
            Undo.RecordObject(_graphAsset, "State Machine (Disconnect)");
            Undo.RecordObject(from, "State Machine (Disconnect)");

            from.RemoveTransition(transition);

            // Sub-asset에서도 제거
            if (AssetDatabase.Contains(transition))
            {
                AssetDatabase.RemoveObjectFromAsset(transition);
            }

            // Transition 삭제
            Undo.DestroyObjectImmediate(transition);
            EditorUtility.SetDirty(from);
            EditorUtility.SetDirty(to);
            EditorUtility.SetDirty(_graphAsset);
            AssetDatabase.SaveAssets();
        }
    }
#endif
}