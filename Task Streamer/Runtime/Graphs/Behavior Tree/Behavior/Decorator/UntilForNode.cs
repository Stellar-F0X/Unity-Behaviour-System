namespace TaskStreamer.BT
{
    public class UntilForNode : DecoratorNode
    {
        public enum EUntilCondition
        {
            Failure = 1,
            Success = 2
        };

        public EUntilCondition targetResult = EUntilCondition.Success;


        public override string tooltip
        {
            get { return "Executes the child node repeatedly until it returns the specified result."; }
        }


        protected override Status OnUpdate()
        {
            switch (child.UpdateNode())
            {
                case Status.Failure:
                {
                    if (targetResult == EUntilCondition.Failure)
                    {
                        return Status.Failure;
                    }

                    break;
                }

                case Status.Success:
                {
                    if (targetResult == EUntilCondition.Success)
                    {
                        return Status.Success;
                    }

                    break;
                }
                
                default: return Status.Running;
            }
            
            return Status.Running;
        }
    }
}