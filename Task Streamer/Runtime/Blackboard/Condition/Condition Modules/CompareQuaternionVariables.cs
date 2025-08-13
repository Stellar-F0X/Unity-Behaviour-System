using System;
using UnityEngine;

namespace TaskStreamer
{
    [Serializable]
    public class CompareQuaternionVariables : ConditionModule<Quaternion>
    {
        public override ComparisonType availableOperators
        {
            get { return ComparisonType.Equal | ComparisonType.NotEqual; }
        }

        
        public override bool Execute()
        {
            switch (availableOperators)
            {
                case ComparisonType.Equal: return variableA.value.Equals(variableB.value);
                
                case ComparisonType.NotEqual: return !variableA.value.Equals(variableB.value);
                
                default: throw new NotImplementedException();
            }
        }
    }
}