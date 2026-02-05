using System;
using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;

namespace TaskStreamer.Runtime.BT
{
    [Serializable]
    public abstract class BehaviorNodeBase : NodeBase, IMissingTaskRemovable
    {
        /// 이 변수는 현재 노드가 속한 BehaviorTree를 참조하며, 부모-자식 노드 구조 관리 및 실행 흐름을 담당합니다.
        /// 내부적으로만 접근 및 설정 가능합니다.
        private BehaviorTree _tree;


        /// 이 변수는 현재 BehaviorNode의 부모 노드를 참조하며, 노드 계층 구조를 관리하는 데 사용됩니다.
        /// 외부에서 이 값을 설정할 수 없으며 내부적으로만 사용됩니다.
        [SerializeReference, DontCreateProperty, HideInInspector]
        private BehaviorNodeBase _parent;


        /// BehaviorNodeBase 클래스에서 사용되는 비공개 변수로, 노드와 연관된 ServiceBase 객체 목록을 관리합니다.
        [SerializeReference, HideInInspector]
        private List<ServiceBase> _services = new List<ServiceBase>(1);


        /// 자식 노드들을 저장하는 리스트로, 각 BehaviorNodeBase 객체가 하위 노드 구조를 관리하기 위해 사용됩니다.
        [SerializeReference, DontCreateProperty, HideInInspector]
        protected List<BehaviorNodeBase> _children = new List<BehaviorNodeBase>(1);



        /// <summary>
        /// 각 노드의 타입을 반환하는 속성으로, 현재 노드가 어떤 역할을 수행하는지 정의합니다.
        /// (예: Root, Action, Composite, Decorator, SubGraph).
        /// </summary>
        public abstract BehaviorNodeType nodeType
        {
            get;
        }

        
        /// <summary>
        /// 노드 실행 상태를 나타내는 속성입니다. Success, Failure, Running 값 중 하나를 가질 수 있습니다.
        /// </summary>
        public Status status
        {
            get;
            private set;
        }

        
        /// callStackID는 트리 노드의 호출 스택 ID로, 노드 실행 및 상태 관리에 사용됩니다.
        /// 내부에서만 설정 가능하며 외부에서는 읽기만 가능합니다.
        public int callStackID
        {
            get;
            internal set;
        }

        
        /// <summary>
        /// 해당 노드의 계층적 깊이를 나타내는 속성으로, 트리의 루트에서 해당 노드까지의 거리(깊이)를 나타냅니다.
        /// 내부적으로 설정되며, 외부에서는 읽기만 가능합니다.
        /// </summary>
        public int depth
        {
            get;
            internal set;
        }

        
        /// <summary>
        /// 노드의 부모 노드를 가져오거나 설정합니다. 내부적으로만 설정 가능합니다.
        /// </summary>
        public BehaviorNodeBase parent
        {
            get { return _parent; }

            internal set { _parent = value; }
        }

        
        /// <summary>
        /// 이 속성은 현재 노드가 속한 BehaviorTree 객체를 참조합니다.
        /// 노드와 트리를 연결하며, 내부적으로 설정되고 접근됩니다.
        /// </summary>
        public BehaviorTree tree
        {
            get { return _tree; }

            internal set { _tree = value; }
        }

        
        /// <summary> 노드의 서비스 목록을 읽기 전용으로 반환합니다. </summary>
        internal List<ServiceBase> services
        {
            get { return _services; }
        }
        
        
        public float enteredTime
        {
            get;
            private set;
        }


        public float elapsedTime
        {
            get { return Time.time - enteredTime; }
        }


        
        /// 노드의 업데이트 로직을 실행하여 현재 상태를 반환합니다.
        /// <return> 노드 실행 결과 상태를 반환합니다.
        public Status UpdateNode()
        {
            this.callCount++;

            if (this.CanVisit() == false)
            {
                this.status = Status.Failure;
                return this.status;
            }

            if (callState == NodeCallState.BeforeEnter)
            {
                this.EnterNode();
            }

            if (this.callState == NodeCallState.Updating)
            {
                foreach (ServiceBase service in _services)
                {
                    if (service.enable)
                    {
                        service.OnUpdate();
                    }
                }
                
                this.status = this.OnUpdate();

                if (this.status == Status.Running)
                {
                    return Status.Running;
                }

                //만약 Tree의 마지막 실행 노드가 자기 자신이 아니라면 현재 노드를 실행 스택에서 빼고 실행을 중단한다.
                if (this.tree.interrupter.GetCurrentNode(callStackID) != this)
                {
                    this.tree.interrupter.AbortSubtreeFrom(callStackID, this);
                }

                this.callState = NodeCallState.BeforeExit;
            }

            if (this.callState == NodeCallState.BeforeExit)
            {
                this.ExitNode();
            }

            return this.status;
        }
        
        

        /// 노드가 실행을 시작할 때 호출되며, 호출 스택에 추가하고 서비스를 초기화합니다.
        internal override sealed void EnterNode()
        {
            this.tree.interrupter.PushInCallStack(callStackID, this);
            this.enteredTime = Time.time;

            foreach (ServiceBase service in _services)
            {
                if (service.enable)
                {
                    service.OnStart();
                }
            }
            
            this.onNodeEnter?.Invoke(this);
            this.OnEnter();
            this.callState = NodeCallState.Updating;
        }

        

        /// 노드 실행 종료 시 호출되어 자원을 정리하고 상태를 초기화하는 메서드입니다.
        internal override sealed void ExitNode()
        {
            this.tree.interrupter.PopInCallStack(callStackID);
            
            foreach (ServiceBase service in _services)
            {
                if (service.enable)
                {
                    service.OnStop();
                }
            }
            
            this.OnExit();
            this.onNodeExit?.Invoke(this);
            this.callState = NodeCallState.BeforeEnter;
            this.enteredTime = 0;

            // If a parent node fails during execution, this node's result is set to Failure.
            this.status = (this.status == Status.Running ? Status.Failure : this.status);
        }
        
        
        
        private bool CanVisit()
        {
            int count = _services.Count;

            if (_services is null || count == 0)
            {
                return true;
            }
            
            for (int index = 0; index < count; ++index)
            {
                ServiceBase service = _services[index];

                if (service.enable == false)
                {
                    continue;
                }

                //하나라도 방문을 불허한다면, 바로 종료한다.
                if (service.CanVisit() == false)
                {
                    return false;
                }
            }

            return true;
        }

        

        /// 노드의 핵심 동작을 업데이트하는 추상 메서드로, 파생 클래스에서 구현되어야 합니다.
        /// <return> 노드 동작의 실행 결과를 반환합니다.
        protected abstract Status OnUpdate();

        

#if UNITY_EDITOR
        /// <summary>
        /// Missing Object(null)가 된 자식 노드와 서비스들을 제거합니다.
        /// 스크립트가 삭제되거나 이름이 변경된 경우 SerializeReference가 null이 됩니다.
        /// </summary>
        /// <returns>제거된 객체 수 (자식 노드 + 서비스)</returns>
        int IMissingTaskRemovable.RemoveMissingTasks()
        {
            int removedCount = _children.RemoveAll(static c => c == null);
            removedCount += _services.RemoveAll(static s => s == null);
            return removedCount;
        }
#endif
    }
}