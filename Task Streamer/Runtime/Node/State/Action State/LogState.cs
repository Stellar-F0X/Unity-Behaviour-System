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
            Debug.Log(enterLogMessage);
        }

        protected override void OnUpdate()
        {
            Debug.Log(updateLogMessage);
        }
        
        protected override void OnExit()
        {
            Debug.Log(exitLogMessage);
        }
    }
}