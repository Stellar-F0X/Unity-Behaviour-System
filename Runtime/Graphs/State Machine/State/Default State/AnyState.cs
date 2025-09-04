using Unity.Properties;

namespace TaskStreamer.FSM
{
    [GeneratePropertyBag, Readable]
    public class AnyState : StateBase
    {
        public override StateNodeType nodeType
        {
            get { return StateNodeType.Any; }
        }

        protected override void OnUpdate() { }
    }
}