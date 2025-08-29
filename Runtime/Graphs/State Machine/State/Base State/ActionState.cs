namespace TaskStreamer.FSM
{
    public abstract class ActionState : StateBase
    {
        public override StateNodeType nodeType
        {
            get { return StateNodeType.Action; }
        }
    }
}