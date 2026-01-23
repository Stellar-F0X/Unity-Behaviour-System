using System;

namespace TaskStreamer.Runtime
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class HideInCreationMenuAttribute : Attribute { }
}