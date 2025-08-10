using UnityEngine;

namespace TaskStreamer.BT
{
    public class LogNode : ActionNode
    {
        public string onEnterMessages;
        public string onUpdateMessages;
        public string onExitMessages;


        protected override void OnEnter()
        {
            if (string.IsNullOrEmpty(onEnterMessages))
            {
                return;
            }

            Debug.Log(onEnterMessages);
        }


        protected override Status OnUpdate()
        {
            if (string.IsNullOrEmpty(onUpdateMessages) == false)
            {
                Debug.Log(onUpdateMessages);
            }

            return Status.Success;
        }


        protected override void OnExit()
        {
            if (string.IsNullOrEmpty(onExitMessages))
            {
                return;
            }
            
            Debug.Log(onExitMessages);
        }
    }
}