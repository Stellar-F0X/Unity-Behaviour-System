using Unity.Properties;

namespace TaskStreamer.Runtime.FSM
{
    [GeneratePropertyBag, Readable]
    internal class SubBTState : SubGraphState
    {
        public override GraphType subGraphType
        {
            get { return GraphType.BT; }
        }
    }
}