using System;

namespace TaskStreamer
{
    [Serializable]
    public class CompareBoolVariables : ConditionModule<bool>
    {
        public override EComparison availableOperators
        {
            get { return EComparison.BooleanPreset; }
        }

        
        public override bool Execute()
        {
            switch (comparison)
            {
                case EComparison.Equal: return variableA.value == variableB.value;
                
                case EComparison.NotEqual: return variableA.value != variableB.value;
                
                default: return false;
            }
        }
    }
}