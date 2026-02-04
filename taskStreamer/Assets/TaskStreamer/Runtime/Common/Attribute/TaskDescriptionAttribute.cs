using System;
using System.Runtime.CompilerServices;

namespace TaskStreamer.Runtime
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, Inherited = false)]
    public class TaskDescriptionAttribute : Attribute
    {
        public TaskDescriptionAttribute([CallerFilePath] string filePath = "")
        {
            this.filePath = filePath;
        }
        
        internal string filePath;
    }
}