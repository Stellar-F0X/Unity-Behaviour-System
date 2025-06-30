using System;
using UnityEngine;

namespace TaskStreamer
{
    [Serializable]
    public class CompareQuaternionVariables : ConditionModule<Quaternion>
    {
        public override EComparison availableOperators
        {
            get { return EComparison.Equal | EComparison.NotEqual; }
        }

        
        public override bool Execute()
        {
            switch (availableOperators)
            {
                case EComparison.Equal: return variableA.value.Equals(variableB.value);
                
                case EComparison.NotEqual: return !variableA.value.Equals(variableB.value);
                
                default: throw new NotImplementedException();
            }
        }
    }
}