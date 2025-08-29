using Unity.Properties;

namespace TaskStreamer.BT
{
    [GeneratePropertyBag]
    public class InverterNode : DecoratorNode
    {
        public override string tooltip
        {
            get { return "Inverts the result of the child node (Success to Failure, Failure to Success)"; }
        }

        protected override Status OnUpdate()
        {
            switch (child.UpdateNode())
            {
                case Status.Failure: return Status.Success;

                case Status.Success: return Status.Failure;
                
                default: return Status.Running;
            }
        }
    }
}