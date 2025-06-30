using System;

namespace TaskStreamer
{
    [Serializable]
    public class CompareStringVariables : ConditionModule<string>
    {
        public override EComparison availableOperators
        {
            get { return EComparison.Equal | EComparison.NotEqual; }
        }

        
        public override bool Execute()
        {
            switch (availableOperators)
            {
                case EComparison.Equal: return variableA.value.CompareTo(variableB.value) == 0; 
                    
                case EComparison.NotEqual: return variableA.value.CompareTo(variableB.value) != 0;
                
                default: throw new NotImplementedException();
            }
        }
    }
}