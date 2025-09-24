using Unity.Properties;

namespace TaskStreamer.BT
{
    [GeneratePropertyBag, Readable]
    public sealed class SelectorNode : CompositeNode
    {
        protected override Status OnUpdate()
        {
            if (children is null || children.Count == 0)
            {
                return Status.Failure;
            }

            switch (children[_currentChildrenIndex].UpdateNode())
            {
                case Status.Success: return Status.Success;

                case Status.Running: return Status.Running;

                case Status.Failure: _currentChildrenIndex++; break;
            }
            
            if (_currentChildrenIndex == children.Count)
            {
                return Status.Failure;
            }
            else
            {
                return Status.Running;
            }
        }
    }
}