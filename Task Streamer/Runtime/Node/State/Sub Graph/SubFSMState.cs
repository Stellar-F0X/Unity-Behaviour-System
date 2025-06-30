namespace TaskStreamer.FSM
{
    public class SubFSMState : SubGraphState
    {
        public override EGraphType subGraphType
        {
            get { return EGraphType.FSM; }
        }
    }
}