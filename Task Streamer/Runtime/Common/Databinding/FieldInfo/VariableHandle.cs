using System;
using System.Collections.Generic;
using TaskStreamer.Utility;

namespace TaskStreamer.Injection
{
    public class VariableHandle
    {
        public VariableHandle(string context, object value, object container, IEnumerable<Attribute> attributes, Delegate getValue, Delegate setValue)
        {
            this.value = value;
            this.context = context;
            this._container = container;
            this.fieldType = value.GetType();
            this._fieldAttributes = attributes;
            this._setVariableToContainer = setValue;
            this._getVariableFromContainer = getValue;
        }
        
        public readonly string context;
        public readonly Type fieldType;
        public readonly object value;
        
        private readonly object _container;
        
        private readonly Delegate _getVariableFromContainer;
        private readonly Delegate _setVariableToContainer;
        
        private readonly IEnumerable<Attribute> _fieldAttributes;


        public void SetValue(object newValue)
        {
            _setVariableToContainer?.DynamicInvoke(_container, newValue);
        }


        public T GetValue<T>()
        {
            return (this.GetValue() is T convertedValue) ? convertedValue : default;
        }

        
        public object GetValue()
        {
            return _getVariableFromContainer?.DynamicInvoke(_container);
        }

        
        public T GetAttribute<T>()
        {
            return _fieldAttributes.GetAttribute<T>();
        }
    }
}