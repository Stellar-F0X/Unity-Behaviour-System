using Unity.Properties;

namespace TaskStreamer.Runtime.BT
{
    [GeneratePropertyBag, TaskDescription]
    internal class SubBTNode : SubGraphNode
    {
        public override GraphType subGraphType
        {
            get { return GraphType.BT; }
        }
    }
}