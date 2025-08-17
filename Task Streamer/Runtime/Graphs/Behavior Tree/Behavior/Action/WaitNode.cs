using UnityEngine;

namespace TaskStreamer.BT
{
    [Readable]
    public class WaitNode : ActionNode
    {
        public BlackboardVariable<float> waitTime = 1f;
        
        private float _startTime;
        
        protected override void OnEnter()
        {
            _startTime = Time.time;
        }

        protected override Status OnUpdate()
        {
            if (Time.time < _startTime + waitTime.value)
            {
                return Status.Running;
            }
            
            
            return Status.Success;
        }
    }
}