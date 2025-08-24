using Unity.Properties;

namespace TaskStreamer.FSM
{
    [GeneratePropertyBag]
    public class ExitState : StateBase
    {
        public override StateNodeType nodeType
        {
            get { return StateNodeType.Exit; }
        }

        protected override void OnUpdate() { }
    }
}