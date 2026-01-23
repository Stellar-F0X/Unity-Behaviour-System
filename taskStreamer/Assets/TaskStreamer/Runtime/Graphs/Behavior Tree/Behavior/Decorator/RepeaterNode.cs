using System;
using Unity.Properties;

namespace TaskStreamer.Runtime.BT
{
    [Serializable, GeneratePropertyBag, Readable]
    public class RepeaterNode : DecoratorNode
    {
        [DefaultValue(5)]
        public BlackboardVariable<int> repeatCount;
        
        private int _currentCount = 0;


        public override string tooltip
        {
            get { return "Repeats the child node a specified number of times."; }
        }


        protected override void OnEnter()
        {
            _currentCount = 0;
        }


        protected override Status OnUpdate()
        {
            if (_currentCount < repeatCount.value)
            {
                if (child.UpdateNode() != Status.Running)
                {
                    _currentCount++;
                }
                
                return Status.Running;
            }

            return Status.Success;
        }
    }
}