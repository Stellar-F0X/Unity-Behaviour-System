using System;
using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;

namespace TaskStreamer.Injection
{
    public class FieldCollectProcessor : DefaultVisitProcessor
    {
        private delegate TValue ValueGetter<TContainer, TValue>(ref TContainer container);


        private delegate void ValueSetter<TContainer, TValue>(ref TContainer container, TValue value);
        
        
        public FieldCollectProcessor(List<object> propertiesContainer)
        {
            this._propertiesContainer = propertiesContainer;
        }


        private readonly List<object> _propertiesContainer;


        protected override void VisitProperty<TContainer, TValue>(Property<TContainer, TValue> property, ref TContainer container, ref TValue value)
        {
            if (value == null)
            {
                Debug.LogError($"{typeof(TContainer)}'s {property.Name} field is NullReference");
                return;
            }
            
            Delegate getValue = (ValueGetter<TContainer, TValue>)property.GetValue;
            Debug.Assert(getValue != null, "getValue is null");
            
            Delegate setValue = (ValueSetter<TContainer, TValue>)property.SetValue;
            Debug.Assert(setValue != null, "setValue is null");

            _propertiesContainer.Add(new VariableHandle(property.Name, value, container, property.GetAttributes(), getValue, setValue));
        }
    }
}