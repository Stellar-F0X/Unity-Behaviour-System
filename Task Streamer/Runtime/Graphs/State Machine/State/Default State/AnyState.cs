namespace TaskStreamer.FSM
{
    public class AnyState : StateBase
    {
        public override StateNodeType nodeType
        {
            get { return StateNodeType.Any; }
        }

        protected override void OnUpdate() { }
    }
}