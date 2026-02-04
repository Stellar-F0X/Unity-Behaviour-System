using System;
using TaskStreamer.Runtime.BT;
using Unity.Properties;
using UnityEngine;

namespace TaskStreamer.Runtime
{
    [Serializable, GeneratePropertyBag, TaskDescription]
    public partial class SetIntAnimatorParameter : ServiceBase
    {
        public BlackboardVariable<Animator> animator;
        public BlackboardVariable<string> parameterName;
        public BlackboardVariable<int> value;
        
        
        public override void OnStart()
        {
            if (animator.value == null || string.IsNullOrEmpty(parameterName?.value) || value?.value == null)
            {
                Debug.LogError($"{typeof(SetIntAnimatorParameter)}: null or empty");
                return;
            }
            
            animator.value.SetInteger(parameterName.value, value.value);
        }
    }
}