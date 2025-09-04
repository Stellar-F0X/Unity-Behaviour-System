using System.Collections.Generic;
using Unity.Properties;

namespace TaskStreamer.BT
{
    [GeneratePropertyBag, Readable]
    public sealed class RootNode : BehaviorNodeBase, IChildProvider
    {
        public RootNode()
        {
            _children.Add(null);
        }
        
        
        public BehaviorNodeBase child
        {
            get { return _children[0]; }
            
            set { _children[0] = value; }
        }

        public override BehaviorNodeType nodeType
        {
            get { return BehaviorNodeType.Root; }
        }
        
        public BehaviorNodeBase this[int index]
        {
            get { return _children[index]; }
        }
        
        public int childCount
        {
            get { return _children.Count; }
        }
        
        public IEnumerable<NodeBase> GetChildren()
        {
            return _children;
        }

        
        protected override Status OnUpdate()
        {
            if (child is null)
            {
                return Status.Failure;
            }
            else
            {
                return child.UpdateNode();
            }
        }
    }
}