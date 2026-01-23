using System;
using TaskStreamer.Runtime;
using TaskStreamer.Runtime.BT;
using Unity.Properties;
using UnityEngine;

namespace TaskStreamer.Runtime
{
    [Serializable, GeneratePropertyBag, Readable]
    public partial class SetTriggerAnimatorParameter : ServiceBase
    {
        public BlackboardVariable<Animator> animator;
        public BlackboardVariable<string> parameterName;
        
        
        public override void OnStart()
        {
            if (animator.value == null || string.IsNullOrEmpty(parameterName?.value))
            {
                Debug.LogError($"{typeof(SetTriggerAnimatorParameter)}: null or empty");
                return;
            }
            
            animator.value.SetTrigger(parameterName.value);
        }
    }
}