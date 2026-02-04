using Unity.Properties;

namespace TaskStreamer.Runtime.FSM
{
    [GeneratePropertyBag, TaskDescription]
    internal class SubBTState : SubGraphState
    {
        public override GraphType subGraphType
        {
            get { return GraphType.BT; }
        }
    }
}