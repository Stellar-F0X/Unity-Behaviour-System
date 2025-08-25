using System;
using TaskStreamer.Utility;
using UnityEngine;

namespace TaskStreamer
{
    /// <summary> Variable wrapper class </summary>
    [Serializable, Readable]
    public abstract class BlackboardVariable : ISerializationCallbackReceiver
    {
        internal const string DEFAULT_VARIABLE_NAME = "#Local__Variable#";
        
        
        /// Variable의 내부적으로 사용되는 문자열 키를 저장하는 필드.
        /// key 프로퍼티를 통해 접근 가능하며, key 변경 시 해시 값(_keyHash)이 자동으로 갱신됨.
        [SerializeField]
        protected string _key;

        
        
        /// _keyHash는 현재 Variable 객체의 키 문자열에 대해 생성된 해시 값을 저장하는 필드입니다.
        /// 문자열 키는 StringUtility.StringToHash 메서드를 사용해 해시로 변환됩니다.
        [SerializeField]
        protected int _keyHash;

        
        
        /// <summary>
        /// 변수를 고유하게 식별하기 위한 UGUID 형태의 유니크한 식별자.
        /// </summary>
        [SerializeField]
        protected UGUID _guid = UGUID.Create();

        
        
        /// 직렬화된 타입 이름을 저장하는 문자열 변수로, 타입 정보를 유지 및 복원하는 데 사용된다.
        [SerializeField]
        protected string _typeName;

        
        
        [SerializeField]
        protected bool _isShareable;


        /// 변수의 데이터 타입을 나타내는 속성입니다.
        /// 해당 변수의 형식을 정의하며, 런타임 타입 정보를 제공합니다.
        internal Type type
        {
            get;
            set;
        }

        
        /// UGUID 타입의 GUID를 나타내는 읽기 전용 속성.
        /// 각 Variable 객체를 고유하게 식별하기 위해 사용된다.
        internal UGUID guid
        {
            get { return _guid; }
        }

        
        /// 변수의 고유 식별 키를 가져오거나 설정합니다. 키가 설정될 때, 해당 키의 해시 값이 자동으로 갱신됩니다.
        internal virtual string key
        {
            get { return this._key; }

            set { _keyHash = StringUtility.StringToHash(_key = value); }
        }

        
        /// key 값에 대해 고유한 해시를 반환합니다.
        /// 문자열 키를 해시로 변환하여 빠르게 비교하거나 조회하는 용도로 사용됩니다.
        internal int keyHash
        {
            get { return this._keyHash; }
        }

        
        /// 모든 타입의 값을 포함할 수 있는 '박싱된 값'을 가져오거나 설정합니다.
        internal abstract object boxedValue
        {
            get;
            set;
        }

        
        /// <summary> Indicates whether the variable is shared globally (true) or local to a specific node (false). </summary>
        internal bool isShared
        {
            get { return this._isShareable; }
            
            set { this._isShareable = value; }
        }
        
        

        /// 직렬화 이전에 호출되며, 변수의 타입 정보를 저장합니다.
        /// 타입이 null이면 디버그 검사를 통해 경고를 발생시킵니다.
        public void OnBeforeSerialize()
        {
            Debug.Assert(type is not null, "Failed to serialize a property.");
            this._typeName = type.AssemblyQualifiedName;
        }


        /// 직렬화 이후에 호출되며, 직렬화된 데이터로부터 타입 정보를 복원합니다.
        /// _typeName이 비어 있지 않은지 확인하고, 이를 통해 타입을 로드합니다.
        public void OnAfterDeserialize()
        {
            Debug.Assert(_typeName.IsNotNullOrEmpty(), "Failed to deserialize a property.");
            this.type = Type.GetType(_typeName);
        }


        /// <summary>
        /// Creates a duplicate of the current BlackboardVariable instance.
        /// </summary>
        /// <returns>A new instance of BlackboardVariable that is a copy of the current instance.</returns>
        internal abstract BlackboardVariable Duplicate();
    }


    /// <summary> Abstract base class representing a variable within the Blackboard system </summary>
    [Serializable, Readable]
    public class BlackboardVariable<TValue> : BlackboardVariable
    {
        [SerializeField]
        protected TValue _value;
        

        /// <summary> Gets or sets the value associated with the blackboard variable, with type checking and validation. </summary>
        public virtual TValue value
        {
            get { return _value; }

            set { _value = value; }
        }


        internal override object boxedValue
        {
            get { return _value; }

            set { _value = (value is TValue converted) ? converted : _value; }
        }


        /// <summary> 현재 객체를 복제한 새로운 BlackboardVariable 인스턴스를 반환합니다. </summary>
        /// <returns> 복제된 BlackboardVariable 객체. </returns>
        internal override BlackboardVariable Duplicate()
        {
            var clone = new BlackboardVariable<TValue>();
            clone._typeName = this._typeName;
            clone._guid = this._guid;
            clone.value = this.value;
            clone.type = this.type;
            clone.key = this.key;
            return clone;
        }
    }
}