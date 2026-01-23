using Unity.Properties;
using UnityEngine;

namespace TaskStreamer.Runtime.FSM
{
    [GeneratePropertyBag, Readable]
    public class ExitState : StateBase
    {
        public override StateNodeType nodeType
        {
            get { return StateNodeType.Exit; }
        }


        protected override void OnUpdate()
        {
            base.machine.StopGraph();
        }
    }
}