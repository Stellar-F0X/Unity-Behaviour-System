using System;

namespace TaskStreamer
{
    [Serializable]
    public class CompareIntVariables : ConditionModule<int>
    {
        public override EComparison availableOperators
        {
            get { return EComparison.NumericPreset; }
        }

        
        public override bool Execute()
        {
            switch (comparison)
            {
                case EComparison.None: return false;

                case EComparison.Equal: return variableA.value == variableB.value;

                case EComparison.NotEqual: return variableA.value != variableB.value;

                case EComparison.GreaterThan: return variableA.value > variableB.value;

                case EComparison.GreaterThanOrEqual: return variableA.value >= variableB.value;

                case EComparison.LessThan: return variableA.value < variableB.value;

                case EComparison.LessThanOrEqual: return variableA.value <= variableB.value;
                
                default: return false;
            }
        }
    }
}