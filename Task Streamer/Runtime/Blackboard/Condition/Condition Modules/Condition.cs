using System;
using TaskStreamer.Injection;
using TaskStreamer.Utility;
using Unity.Properties;
using UnityEngine;

namespace TaskStreamer
{
    /// <summary>Condition 클래스는 다양한 비교 조건을 정의하는 기본 추상 클래스입니다.</summary>
    [Serializable, Readable]
    public abstract class Condition
    {
        /// <summary> 기본 비교 방식에 대한 값을 나타냅니다. </summary>
        public const Comparison DEFAULT_COMPARISON = Comparison.EQ | Comparison.NE;


        /// <summary> Condition에서 사용되는 비교 유형을 나타냅니다. </summary>
        [SerializeField, DontCreateProperty]
        internal Comparison comparisonValue;


        /// <summary> 지정된 비교 유형을 나타냅니다. </summary>
        [SerializeField, DontCreateProperty]
        internal Comparison configuredComparisonType;



        /// <summary> Provides additional information about the purpose or usage of the property. </summary>
        public virtual string tooltip
        {
            get { return "The operation is always performed with the left side as the standard."; }
        }


        /// <summary> 왼쪽 변수에 캡슐화된 블랙보드 변수를 가져오거나 설정합니다. </summary>
        public abstract BlackboardVariable lVariable
        {
            get;
            internal set;
        }


        /// <summary>Encapsulated property representing the right-side variable in the condition.</summary>
        public abstract BlackboardVariable rVariable
        {
            get;
            internal set;
        }


        public abstract Type genericType
        {
            get;
        }


        public abstract Type valueType
        {
            get;
        }


        /// <summary>주어진 비교 조건을 기반으로 실행 결과를 반환합니다.</summary>
        /// <param name="comparison">실행에 사용할 비교 연산자입니다.</param>
        /// <return>비교 결과에 따라 true 또는 false를 반환합니다.</return>
        public abstract bool Execute(Comparison comparison);
    }


    /// <summary> 조건을 나타내는 추상 클래스입니다. </summary>
    [Serializable]
    public abstract class Condition<TValue> : Condition
    {
        /// <summary> 좌측 블랙보드 변수를 나타냅니다. </summary>
        [CreateProperty]
        public BlackboardVariable<TValue> leftVariable;


        /// <summary> Represents the secondary variable in a Condition used for comparison. </summary>
        [CreateProperty]
        public BlackboardVariable<TValue> rightVariable;


        /// <summary> 캡슐화된 왼쪽 변수 </summary>
        public override sealed BlackboardVariable lVariable
        {
            get { return this.leftVariable; }

            internal set { this.leftVariable = (BlackboardVariable<TValue>)value; }
        }

        /// <summary> Encapsulated right-side variable. </summary>
        public override sealed BlackboardVariable rVariable
        {
            get { return this.rightVariable; }

            internal set { this.rightVariable = (BlackboardVariable<TValue>)value; }
        }


        public override sealed Type genericType
        {
            get { return typeof(Condition<TValue>); }
        }


        public override sealed Type valueType
        {
            get { return typeof(TValue); }
        }
    }
}