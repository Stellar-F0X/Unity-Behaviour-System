using System;

namespace TaskStreamer.BT
{
    [Serializable]
    public sealed class BlackboardBasedConditionNode : ConditionNodeBase
    {
        public override string tooltip
        {
            get 
            {
                return "You can set conditions based on blackboard data." +
                         "\nSuccessful: it executes its child." +
                         "\nFailed: it stops all children that were running under this node with Failure."; 
            }
        }

        
        protected override Status OnUpdate()
        {
            if (base.conditions is not null && this.CheckCondition())
            {
                return base.child.UpdateNode();
            }
            else
            {
                return Status.Failure;
            }
        }
    }
}