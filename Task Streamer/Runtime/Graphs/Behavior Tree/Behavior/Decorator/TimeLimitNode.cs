using System;
using Unity.Properties;
using UnityEngine;

namespace TaskStreamer.BT
{
    [Serializable, GeneratePropertyBag, Readable]
    public class TimeLimitNode : DecoratorNode
    {
        [SetValue(1f)]
        public BlackboardVariable<float> limitTime;
        
        private float _startTime;


        public override string tooltip
        {
            get { return "Limits execution of child node to a specified duration. \nReturns Failure if time expires."; }
        }


        protected override void OnEnter()
        {
            _startTime = Time.time;
        }


        protected override Status OnUpdate()
        {
            if (_startTime + limitTime.value > Time.time)
            {
                return child.UpdateNode();
            }

            return Status.Failure;
        }
    }
}