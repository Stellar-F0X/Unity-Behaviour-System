using System;
using System.Collections.Generic;
using System.Reflection;
using TaskStreamer.FSM;
using TaskStreamer.Utility;
using Unity.Properties;

namespace TaskStreamer.Injection
{
    public class DefaultVisitProcessor : PropertyVisitor
    {
        public DefaultVisitProcessor()
        {
            _VisitAvailable.Add(typeof(List<NodeGroup>));
            _VisitAvailable.Add(typeof(List<Transition>));
            _VisitAvailable.Add(typeof(List<Condition>));
            _VisitAvailable.Add(typeof(KeyValuePair<UGUID, Graph>));
            _VisitAvailable.Add(typeof(KeyValuePair<UGUID, NodeBase>));
        }
        
        
        private readonly static HashSet<ICustomAttributeProvider> _VisitAvailable = new HashSet<ICustomAttributeProvider>();
        
        
        protected override bool IsExcluded<TContainer, TValue>(Property<TContainer, TValue> property, ref TContainer container, ref TValue value)
        {
            Type type = property.DeclaredValueType();

            if (_VisitAvailable.Contains(type))
            {
                return false; //Ignore filtering
            }
            
            if (type.HasAttribute<ReadableAttribute>())
            {
                _VisitAvailable.Add(type);
                return false; //Ignore filtering
            }
            else
            {
                return true; //Filtering this TValue type
            }
        }
    }
}