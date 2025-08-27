using System;
using TaskStreamer.Utility;
using UnityEngine;

namespace TaskStreamer
{
    /// <summary> Variable wrapper class </summary>
    [Serializable, Readable]
    public abstract class BlackboardVariable : ISerializationCallbackReceiver
    {
        /// <summary> 변수의 기본 이름으로 사용되는 상수 </summary>
        internal const string DEFAULT_VARIABLE_NAME = "#__Local_Variable#__";


        /// Variable의 내부적으로 사용되는 문자열 키를 저장하는 필드.
        /// key 프로퍼티를 통해 접근 가능하며, key 변경 시 해시 값(_keyHash)이 자동으로 갱신됨.
        [SerializeField]
        protected string _key;


        /// <summary> _key에 의해 생성된 해시 값을 저장하는 필드. </summary>
        [SerializeField]
        protected int _keyHash;


        /// <summary> 변수를 고유하게 식별하기 위한 UGUID 형태의 유니크한 식별자를 저장하는 필드. </summary>
        [SerializeField]
        protected UGUID _guid = UGUID.Create();


        /// <summary> 직렬화된 타입 이름을 저장하며, 타입 정보를 유지 및 복원하는 데 사용된다. </summary>
        [SerializeField]
        protected string _typeName;


        /// <summary> 이 변수가 공유 가능한 상태인지 여부를 나타내는 플래그. </summary>
        [SerializeField]
        protected bool _isShareable;


        /// 변수 사용 힌트를 저장하는 필드.
        /// 사용 방식은 VariableUsageHint 열거형 값으로 정의됩니다.
        [SerializeField]
        protected VariableUsage _usage;


        /// <summary> 현재 변수에 구현된 Type을 나타내는 프로퍼티. </summary>
        internal Type implementedType
        {
            get;
            set;
        }

        /// <summary> 이 변수의 제네릭 타입을 반환하는 프로퍼티. </summary>
        internal abstract Type genericVariableType
        {
            get;
        }

        /// <summary> 해당 변수의 데이터 타입(Type)을 나타냄. </summary>
        internal abstract Type valueType
        {
            get;
        }


        /// <summary> 고유 식별자를 반환하는 내부 프로퍼티. </summary>
        internal UGUID guid
        {
            get { return _guid; }
        }


        /// <summary>변수의 문자열 키를 가져오거나 설정합니다.</summary>
        /// 값을 설정하면 해당 문자열의 해시 값이 자동으로 갱신됩니다.
        internal virtual string key
        {
            get { return this._key; }

            set { this._keyHash = StringUtility.StringToHash((_key = value)); }
        }


        /// <summary> 문자열 키의 해시 값을 저장합니다. </summary>
        internal int keyHash
        {
            get { return this._keyHash; }
        }


        /// <summary> 변수가 조건 평가 또는 필드로 사용되는지 나타내는 힌트를 반환합니다. </summary>
        internal VariableUsage usage
        {
            get { return _usage; }

            set { _usage = value; }
        }


        /// <summary> 변수의 값을 일반적인 object 타입으로 저장하고 관리하는 프로퍼티. </summary>
        internal abstract object boxedValue
        {
            get;
            set;
        }


        /// <summary> Blackboard 변수가 공유 상태인지 여부를 나타내는 속성. </summary>
        internal bool isShared
        {
            private set { _isShareable = value; }

            get { return _isShareable; }
        }



        /// <summary> 새 블랙보드 변수 인스턴스를 생성합니다. </summary>
        /// <param name="implementedType">생성할 변수의 타입입니다.</param>
        /// <param name="usage">변수의 사용 목적(필드 또는 조건)입니다.</param>
        /// <param name="shared">변수의 공유 가능 여부를 설정합니다.</param>
        /// <return>생성된 블랙보드 변수 인스턴스를 반환합니다.</return>
        public static BlackboardVariable Create(Type implementedType, bool shared)
        {
            Debug.Assert(implementedType is not null, $"{typeof(ObjectFactory)}: Wrong blackboard variable type");

            BlackboardVariable createdVariable = Activator.CreateInstance(implementedType) as BlackboardVariable;

            Debug.Assert(createdVariable is not null, $"{typeof(ObjectFactory)}: Failed to create a blackboard variable.");
            createdVariable._isShareable = shared;
            return createdVariable;
        }


        /// <summary> 직렬화 이전에 호출되며, 변수의 타입 정보를 저장합니다. </summary>
        /// 타입이 null이면 디버그 검사를 통해 경고를 발생시킵니다.
        public void OnBeforeSerialize()
        {
            Debug.Assert(implementedType is not null, "Failed to serialize a property.");
            this._typeName = implementedType.AssemblyQualifiedName;
        }


        /// <summary> 직렬화 이후에 호출되며, 직렬화된 데이터로부터 타입 정보를 복원합니다. </summary>
        public void OnAfterDeserialize()
        {
            Debug.Assert(_typeName.IsNotNullOrEmpty(), "Failed to deserialize a property.");
            this.implementedType = Type.GetType(_typeName);
        }


        /// <summary>현재 BlackboardVariable 인스턴스의 복제본을 생성합니다.</summary>
        /// <return>현재 인스턴스의 복제본인 새로운 BlackboardVariable.</return>
        internal abstract BlackboardVariable Duplicate();
    }


