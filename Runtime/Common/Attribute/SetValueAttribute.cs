using System;

namespace TaskStreamer
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class SetValueAttribute : Attribute
    {
        public SetValueAttribute(object defaultValue)
        {
            this.defaultValue = defaultValue;
        }
        
        public object defaultValue;
    }
}