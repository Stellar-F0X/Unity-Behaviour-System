using System;
using System.Collections.Generic;
using TaskStreamer.Utility;

namespace TaskStreamer.Injection
{
    /// <summary>주어진 컨텍스트 및 값 정보를 포함하는 변수를 다루는 클래스입니다.</summary>
    public class VariableHandle
    {
        /// <summary> 특정 변수의 핸들링을 담당하는 객체 </summary>
        public VariableHandle(string context, object value, object container, Type fieldType, IEnumerable<Attribute> attributes, Delegate getValue, Delegate setValue)
        {
            this.value = value;
            this.context = context;
            this._container = container;
            this.fieldType = fieldType;
            this._fieldAttributes = attributes;
            this._setVariableToContainer = setValue;
            this._getVariableFromContainer = getValue;
        }
        

        /// <summary> 현재 변수의 컨텍스트 이름을 나타냅니다. </summary>
        public readonly string context;

        
        /// <summary> 필드 타입을 나타내는 타입 객체입니다. </summary>
        public readonly Type fieldType;

        
        /// <summary> 실제 저장된 값을 나타냅니다. </summary>
        public readonly object value;

        
        /// <summary>_container를 저장하는 변수입니다.</summary>
        private readonly object _container;

        
        /// <summary>Delegate for retrieving a variable's value from the container.</summary>
        private readonly Delegate _getVariableFromContainer;

        
        /// <summary> Delegate used to set a variable's value into its container </summary>
        private readonly Delegate _setVariableToContainer;

        
        /// <summary> 필드와 연관된 모든 Attribute를 저장하는 변수입니다. </summary>
        private readonly IEnumerable<Attribute> _fieldAttributes;


        
        /// <summary>지정된 값을 설정합니다.</summary>
        /// <param name="newValue">설정할 새로운 값입니다.</param>
        public void SetValue(object newValue)
        {
            _setVariableToContainer?.DynamicInvoke(_container, newValue);
        }

        

        /// <summary>Generics를 활용하여 원하는 타입의 값을 반환합니다.</summary>
        /// <returns>성공 시, 변환된 값; 실패 시, 기본 값(default).</returns>
        public T GetValue<T>()
        {
            return (this.GetValue() is T convertedValue) ? convertedValue : default;
        }

        

        /// <summary>컨테이너에서 값을 가져옵니다.</summary>
        /// <return>가져온 값(오브젝트 타입)</return>
        public object GetValue()
        {
            return _getVariableFromContainer?.DynamicInvoke(_container);
        }
        


        /// <summary>제네릭 타입의 어트리뷰트를 반환한다.</summary>
        /// <return>찾은 제네릭 타입의 어트리뷰트.</return>
        public T GetAttribute<T>() where T : Attribute
        {
            return _fieldAttributes.GetAttribute<T>();
        }
    }
}