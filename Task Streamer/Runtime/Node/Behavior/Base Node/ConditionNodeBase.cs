using UnityEngine;

namespace TaskStreamer.BT
{
    public abstract class ConditionNodeBase : DecoratorNode
    {
        [Tooltip("How to evaluate conditions \n(Any: OR logic, All: AND logic)")]
        public ECompleteType completeType;
        
        [Space(2)]
        public BlackboardBasedCondition conditions;
        
        
        protected virtual bool CheckCondition()
        {
            return conditions.Execute(completeType);
        }
    }
}