using System;
using UnityEngine;

namespace TaskStreamer
{
    [Serializable]
    public class CompareVector3Variables : ConditionModule<Vector3>
    {
        public override EComparison availableOperators
        {
            get { return EComparison.Equal | EComparison.NotEqual; }
        }

        public override bool Execute()
        {
            switch (availableOperators)
            {
                case EComparison.Equal: return Mathf.Approximately(variableA.value.sqrMagnitude, variableB.value.sqrMagnitude);
                
                case EComparison.NotEqual: return !Mathf.Approximately(variableA.value.sqrMagnitude, variableB.value.sqrMagnitude);

                default: throw new NotImplementedException();
            }
        }
    }
}