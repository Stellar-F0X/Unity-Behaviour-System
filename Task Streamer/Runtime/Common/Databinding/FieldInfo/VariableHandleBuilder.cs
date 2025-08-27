using System;
using System.Collections.Generic;
using UnityEngine;

namespace TaskStreamer.Injection
{
    /// <summary> VariableHandleBuilder를 생성 및 초기화하기 위한 구조체 </summary>
    internal struct VariableHandleBuilder
    {
        /// <summary> 변수의 이름을 나타내는 문자열 </summary>
        private string _handleName;

        /// <summary> 변수의 값을 저장하는 역할을 하는 필드입니다. </summary>
        private object _value;

        /// <Summary> 변수 값이 저장된 컨테이너를 참조합니다. </Summary>
        private object _container;

        /// <summary> 필드의 Type 정보를 나타내는 변수 </summary>
        private Type _fieldType;

        /// <summary> Delegate for accessing the variable's value. </summary>
        private Delegate _getter;

        /// <summary> Setter를 저장하는 위임 개체입니다. </summary>
        private Delegate _setter;

        /// <summary> 변수에 설정된 Attributes를 저장하는 컬렉션입니다. </summary>
        private IEnumerable<Attribute> _attributes;


        /// <summary>
        /// 지정된 이름, 값 및 컨테이너를 기반으로 VariableHandleBuilder를 생성합니다.
        /// </summary>
        /// <param name="handleName">핸들의 이름입니다.</param>
        /// <param name="value">핸들의 초기 값입니다.</param>
        /// <param name="valueContainer">핸들이 속한 컨테이너 객체입니다.</param>
        /// <returns>지정된 매개변수로 초기화된 VariableHandleBuilder를 반환합니다.</returns>
        public static VariableHandleBuilder GetHandle(string handleName, object value, object valueContainer)
        {
            VariableHandleBuilder builder = new VariableHandleBuilder();
            builder._handleName = handleName;
            builder._container = valueContainer;
            builder._value = value;
            return builder;
        }


        /// <summary>
        /// fieldType 설정.
        /// </summary>
        /// <param name="fieldType">필드에 사용될 Type 객체.</param>
        /// <returns>설정이 적용된 VariableHandleBuilder.</returns>
        public VariableHandleBuilder WithFieldType(Type fieldType)
        {
            Debug.Assert(fieldType is not null, "Field type must not be null. Please provide a valid Type object.");
            _fieldType = fieldType;
            return this;
        }


        /// <summary>
        /// 속성을 설정하여 VariableHandleBuilder를 생성합니다.
        /// </summary>
        /// <param name="attributes">설정할 속성 컬렉션</param>
        /// <returns>속성이 적용된 VariableHandleBuilder</returns>
        public VariableHandleBuilder WithAttributes(IEnumerable<Attribute> attributes)
        {
            Debug.Assert(attributes is not null, "Attributes collection must not be null. Please provide a valid collection of attributes.");
            _attributes = attributes;
            return this;
        }


        /// <summary>
        /// Getter를 추가하여 VariableHandleBuilder를 반환.
        /// </summary>
        /// <param name="getter">Getter로 사용할 Delegate.</param>
        /// <typeparam name="TGetter">Delegate의 타입.</typeparam>
        /// <returns>업데이트된 VariableHandleBuilder 인스턴스.</returns>
        public VariableHandleBuilder WithGetter<TGetter>(TGetter getter) where TGetter : Delegate
        {
            Debug.Assert(getter is not null, "Getter delegate must not be null. Please provide a valid getter function.");
            _getter = getter;
            return this;
        }


        /// <summary> Setter를 설정합니다. </summary>
        /// <param name="setter">설정할 Setter 델리게이트</param>
        /// <returns>설정이 적용된 VariableHandleBuilder를 반환</returns>
        public VariableHandleBuilder WithSetter<TSetter>(TSetter setter) where TSetter : Delegate
        {
            Debug.Assert(setter is not null, "Setter delegate must not be null. Please provide a valid setter function.");
            _setter = setter;
            return this;
        }


        /// <summary>
        /// 빌드 수행. value가 null인 경우 fieldType이 반드시 제공되어야 함.
        /// getter/setter/attributes는 null 허용(원래 VariableHandle 동작과 호환).
        /// </summary>
        /// <returns>구성된 VariableHandle 인스턴스를 반환.</returns>
        public VariableHandle Build()
        {
            // 기본 검증: context는 가급적 제공되어야 함
            Debug.Assert(string.IsNullOrEmpty(_handleName) == false, "VariableHandle requires a non-empty context. Use WithContext(...) to set it.");

            // value가 null이면 fieldType이 반드시 필요 (즉, 모두 Null이면 안 됨.)
            Debug.Assert(_value != null || _fieldType != null, "When value is null, a field type must be provided via WithFieldType(...).");

            // fieldType이 명시되어 있지 않으면 value로부터 유추
            if (_fieldType == null)
            {
                _fieldType = _value?.GetType() ?? throw new ArgumentNullException($"Could not determine field type: Value and field type are both null.");
            }

            return new VariableHandle(_handleName, _value, _container, _fieldType, _attributes, _getter, _setter);
        }
    }
}