using System;
using System.Collections.Generic;
using Unity.Properties;

namespace TaskStreamer.Injection
{
    public class FieldCollectProcessor : DefaultVisitProcessor
    {
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

            _propertiesContainer.Add(new Tuple<string, object, IEnumerable<Attribute>>(property.Name, value, property.GetAttributes()));
        }
    }
}