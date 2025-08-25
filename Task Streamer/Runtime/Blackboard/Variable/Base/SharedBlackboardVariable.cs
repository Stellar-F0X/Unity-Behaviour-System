using System;
using TaskStreamer.Utility;
using UnityEngine;

namespace TaskStreamer
{
    [Serializable]
    public class SharedBlackboardVariable<TValue> : BlackboardVariable<TValue>, ISharedBlackboardVariable
    {
        [SerializeReference]
        private IBlackboard _blackboard;


        internal override string key
        {
            get
            {
                return _blackboard.FindVariable(base._guid)?.key;
            }

            set
            {
                BlackboardVariable variable = _blackboard.FindVariable(base._guid);

                if (variable is not null)
                {
                    variable.key = value;
                }
            }
        }


        public override TValue value
        {
            get
            {
                if (_blackboard.FindVariable(base._guid) is BlackboardVariable<TValue> variable)
                {
                    return variable.value;
                }

                return default;
            }

            set
            {
                if (_blackboard.FindVariable(base._guid) is BlackboardVariable<TValue> variable)
                {
                    variable.value = value;
                }
                else
                {
                    Debug.LogError("Can't find variable");
                }
            }
        }


        internal override object boxedValue
        {
            get
            {
                return this.value;
            }

            set
            {
                if (value is TValue convertedValue)
                {
                    this.value = convertedValue;
                }
            }
        }


        public void SetBlackboardAndVariableReference(in IBlackboard blackboard, in UGUID variableGuid)
        {
            Debug.Assert(blackboard is not null, "Blackboard is null");
            this._blackboard = blackboard;

            BlackboardVariable variable = blackboard.FindVariable(variableGuid);
            Debug.Assert(variable is not null, "Blackboard variable is null");

            this._guid = variableGuid;
        }


        internal override BlackboardVariable Duplicate()
        {
            var clone = new SharedBlackboardVariable<TValue>();
            clone._typeName = this._typeName;
            clone.value = this.value;
            clone.type = this.type;
            clone.key = this.key;
            return clone;
        }
    }
}