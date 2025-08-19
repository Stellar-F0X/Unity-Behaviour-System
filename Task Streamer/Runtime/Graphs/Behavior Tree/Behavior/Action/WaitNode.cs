using System;
using UnityEngine;

namespace TaskStreamer.BT
{
    [Readable, Serializable]
    public class WaitNode : ActionNode
    {
        [SetValue(1f)]
        public BlackboardVariable<float> waitTime;
        
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