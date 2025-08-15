using System;

namespace TaskStreamer
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
    public class ReadableAttribute : Attribute { }
}