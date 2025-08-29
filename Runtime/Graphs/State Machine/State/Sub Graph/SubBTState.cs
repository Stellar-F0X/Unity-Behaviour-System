using Unity.Properties;

namespace TaskStreamer.FSM
{
    [GeneratePropertyBag, Readable]
    public class SubBTState : SubGraphState
    {
        public override GraphType subGraphType
        {
            get { return GraphType.BT; }
        }
    }
}