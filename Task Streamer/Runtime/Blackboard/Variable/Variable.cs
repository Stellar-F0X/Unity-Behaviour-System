using System;
using TaskStreamer.Utility;
using UnityEngine;

namespace TaskStreamer
{
    //TODO: 유니티가 Generic 타입을 직렬화하지 못해서 직접 콘크리트 클래스를 구현해줘야 됨.
    //Unity BehaviorTree가 그러지 않아도 작동하는 이유는 직렬화를 JSON으로 해서 인듯.
    //이 프로젝트도 추후 데이터를 JSON으로 저장하게 전환한다면 구체 클래스를 제거.

    /// <summary>
    /// Abstract class for flexible variable types, supporting serialization and duplication.
    /// </summary>
    [Serializable]
    public abstract class Variable : ISerializationCallbackReceiver
    {
        /// 상수 문자열로, 로컬 변수 또는 기본값을 나타내는 변수 이름을 정의합니다.
        /// 사용할 변수 값이 없거나 초기화되지 않은 경우에 주로 사용됩니다.
        internal const string DEFAULT_LOCAL_VARIABLE_NAME = "#Local__Variable#";

        
        /// Variable 클래스는 시리얼라이즈 가능한 값을 정의하며 Unity의 직렬화 및 타입 관리와 관련된 공통 기능을 제공합니다.
        protected Variable()
        {
            _guid = UGUID.Create();
            _keyHash = -1;
        }

        /// Variable의 내부적으로 사용되는 문자열 키를 저장하는 필드.
        /// key 프로퍼티를 통해 접근 가능하며, key 변경 시 해시 값(_keyHash)이 자동으로 갱신됨.
        [SerializeField]
        private string _key;

        /// _keyHash는 현재 Variable 객체의 키 문자열에 대해 생성된 해시 값을 저장하는 필드입니다.
        /// 문자열 키는 StringUtility.StringToHash 메서드를 사용해 해시로 변환됩니다.
        [SerializeField]
        private int _keyHash;

        /// <summary>
        /// 변수를 고유하게 식별하기 위한 UGUID 형태의 유니크한 식별자.
        /// </summary>
        [SerializeField]
        private UGUID _guid;

        /// 직렬화된 타입 이름을 저장하는 문자열 변수로, 타입 정보를 유지 및 복원하는 데 사용된다.
        [SerializeField]
        private string _typeName;


        /// 변수의 데이터 타입을 나타내는 속성입니다.
        /// 해당 변수의 형식을 정의하며, 런타임 타입 정보를 제공합니다.
        public Type type
        {
            get;
            set;
        }

        /// UGUID 타입의 GUID를 나타내는 읽기 전용 속성.
        /// 각 Variable 객체를 고유하게 식별하기 위해 사용된다.
        public UGUID guid
        {
            get { return _guid; }
        }

        /// 변수의 고유 식별 키를 가져오거나 설정합니다. 키가 설정될 때, 해당 키의 해시 값이 자동으로 갱신됩니다.
        public string key
        {
            get { return this._key; }

            set { _keyHash = StringUtility.StringToHash(_key = value); }
        }

        /// key 값에 대해 고유한 해시를 반환합니다.
        /// 문자열 키를 해시로 변환하여 빠르게 비교하거나 조회하는 용도로 사용됩니다.
        public int keyHash
        {
            get { return this._keyHash; }
        }

        /// 모든 타입의 값을 포함할 수 있는 '박싱된 값'을 가져오거나 설정합니다.
        public abstract object boxedValue
        {
            get;
            set;
        }


        /// 현재 변수와 동일한 데이터를 포함하는 새로운 Variable 객체를 생성하여 반환합니다.
        /// <returns>
        /// 복제된 Variable 객체를 반환합니다.
        /// </returns>
        public Variable Duplicate()
        {
            Variable clone = Activator.CreateInstance(type) as Variable;
            clone._typeName = this._typeName;
            clone.type = this.type;
            clone._keyHash = this._keyHash;
            clone._key = this._key;
            clone._guid = this._guid;
            return clone;
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
    }


    /// 모든 Variable 클래스의 기본 추상 클래스.
    /// 직렬화 기능 및 공통된 속성을 정의.
    [Serializable]
    public abstract class Variable<T> : Variable
    {
        /// Variable<T> 클래스에서 변수의 값을 저장하기 위해 사용되는 내부 필드입니다.
        [SerializeField]
        protected T _value;


        /// 변수의 값을 가져오거나 설정합니다.
        public virtual T value
        {
            get { return _value; }
            
            set { _value = value; }
        }

        /// <summary>
        /// Object representation of the variable's value that supports boxed type access and modification.
        /// </summary>
        public override sealed object boxedValue
        {
            get { return _value; }
            
            set { this.TrySet(value); }
        }


        /// 주어진 값을 특정 타입으로 변환하여 설정을 시도합니다.
        /// <param name="newValue">설정하려는 새로운 값</param>
        private void TrySet(object newValue)
        {
            if (newValue is T convertedValue)
            {
                this._value = convertedValue;
            }
            else
            {
                Debug.LogError($"Failed to set value. Type: {newValue.GetType().Name} mismatch.");
            }
        }
    }
}