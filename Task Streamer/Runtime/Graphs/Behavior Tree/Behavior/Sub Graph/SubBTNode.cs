namespace TaskStreamer.BT
{
    public class SubBTNode : SubGraphNode
    {
        public override GraphType subGraphType
        {
            get { return GraphType.BT; }
        }
    }
}