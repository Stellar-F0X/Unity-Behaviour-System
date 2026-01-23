using System;
using Unity.Properties;
using UnityEngine;

namespace TaskStreamer.Runtime.BT
{
    [Serializable, GeneratePropertyBag, Readable]
    public class CrossFadeNode : ActionNode
    {
        public BlackboardVariable<Animator> animator;
        public BlackboardVariable<string> animationName;

        [DefaultValue(0)]
        public BlackboardVariable<int> layer;

        [DefaultValue(0.1f)]
        [Tooltip("Duration of the crossfade transition in seconds.")]
        public BlackboardVariable<float> transitionDuration;
        public BlackboardVariable<float> timeOffset;

        [Tooltip("Time in seconds to wait for the transition to complete before returning success.")]
        public BlackboardVariable<float> transitionTime;
        public BlackboardVariable<bool> useFixedCrossFade;
        public BlackboardVariable<bool> waitForTransition;

        [SerializeField, DontCreateProperty]
        private int _animationHash;
        private float _playStartTime;
        private bool _invalidated;


        public override void OnAwake()
        {
            if (string.IsNullOrEmpty(animationName.value))
            {
                Debug.LogError($"{typeof(CrossFadeNode)}: Animation Name is empty.");
                _animationHash = -1;
                return;
            }

            _animationHash = Animator.StringToHash(animationName.value);
        }


        protected override void OnEnter()
        {
            if (_animationHash == -1)
            {
                return;
            }

            if (useFixedCrossFade.value)
            {
                animator.value.CrossFadeInFixedTime
                (
                    _animationHash, 
                    transitionDuration.value,
                    layer.value,
                    timeOffset.value,
                    transitionTime.value
                );
            }
            else
            {
                animator.value.CrossFade
                (
                    _animationHash,
                    transitionDuration.value,
                    layer.value,
                    timeOffset.value,
                    transitionTime.value
                );
            }

            _playStartTime = Time.time;
        }


        protected override Status OnUpdate()
        {
            if (_animationHash == -1)
            {
                return Status.Failure;
            }

            if (waitForTransition.value)
            {
                if (Time.time >= _playStartTime + transitionTime.value)
                {
                    return Status.Success;
                }

                return Status.Running;
            }
            else
            {
                //기다리지 않는다면, 즉시 성공 반환.
                return Status.Success;
            }
        }
    }
}