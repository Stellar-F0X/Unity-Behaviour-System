using System;

namespace TaskStreamer.FSM
{
    public class SubBTState : SubGraphState
    {
        public override GraphType subGraphType
        {
            get { return GraphType.BT; }
        }
    }
}