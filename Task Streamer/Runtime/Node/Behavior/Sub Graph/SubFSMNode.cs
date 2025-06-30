namespace TaskStreamer.BT
{
    public class SubFSMNode : SubGraphNode
    {
        public override EGraphType subGraphType
        {
            get { return EGraphType.FSM; }
        }
    }
}