using System;

namespace TaskStreamer.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Field)]
    public class StreamSerializableAttribute : Attribute
    {
        
    }
}