    /// <summary> Abstract base class representing a variable within the Blackboard system </summary>
    [Serializable, Readable]
    public class BlackboardVariable<TValue> : BlackboardVariable, ISharedBlackboardVariable
    {
        /// <summary> BlackboardVariable의 값을 저장하는 필드. </summary>
        [SerializeField]
        protected TValue _value;

        /// <summary> BlackboardData 객체를 참조하는 필드로, 현재 변수의 소속된 Blackboard를 나타냅니다. </summary>
        [SerializeField]
        private protected BlackboardData _blackboard;


        /// <summary> BlackboardVariable의 키를 나타내며, GetKey와 SetKey 메소드를 통해 검색 또는 설정 가능. </summary>
        internal override sealed string key
        {
            get { return this.GetKey(); }

            set { this.SetKey(value); }
        }


        /// <summary> 해당 변수의 값을 가져오거나 설정합니다. </summary>
        public virtual TValue value
        {
            get { return this.GetValue(); }

            set { this.SetValue(value); }
        }


        /// <summary> 제네릭 변수 타입을 나타내는 프로퍼티. </summary>
        internal override sealed Type genericVariableType
        {
            get { return typeof(BlackboardVariable<TValue>); }
        }


        /// <summary> 변수가 표현하는 값의 타입을 반환합니다. </summary>
        internal override sealed Type valueType
        {
            get { return typeof(TValue); }
        }


        /// <summary> 값을 박스형 오브젝트로 접근 및 설정할 수 있는 프로퍼티. </summary>
        internal override sealed object boxedValue
        {
            get { return _value; }

            set { _value = (value is TValue converted) ? converted : _value; }
        }


        /// <summary> 변수의 키를 반환합니다. </summary>
        /// <returns> 변수의 고유 키. </returns>
        private string GetKey()
        {
            if (this._isShareable)
            {
                Debug.Assert(_blackboard != null, "A reference to the Blackboard is required but was not found");
                BlackboardVariable variable = _blackboard.FindVariable(base._guid);

                Debug.Assert(variable is not null, "Failed to find variable with GUID in blackboard");
                return variable.key;
            }

            return base.key; //잘못하면 무한 루프 걸리니 주의.
        }


        /// <summary> 지정된 키를 사용하여 변수의 키를 설정합니다. </summary>
        /// <param name="newKey">새로 설정할 키입니다.</param>
        private void SetKey(in string newKey)
        {
            if (this._isShareable)
            {
                Debug.Assert(_blackboard != null, "A reference to the Blackboard is required but was not found");
                BlackboardVariable variable = _blackboard.FindVariable(base._guid);

                Debug.Assert(variable is not null, "Failed to find variable with GUID in blackboard");
                variable.key = newKey;
            }

            base.key = newKey; //잘못하면 무한 루프 걸리니 주의.
        }


        /// <summary> 새로운 값을 변수에 설정합니다. </summary>
        /// <param name="newValue">설정할 값입니다.</param>
        private void SetValue(TValue newValue)
        {
            if (_isShareable == false)
            {
                this._value = newValue;
                return;
            }

            Debug.Assert(_blackboard != null, "A reference to the Blackboard is required but was not found");

            if (_blackboard.FindVariable(base._guid) is BlackboardVariable<TValue> variable)
            {
                variable.value = newValue;
            }
            else
            {
                Debug.LogError("Can't find variable");
            }
        }


        /// <summary>
        /// Blackboard에서 공유 가능한 상황을 고려하여 값을 얻습니다.
        /// </summary>
        /// <returns>현재 값 또는 연결된 Blackboard에서 구한 값</returns>
        private TValue GetValue()
        {
            if (_isShareable == false)
            {
                return this._value;
            }

            Debug.Assert(_blackboard != null, "A reference to the Blackboard is required but was not found");

            if (_blackboard.FindVariable(base._guid) is BlackboardVariable<TValue> variable)
            {
                return variable.value;
            }
            else
            {
                Debug.LogError("Can't find variable");
            }

            return default;
        }


        /// <summary> 현재 객체를 복제한 새로운 BlackboardVariable 인스턴스를 반환합니다. </summary>
        /// <returns> 복제된 BlackboardVariable 객체. </returns>
        internal override BlackboardVariable Duplicate()
        {
            BlackboardVariable<TValue> clone = Create(implementedType, _isShareable) as BlackboardVariable<TValue>;
            Debug.Assert(clone is not null, "Failed to duplicate a blackboard variable.");

            clone.implementedType = this.implementedType;
            clone._isShareable = this._isShareable;
            clone._blackboard = this._blackboard;
            clone._typeName = this._typeName;
            clone._guid = this._guid;
            clone.value = this.value;
            clone.key = this.key;
            return clone;
        }


        /// <summary> 공유 가능한 블랙보드 변수에 블랙보드와 변수 GUID를 설정합니다. </summary>
        /// <param name="blackboard"> 연결할 블랙보드 데이터입니다. </param>
        /// <param name="variableGuid"> 설정할 변수의 GUID입니다. </param>
        void ISharedBlackboardVariable.SetBlackboardAndVariableReference(in BlackboardData blackboard, in UGUID variableGuid)
        {
            if (_isShareable == false)
            {
                Debug.LogError("Variable is not shareable and cannot bind to shared blackboard");
                return;
            }

            Debug.Assert(blackboard is not null, "Cannot bind to null blackboard reference");
            this._blackboard = blackboard;

            Debug.Assert(blackboard.HasVariable(variableGuid), "Failed to find variable with GUID in blackboard");
            this._guid = variableGuid;
        }
    }
}