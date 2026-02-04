using System;
using Unity.Properties;
using UnityEngine;

namespace TaskStreamer.Runtime.FSM
{
    [Serializable, GeneratePropertyBag, TaskDescription]
    internal class AnyTransition : Transition
    {
        internal AnyTransition(NodeBase sourceNode, NodeBase destinationNode) : base(sourceNode, destinationNode) { }


        [SerializeField, PropertyOrder(0)]
        private BlackboardVariable<bool> _canTransitionToSelf;


        
        public override bool CheckConditions()
        {
            if (this._canTransitionToSelf.value)
            {
                return base.CheckConditions();
            }
            
            if (this._destinationNode.callState == NodeCallState.Updating)
            {
                return false;
            }
            else
            {
                return base.CheckConditions();
            }
        }
    }
}