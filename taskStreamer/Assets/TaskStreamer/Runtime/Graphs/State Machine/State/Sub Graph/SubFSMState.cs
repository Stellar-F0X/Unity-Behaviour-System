using Unity.Properties;

namespace TaskStreamer.Runtime.FSM
{
    [GeneratePropertyBag, TaskDescription]
    internal class SubFSMState : SubGraphState 
    { 
        public override GraphType subGraphType 
        { 
            get { return GraphType.FSM; } 
        } 
    } 
}