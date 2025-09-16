using System;
using TaskStreamer;
using TaskStreamer.BT;
using Unity.Properties;
using UnityEngine;

namespace TaskStreamer
{
    [Serializable, GeneratePropertyBag, Readable]
    public partial class SetBoolAnimatorParameter : ServiceBase
    {
        public BlackboardVariable<Animator> animator;
        public BlackboardVariable<string> parameterName;
        public BlackboardVariable<bool> value;
        
        
        public override void OnStart()
        {
            if (animator.value == null || string.IsNullOrEmpty(parameterName?.value) || value?.value == null)
            {
                Debug.LogError($"{typeof(SetBoolAnimatorParameter)}: null or empty");
                return;
            }

            animator.value.SetBool(parameterName.value, value.value);
        }
    }
}