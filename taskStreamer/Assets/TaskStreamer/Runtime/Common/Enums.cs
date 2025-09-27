using System;
using UnityEngine;

namespace TaskStreamer
{
    /// <summary> 상태 머신(FSM)에서 사용되는 노드의 유형을 정의합니다. </summary>
    public enum StateNodeType : byte
    {
        /// <summary>진입 상태 - 상태 머신의 시작점 (시스템에서 자동 생성됨)</summary>
        [Tooltip("State machine entry point. Cannot be created directly by user.")]
        Enter, // 진입 상태 (직접 생성 불가)
        
        /// <summary>임의 상태 전환을 위한 특수 상태 (시스템에서 자동 생성됨)</summary>
        [Tooltip("Special state that can transition from any other state. Cannot be created directly by user.")]
        Any,   // Any 상태 (직접 생성 불가)
        
        /// <summary>사용자가 정의할 수 있는 일반적인 액션 상태</summary>
        [Tooltip("Regular action state that can be created and customized by user.")]
        Action,  // 유저가 직접 생성 가능한 일반 상태
        
        /// <summary>하위 그래프를 포함하는 복합 상태</summary>
        [Tooltip("Composite state containing a sub-state machine or sub-graph.")]
        SubGraph,
        
        /// <summary>종료 상태 - 상태 머신의 종료점 (시스템에서 자동 생성됨)</summary>
        [Tooltip("State machine exit point. Cannot be created directly by user.")]
        Exit   // 종료 상태 (직접 생성 불가)
    };
    
    
    
    /// <summary> 행동 트리(Behavior Tree)에서 사용되는 노드의 유형을 정의합니다. </summary>
    public enum BehaviorNodeType : byte
    {
        /// <summary>행동 트리의 루트 노드 - 모든 실행의 시작점</summary>
        [Tooltip("Root node of the behavior tree. Starting point for all executions.")]
        Root,
        
        /// <summary>실제 행동이나 작업을 수행하는 잎 노드</summary>
        [Tooltip("Leaf node that performs actual actions or tasks.")]
        Action,
        
        /// <summary>여러 자식 노드를 관리하는 합성 노드 (Sequence, Selector, Parallel 등)</summary>
        [Tooltip("Node that manages multiple child nodes (Sequence, Selector, Parallel, etc.).")]
        Composite,
        
        /// <summary>하나의 자식 노드를 수식하는 장식자 노드 (Inverter, Repeater 등)</summary>
        [Tooltip("Node that modifies the behavior of a single child node (Inverter, Repeater, etc.).")]
        Decorator,
        
        /// <summary>다른 행동 트리나 하위 그래프를 실행하는 노드</summary>
        [Tooltip("Node that executes another behavior tree or sub-graph.")]
        SubGraph
    };
    
    
    
    /// <summary> 값 비교 연산에 사용되는 비교 타입을 정의합니다. </summary>
    [Flags]
    public enum Comparison : byte
    {
        /// <summary>Equals (==)</summary>
        [Tooltip("Equal (==)")]
        EQ = 1 << 0,

        /// <summary>Not Equals (!=)</summary>
        [Tooltip("Not Equal (!=)")]
        NE = 1 << 1,

        /// <summary>Greater Than (&gt;)</summary>
        [Tooltip("Greater Than (>)")]
        GT = 1 << 2,

        /// <summary>Greater Than or Equal (&gt;=)</summary>
        [Tooltip("Greater Than or Equal (>=)")]
        GE = 1 << 3,

        /// <summary>Less Than (&lt;)</summary>
        [Tooltip("Less Than (<)")]
        LT = 1 << 4,

        /// <summary>Less Than or Equal (&lt;=)</summary>
        [Tooltip("Less Than or Equal (<=)")]
        LE = 1 << 5
    };
    
    
    
    /// <summary> Parallel 노드에서 자식 노드들의 성공/실패를 평가하는 정책을 정의합니다. </summary>
    public enum ParallelPolicy : byte
    {
        /// <summary>모든 자식 노드가 성공해야만 성공을 반환. 하나라도 실패하면 즉시 실패</summary>
        [Tooltip("Returns Success only when all child nodes succeed. Returns Failure immediately if any child fails.")]
        RequireAllSuccess,

        /// <summary>하나 이상의 자식 노드가 성공하면 즉시 성공을 반환. 모든 자식이 실패해야만 실패</summary>
        [Tooltip("Returns Success immediately when at least one child node succeeds. Returns Failure only when all children fail.")]
        RequireOneSuccess,

        /// <summary>모든 자식 노드가 실패해야만 성공을 반환. 하나라도 성공하면 즉시 실패</summary>
        [Tooltip("Returns Success only when all child nodes fail. Returns Failure immediately if any child succeeds.")]
        RequireAllFailure,

        /// <summary>하나 이상의 자식 노드가 실패하면 즉시 성공을 반환. 모든 자식이 성공해야만 실패</summary>
        [Tooltip("Returns Success immediately when at least one child node fails. Returns Failure only when all children succeed.")]
        RequireOneFailure
    };
    
    
    
    /// <summary> 노드 실행 중 호출되는 콜백의 상태를 나타냅니다. </summary>
    public enum NodeCallState : byte
    {
        /// <summary>노드 진입 직전 상태</summary>
        [Tooltip("State before entering the node execution.")]
        BeforeEnter,
        
        /// <summary>노드 업데이트 중 상태</summary>
        [Tooltip("State during node update execution.")]
        Updating,
        
        /// <summary>노드 종료 직전 상태</summary>
        [Tooltip("State before exiting the node execution.")]
        BeforeExit,
    };
    
    
    
