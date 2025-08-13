namespace TaskStreamer.FSM
{
    public class EnterState : StateBase
    {
        public override StateNodeType nodeType
        {
            get { return StateNodeType.Enter; }
        }

        protected override void OnUpdate()
        {
            
        }
    }
}