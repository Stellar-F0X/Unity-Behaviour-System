using UnityEngine;

namespace TaskStreamer.BT
{
    public class RepeaterNode : DecoratorNode
    {
        public uint repeatCount;
        
        [SerializeField, ReadOnly]
        private int _currentCount;


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
            if (_currentCount < repeatCount)
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