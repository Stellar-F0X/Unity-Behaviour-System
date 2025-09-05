using System;

namespace TaskStreamer
{
    /// <summary> Used for initializing default values for Blackboard Variables. </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class DefaultValueAttribute : Attribute
    {
        /// <summary> The default value must be same type as BlackboardVariable's generic type parameter. </summary>
        public DefaultValueAttribute(object defaultValue)
        {
            this.defaultValue = defaultValue;
        }
        
        public readonly object defaultValue;
    }
}