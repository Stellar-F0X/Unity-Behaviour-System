using TaskStreamer.Runtime.BT;

namespace TaskStreamer.Runtime.Utility
{
    /// <summary> 트리 순회 정보를 담는 구조체 </summary>
    internal readonly struct TreeTraversal
    {
        public TreeTraversal(BehaviorNodeBase node, int depth = 0, int stackID = 0)
        {
            this.node = node;
            this.depth = depth;
            this.stackID = stackID;
        }

        public readonly BehaviorNodeBase node;
        public readonly int depth;
        public readonly int stackID;
    }
}