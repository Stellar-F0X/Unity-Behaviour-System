using System;

namespace TaskStreamer
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ComparableAttribute : Attribute
    {
        public ComparableAttribute(Comparison comparison)
        {
            this.comparison = comparison;
        }

        public Comparison comparison;
    }
}