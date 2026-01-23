using Unity.Properties;

namespace TaskStreamer.Runtime.FSM
{
    [GeneratePropertyBag, Readable]
    public class EnterState : StateBase
    {
        public override StateNodeType nodeType
        {
            get { return StateNodeType.Enter; }
        }

        protected override void OnUpdate() { }
    }
}