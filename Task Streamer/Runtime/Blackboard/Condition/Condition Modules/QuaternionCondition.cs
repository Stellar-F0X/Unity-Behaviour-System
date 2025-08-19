using System;
using UnityEngine;

namespace TaskStreamer
{
    [Serializable, Comparable(Comparison.EQ | Comparison.NE)]
    public class QuaternionCondition : Condition<Quaternion>
    {
        public override bool Execute(Comparison comparison)
        {
            switch (comparison)
            {
                case Comparison.EQ: return leftVariable.value.Equals(rightVariable.value);
                
                case Comparison.NE: return !leftVariable.value.Equals(rightVariable.value);
                
                default: throw new NotImplementedException();
            }
        }
    }
}