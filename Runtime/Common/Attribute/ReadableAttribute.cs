using System;

namespace TaskStreamer
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, Inherited = false)]
    public class ReadableAttribute : Attribute { }
}