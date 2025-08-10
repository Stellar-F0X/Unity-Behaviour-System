namespace TaskStreamer.BT
{
    public sealed class SelectorNode : CompositeNode
    {
        private bool _isChildrenInvalid;

        
        public override void OnAwake()
        {
            _isChildrenInvalid = children is null || children.Count == 0;
        }

        
        protected override Status OnUpdate()
        {
            if (_isChildrenInvalid)
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