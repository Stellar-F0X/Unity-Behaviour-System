using Unity.Properties;

namespace TaskStreamer.FSM
{
    [GeneratePropertyBag, Readable]
    public class ExitState : StateBase
    {
        public override StateNodeType nodeType
        {
            get { return StateNodeType.Exit; }
        }

        protected override void OnUpdate() { }
    }
}