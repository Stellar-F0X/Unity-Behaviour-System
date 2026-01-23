using Unity.Properties;

namespace TaskStreamer.Runtime.BT
{
    [GeneratePropertyBag, Readable]
    internal class SubBTNode : SubGraphNode
    {
        public override GraphType subGraphType
        {
            get { return GraphType.BT; }
        }
    }
}