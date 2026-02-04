using System;
using Unity.Properties;

namespace TaskStreamer.Runtime.FSM
{
    [Serializable, GeneratePropertyBag, TaskDescription]
    public class WaitState : ActionState
    {
        [DefaultValue(1f)]
        public BlackboardVariable<float> waitTime;
        
        private float _startTime;
        
        protected override void OnEnter()
        {
            this.blockTransition = true;
        }

        protected override void OnUpdate()
        {
            if (base.elapsedTime < waitTime.value)
            {
                return;
            }
            
            this.blockTransition = false;
        }
    }
}