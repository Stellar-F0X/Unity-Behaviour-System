using System;
using System.Runtime.CompilerServices;
using UnityEngine;

[assembly: InternalsVisibleTo("TaskStreamer.Tool")]
namespace TaskStreamer
{
    // 상태 노드의 유형을 정의하는 열거형
    public enum EStateNodeType : byte
    {
        Enter, // 진입 상태 (직접 생성 불가)
        
        Any,   // Any 상태 (직접 생성 불가)
        
        Action,  // 유저가 직접 생성 가능한 일반 상태
        
        SubGraph,
        
        Exit   // 종료 상태 (직접 생성 불가)
    };
    
    
    public enum EBehaviorNodeType : byte
    {
        Root,
        
        Action,
        
        Composite,
        
        Decorator,
        
        SubGraph
    };
    
    
    [Flags]
    public enum EComparison : byte
    {
        /// <summary>None</summary>
        None = 0,
        
        /// <summary> Equal (==)</summary>
        Equal = 1 << 0,
        
        /// <summary>Not Equal (!=)</summary>
        NotEqual = 1 << 1,
        
        /// <summary>Greater Than (&gt;)</summary>
        GreaterThan = 1 << 2,
        
        /// <summary>Greater Than or Equal (&gt;=)</summary>
        GreaterThanOrEqual = 1 << 3,
        
        /// <summary>Less Than (&lt;)</summary>
        LessThan = 1 << 4,
        
        /// <summary>Less Than or Equal (&lt;=)</summary>
        LessThanOrEqual = 1 << 5,
        
        /// <summary>Numeric preset: all numeric comparisons</summary>
        NumericPreset = Equal | NotEqual | GreaterThan | GreaterThanOrEqual | LessThan | LessThanOrEqual,
        
        /// <summary>Boolean preset: Equal, NotEqual</summary>
        BooleanPreset = Equal | NotEqual,
        
        /// <summary>Object preset: Equal, NotEqual</summary>
        ObjectPreset = Equal | NotEqual
    };
    
    
    public enum EParallelPolicy : byte
    {
        [Tooltip("Returns Success only when all child nodes succeed. Returns Failure immediately if any child fails.")]
        RequireAllSuccess,

        [Tooltip("Returns Success immediately when at least one child node succeeds. Returns Failure only when all children fail.")]
        RequireOneSuccess,

        [Tooltip("Returns Success only when all child nodes fail. Returns Failure immediately if any child succeeds.")]
        RequireAllFailure,

        [Tooltip("Returns Success immediately when at least one child node fails. Returns Failure only when all children succeed.")]
        RequireOneFailure
    };
    
    
    public enum ENodeCallState : byte
    {
        BeforeEnter,
        
        Updating,
        
        BeforeExit,
    };
    
    
    public enum EGraphType : byte
    {
        /// <summary> Behavior Tree. </summary>
        BT = 0,
        
        /// <summary> Finite State Machine. </summary>
        FSM = 1,
        
        /// <summary> Goal Oriented Action Planning. </summary>
        GOAP = 2
    };
    
    
    public enum EStatus : byte
    {
        Running,
        
        Failure,
        
        Success
    };
    
    
    public enum ECompleteType
    {
        Any,
        
        All
    };
    
    
    public enum ETickMode : byte
    {
        /// <summary> None </summary>
        None,
        
        /// <summary> Update </summary>
        MenualUpdate,
        
        /// <summary> Fixed Update </summary>
        FixedUpdate,
        
        /// <summary> Late Update </summary>
        LateUpdate,
        
        /// <summary> External Update </summary>
        ExternalUpdate,
    };
}