    /// <summary> TaskStreamer에서 지원하는 그래프 타입을 정의합니다. </summary>
    public enum GraphType : byte
    {
        /// <summary>행동 트리 (Behavior Tree) - 계층적 의사결정 구조</summary>
        [Tooltip("Behavior Tree - Hierarchical decision-making structure for AI behavior.")]
        BT = 0,
        
        /// <summary>유한 상태 머신 (Finite State Machine) - 상태 기반 행동 제어</summary>
        [Tooltip("Finite State Machine - State-based behavior control system.")]
        FSM = 1,
        
        /// <summary>목표 지향 액션 플래닝 (Goal Oriented Action Planning) - 목표 달성을 위한 자동 계획 생성</summary>
        [Tooltip("Goal Oriented Action Planning - Automatic planning system to achieve specified goals.")]
        GOAP = 2
    };
    
    
    
    /// <summary> 노드 실행 결과 상태를 나타냅니다. </summary>
    public enum Status : byte
    {
        /// <summary>실행 중 - 노드가 아직 작업을 완료하지 않음</summary>
        [Tooltip("Node is still executing and has not completed its task.")]
        Running,
        
        /// <summary>실패 - 노드 실행이 실패로 완료됨</summary>
        [Tooltip("Node execution completed with failure.")]
        Failure,
        
        /// <summary>성공 - 노드 실행이 성공으로 완료됨</summary>
        [Tooltip("Node execution completed successfully.")]
        Success
    };
    
    
    
    /// <summary> 다중 조건 평가 시 사용되는 정책을 정의합니다. </summary>
    public enum EvaluationPolicy : byte
    {
        /// <summary>하나라도 조건이 만족되면 true</summary>
        [Tooltip("Returns true if any of the conditions is satisfied.")]
        Any,
        
        /// <summary>모든 조건이 만족되어야 true</summary>
        [Tooltip("Returns true only if all conditions are satisfied.")]
        All
    };
    
    
    
    /// <summary> 그래프 업데이트 실행 타이밍을 정의합니다. </summary>
    public enum TickMode : byte
    {
        /// <summary>업데이트 없음</summary>
        [Tooltip("No automatic updates.")]
        None,
        
        /// <summary>수동 업데이트 - 명시적 호출 시에만 실행</summary>
        [Tooltip("Manual update - executes only when explicitly called.")]
        ManualUpdate,
        
        /// <summary>고정 업데이트 - Unity의 FixedUpdate 타이밍에 실행</summary>
        [Tooltip("Fixed update timing - executes during Unity's FixedUpdate cycle.")]
        FixedUpdate,
        
        /// <summary>늦은 업데이트 - Unity의 LateUpdate 타이밍에 실행</summary>
        [Tooltip("Late update timing - executes during Unity's LateUpdate cycle.")]
        LateUpdate,
        
        /// <summary>외부 업데이트 - 외부 시스템에 의해 제어됨</summary>
        [Tooltip("External update - controlled by external systems.")]
        ExternalUpdate,
    };

    

    /// <summary> 
    /// 그래프 순회(탐색) 방식의 유형을 정의합니다.
    /// </summary>
    public enum GraphIteratorType : byte
    {
        /// <summary> 
        /// 그래프의 기본 순회 방식을 정의합니다.  
        ///<para> BT - BFS (너비 우선 탐색) </para>
        ///FSM - LS (선형 탐색)
        /// </summary>
        [Tooltip("Default traversal mode depending on graph type. BT uses BFS, FSM uses LS.")]
        Default,
    
        /// <summary> 
        /// Linear Search — 노드를 만나는 순서대로 차례로 처리하는 방식입니다. 
        /// </summary>
        [Tooltip("Nodes are processed sequentially in the order they are encountered.")]
        LS,
    
        /// <summary> 
        /// Breadth-First Search — 현재 깊이의 모든 노드를 먼저 탐색한 후 다음 깊이로 진행하는 방식입니다. 
        /// </summary>
        [Tooltip("Explores all nodes at the current depth before moving to the next depth level.")]
        BFS
    };
    
    
    
    /// <summary> Until 노드가 지정된 조건이 만족될 때까지 자식 노드를 반복 실행하는 방식을 정의합니다. </summary>
    [Tooltip("Defines how the Until node repeatedly executes its child node until a specified condition is met.")]
    public enum UntilCondition
    {
        /// <summary>자식 노드가 실패할 때까지 반복 실행</summary>
        [Tooltip("Repeatedly executes the child node until it fails.")]
        Failure = 1,

        /// <summary>자식 노드가 성공할 때까지 반복 실행</summary>
        [Tooltip("Repeatedly executes the child node until it succeeds.")]
        Success = 2
    };

    
    
    /// <summary> 변수가 조건 평가에 사용됨을 나타냅니다. </summary>
    [Tooltip("Defines how variables are used in evaluation or as fields.")]
    public enum VariableUsage
    {
        /// <summary> 필드 변수로 사용됨을 나타냅니다. </summary>
        [Tooltip("Variable used as a field.")]
        Field = 0,

        /// <summary> 변수가 조건 평가에 사용됨을 나타냅니다. </summary>
        [Tooltip("Variable used in condition evaluation.")]
        Condition = 1
    };


    /// <summary> Represents the addition of an element to the list in the context of a notification for list changes.  </summary>
    public enum NotifyListChanged : byte
    {
        /// <summary> Represents the removal of an element from the list in the context of a notification for list changes. </summary>
        Remove = 0,

        /// <summary> Represents an addition operation to a list, typically indicating that an item has been added in the context of a list change notification. </summary>
        Add = 1,
    }


    [Flags]
    public enum SubGraphTransitionPolicy
    {
        AllowAnyNodeTransition = 1,
        AllowLinkedTransition = 2,
    }
}