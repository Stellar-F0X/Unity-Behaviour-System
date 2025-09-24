using Unity.Properties;

namespace TaskStreamer.BT
{
    [GeneratePropertyBag, Readable]
    public sealed class SequencerNode : CompositeNode
    {
        protected override Status OnUpdate()
        {
            if (children is null || children.Count == 0)
            {
                return Status.Failure;
            }

            switch (children[_currentChildrenIndex].UpdateNode())
            {
                case Status.Running: return Status.Running;

                case Status.Failure: return Status.Failure;

                case Status.Success: _currentChildrenIndex++; break;
            }

            if (_currentChildrenIndex == children.Count)
            {
                return Status.Success;
            }
            else
            {
                return Status.Running;
            }
        }
    }
}