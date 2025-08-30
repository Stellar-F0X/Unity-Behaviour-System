using System.Collections.Generic;
using TaskStreamer.Utility;
using UnityEngine;

namespace TaskStreamer.BT
{
    /// <summary> Behavior Tree의 노드 호출 스택과 서브트리 중단 기능을 관리하는 클래스 </summary>
    public class TreeInterrupter
    {
        /// <summary> Behavior Tree의 중단(Abort) 작업 처리를 위한 필수 정보를 나타내는 구조체 </summary>
        private readonly struct AbortInfo
        {
            /// <summary>
            /// 비동기 작업 중단에 대한 정보를 저장하는 불변 구조체입니다.
            /// </summary>
            public AbortInfo(int callStackID, BehaviorNodeBase targetNode = null)
            {
                this.callStackID = callStackID;
                this.targetNode = targetNode;
            }

            /// <summary>
            /// 특정 호출 스택(Call Stack)을 식별하기 위한 고유 ID입니다.
            /// </summary>
            public readonly int callStackID;

            /// <summary>
            /// 특정 작업(Call Stack) 중단 시 대상이 되는 노드를 나타냅니다. null일 경우 전체 스택이 중단됨.
            /// </summary>
            public readonly BehaviorNodeBase targetNode; //null이면 전체 스택 중단
        }

        /// <summary>
        /// 트리형 비헤이비어 그래프의 실행 중단 및 호출 스택을 관리하는 클래스입니다.
        /// </summary>
        public TreeInterrupter(Graph graph, int callStackSize)
        {
            int count = callStackSize + 1;
            int excludingRootCount = graph.count - 1;

            this._runtimeCallStack = new FixedList<Stack<BehaviorNodeBase>>(count);
            this._abortQueue = new FixedQueue<AbortInfo>(excludingRootCount); //count excluding root node

            for (int i = 0; i < count; ++i)
            {
                _runtimeCallStack.Add(new Stack<BehaviorNodeBase>());
            }
        }


        /// <summary>
        /// 서브트리 중단 동작이 진행 중인지 여부를 나타내는 플래그입니다.
        /// </summary>
        private bool _isAbortSubtreeInProgress = false;

        /// <summary>
        /// 실행 중 호출 스택을 관리하기 위한 <see cref="FixedList{T}"/>로, 각 호출 스택은 <see cref="Stack{T}"/> 형태로 구성된다.
        /// </summary>
        private FixedList<Stack<BehaviorNodeBase>> _runtimeCallStack;

        /// <summary>
        /// 중단 작업(Abort)의 정보를 보관하고 관리하기 위한 대기열입니다.
        /// </summary>
        private FixedQueue<AbortInfo> _abortQueue;



        /// <summary>
        /// 실행 중인 모든 호출 스택과 중지 대기열을 초기화합니다.
        /// </summary>
        public void ClearCallStack()
        {
            _isAbortSubtreeInProgress = false;

            _abortQueue.Clear();
            
            for (int i = 0; i < _runtimeCallStack.count; ++i)
            {
                _runtimeCallStack[i].Clear();
            }
        }


        /// <summary>
        /// 호출 스택의 현재 실행 중인 노드를 반환합니다.
        /// </summary>
        /// <param name="callStackID">확인할 호출 스택의 ID</param>
        /// <returns>현재 노드, 없으면 null</returns>
        public NodeBase GetCurrentNode(in int callStackID)
        {
            if (this.IsValidCallStack(callStackID) == false || _runtimeCallStack[callStackID].Count == 0)
            {
                return null;
            }

            return _runtimeCallStack[callStackID].Peek();
        }


        /// <summary>
        /// 호출 스택에 특정 노드를 추가합니다.
        /// </summary>
        /// <param name="callStackID">호출 스택 ID</param>
        /// <param name="node">추가할 노드</param>
        public void PushInCallStack(in int callStackID, BehaviorNodeBase node)
        {
            _runtimeCallStack[callStackID].Push(node);
        }


        /// <summary>
        /// 지정된 호출 스택에서 최상단 노드를 제거합니다.
        /// </summary>
        /// <param name="callStackID">호출 스택 ID</param>
        public void PopInCallStack(in int callStackID)
        {
            if (this.IsValidCallStack(callStackID) == false || _runtimeCallStack[callStackID].Count == 0)
            {
                Debug.LogWarning($"호출 스택 ID {callStackID}에서 꺼낼 노드가 없습니다.");
                return;
            }

            _runtimeCallStack[callStackID].Pop();
        }


