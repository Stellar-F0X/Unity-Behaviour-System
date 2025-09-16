using System;
using TaskStreamer.Utility;
using UnityEngine;

namespace TaskStreamer
{
    /// <summary> Represents a wrapper for variables used in a blackboard system. </summary>
    [Serializable, Readable]
    public abstract class BlackboardVariable : ISerializationCallbackReceiver
    {
        /// <summary> 변수의 기본 이름으로 사용되는 상수 </summary>
        internal const string DEFAULT_VARIABLE_NAME = "#__Local_Variable#__";


        /// <summary> 변수의 내부적으로 사용되는 문자열 키를 저장하는 필드 </summary>
        [SerializeField]
        protected string _key;


        /// <summary> _key값을 기반으로 생성된 해시 값을 저장하는 필드. </summary>
        [SerializeField]
        protected int _keyHash;


        /// <summary> 변수를 고유하게 식별하기 위한 UGUID 형태의 유니크한 식별자를 저장하는 필드 </summary>
        [SerializeField]
        protected UGUID _guid = UGUID.Create();


        /// <summary> 직렬화된 타입 이름을 저장하며, 타입 정보를 유지 및 복원하는 데 사용된다. </summary>
        [SerializeField]
        protected string _typeName;


        /// <summary> 이 변수가 공유 상태인지 나타내는 플래그 </summary>
        [SerializeField]
        protected bool _isShared;


        /// <summary> 변수 사용 힌트를 저장하는 필드 </summary>
        [SerializeField]
        protected VariableUsage _usage;


        /// <summary> 블랙보드 변수에서 구현된 타입 정보를 나타내는 프로퍼티 </summary>
        internal Type implementedType
        {
            get;
            set;
        }

        /// <summary> 제네릭 변수의 타입을 나타내는 속성 </summary>
        internal abstract Type genericVariableType
        {
            get;
        }

        /// <summary> 이 변수의 실제 데이터 유형을 반환 </summary>
        internal abstract Type valueType
        {
            get;
        }


        /// <summary> 변수의 고유 식별자를 나타내는 프로퍼티 </summary>
        internal UGUID guid
        {
            get { return _guid; }
        }


        /// <summary> 변수의 이름을 가져오거나 설정하는 프로퍼티 </summary>
        internal virtual string key
        {
            get { return this._key; }

            set { this._keyHash = StringUtility.StringToHash((_key = value)); }
        }


        /// <summary> 키의 해시값을 반환합니다 </summary>
        internal int keyHash
        {
            get { return this._keyHash; }
        }


        /// <summary> 직렬화 콜백을 구현한 BlackboardVariable의 추상 기본 클래스 </summary>
        internal VariableUsage usage
        {
            get { return _usage; }

            set { _usage = value; }
        }


        /// <summary> 박스 처리된 값을 반환하거나 설정하는 프로퍼티 </summary>
        internal abstract object boxedValue
        {
            get;
            set;
        }


        /// <summary> 변수의 공유 여부를 나타내는 속성 </summary>
        internal bool isShared
        {
            get { return _isShared; }
        }



        /// <summary> 새 블랙보드 변수 인스턴스를 생성합니다. </summary>
        /// <param name="implementedType">생성할 변수의 타입입니다.</param>
        /// <param name="shared">변수의 공유 가능 여부를 설정합니다.</param>
        /// <returns>생성된 블랙보드 변수 인스턴스를 반환합니다.</returns>
        internal static BlackboardVariable Create(Type implementedType, bool shared)
        {
            Debug.Assert(implementedType is not null, $"{typeof(ObjectFactory)}: Wrong blackboard variable type");
            BlackboardVariable createdVariable = Activator.CreateInstance(implementedType) as BlackboardVariable;
            Debug.Assert(createdVariable is not null, $"{typeof(ObjectFactory)}: Failed to create a blackboard variable.");

            createdVariable.implementedType = implementedType;
            createdVariable._typeName = implementedType.AssemblyQualifiedName;
            createdVariable._isShared = shared;
            return createdVariable;
        }


