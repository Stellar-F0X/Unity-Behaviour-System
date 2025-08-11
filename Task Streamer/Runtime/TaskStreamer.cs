using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

[assembly: InternalsVisibleTo("TaskStreamer.Tool")]

namespace TaskStreamer
{
    [DefaultExecutionOrder(-1)]
    public sealed class TaskStreamer : MonoBehaviour
    {
        internal event Action onNodeFixedUpdate;

        internal event Action onNodeLateUpdate;

        internal event Action onNodeGizmosUpdate;
        
        [SerializeField]
        private TickMode _tickMode = TickMode.ManualUpdate;

        [SerializeField]
        private GraphAsset _graphAsset;

        [SerializeField]
        private bool _pauseUpdate;


        internal GraphAsset graphAsset
        {
            get { return _graphAsset; }
        }

        internal Blackboard blackboard
        {
            get { return _graphAsset?.blackboard; }
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

            _graphAsset = _graphAsset.Clone(this);

            if (this.graphAsset == null)
            {
                return;
            }

            foreach (NodeBase node in _graphAsset.graphs.SelectMany(graph => graph.GetGraphIterator(GraphIteratorType.BFS)))
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
            if (this.blackboard is null || enabled == false)
            {
                throw new InvalidOperationException("BehaviourTree 또는 Blackboard가 활성화되어 있지 않습니다.");
            }

            Variable foundVariable = blackboard.FindVariable(key);

            if (foundVariable is Variable<TValue> valueVariable)
            {
                valueVariable.value = value;
                return;
            }

            throw new KeyNotFoundException($"키 '{key}'에 해당하는 프로퍼티를 찾을 수 없습니다.");
        }


        public TValue GetVariable<TValue>(in string key)
        {
            if (this.blackboard is null || enabled == false)
            {
                throw new InvalidOperationException("BehaviourTree 또는 Blackboard가 활성화되어 있지 않습니다.");
            }

            Variable foundVariable = blackboard.FindVariable(key);

            if (foundVariable is Variable<TValue> valueVariable)
            {
                return valueVariable.value;
            }

            throw new KeyNotFoundException($"키 '{key}'에 해당하는 프로퍼티를 찾을 수 없습니다.");
        }
    }
}