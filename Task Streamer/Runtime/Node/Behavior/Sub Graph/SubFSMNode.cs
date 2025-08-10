namespace TaskStreamer.BT
{
    public class SubFSMNode : SubGraphNode
    {
        public override GraphType subGraphType
        {
            get { return GraphType.FSM; }
        }
    }
}