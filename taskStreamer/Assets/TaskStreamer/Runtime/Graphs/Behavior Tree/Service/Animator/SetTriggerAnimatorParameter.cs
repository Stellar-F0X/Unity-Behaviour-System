using System;
using TaskStreamer;
using TaskStreamer.BT;
using Unity.Properties;
using UnityEngine;

namespace TaskStreamer
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