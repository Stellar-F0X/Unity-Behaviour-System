using System;
using Unity.Properties;
using UnityEngine;

namespace TaskStreamer.Runtime.BT
{
    [Serializable, GeneratePropertyBag, Readable]
    public class LogTunnelNode : DecoratorNode
    {
        public BlackboardVariable<string> onEnterMessages;

        public BlackboardVariable<string> onUpdateMessages;

        public BlackboardVariable<string> onExitMessages;


        protected override void OnEnter()
        {
            if (string.IsNullOrEmpty(onEnterMessages.value))
            {
                return;
            }
            
            Debug.Log(onEnterMessages.value);
        }


        protected override Status OnUpdate()
        {
            if (string.IsNullOrEmpty(onUpdateMessages.value))
            {
                return Status.Failure;
            }
            
            Debug.Log(onUpdateMessages.value);

            if (this.child is null)
            {
                return Status.Failure;
            }
            else
            {
                return child.UpdateNode();
            }
        }


        protected override void OnExit()
        {
            if (string.IsNullOrEmpty(onExitMessages.value))
            {
                return;
            }
            
            Debug.Log(onExitMessages.value);
        }
    }
}