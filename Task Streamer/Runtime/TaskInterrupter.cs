using System.Collections.Generic;
using TaskStreamer.Utility;
using UnityEngine;

namespace TaskStreamer.BT
{
    public class TreeInterrupter
    {
        /// <summary> 중단(Abort) 작업 정보를 담는 구조체 </summary>
        private readonly struct AbortInfo
        {
            public AbortInfo(int callStackID, BehaviorNodeBase targetNode = null)
            {
                this.callStackID = callStackID;
                this.targetNode = targetNode;
            }

            public readonly int callStackID;
            public readonly BehaviorNodeBase targetNode; //null이면 전체 스택 중단
        }

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


        private bool _isAbortSubtreeInProgress = false;

        private FixedList<Stack<BehaviorNodeBase>> _runtimeCallStack;

        private FixedQueue<AbortInfo> _abortQueue;

        

        /// <summary>
        /// 콜스택과 중지 대기열을 초기화합니다.
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


        /// <summary> 지정된 호출 스택의 현재 실행 중인 노드를 반환 </summary>
        /// <param name="callStackID">호출 스택 ID</param>
        /// <returns>현재 노드, 없으면 null</returns>
        public NodeBase GetCurrentNode(in int callStackID)
        {
            if (this.IsValidCallStack(callStackID) == false || _runtimeCallStack[callStackID].Count == 0)
            {
                return null;
            }

            return _runtimeCallStack[callStackID].Peek();
        }


        /// <summary> 지정된 호출 스택에 노드를 푸시 </summary>
        /// <param name="callStackID">호출 스택 ID</param>
        /// <param name="node">푸시할 노드</param>
        public void PushInCallStack(in int callStackID, BehaviorNodeBase node)
        {
            _runtimeCallStack[callStackID].Push(node);
        }


        /// <summary> 지정된 호출 스택에서 최상단 노드를 팝 </summary>
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
        /// 지정된 노드부터 상위까지의 서브트리를 중단
        /// 해당 노드보다 깊은 depth의 노드들을 모두 정리
        /// </summary>
        /// <param name="callStackID">호출 스택 ID</param>
        /// <param name="node">중단할 기준 노드</param>
        public void AbortSubtreeFrom(in int callStackID, BehaviorNodeBase node)
        {
            _abortQueue.Clear();
            _abortQueue.Enqueue(new AbortInfo(callStackID, node));

            this.ProcessAbortQueue(true);
        }


        /// <summary> 지정된 호출 스택의 전체 서브트리를 중단 </summary>
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


        /// <summary> 중단 큐를 처리하여 노드들을 정리 </summary>
        /// <param name="abortQueue">중단할 노드들의 큐</param>
        /// <param name="hasTargetNode">특정 노드까지만 중단할지 여부</param>
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


        /// <summary> 특정 노드까지의 타겟 중단을 처리 </summary>
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


        /// <summary> 전체 스택 중단을 처리 </summary>
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


        /// <summary> 노드 종료 처리 (병렬 노드의 경우 자식들도 중단) </summary>
        private void ProcessNodeExit(NodeBase node)
        {
            if (node is BehaviorNodeBase behaviourNode)
            {
                bool isCompositeNode = behaviourNode.nodeType == EBehaviorNodeType.Composite;
                
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


        /// <summary> 호출 스택이 유효한지 확인  </summary>
        private bool IsValidCallStack(int callStackID)
        {
            return callStackID >= 0 && callStackID < _runtimeCallStack.count;
        }
    }
}