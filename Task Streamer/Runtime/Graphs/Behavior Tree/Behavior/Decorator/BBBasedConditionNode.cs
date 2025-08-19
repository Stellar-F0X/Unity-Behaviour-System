using System;
using Unity.Properties;

namespace TaskStreamer.BT
{
    [Serializable, Readable]
    public sealed class BBBasedConditionNode : DecoratorNode
    {
        public BlackboardBasedCondition conditions = new BlackboardBasedCondition();
        
        public override string tooltip
        {
            get 
            {
                return "You can set conditions based on blackboard data." +
                         "\nSuccessful: it executes its child." +
                         "\nFailed: it stops all children that were running under this node with Failure."; 
            }
        }


        internal override void OnCreateInEditor()
        {
            description = "BB = Blackboard";
        }


        protected override Status OnUpdate()
        {
            if (this.conditions is not null && this.conditions.Execute())
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