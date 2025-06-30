using System;
using UnityEngine;

namespace TaskStreamer
{
    [Serializable]
    public class CompareFloatVariables : ConditionModule<float>
    {
        public override EComparison availableOperators
        {
            get { return EComparison.NumericPreset; }
        }

        public override bool Execute()
        {
            switch (availableOperators)
            {
                case EComparison.Equal: return Mathf.Approximately(variableA.value, variableB.value);

                case EComparison.NotEqual: return !Mathf.Approximately(variableA.value, variableB.value);

                case EComparison.GreaterThan: return variableA.value > variableB.value;

                case EComparison.GreaterThanOrEqual: return variableA.value >= variableB.value;

                case EComparison.LessThan: return variableA.value < variableB.value;

                case EComparison.LessThanOrEqual: return variableA.value <= variableB.value;
                
                default: throw new NotImplementedException();
            }
        }
    }
}