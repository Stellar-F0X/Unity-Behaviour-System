using System;
using UnityEngine;

namespace TaskStreamer
{
    [Serializable]
    public class CompareVector2Variables : ConditionModule<Vector2>
    {
        public override ComparisonType availableOperators
        {
            get { return ComparisonType.Equal | ComparisonType.NotEqual; }
        }

        public override bool Execute()
        {
            switch (availableOperators)
            {
                case ComparisonType.Equal: return Mathf.Approximately(variableA.value.sqrMagnitude, variableB.value.sqrMagnitude);
                
                case ComparisonType.NotEqual: return !Mathf.Approximately(variableA.value.sqrMagnitude, variableB.value.sqrMagnitude);

                default: throw new NotImplementedException();
            }
        }
    }
}