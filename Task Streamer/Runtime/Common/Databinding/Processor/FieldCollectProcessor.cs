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
                return;
            }
            
            VariableHandle handle = VariableHandleBuilder.GetHandle(property.Name, value, container)
                                                         .WithAttributes(property.GetAttributes())
                                                         .WithGetter((ValueGetter<TContainer, TValue>)property.GetValue)
                                                         .WithSetter((ValueSetter<TContainer, TValue>)property.SetValue)
                                                         .Build();

            _propertiesContainer.Add(handle);
        }
    }
}