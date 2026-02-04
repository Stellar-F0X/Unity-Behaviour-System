using System;
using Unity.Properties;
using UnityEngine;

namespace TaskStreamer.Runtime.BT
{
    [Serializable, GeneratePropertyBag, TaskDescription]
    public class LogService : ServiceBase
    {
        public BlackboardVariable<string> onStart;

        public BlackboardVariable<string> onUpdate;

        public BlackboardVariable<string> onStop;


        public override void OnStart()
        {
            if (string.IsNullOrEmpty(onStart?.value))
            {
                return;
            }

            Debug.Log(onStart.value);
        }

        
        public override void OnUpdate()
        {
            if (string.IsNullOrEmpty(onUpdate?.value))
            {
                return;
            }

            Debug.Log(onUpdate.value);
        }


        public override void OnStop()
        {
            if (string.IsNullOrEmpty(onStop?.value))
            {
                return;
            }

            Debug.Log(onStop.value);
        }
    }
}