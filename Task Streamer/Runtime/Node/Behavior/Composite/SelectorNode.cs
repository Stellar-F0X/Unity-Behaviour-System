namespace TaskStreamer.BT
{
    public sealed class SelectorNode : CompositeNode
    {
        private bool _isChildrenInvalid;

        
        public override void OnAwake()
        {
            _isChildrenInvalid = children is null || children.Count == 0;
        }

        
        protected override EStatus OnUpdate()
        {
            if (_isChildrenInvalid)
            {
                return EStatus.Failure;
            }

            switch (children[_currentChildrenIndex].UpdateNode())
            {
                case EStatus.Success: return EStatus.Success;

                case EStatus.Running: return EStatus.Running;

                case EStatus.Failure: _currentChildrenIndex++; break;
            }
            
            if (_currentChildrenIndex == children.Count)
            {
                return EStatus.Failure;
            }
            else
            {
                return EStatus.Running;
            }
        }
    }
}