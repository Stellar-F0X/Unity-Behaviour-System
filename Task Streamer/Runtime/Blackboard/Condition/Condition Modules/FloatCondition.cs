using System;
using UnityEngine;

namespace TaskStreamer
{
    [Serializable, Comparable(Comparison.EQ | Comparison.NE | Comparison.GT | Comparison.LT | Comparison.GE | Comparison.LE)]
    public class FloatCondition : Condition<float>
    {
        public override bool Execute(Comparison comparison)
        {
            switch (comparison)
            {
                case Comparison.EQ: return Mathf.Approximately(leftVariable.value, rightVariable.value);

                case Comparison.NE: return !Mathf.Approximately(leftVariable.value, rightVariable.value);

                case Comparison.GT: return leftVariable.value > rightVariable.value;

                case Comparison.GE: return leftVariable.value >= rightVariable.value;

                case Comparison.LT: return leftVariable.value < rightVariable.value;

                case Comparison.LE: return leftVariable.value <= rightVariable.value;

                default: throw new NotImplementedException();
            }
        }
    }
}