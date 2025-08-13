using System;

namespace TaskStreamer
{
    [Serializable]
    public class CompareStringVariables : ConditionModule<string>
    {
        public override ComparisonType availableOperators
        {
            get { return ComparisonType.Equal | ComparisonType.NotEqual; }
        }

        
        public override bool Execute()
        {
            switch (availableOperators)
            {
                case ComparisonType.Equal: return variableA.value.CompareTo(variableB.value) == 0; 
                    
                case ComparisonType.NotEqual: return variableA.value.CompareTo(variableB.value) != 0;
                
                default: throw new NotImplementedException();
            }
        }
    }
}