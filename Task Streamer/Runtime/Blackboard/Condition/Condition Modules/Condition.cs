using System;
using Unity.Properties;
using UnityEngine;

namespace TaskStreamer
{
    [Serializable, Readable]
    public abstract class Condition
    {
        [SerializeField, DontCreateProperty]
        internal Comparison comparisonValue;

        [SerializeField, DontCreateProperty]
        internal Comparison configuredComparisonType;

        public virtual string tooltip
        {
            get { return "The operation is always performed with the left side as the standard."; }
        }

        public abstract BlackboardVariable encapsulatedLeftVariable
        {
            get;
            internal set;
        }

        public abstract BlackboardVariable encapsulatedRightVariable
        {
            get;
            internal set;
        }

        public abstract bool Execute(Comparison comparison);
    }


    [Serializable, Comparable(Comparison.EQ | Comparison.NE)]
    public abstract class Condition<T> : Condition
    {
        [CreateProperty]
        public BlackboardVariable<T> leftVariable;

        [CreateProperty]
        public BlackboardVariable<T> rightVariable;


        public override sealed BlackboardVariable encapsulatedLeftVariable
        {
            get { return this.leftVariable; }

            internal set { this.leftVariable = (BlackboardVariable<T>)value; }
        }

        public override sealed BlackboardVariable encapsulatedRightVariable
        {
            get { return this.rightVariable; }

            internal set { this.rightVariable = (BlackboardVariable<T>)value; }
        }
    }
}