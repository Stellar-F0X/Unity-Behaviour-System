using System;
using UnityEngine;

namespace TaskStreamer
{
    [Serializable]
    public class CompareGameObjectVariables : ConditionModule<GameObject>
    {
        public override EComparison availableOperators
        {
            get { return EComparison.Equal | EComparison.NotEqual; }
        }

        
        public override bool Execute()
        {
            switch (availableOperators)
            {
                case EComparison.Equal: return variableA.value == variableB.value;

                case EComparison.NotEqual: return variableA.value != variableB.value;
                
                default: throw new NotImplementedException();
            }
        }
    }
}