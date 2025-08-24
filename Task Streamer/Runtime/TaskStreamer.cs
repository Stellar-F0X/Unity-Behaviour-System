using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TaskStreamer
{
    [DefaultExecutionOrder(-1)]
    public sealed class TaskStreamer : MonoBehaviour, ISerializationCallbackReceiver
    {
        internal event Action onNodeFixedUpdate;

        internal event Action onNodeLateUpdate;

        internal event Action onNodeGizmosUpdate;

        [SerializeField]
        private TickMode _tickMode = TickMode.ManualUpdate;

        [SerializeField]
        private bool _pauseUpdate;

        [SerializeField]
        private GraphAsset _graphAsset;

        [SerializeField]
        private RuntimeBlackboard _runtimeBlackboard = new RuntimeBlackboard();


        internal GraphAsset graphAsset
        {
            get { return _graphAsset; }
        }

        internal RuntimeBlackboard runtimeBlackboard
        {
            get { return _runtimeBlackboard; }
        }

        public bool pause
        {
            get { return _pauseUpdate; }

            set { _pauseUpdate = value; }
        }

        public TickMode tickMode
        {
            get { return _tickMode; }

            set { _tickMode = value; }
        }


        private void Awake()
        {
            if (this._graphAsset == null) // GraphGroup가 할당되지 않은 경우이다.
            {
                return;
            }

            _runtimeBlackboard?.InitializeOnEnterRuntime();
            
            _graphAsset = _graphAsset.Clone(this);

            if (this.graphAsset == null)
            {
                return;
            }

            foreach (NodeBase node in _graphAsset.graphs.SelectMany(graph => graph.GetIterator(GraphIteratorType.BFS)))
            {
                node.OnAwake();
            }
        }


        private void OnDestroy()
        {
            this.onNodeLateUpdate = null;

            this.onNodeFixedUpdate = null;

            this.onNodeGizmosUpdate = null;
        }


        private bool TryExecuteGraph(TickMode callingTickMethodType)
        {
            if (callingTickMethodType != this._tickMode || this.pause)
            {
                return false;
            }

            if (this._graphAsset == null)
            {
                Debug.LogError($"{typeof(TaskStreamer)}: Graph asset does not exist.");
                this.enabled = false;
                return false;
            }

            if (this._graphAsset.main.UpdateGraph() != Status.Running)
            {
                this._graphAsset.main.ResetGraph();
            }

            return true;
        }


        private void Update()
        {
            this.TryExecuteGraph(TickMode.ManualUpdate);
        }


        private void FixedUpdate()
        {
            this.TryExecuteGraph(TickMode.FixedUpdate);
            this.onNodeFixedUpdate?.Invoke();
        }


        private void LateUpdate()
        {
            this.TryExecuteGraph(TickMode.LateUpdate);
            this.onNodeLateUpdate?.Invoke();
        }


        private void OnDrawGizmos()
        {
            this.onNodeGizmosUpdate?.Invoke();
        }


        public void ExternalUpdate()
        {
            if (_tickMode == TickMode.ExternalUpdate)
            {
                this.TryExecuteGraph(TickMode.ExternalUpdate);
            }
            else
            {
                Debug.LogWarning("ExternalUpdate는 tickMode가 ExternalUpdate로 설정되어 있을 때만 호출해야 합니다.");
            }
        }


        public void SetVariable<TValue>(in string key, TValue value)
        {
            if (this._runtimeBlackboard is null || enabled == false)
            {
                throw new InvalidOperationException("BehaviourTree 또는 Blackboard가 활성화되어 있지 않습니다.");
            }

            BlackboardVariable foundVariable = _runtimeBlackboard.FindVariable(key);

            if (foundVariable is BlackboardVariable<TValue> valueVariable)
            {
                valueVariable.value = value;
                return;
            }

            throw new KeyNotFoundException($"키 '{key}'에 해당하는 프로퍼티를 찾을 수 없습니다.");
        }


        public TValue GetVariable<TValue>(in string key)
        {
            if (this._runtimeBlackboard is null || enabled == false)
            {
                throw new InvalidOperationException("BehaviourTree 또는 Blackboard가 활성화되어 있지 않습니다.");
            }

            BlackboardVariable foundVariable = _runtimeBlackboard.FindVariable(key);

            if (foundVariable is BlackboardVariable<TValue> valueVariable)
            {
                return valueVariable.value;
            }

            throw new KeyNotFoundException($"키 '{key}'에 해당하는 프로퍼티를 찾을 수 없습니다.");
        }



        private void UpdateRuntimeBlackboardVariables()
        {
            //Runtime Blackboard에 SO Blackboard에선 진작 수정돼서 없는 Variable이 있는지 확인과 동시에 제거한다.
            for (int index = _runtimeBlackboard.count - 1; index >= 0; --index)
            { 
                BlackboardVariable original = _runtimeBlackboard.variables[index];
                
                BlackboardVariable replica = _graphAsset.blackboard.FindVariable(original.guid);

                if (replica is null)
                {
                    _runtimeBlackboard.RemoveVariable(original);
                }
                else
                {
                    original.key = replica.key;
                }
            }

            //SO Blackboard엔 있지만 Runtime Blackboard에는 없는 Variable들을 Runtime Blackboard에 추가한다.
            foreach (BlackboardVariable variable in _graphAsset.blackboard.variables)
            {
                BlackboardVariable replica = _runtimeBlackboard.FindVariable(variable.guid);

                if (replica is null)
                {
                    _runtimeBlackboard.AddVariable(variable.Duplicate());
                }
                else
                {
                    replica.key = variable.key;
                }
            }
        }
        


        public void OnBeforeSerialize()
        {
            //GraphAsset이 없어졌거나 Blackboard가 제거될 경우 Variable 리스트를 삭제하고 함수를 종료한다.
            if (_graphAsset == null || _graphAsset.blackboard == null)
            {
                this._runtimeBlackboard.ClearVariables();
                return;
            }
            
            //마지막 반영 버전과 같으면 굳이 다시 업데이트하지 않고 함수를 종료한다.
            if (_runtimeBlackboard.CanUpdateable(_graphAsset.blackboard.appliedVersion))
            {
                return;
            }

            this.UpdateRuntimeBlackboardVariables();
        }


        public void OnAfterDeserialize() { }
    }
}