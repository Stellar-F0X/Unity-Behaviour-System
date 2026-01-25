using System.Collections.Generic;

namespace TaskStreamer.Runtime.BT
{
    public interface IChildNode
    {
        public BehaviorNodeBase this[int index] { get; }
        
        public int childCount { get; }
        
        public IEnumerable<NodeBase> GetChildren();
    }
}