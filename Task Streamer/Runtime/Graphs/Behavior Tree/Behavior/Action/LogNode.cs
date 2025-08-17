using UnityEngine;

namespace TaskStreamer.BT
{
    public class LogNode : ActionNode
    {
        public BlackboardVariable<string> onEnterMessages = "";
        
        public BlackboardVariable<string> onUpdateMessages = "";
        
        public BlackboardVariable<string> onExitMessages = "";


        
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
            if (string.IsNullOrEmpty(onUpdateMessages.value) == false)
            {
                Debug.Log(onUpdateMessages.value);
            }

            return Status.Success;
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