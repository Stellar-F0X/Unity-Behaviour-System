using UnityEngine;

namespace TaskStreamer.FSM
{
    public class LogState : ActionState
    {
        public string enterLogMessage;
        public string updateLogMessage;
        public string exitLogMessage;


        protected override void OnEnter()
        {
            if (string.IsNullOrEmpty(enterLogMessage))
            {
                return;
            }

            Debug.Log(enterLogMessage);
        }
        

        protected override void OnUpdate()
        {
            if (string.IsNullOrEmpty(updateLogMessage))
            {
                return;
            }
            
            Debug.Log(updateLogMessage);
        }
        

        protected override void OnExit()
        {
            if (string.IsNullOrEmpty(exitLogMessage))
            {
                return;
            }
            
            Debug.Log(exitLogMessage);
        }
    }
}