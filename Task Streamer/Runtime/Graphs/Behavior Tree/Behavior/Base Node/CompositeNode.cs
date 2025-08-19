using System;
using System.Collections.Generic;

namespace TaskStreamer.BT
{
    public abstract class CompositeNode : BehaviorNodeBase, IChildProvider
    {
        protected int _currentChildrenIndex = 0;

        public BehaviorNodeBase this[int index]
        {
            get { return children[index]; }
        }
        
        public List<BehaviorNodeBase> children
        {
            get { return base._children; }
        }

        public override sealed BehaviorNodeType nodeType
        {
            get { return BehaviorNodeType.Composite; }
        }

        public int childCount
        {
            get { return base._children.Count; }
        }
        
        
        public IEnumerable<NodeBase> GetChildren()
        {
            return children;
        }


        internal override void OnInstantiate()
        {
            base.onNodeExit -= this.ResetChildrenIndex;
            base.onNodeExit += this.ResetChildrenIndex;
        }


        private void ResetChildrenIndex()
        {
            _currentChildrenIndex = 0;
        }
    }
}
