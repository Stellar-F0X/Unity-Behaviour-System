namespace TaskStreamer.BT
{
    public class BlackboardBasedUntilForNode : ConditionNodeBase
    {
        public override string tooltip
        {
            get { return "Keeps executing the child node until all blackboard conditions are satisfied."; }
        }


        protected override Status OnUpdate()
        {
            if (conditions != null && this.CheckCondition())
            {
                return Status.Success;
            }
            else
            {
                child.UpdateNode();
                return Status.Running;
            }
        }
    }
}