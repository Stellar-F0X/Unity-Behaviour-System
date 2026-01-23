using System;

namespace TaskStreamer.Runtime
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class PropertyOrderAttribute : Attribute
    {
        public PropertyOrderAttribute(int priority = Int32.MaxValue)
        {
            this.priority = priority;
        }
        
        public int priority; 
    }
}