using System.Collections.Generic;
using UnityEngine;

namespace TaskStreamer.BT
{
    public class ParallelNode : CompositeNode
    {
        [Tooltip("Determines how success or failure is evaluated among child nodes.")]
        public EParallelPolicy parallelPolicy;

        [Tooltip("Stop updating children as soon as the policy resolves to Success or Failure. If disabled, all children are evaluated every tick.")]
        public bool shortCircuit = true;

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


        protected override EStatus OnUpdate()
        {
            int count = children.Count;
            
            for (int i = 0; i < count; ++i)
            {
                if (_isChildStopped[i])
                {
                    continue;
                }

                if (this.CanContinue(i, out EStatus result) == false)
                {
                    return result;
                }
            }

            return this.EvaluatePolicy();
        }

        
        private bool CanContinue(int index, out EStatus result)
        {
            switch (children[index].UpdateNode())
            {
                case EStatus.Success:
                {
                    _isChildStopped[index] = true;
                    ++_successfulCount;
                    break;
                }

                case EStatus.Failure:
                {
                    _isChildStopped[index] = true;
                    ++_failedCount;
                    break;
                }
            }

            if (this.shortCircuit)
            {
                result = this.EvaluatePolicy();

                if (result != EStatus.Running)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }

            result = EStatus.Running;
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


        protected virtual EStatus EvaluatePolicy()
        {
            switch (parallelPolicy)
            {
                case EParallelPolicy.RequireAllSuccess:
                {
                    if (_successfulCount == children.Count)
                    {
                        return EStatus.Success;
                    }

                    if (_failedCount > 0)
                    {
                        return EStatus.Failure;
                    }

                    break;
                }

                case EParallelPolicy.RequireAllFailure:
                {
                    if (_failedCount == children.Count)
                    {
                        return EStatus.Success;
                    }

                    if (_successfulCount > 0)
                    {
                        return EStatus.Failure;
                    }

                    break;
                }

                case EParallelPolicy.RequireOneSuccess:
                {
                    if (_successfulCount > 0)
                    {
                        return EStatus.Success;
                    }

                    if (_successfulCount + _failedCount == children.Count)
                    {
                        return EStatus.Failure;
                    }

                    break;
                }

                case EParallelPolicy.RequireOneFailure:
                {
                    if (_failedCount > 0)
                    {
                        return EStatus.Success;
                    }

                    if (_successfulCount + _failedCount == children.Count)
                    {
                        return EStatus.Failure;
                    }

                    break;
                }
            }

            return EStatus.Running;
        }
    }
}