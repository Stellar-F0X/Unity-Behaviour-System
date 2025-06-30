namespace TaskStreamer.BT
{
    public abstract class ActionNode : BehaviorNodeBase
    {
        public override sealed EBehaviorNodeType nodeType
        {
            get { return EBehaviorNodeType.Action; }
        }
    }
}