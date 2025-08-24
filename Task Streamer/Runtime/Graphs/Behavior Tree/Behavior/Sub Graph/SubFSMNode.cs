using Unity.Properties;

namespace TaskStreamer.BT
{
    [GeneratePropertyBag, Readable]
    public class SubFSMNode : SubGraphNode
    {
        public override GraphType subGraphType
        {
            get { return GraphType.FSM; }
        }
    }
}