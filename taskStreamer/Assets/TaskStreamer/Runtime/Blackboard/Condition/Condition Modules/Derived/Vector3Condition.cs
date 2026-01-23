using System;
using UnityEngine;

namespace TaskStreamer.Runtime
{
    [Serializable, Readable, Comparable(Comparison.EQ | Comparison.NE | Comparison.GT | Comparison.LT)]
    public class Vector3Condition : Condition<Vector3>
    {
        public override bool Execute(Comparison comparison)
        {
            switch (comparison)
            {
                case Comparison.EQ: return Mathf.Approximately(leftVariable.value.sqrMagnitude, rightVariable.value.sqrMagnitude);
                
                case Comparison.NE: return !Mathf.Approximately(leftVariable.value.sqrMagnitude, rightVariable.value.sqrMagnitude);
                
                case Comparison.LT: return leftVariable.value.sqrMagnitude < rightVariable.value.sqrMagnitude;
                
                case Comparison.GT: return leftVariable.value.sqrMagnitude > rightVariable.value.sqrMagnitude;

                default: throw new NotImplementedException();
            }
        }
    }
}