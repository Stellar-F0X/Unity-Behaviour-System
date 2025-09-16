using System;
using TaskStreamer;
using TaskStreamer.BT;
using Unity.Properties;
using UnityEngine;

namespace TaskStreamer
{
    [Serializable, GeneratePropertyBag, Readable]
    public partial class SetFloatAnimatorParameter : ServiceBase
    {
        public BlackboardVariable<Animator> animator;
        public BlackboardVariable<string> parameterName;
        public BlackboardVariable<float> value;
        
        
        public override void OnStart()
        {
            if (animator.value == null || string.IsNullOrEmpty(parameterName?.value) || value?.value == null)
            {
                Debug.LogError($"{typeof(SetFloatAnimatorParameter)}: null or empty");
                return;
            }
            
            animator.value.SetFloat(parameterName.value, value.value);
        }
    }
}