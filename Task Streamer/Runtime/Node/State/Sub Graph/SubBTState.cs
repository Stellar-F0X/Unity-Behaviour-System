namespace TaskStreamer.FSM
{
    public class SubBTState : SubGraphState
    {
        public override EGraphType subGraphType
        {
            get { return EGraphType.BT; }
        }
    }
}