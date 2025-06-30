using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[assembly: InternalsVisibleTo("TaskStreamer.Tool")]

namespace TaskStreamer
{
    [DefaultExecutionOrder(-1), AddComponentMenu("Task Streamer/Task Streamer")]
    public sealed class TaskStreamer : MonoBehaviour
    {
        internal event Action onNodeFixedUpdate;

        internal event Action onNodeLateUpdate;

        internal event Action onNodeGizmosUpdate;
        
        [SerializeField]
        private ETickMode _tickMode = ETickMode.MenualUpdate;

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

        public ETickMode tickMode
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

            foreach (Graph graph in _graphAsset.graphs)
            {
                foreach (NodeBase node in graph.GetGraphIterator())
                {
                    node.OnAwake();
                }
            }

            Debug.Log("==================초기화===================");
        }


        private void OnDestroy()
        {
            this.onNodeLateUpdate = null;

            this.onNodeFixedUpdate = null;

            this.onNodeGizmosUpdate = null;
        }


        private bool TryExecuteGraph(ETickMode callingTickMethodType)
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

            if (this._graphAsset.main.UpdateGraph() != EStatus.Running)
            {
                this._graphAsset.main.ResetGraph();
            }

            return true;
        }


        private void Update()
        {
            this.TryExecuteGraph(ETickMode.MenualUpdate);
        }


        private void FixedUpdate()
        {
            this.TryExecuteGraph(ETickMode.FixedUpdate);
            this.onNodeFixedUpdate?.Invoke();
        }


        private void LateUpdate()
        {
            this.TryExecuteGraph(ETickMode.LateUpdate);
            this.onNodeLateUpdate?.Invoke();
        }


        private void OnDrawGizmos()
        {
            this.onNodeGizmosUpdate?.Invoke();
        }


        public void ExternalUpdate()
        {
            if (_tickMode == ETickMode.ExternalUpdate)
            {
                this.TryExecuteGraph(ETickMode.ExternalUpdate);
            }
            else
            {
                Debug.LogWarning("ExternalUpdate는 tickUpdateMode가 ExternalUpdate로 설정되어 있을 때만 호출해야 합니다.");
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