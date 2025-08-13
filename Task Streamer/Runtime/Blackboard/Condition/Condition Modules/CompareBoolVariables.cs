using System;

namespace TaskStreamer
{
    [Serializable]
    public class CompareBoolVariables : ConditionModule<bool>
    {
        public override ComparisonType availableOperators
        {
            get { return ComparisonType.BooleanPreset; }
        }

        
        public override bool Execute()
        {
            switch (comparison)
            {
                case ComparisonType.Equal: return variableA.value == variableB.value;
                
                case ComparisonType.NotEqual: return variableA.value != variableB.value;
                
                default: return false;
            }
        }
    }
}