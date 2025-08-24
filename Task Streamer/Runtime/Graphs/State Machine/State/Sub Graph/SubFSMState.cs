using Unity.Properties;

namespace TaskStreamer.FSM
{
    [GeneratePropertyBag, Readable]
    public class SubFSMState : SubGraphState
    {
        public override GraphType subGraphType
        {
            get { return GraphType.FSM; }
        }
    }
}