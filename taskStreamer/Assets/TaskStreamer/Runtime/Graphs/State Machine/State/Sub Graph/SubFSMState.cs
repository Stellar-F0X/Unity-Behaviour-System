using Unity.Properties;

namespace TaskStreamer.FSM
{
    [GeneratePropertyBag, Readable]
    internal class SubFSMState : SubGraphState
    {
        public override GraphType subGraphType
        {
            get { return GraphType.FSM; }
        }
    }
}