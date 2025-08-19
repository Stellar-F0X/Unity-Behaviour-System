using System;
using TaskStreamer.Utility;
using UnityEngine;

namespace TaskStreamer.Tool
{
    public class ConditionFactoryModule : FactoryModule<Condition>
    {
        public ConditionFactoryModule(Type targetType, string title, int layer = 1) : base(targetType, title, true, true, layer) { }

        protected override Condition Create(Type type, Vector2 position)
        {
            Condition module = TSObjectFactory.CreateConditionModule(type);
            Debug.Assert(module is not null, "ConditionModule is null");
            return module;
        }
    }
}