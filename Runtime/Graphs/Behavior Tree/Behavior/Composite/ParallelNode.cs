using System;
using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;

namespace TaskStreamer.BT
{
    [Serializable, GeneratePropertyBag, Readable]
    public class ParallelNode : CompositeNode
    {
        [Tooltip("Determines how success or failure is evaluated among child nodes.")]
        public BlackboardVariable<ParallelPolicy> parallelPolicy;

        [DefaultValue(true)]
        [Tooltip("Stop updating children as soon as the policy resolves to Success or Failure. If disabled, all children are evaluated every tick.")]
        public BlackboardVariable<bool> shortCircuit;

        private int _successfulCount = 0;
        private int _failedCount = 0;

        private List<bool> _isChildStopped;


        public override string tooltip
        {
            get
            {
                return "Executes multiple child nodes simultaneously.\n" +
                       "Success/failure is determined by the configured policy.";
            }
        }


        public override void OnAwake()
        {
            _isChildStopped = new List<bool>(children.Count);

            for (int i = 0; i < children.Count; ++i)
            {
                _isChildStopped.Add(false);
            }
        }


        protected override void OnEnter()
        {
            _failedCount = 0;
            _successfulCount = 0;

            if (children is null || children.Count == 0)
            {
                return;
            }

            int count = children.Count;

            for (int i = 0; i < count; ++i)
            {
                _isChildStopped[i] = false;
            }
        }


        protected override Status OnUpdate()
        {
            int count = children.Count;
            
            for (int i = 0; i < count; ++i)
            {
                if (_isChildStopped[i])
                {
                    continue;
                }

                if (this.CanContinue(i, out Status result) == false)
                {
                    return result;
                }
            }

            return this.EvaluatePolicy();
        }

        
        private bool CanContinue(int index, out Status result)
        {
            switch (children[index].UpdateNode())
            {
                case Status.Success:
                {
                    _isChildStopped[index] = true;
                    ++_successfulCount;
                    break;
                }

                case Status.Failure:
                {
                    _isChildStopped[index] = true;
                    ++_failedCount;
                    break;
                }
            }

            if (this.shortCircuit.value)
            {
                result = this.EvaluatePolicy();

                if (result != Status.Running)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }

            result = Status.Running;
            return true;
        }


        protected override void OnExit()
        {
            if (children is null || children.Count == 0)
            {
                return;
            }

            int count = children.Count;

            for (int i = 0; i < count; ++i)
            {
                if (_isChildStopped[i])
                {
                    _isChildStopped[i] = false; // Reset the stopped state
                }
                else
                {
                    tree.interrupter.AbortSubtree(children[i].callStackID);
                }
            }

            this._failedCount = 0;
            this._successfulCount = 0;
        }


        protected virtual Status EvaluatePolicy()
        {
            switch (parallelPolicy.value)
            {
                case ParallelPolicy.RequireAllSuccess:
                {
                    if (_successfulCount == children.Count)
                    {
                        return Status.Success;
                    }

                    if (_failedCount > 0)
                    {
                        return Status.Failure;
                    }

                    break;
                }

                case ParallelPolicy.RequireAllFailure:
                {
                    if (_failedCount == children.Count)
                    {
                        return Status.Success;
                    }

                    if (_successfulCount > 0)
                    {
                        return Status.Failure;
                    }

                    break;
                }

                case ParallelPolicy.RequireOneSuccess:
                {
                    if (_successfulCount > 0)
                    {
                        return Status.Success;
                    }

                    if (_successfulCount + _failedCount == children.Count)
                    {
                        return Status.Failure;
                    }

                    break;
                }

                case ParallelPolicy.RequireOneFailure:
                {
                    if (_failedCount > 0)
                    {
                        return Status.Success;
                    }

                    if (_successfulCount + _failedCount == children.Count)
                    {
                        return Status.Failure;
                    }

                    break;
                }
            }

            return Status.Running;
        }
    }
}