using System;

namespace TaskStreamer.FSM
{
    [Serializable, Readable]
    public class WaitState : ActionState
    {
        [SetValue(1f)]
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