        /// <summary> 새 블랙보드 변수 인스턴스를 생성합니다. </summary>
        /// <param name="implementedType">생성할 변수의 타입입니다.</param>
        /// <param name="shared">변수의 공유 가능 여부를 설정합니다.</param>
        /// <param name="variableGuid">변수의 고유 식별자입니다.</param>
        /// <returns>생성된 블랙보드 변수 인스턴스를 반환합니다.</returns>
        internal static BlackboardVariable Create(Type implementedType, UGUID variableGuid, bool shared)
        {
            Debug.Assert(variableGuid.IsEmpty() == false, "reference Guid is empty");
            BlackboardVariable createdVariable = BlackboardVariable.Create(implementedType, shared);

            createdVariable._guid = variableGuid;
            return createdVariable;
        }


        /// <summary> 지정된 BlackboardVariable에 값을 설정하려고 시도합니다. </summary>
        /// <param name="variable">값을 설정할 블랙보드 변수입니다.</param>
        /// <param name="value">설정할 값입니다.</param>
        internal static void TrySetValue<TValue>(BlackboardVariable variable, in TValue value)
        {
            if (variable is BlackboardVariable<TValue> typedVariable)
            {
                typedVariable.value = value;
            }
            else
            {
                Debug.LogError($"Can't set value to {typeof(BlackboardVariable<TValue>)} variable");
            }
        }


        /// <summary> 직렬화 이전에 호출되며, 변수의 타입 정보를 저장합니다. </summary>
        /// <remarks> 타입이 null이면 디버그 검사를 통해 경고를 발생시킵니다. </remarks>
        public void OnBeforeSerialize()
        {
            if (implementedType is null)
            {
                //필드명아 바뀌거나, 제네릭 타입이 바뀌면 필드가 초기화되는데, 
                //그때 implementedType, TypeName이 마찬가지로 사라지므로 다시 대입.
                this.implementedType = this.GetType();
            }

            if (implementedType is not null)
            {
                this._typeName = implementedType.AssemblyQualifiedName;
            }
            else
            {
                Debug.Log("Failed to serialize a property.");
            }
        }


        /// <summary> 직렬화된 데이터로부터 타입 정보를 복원합니다. </summary>
        public void OnAfterDeserialize()
        {
            if (_typeName.IsNotNullOrEmpty())
            {
                this.implementedType = Type.GetType(_typeName);
            }
            else
            {
                Debug.LogError("Failed to deserialize a property.");
            }
        }


        /// <summary>현재 BlackboardVariable 인스턴스의 복제본을 생성합니다.</summary>
        /// <return>현재 인스턴스의 복제본인 새로운 BlackboardVariable을 반환합니다.</return>
        internal abstract BlackboardVariable Duplicate();
    }


    /// <summary> Abstract base class representing a variable within the Blackboard system </summary>
    [Serializable, Readable]
    public class BlackboardVariable<TValue> : BlackboardVariable, ISharedBlackboardVariable
    {
        /// <summary> BlackboardVariable의 값을 저장하는 필드. </summary>
        [SerializeField]
        protected TValue _value;


        /// <summary> 이 변수의 소속된 블랙보드를 나타내는 필드 </summary>
        [SerializeField]
        private protected BlackboardAsset _blackboard;



        /// <summary> 공유된 BlackboardVariable이 유효한지 여부를 나타냅니다 </summary>
        bool ISharedBlackboardVariable.isValid
        {
            get { return _blackboard != null && _blackboard.HasVariable(this._guid); }
        }


        /// <summary> 블랙보드 변수의 키를 가져오거나 설정합니다 </summary>
        internal override sealed string key
        {
            get { return this.GetKey(); }

            set { this.SetKey(value); }
        }


        /// <summary> 변수의 값을 가져오거나 설정합니다. </summary>
        public virtual TValue value
        {
            get { return this.GetValue(); }

            set { this.SetValue(value); }
        }