        /// <summary>
        /// 지정된 노드에서 시작하여 상위 서브트리를 중단합니다.
        /// </summary>
        /// <param name="callStackID">호출 스택의 ID</param>
        /// <param name="node">중단할 기준 노드</param>
        public void AbortSubtreeFrom(in int callStackID, BehaviorNodeBase node)
        {
            _abortQueue.Clear();
            _abortQueue.Enqueue(new AbortInfo(callStackID, node));

            this.ProcessAbortQueue(true);
        }


        /// <summary>
        /// 지정된 호출 스택의 모든 서브트리를 중단합니다.
        /// </summary>
        /// <param name="callStackID">중단할 호출 스택 ID</param>
        public void AbortSubtree(in int callStackID)
        {
            if (_isAbortSubtreeInProgress)
            {
                _abortQueue.Enqueue(new AbortInfo(callStackID));
                return;
            }

            _isAbortSubtreeInProgress = true;
            _abortQueue.Enqueue(new AbortInfo(callStackID));

            this.ProcessAbortQueue(false);
            _isAbortSubtreeInProgress = false;
        }


        /// <summary>
        /// 중단 큐를 처리하여 노드들을 정리합니다.
        /// </summary>
        /// <param name="hasTargetNode">특정 노드까지만 중단할지 여부를 나타냅니다.</param>
        private void ProcessAbortQueue(bool hasTargetNode)
        {
            while (_abortQueue.count > 0)
            {
                AbortInfo current = _abortQueue.Dequeue();

                if (this.IsValidCallStack(current.callStackID) == false || _runtimeCallStack[current.callStackID].Count == 0)
                {
                    continue;
                }

                if (hasTargetNode)
                {
                    this.ProcessTargetedAbort(current);
                }
                else
                {
                    this.ProcessFullStackAbort(current);
                }
            }
        }


        /// <summary>
        /// 특정 노드까지의 타겟 중단을 처리합니다.
        /// </summary>
        /// <param name="abortInfo">중단 대상 정보를 담고 있는 AbortInfo 구조체입니다.</param>
        private void ProcessTargetedAbort(AbortInfo abortInfo)
        {
            int currentID = abortInfo.callStackID;

            if (_runtimeCallStack[currentID].Count == 0)
            {
                return;
            }

            BehaviorNodeBase targetNode = abortInfo.targetNode;
            BehaviorNodeBase peekNode = _runtimeCallStack[currentID].Peek();

            // 타겟 노드보다 깊은 depth에 위치한 노드들. 즉 자식들을 모두 정리.
            while (peekNode.depth > targetNode.depth && peekNode.Equals(targetNode) == false)
            {
                this.ProcessNodeExit(peekNode);

                if (_runtimeCallStack[currentID].Count == 0)
                {
                    break;
                }

                peekNode = _runtimeCallStack[currentID].Peek();
            }
        }


        /// <summary>
        /// 전체 콜스택의 중단 작업을 처리합니다.
        /// </summary>
        /// <param name="abortInfo">처리할 중단 작업에 대한 정보입니다.</param>
        private void ProcessFullStackAbort(AbortInfo abortInfo)
        {
            int currentID = abortInfo.callStackID;

            if (_runtimeCallStack[currentID].Count == 0)
            {
                return;
            }

            this.ProcessNodeExit(_runtimeCallStack[currentID].Peek());

            if (_runtimeCallStack[currentID].Count > 0)
            {
                // 다음 노드도 중단 큐에 추가
                BehaviorNodeBase nextNode = _runtimeCallStack[currentID].Peek();
                _abortQueue.Enqueue(new AbortInfo(nextNode.callStackID));
            }
        }


        /// <summary>
        /// 지정된 노드의 종료 작업을 처리합니다.
        /// </summary>
        /// <param name="node">종료 작업을 처리할 대상 노드입니다.</param>
        private void ProcessNodeExit(NodeBase node)
        {
            if (node is BehaviorNodeBase behaviourNode)
            {
                bool isCompositeNode = behaviourNode.nodeType == BehaviorNodeType.Composite;
                
                if (isCompositeNode && behaviourNode is ParallelNode parallelNode)
                {
                    // 병렬 노드의 모든 자식들을 중단 큐에 추가
                    foreach (BehaviorNodeBase child in parallelNode.GetChildren())
                    {
                        _abortQueue.Enqueue(new AbortInfo(child.callStackID));
                    }
                }
            }

            node.ExitNode();
        }


        /// <summary> 호출 스택 ID가 유효한지 확인합니다. </summary>
        /// <param name="callStackID">유효성 검사를 수행할 호출 스택 ID</param>
        /// <returns>유효하면 true, 그렇지 않으면 false</returns>
        private bool IsValidCallStack(int callStackID)
        {
            return callStackID >= 0 && callStackID < _runtimeCallStack.count;
        }
    }
}