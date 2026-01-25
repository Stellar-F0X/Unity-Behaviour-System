using System;
using TaskStreamer.Runtime.Utility;
using Unity.Properties;
using UnityEngine;
using UnityEngine.Assertions;

namespace TaskStreamer.Runtime.FSM
{
    /// <summary>
    /// 유한 상태 머신(FSM)의 상태 노드 중 하나로, 특정 서브그래프의 동작을 캡슐화하는 추상 클래스.
    /// 서브그래프와 관련된 작업을 처리하기 위한 기능을 제공하며, 그래프 구조 안에서 서브그래프의 역할을 정의한다.
    /// </summary>
    /// <remarks>
    /// <see cref="SubGraphState"/> 클래스의 각 인스턴스는 고유 식별자로 정의된 개별 서브그래프를 나타낸다.
    /// 서브클래스는 사용하려는 서브그래프의 구체적인 동작과 유형을 반드시 구현해야 한다.
    /// </remarks>
    [Serializable, GeneratePropertyBag, Readable]
    internal abstract class SubGraphState : StateBase, ISubGraph
    {
        /// <summary>
        /// 특정 상태가 실행 중(execution)에도 전이(transition)가 가능한지를 나타내는 변수입니다.
        /// </summary>
        /// <remarks>
        /// 이 변수는 상태 실행 도중 전이 허용 여부를 정의하며, <see cref="BlackboardVariable{TValue}"/>로 구현되어
        /// 런타임 중 동적으로 값을 관리할 수 있습니다.
        /// </remarks>
        public BlackboardVariable<SubGraphTransitionPolicy> transitionPolicy;


        /// <summary>
        /// 현재 상태와 연관된 서브그래프를 나타내는 비공개 인스턴스입니다.
        /// </summary>
        /// <remarks>
        /// 이 변수는 상태와 속한 서브그래프에 대한 참조를 저장합니다.
        /// 상태가 초기화될 때 초기화되며, 상태 전이 및 상태 업데이트 중 서브그래프의 상태를 재설정하거나 업데이트하는 데 중요합니다.
        /// 서브그래프의 생명주기를 관리하는 역할을 수행합니다.
        /// </remarks>
        [DontCreateProperty]
        private Graph _subGraph;

        /// <summary>
        /// 해당 상태와 연관된 서브 그래프의 고유 식별자(GUID)를 나타내는 비공개 변수입니다.
        /// </summary>
        /// <remarks>
        /// 이 변수는 시스템 내 특정 서브 그래프를 참조하기 위해 사용됩니다.
        /// 상태 전환 및 상태 머신 내 서브 그래프 간 상호작용을 지원하는 데 필수적입니다.
        /// </remarks>
        [SerializeField, DontCreateProperty]
        private UGUID _subGraphGuid;


        /// <summary>
        /// 상태 노드의 타입을 나타내는 추상 속성입니다.
        /// </summary>
        /// <remarks>
        /// 이 속성은 상태 노드의 유형을 반환하며, 서브그래프 상태(SubGraphState)의 경우 항상 <see cref="StateNodeType.SubGraph"/>를 반환합니다.
        /// </remarks>
        /// <returns>상태 노드의 타입을 나타내는 <see cref="StateNodeType"/> 값.</returns>
        public override StateNodeType nodeType
        {
            get { return StateNodeType.SubGraph; }
        }


        /// <summary>
        /// 서브그래프의 고유 식별자를 나타내는 프로퍼티.
        /// </summary>
        /// <remarks>
        /// 이 프로퍼티는 서브그래프를 식별하기 위해 사용되는 <c>GUID</c> 값을 저장 및 제공한다.
        /// </remarks>
        public UGUID subGraphGuid
        {
            get { return _subGraphGuid; }

            set { _subGraphGuid = value; }
        }


        /// <summary>
        /// 추상 상태 클래스에서 특정 하위 그래프의 유형(GraphType)을 나타내는 속성입니다.
        /// </summary>
        /// <remarks>
        /// 이 속성은 하위 그래프의 종류를 정의하며, 해당 상태에서 동작하는 그래프의 유형을 식별하는 데 사용됩니다.
        /// GraphType 열거형의 값을 반환하며, 예를 들어 Behavior Tree(BT), Finite State Machine(FSM) 등이 포함됩니다.
        /// </remarks>
        public abstract GraphType subGraphType
        {
            get;
        }


