using System;

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


        protected override Status OnUpdate()
        {
            if (_childrenIsInvalid)
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