using System.Collections.Generic;

namespace TaskStreamer.BT
{
    public interface IChildNodeProvider
    {
        public BehaviorNodeBase this[int index] { get; }
        
        public int childCount { get; }
        
        public IEnumerable<NodeBase> GetChildren();
    }
}