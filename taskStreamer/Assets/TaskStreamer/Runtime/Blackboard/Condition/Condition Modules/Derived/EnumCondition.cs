using System;
using System.Collections.Generic;

namespace TaskStreamer.Runtime
{
    [Serializable, Readable, Comparable(Comparison.EQ | Comparison.NE)]
    public class EnumCondition : Condition<Enum>
    {
        public override bool Execute(Comparison comparison) 
        {
            switch (comparison)
            {
                case Comparison.EQ: return EqualityComparer<Enum>.Default.Equals(leftVariable.value, rightVariable.value);

                case Comparison.NE: return !EqualityComparer<Enum>.Default.Equals(leftVariable.value, rightVariable.value);
                
                default: throw new NotImplementedException();
            }
        }
    }
}