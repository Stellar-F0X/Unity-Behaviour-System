namespace TaskStreamer.FSM
{
    public class EnterState : StateBase
    {
        public override EStateNodeType nodeType
        {
            get { return EStateNodeType.Enter; }
        }

        protected override void OnUpdate()
        {
            
        }
    }
}