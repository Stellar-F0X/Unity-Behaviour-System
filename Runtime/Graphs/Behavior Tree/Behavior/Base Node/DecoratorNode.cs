using System.Collections.Generic;

namespace TaskStreamer.BT
{
    public abstract class DecoratorNode : BehaviorNodeBase, IChildProvider
    {
        public DecoratorNode()
        {
            _children.Add(null);
        }


        public BehaviorNodeBase child
        {
            get { return _children[0]; }

            set { _children[0] = value; }
        }


        public override sealed BehaviorNodeType nodeType
        {
            get { return BehaviorNodeType.Decorator; }
        }

        public BehaviorNodeBase this[int index]
        {
            get { return _children[index]; }
        }

        public int childCount
        {
            get { return _children[0] == null ? 0 : _children.Count; }
        }


        public IEnumerable<NodeBase> GetChildren()
        {
            return _children;
        }
    }
}