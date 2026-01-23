using System.Collections.Generic;

namespace TaskStreamer.Runtime.BT
{
    public interface IChildProvider
    {
        public BehaviorNodeBase this[int index] { get; }
        
        public int childCount { get; }
        
        public IEnumerable<NodeBase> GetChildren();
    }
}