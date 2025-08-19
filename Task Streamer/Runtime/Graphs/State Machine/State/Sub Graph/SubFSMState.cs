using System;

namespace TaskStreamer.FSM
{
    public class SubFSMState : SubGraphState
    {
        public override GraphType subGraphType
        {
            get { return GraphType.FSM; }
        }
    }
}