using System;

namespace TaskStreamer
{
    [Serializable, Readable, Comparable(Comparison.EQ | Comparison.NE | Comparison.GT | Comparison.LT | Comparison.GE | Comparison.LE)]
    public class DoubleCondition : Condition<double>
    {
        public override bool Execute(Comparison comparison)
        {
            switch (comparison)
            {
                case Comparison.EQ: return Math.Abs(leftVariable.value - rightVariable.value) < double.Epsilon;

                case Comparison.NE: return Math.Abs(leftVariable.value - rightVariable.value) > double.Epsilon;

                case Comparison.GT: return leftVariable.value > rightVariable.value;

                case Comparison.GE: return leftVariable.value >= rightVariable.value;

                case Comparison.LT: return leftVariable.value < rightVariable.value;

                case Comparison.LE: return leftVariable.value <= rightVariable.value;

                default: throw new NotImplementedException();
            }
        }
    }
}