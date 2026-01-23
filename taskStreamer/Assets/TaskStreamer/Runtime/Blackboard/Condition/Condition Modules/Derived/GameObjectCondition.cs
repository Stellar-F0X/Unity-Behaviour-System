using System;
using UnityEngine;

namespace TaskStreamer.Runtime
{
    [Serializable, Readable, Comparable(Comparison.EQ | Comparison.NE)]
    public class GameObjectCondition : Condition<GameObject>
    {
        public override bool Execute(Comparison comparison)
        {
            switch (comparison)
            {
                case Comparison.EQ: return leftVariable.value == rightVariable.value;

                case Comparison.NE: return leftVariable.value != rightVariable.value;
                
                default: throw new NotImplementedException();
            }
        }
    }
}