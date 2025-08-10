using System;
using UnityEngine;

namespace TaskStreamer
{
    [Serializable]
    public class CompareFloatVariables : ConditionModule<float>
    {
        public override ComparisonType availableOperators
        {
            get { return ComparisonType.NumericPreset; }
        }

        public override bool Execute()
        {
            switch (availableOperators)
            {
                case ComparisonType.Equal: return Mathf.Approximately(variableA.value, variableB.value);

                case ComparisonType.NotEqual: return !Mathf.Approximately(variableA.value, variableB.value);

                case ComparisonType.GreaterThan: return variableA.value > variableB.value;

                case ComparisonType.GreaterThanOrEqual: return variableA.value >= variableB.value;

                case ComparisonType.LessThan: return variableA.value < variableB.value;

                case ComparisonType.LessThanOrEqual: return variableA.value <= variableB.value;
                
                default: throw new NotImplementedException();
            }
        }
    }
}