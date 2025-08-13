using System;

namespace TaskStreamer
{
    [Serializable]
    public class CompareIntVariables : ConditionModule<int>
    {
        public override ComparisonType availableOperators
        {
            get { return ComparisonType.NumericPreset; }
        }

        
        public override bool Execute()
        {
            switch (comparison)
            {
                case ComparisonType.None: return false;

                case ComparisonType.Equal: return variableA.value == variableB.value;

                case ComparisonType.NotEqual: return variableA.value != variableB.value;

                case ComparisonType.GreaterThan: return variableA.value > variableB.value;

                case ComparisonType.GreaterThanOrEqual: return variableA.value >= variableB.value;

                case ComparisonType.LessThan: return variableA.value < variableB.value;

                case ComparisonType.LessThanOrEqual: return variableA.value <= variableB.value;
                
                default: return false;
            }
        }
    }
}