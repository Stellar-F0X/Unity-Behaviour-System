using System;
using TaskStreamer.Runtime;
using TaskStreamer.Runtime.Utility;
using UnityEngine;

namespace TaskStreamer.Tool
{
    public class ConditionFactoryModule : FactoryModule<Condition>
    {
        public ConditionFactoryModule(string title, int layer = 1) : base(typeof(Condition), title, true, layer) { }


        protected override Condition Create(Type type, Vector2 position, string entryName)
        {
            Condition module = ObjectFactory.CreateConditionModule(type);
            Debug.Assert(module is not null, "ConditionModule is null");
            return module;
        }
    }
}