        /// <summary> 제네릭 변수의 타입을 나타내는 프로퍼티 </summary>
        internal override sealed Type genericVariableType
        {
            get { return typeof(BlackboardVariable<TValue>); }
        }


        /// <summary> 프로퍼티의 값 타입을 나타냄 </summary>
        internal override sealed Type valueType
        {
            get { return typeof(TValue); }
        }


        /// <summary> 변수의 객체형 값을 가져오거나 설정합니다. </summary>
        internal override sealed object boxedValue
        {
            get { return this.value; }

            set { this.SetValue(value); }
        }


        /// <summary> 변수의 키를 반환합니다. </summary>
        /// <return> 변수의 고유 키. </return>
        private string GetKey()
        {
            if (this._isShared)
            {
                Debug.Assert(_blackboard != null, "A reference to the Blackboard is required but was not found");
                BlackboardVariable variable = _blackboard.FindVariable(base._guid);

                Debug.Assert(variable is not null, "Failed to find variable with GUID in blackboard");
                return variable?.key;
            }

            return base.key; //잘못하면 무한 루프 걸리니 주의.
        }


        /// <summary> 변수의 키를 지정된 값으로 설정합니다. </summary>
        /// <param name="newKey">새로 설정할 키 값입니다.</param>
        private void SetKey(in string newKey)
        {
            if (this._isShared)
            {
                Debug.Assert(_blackboard != null, "A reference to the Blackboard is required but was not found");
                BlackboardVariable variable = _blackboard.FindVariable(base._guid);

                Debug.Assert(variable is not null, "Failed to find variable with GUID in blackboard");
                variable.key = newKey;
            }

            base.key = newKey; //잘못하면 무한 루프 걸리니 주의.
        }


        private void SetValue(object newValue)
        {
            //Null 정도는 허용 값이라 오류를 내지 않아도 됨.
            if (newValue is null)
            {
                return;
            }

            //문제는 완전히 다른 유형의 값이 들어왔을 때.
            if (newValue is TValue typedValue)
            {
                this.SetValue(typedValue);
            }
            else
            {
                Debug.LogError($"'{this.key}' Failed to set value of type '{value?.GetType()}' to BlackboardVariable<{typeof(TValue)}>");
            }
        }


        /// <summary> 새로운 값을 변수에 설정합니다. </summary>
        /// <param name="newValue">설정할 값입니다.</param>
        private void SetValue(TValue newValue)
        {
            if (_isShared == false)
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


        /// <summary> Blackboard에서 공유 가능한 상황을 고려하여 값을 얻습니다. </summary>
        /// <returns> 현재 값 또는 연결된 Blackboard에서 구한 값 </returns>
        private TValue GetValue()
        {
            if (_isShared == false)
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
        /// <return> 복제된 BlackboardVariable 객체. </return>
        internal override BlackboardVariable Duplicate()
        {
            BlackboardVariable<TValue> clone = Create(implementedType, _isShared) as BlackboardVariable<TValue>;
            Debug.Assert(clone is not null, "Failed to duplicate a blackboard variable.");

            clone.implementedType = this.implementedType;
            clone._typeName = this._typeName;
            clone._keyHash = this._keyHash;
            clone._key = this._key;
            clone._guid = this._guid;
            
            clone._isShared = this._isShared;
            clone._blackboard = this._blackboard;
            clone.value = this.value;
            return clone;
        }


        /// <summary> 공유 가능한 블랙보드 변수에 블랙보드 참조를 설정합니다. </summary>
        /// <param name="blackboard"> 연결할 블랙보드 데이터입니다. </param>
        void ISharedBlackboardVariable.SetBlackboardReference(in BlackboardAsset blackboard)
        {
            if (_isShared == false)
            {
                Debug.LogError("Variable is not shareable and cannot bind to shared blackboard");
                return;
            }

            Debug.Assert(blackboard is not null, "Cannot bind to null blackboard reference");
            this._blackboard = blackboard;
            this._isShared = true;
        }
    }
}