        /// <summary>
        /// 상태가 활성화될 때 호출된다. 이 메서드는 subGraphGuid를 사용하여 그래프 에셋에서 서브 그래프를 가져와
        /// _subGraph에 할당하며, 서브 그래프가 올바르게 초기화되었는지 확인하고 그 존재를 보증한다.
        /// </summary>
        public override void OnAwake()
        {
            _subGraph = streamer.graphAsset.GetGraph(subGraphGuid);

            Assert.IsNotNull(_subGraph, "SubGraph not found");
        }


        /// <summary>
        /// 상태가 활성화될 때 실행되며, 관련된 서브 그래프를 적절히 초기화한다.
        /// </summary>
        /// <remarks>
        /// 서브 그래프가 null인 경우 오류 메시지를 기록하고, 초기화가 이루어지지 않는다.
        /// 서브 그래프 초기화와 함께 상태 전이 방지를 위한 blockTransition 속성이 true로 설정된다.
        /// </remarks>
        /// <exception cref="System.NullReferenceException">
        /// 서브 그래프가 활성화되기 전에 초기화되지 않은 경우 예외가 발생한다.
        /// </exception>
        protected override void OnEnter()
        {
            if (_subGraph is null)
            {
                Debug.LogError($"{name} {nameof(OnEnter)}: SubGraph is null");
                return;
            }

            this._subGraph.ResetGraph();

            //서브 그래프로가 실행 중일땐, 실행되다말고, 다른 노드로 이동되면 안되기 때문에 상태 전이를 막는다.
            this.blockTransition = true;
        }


        /// <summary>
        /// 상태가 업데이트될 때 호출되는 메서드.
        /// 상태의 업데이트 주기 동안 논리 처리를 수행하며, 서브 그래프의 상태와 전이 여부를 관리한다.
        /// </summary>
        /// <remarks>
        /// 서브 그래프가 null인 경우 오류를 로그로 출력한다. 서브 그래프의 상태 업데이트 결과가
        /// <c>Status.Running</c>이 아닌 경우, 전이를 차단하지 않도록 <c>blockTransition</c>을 false로 설정한다.
        /// </remarks>
        /// <exception cref="System.NullReferenceException">
        /// <c>_subGraph</c>가 null인 경우 발생.
        /// </exception>
        protected override void OnUpdate()
        {
            if (_subGraph is null)
            {
                Debug.LogError($"{name} {nameof(OnUpdate)}: SubGraph is null");
                return;
            }

            //서브 그래프로가 Failure나 Success를 반환하면, 그래프 동작이 끝났다는 것이므로 상태 전이를 다시 허용한다.
            if (_subGraph.UpdateGraph() != Status.Running)
            {
                this.blockTransition = false;
            }
        }


        /// <summary>
        /// 상태를 종료할 때 필요한 정리 또는 종료 로직을 실행한다.
        /// </summary>
        /// <remarks>
        /// 이 메서드는 연결된 서브 그래프가 상태 종료 과정에서 적절히 중단되도록 보장한다.
        /// 서브 그래프가 null인 경우 오류 메시지가 기록된다.
        /// </remarks>
        protected override void OnExit()
        {
            if (_subGraph is null)
            {
                Debug.LogError($"{name} {nameof(OnExit)}: SubGraph is null");
                return;
            }

            this._subGraph.StopGraph();
        }


        protected override bool CanTransition(out NodeBase nextState)
        {
            if (base.CanTransition(out nextState) == false)
            {
                return false;
            }

            SubGraphTransitionPolicy policy = this.transitionPolicy.value;
            
            //동작 도중에 직접 연결된 트랜지션에 대해서, 갑작스러운 전이를 허용한다면 True를 반환.
            if ((policy & SubGraphTransitionPolicy.AllowLinkedWhileRunning) > 0)
            {
                return true;
            }
            
            if (nextState is null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}