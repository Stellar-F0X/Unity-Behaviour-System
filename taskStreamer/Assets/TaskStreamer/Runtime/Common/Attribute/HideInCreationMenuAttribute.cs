using System;

namespace TaskStreamer
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class HideInCreationMenuAttribute : Attribute { }
}