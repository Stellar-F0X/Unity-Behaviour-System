namespace TaskStreamer.Runtime.FSM
{
    public abstract class ActionState : StateBase
    {
        public override StateNodeType nodeType
        {
            get { return StateNodeType.Action; }
        }
    }
}