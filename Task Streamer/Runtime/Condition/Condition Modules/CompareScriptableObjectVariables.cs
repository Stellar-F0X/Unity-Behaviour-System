using System;
using UnityEngine;

namespace TaskStreamer
{
    [Serializable]
    public class CompareScriptableObjectVariables : ConditionModule<ScriptableObject>
    {
        public override ComparisonType availableOperators
        {
            get { return ComparisonType.Equal | ComparisonType.NotEqual; }
        }

        
        public override bool Execute()
        {
            switch (availableOperators)
            {
                case ComparisonType.Equal: return variableA.value == variableB.value;

                case ComparisonType.NotEqual: return variableA.value != variableB.value;
                
                default: throw new NotImplementedException();
            }
        }
    }
}