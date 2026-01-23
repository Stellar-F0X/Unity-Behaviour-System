namespace TaskStreamer.Runtime.BT
{
    public abstract class ActionNode : BehaviorNodeBase
    {
        public override sealed BehaviorNodeType nodeType
        {
            get { return BehaviorNodeType.Action; }
        }
    }
}