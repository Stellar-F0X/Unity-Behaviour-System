using System;

namespace TaskStreamer.Runtime
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ComparableAttribute : Attribute
    {
        public ComparableAttribute(Comparison comparison)
        {
            this.comparison = comparison;
        }

        public Comparison comparison;
    }
}