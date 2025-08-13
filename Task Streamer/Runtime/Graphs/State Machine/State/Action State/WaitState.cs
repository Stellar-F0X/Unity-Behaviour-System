namespace TaskStreamer.FSM
{
    public class WaitState : ActionState
    {
        public float waitTime = 1f;
        
        protected override void OnEnter()
        {
            this.blockTransition = true;
        }

        protected override void OnUpdate()
        {
            if (base.elapsedTime < waitTime)
            {
                return;
            }
            
            this.blockTransition = false;
        }
    }
}