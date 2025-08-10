using UnityEngine;

namespace TaskStreamer.BT
{
    public abstract class ConditionNodeBase : DecoratorNode
    {
        [Space(2)]
        public BlackboardBasedCondition conditions;
        
        
        protected virtual bool CheckCondition()
        {
            return conditions.Execute();
        }
    }
}