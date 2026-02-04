using System;
using Unity.Properties;
using UnityEngine;

namespace TaskStreamer.Runtime.BT
{
    [TaskDescription, GeneratePropertyBag, Serializable]
    public class WaitNode : ActionNode
    {
        [DefaultValue(1.0f)]
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