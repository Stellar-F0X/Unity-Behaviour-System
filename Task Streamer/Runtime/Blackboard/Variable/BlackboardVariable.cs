using System;
using TaskStreamer.Utility;
using UnityEngine;

namespace TaskStreamer
{
    /// <summary> Variable wrapper class </summary>
    [Serializable, Readable]
    public abstract class BlackboardVariable : ISerializationCallbackReceiver
    {
        internal const string DEFAULT_VARIABLE_NAME = "#__Local_Variable#__";


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
        internal Type implementedType
        {
            get;
            set;
        }

        internal abstract Type genericVariableType
        {
            get;
        }

        internal abstract Type valueType
        {
            get;
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

            set { this._keyHash = StringUtility.StringToHash((_key = value)); }
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
            private set { _isShareable = value; }

            get { return _isShareable; }
        }



        public static BlackboardVariable Create(Type implementedType, bool shared)
        {
            Debug.Assert(implementedType is not null, $"{typeof(ObjectFactory)}: Wrong blackboard variable type");
            
            BlackboardVariable createdVariable = Activator.CreateInstance(implementedType) as BlackboardVariable;
            
            Debug.Assert(createdVariable is not null, $"{typeof(ObjectFactory)}: Failed to create a blackboard variable.");
            createdVariable._isShareable = shared;
            return createdVariable;
        }


        /// 직렬화 이전에 호출되며, 변수의 타입 정보를 저장합니다.
        /// 타입이 null이면 디버그 검사를 통해 경고를 발생시킵니다.
        public void OnBeforeSerialize()
        {
            Debug.Assert(implementedType is not null, "Failed to serialize a property.");
            this._typeName = implementedType.AssemblyQualifiedName;
        }


        /// 직렬화 이후에 호출되며, 직렬화된 데이터로부터 타입 정보를 복원합니다.
        /// _typeName이 비어 있지 않은지 확인하고, 이를 통해 타입을 로드합니다.
        public void OnAfterDeserialize()
        {
            Debug.Assert(_typeName.IsNotNullOrEmpty(), "Failed to deserialize a property.");
            this.implementedType = Type.GetType(_typeName);
        }


        /// <summary>
        /// Creates a duplicate of the current BlackboardVariable instance.
        /// </summary>
        /// <returns>A new instance of BlackboardVariable that is a copy of the current instance.</returns>
        internal abstract BlackboardVariable Duplicate();
    }


    /// <summary> Abstract base class representing a variable within the Blackboard system </summary>
    [Serializable, Readable]
    public class BlackboardVariable<TValue> : BlackboardVariable, ISharedBlackboardVariable
    {
        [SerializeField]
        protected TValue _value;

        [SerializeField]
        private protected BlackboardData _blackboard;


        internal override sealed string key
        {
            get { return this.GetKey(); }

            set { this.SetKey(value); }
        }


        /// <summary> Gets or sets the value associated with the blackboard variable, with type checking and validation. </summary>
        public virtual TValue value
        {
            get { return this.GetValue(); }

            set { this.SetValue(value); }
        }


        internal override sealed Type genericVariableType
        {
            get { return typeof(BlackboardVariable<TValue>); }
        }


        internal override sealed Type valueType
        {
            get { return typeof(TValue); }
        }


        internal override sealed object boxedValue
        {
            get { return _value; }

            set { _value = (value is TValue converted) ? converted : _value; }
        }


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