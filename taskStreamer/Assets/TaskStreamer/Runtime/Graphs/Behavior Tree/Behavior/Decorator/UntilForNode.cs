using System;
using Unity.Properties;

namespace TaskStreamer.Runtime.BT
{
    [Serializable, GeneratePropertyBag, TaskDescription]
    public class UntilForNode : DecoratorNode
    {
        [DefaultValue(UntilCondition.Success)]
        public BlackboardVariable<UntilCondition> targetResult;


        public override string tooltip
        {
            get { return "Executes the child node repeatedly until it returns the specified result."; }
        }


        protected override Status OnUpdate()
        {
            switch (child.UpdateNode())
            {
                case Status.Failure: return targetResult.value == UntilCondition.Failure ? Status.Success : Status.Running;

                case Status.Success: return targetResult.value == UntilCondition.Success ? Status.Success : Status.Running;
                
                default: return Status.Failure;
            }
        }
    }
}