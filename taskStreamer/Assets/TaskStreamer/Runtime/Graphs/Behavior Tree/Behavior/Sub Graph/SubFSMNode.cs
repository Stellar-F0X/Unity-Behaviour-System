using Unity.Properties;

namespace TaskStreamer.Runtime.BT
{
    [GeneratePropertyBag, TaskDescription]
    internal class SubFSMNode : SubGraphNode
    {
        public override GraphType subGraphType
        {
            get { return GraphType.FSM; }
        }
    }
}