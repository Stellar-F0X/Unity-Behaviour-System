namespace TaskStreamer.FSM
{
    public class ExitState : StateBase
    {
        public override EStateNodeType nodeType
        {
            get { return EStateNodeType.Exit; }
        }

        protected override void OnUpdate()
        {
            
        }
    }
}