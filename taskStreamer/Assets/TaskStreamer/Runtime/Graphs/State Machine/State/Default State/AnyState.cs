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


        
        protected override bool CanTransition(out NodeBase nextState)
        {
            if (base.CanTransition(out nextState) == false)
            {
                return false;
            }

            if (nextState is not SubGraphState subGraphState)
            {
                return true;
            }

            SubGraphTransitionPolicy policy = subGraphState.transitionPolicy.value;
            
            //동작 도중에 AnyNode를 통해 갑작스러운 전이를 허용한다면 True를 반환.
            if ((policy & SubGraphTransitionPolicy.AllowAnyWhileRunning) > 0)
            {
                return true;
            }

            return false;
        }
    }
}