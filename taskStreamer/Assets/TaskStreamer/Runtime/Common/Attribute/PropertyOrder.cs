using System;

namespace TaskStreamer
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class PropertyOrder : Attribute
    {
        public PropertyOrder(int priority = Int32.MaxValue)
        {
            this.priority = priority;
        }
        
        public int priority; 
    }
}