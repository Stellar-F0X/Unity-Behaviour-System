using System;

namespace TaskStreamer
{
    [Serializable, Comparable(Comparison.EQ | Comparison.NE | Comparison.GT | Comparison.LT | Comparison.GE | Comparison.LE)]
    public class IntCondition : Condition<int>
    {
        public override bool Execute(Comparison comparison)
        {
            switch (comparison)
            {
                case Comparison.None: return false;

                case Comparison.EQ: return leftVariable.value == rightVariable.value;

                case Comparison.NE: return leftVariable.value != rightVariable.value;

                case Comparison.GT: return leftVariable.value > rightVariable.value;

                case Comparison.GE: return leftVariable.value >= rightVariable.value;

                case Comparison.LT: return leftVariable.value < rightVariable.value;

                case Comparison.LE: return leftVariable.value <= rightVariable.value;
                
                default: return false;
            }
        }
    }
}