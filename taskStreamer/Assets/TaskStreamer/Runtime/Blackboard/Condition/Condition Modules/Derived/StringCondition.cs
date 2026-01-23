using System;

namespace TaskStreamer.Runtime
{
    [Serializable, Readable, Comparable(Comparison.EQ | Comparison.NE)]
    public class StringCondition : Condition<string>
    {
        public override bool Execute(Comparison comparison)
        {
            switch (comparison)
            {
                case Comparison.EQ: return leftVariable.value.CompareTo(rightVariable.value) == 0; 
                    
                case Comparison.NE: return leftVariable.value.CompareTo(rightVariable.value) != 0;
                
                default: throw new NotImplementedException();
            }
        }
    }
}