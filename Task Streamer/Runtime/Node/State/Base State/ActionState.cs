namespace TaskStreamer.FSM
{
    public abstract class ActionState : StateBase
    {
        public override EStateNodeType nodeType
        {
            get { return EStateNodeType.Action; }
        }
    }
}