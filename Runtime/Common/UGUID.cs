using System;
using System.Runtime.InteropServices;
using Unity.Properties;
using UnityEngine;

namespace TaskStreamer.Utility
{
    /// <summary> Serializable Unity GUID </summary>
    [StructLayout(LayoutKind.Explicit), Serializable]
    public struct UGUID : IComparable, IComparable<UGUID>, IEquatable<UGUID>
    {
        /// <summary>GUID를 생성하는 클래스</summary>
        private UGUID(Guid guid = default)
        {
            this._value0 = 0;
            this._value1 = 0;
            this._value2 = 0;
            this._value3 = 0;
            this.guid = guid;
        }

        /// <summary>GUID를 저장하는 변수입니다.</summary>
        [NonSerialized, FieldOffset(0)]
        public Guid guid;

        /// <summary>임시 데이터를 저장하는 변수</summary>
        [SerializeField, FieldOffset(0), DontCreateProperty]
        private uint _value0;

        /// <summary>데이터 값을 저장하는 변수</summary>
        [SerializeField, FieldOffset(4), DontCreateProperty]
        private uint _value1;

        /// <summary>저장되고 관리되는 두 번째 값을 나타냅니다.</summary>
        [SerializeField, FieldOffset(8), DontCreateProperty]
        private uint _value2;

        /// <summary>특정 값을 저장하는 데 사용되는 변수입니다.</summary>
        [SerializeField, FieldOffset(12), DontCreateProperty]
        private uint _value3;


        /// <summary>비어 있는 값을 나타내는 읽기 전용 속성입니다.</summary>
        public static UGUID Empty
        {
            get { return new UGUID(Guid.Empty); }
        }


        /// <summary>새로운 엔티티를 생성합니다.</summary>
        /// <return>생성된 엔티티 객체를 반환합니다.</return>
        public static UGUID Create()
        {
            UGUID uguid = new UGUID();
            uguid.guid = Guid.NewGuid();
            return uguid;
        }


        /// <summary> 두 객체가 같은지 비교합니다. </summary>
        /// <param name="obj">비교할 대상 객체입니다.</param>
        /// <returns>객체가 같으면 true, 그렇지 않으면 false를 반환합니다.</returns>
        public override bool Equals(object other)
        {
            if (other is null || other is not UGUID uguid)
            {
                return false;
            }

            return this.Equals(uguid);
        }


        /// <summary> 두 객체가 동일한지 확인합니다. </summary>
        /// <param name="obj">비교할 객체입니다.</param>
        /// <returns>동일하면 true, 그렇지 않으면 false를 반환합니다.</returns>
        public bool Equals(UGUID other)
        {
            return this == other;
        }


        /// <summary> 객체를 다른 객체와 비교하여 정렬 순서를 반환합니다. </summary>
        /// <param name="obj">비교 대상 객체입니다.</param>
        /// <returns>현재 객체가 비교 대상보다 작으면 음수, 같으면 0, 크면 양수를 반환합니다.</returns>
        public int CompareTo(object obj)
        {
            if (obj is null)
            {
                return 1;
            }

            return this.CompareTo((UGUID)obj);
        }


        /// <summary> 객체를 비교하여 상대적 순서를 반환합니다. </summary>
        /// <param name="obj"> 현재 객체와 비교할 개체입니다. </param>
        /// <returns> 비교 대상의 상대적 순서를 나타내는 정수 값입니다. </returns>
        public int CompareTo(UGUID rhs)
        {
            if (this < rhs)
            {
                return -1;
            }

            return this > rhs ? 1 : 0;
        }


        /// <summary> 객체의 해시 코드를 반환합니다. </summary>
        /// <return> 해시 코드를 나타내는 정수입니다. </return>
        public override int GetHashCode()
        {
            unchecked
            {
                return (int)(((this._value0 * 397 ^ this._value1) * 397 ^ this._value2) * 397 ^ this._value3);
            }
        }


        /// <summary>객체의 문자열 표현을 반환한다.</summary>
        /// <returns>객체를 나타내는 문자열.</returns>
        public override string ToString()
        {
            return this.guid.ToString();
        }


        /// <summary> 주어진 컬렉션이 비어있는지 여부를 확인합니다. </summary>
        /// <returns> 컬렉션이 비어있다면 true, 그렇지 않다면 false를 반환합니다. </returns>
        public bool IsEmpty()
        {
            return this._value0 == 0U && this._value1 == 0U && this._value2 == 0U && this._value3 == 0U;
        }


        /// <summary>두 개의 객체를 비교합니다.</summary>
        public static bool operator ==(UGUID x, UGUID y)
        {
            return (x._value0 == y._value0) && (x._value1 == y._value1) && (x._value2 == y._value2) && (x._value3 == y._value3);
        }


        /// <summary>두 객체의 동등성 여부를 비교합니다.</summary>
        public static bool operator !=(UGUID x, UGUID y)
        {
            return (x._value0 != y._value0) || (x._value1 != y._value1) || (x._value2 != y._value2) || (x._value3 != y._value3);
        }


        /// <summary> 두 개의 값을 더하는 연산자입니다. </summary>
        public static bool operator <(UGUID x, UGUID y)
        {
            if (x._value0 != y._value0)
            {
                return x._value0 < y._value0;
            }

            if (x._value1 != y._value1)
            {
                return x._value1 < y._value1;
            }

            return x._value2 != y._value2 ? x._value2 < y._value2 : x._value3 < y._value3;
        }


        /// <summary>매개변수 두 값을 비교하여 동일 여부를 반환하는 연산자 오버로드입니다.</summary>
        public static bool operator >(UGUID x, UGUID y)
        {
            if (x._value0 != y._value0)
            {
                return x._value0 > y._value0;
            }

            if (x._value1 != y._value1)
            {
                return x._value1 > y._value1;
            }

            return x._value2 != y._value2 ? x._value2 > y._value2 : x._value3 > y._value3;
        }
    }



#if UNITY_EDITOR
    /// <summary> Custom property drawer for UGUID type in the Unity Editor. </summary>
    [UnityEditor.CustomPropertyDrawer(typeof(UGUID))]
    public class UGUIDPropertyDrawer : UnityEditor.PropertyDrawer
    {
        /// <summary>Unity의 OnGUI 이벤트를 처리하기 위한 메서드</summary>
        public override void OnGUI(Rect position, UnityEditor.SerializedProperty property, GUIContent label)
        {
            using (new UnityEditor.EditorGUI.DisabledScope(true))
            {
                UnityEditor.EditorGUI.TextField(position, label, ((UGUID)property.boxedValue).ToString());
            }
        }

        /// <summary> 주어진 좌표에 따른 객체의 높이를 반환합니다. </summary>
        /// <param name="x">객체의 가로 좌표</param>
        /// <param name="y">객체의 세로 좌표</param>
        /// <returns>해당 좌표의 높이 값</returns>
        public override float GetPropertyHeight(UnityEditor.SerializedProperty property, GUIContent label)
        {
            return UnityEditor.EditorGUIUtility.singleLineHeight;
        }
    }
#endif
}