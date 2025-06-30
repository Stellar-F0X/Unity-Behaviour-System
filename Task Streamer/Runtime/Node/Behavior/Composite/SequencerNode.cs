namespace TaskStreamer.BT
{
    public sealed class SequencerNode : CompositeNode
    {
        private int _childrenCount;
        private bool _childrenIsInvalid;

        public override void OnAwake()
        {
            _childrenIsInvalid = children is null || children.Count == 0;
        }


        protected override EStatus OnUpdate()
        {
            if (_childrenIsInvalid)
            {
                return EStatus.Failure;
            }

            switch (children[_currentChildrenIndex].UpdateNode())
            {
                case EStatus.Running: return EStatus.Running;

                case EStatus.Failure: return EStatus.Failure;

                case EStatus.Success: _currentChildrenIndex++; break;
            }

            if (_currentChildrenIndex == children.Count)
            {
                return EStatus.Success;
            }
            else
            {
                return EStatus.Running;
            }
        }
    }
}