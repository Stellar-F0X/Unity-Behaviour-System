using System;
using UnityEngine;

namespace TaskStreamer.FSM
{
    [Serializable, Readable]
    public class LogState : ActionState
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
        

        protected override void OnUpdate()
        {
            if (string.IsNullOrEmpty(onUpdateMessages.value))
            {
                return;
            }
            
            Debug.Log(onUpdateMessages.value);
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