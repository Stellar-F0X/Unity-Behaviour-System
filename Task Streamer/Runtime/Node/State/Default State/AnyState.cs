namespace TaskStreamer.FSM
{
    public class AnyState : StateBase
    {
        public override EStateNodeType nodeType
        {
            get { return EStateNodeType.Any; }
        }

        protected override void OnUpdate()
        {
            
        }
    }
}