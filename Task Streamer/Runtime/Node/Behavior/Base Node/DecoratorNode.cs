using System;
using System.Collections.Generic;

namespace TaskStreamer.BT
{
    [Serializable]
    public abstract class DecoratorNode : BehaviorNodeBase, IChildNodeProvider
    {
        public BehaviorNodeBase child
        {
            get
            {
                if (_children.Count == 0)
                {
                    return null;
                }

                return _children[0];
            }
            
            set
            {
                if (value is null)
                {
                    _children.Clear();
                }
                else if (_children.Count == 1)
                {
                    _children[0] = value;
                }
                else
                {
                    _children.Add(value);
                }
            }
        }


        public override sealed EBehaviorNodeType nodeType
        {
            get { return EBehaviorNodeType.Decorator; }
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
    }
}