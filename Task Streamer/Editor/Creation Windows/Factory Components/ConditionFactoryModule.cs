using System;
using TaskStreamer.Utility;
using UnityEngine;

namespace TaskStreamer.Tool
{
    public class ConditionFactoryModule : FactoryModule<ConditionModule>
    {
        public ConditionFactoryModule(Type targetType, string title, int layer = 1) : base(targetType, title, true, true, layer) { }

        protected override ConditionModule Create(Type type, Vector2 position)
        {
            ConditionModule module = TaskStreamerUtility.CreateConditionModule(type);
            Debug.Assert(module is not null, "ConditionModule is null");
            return module;
        }
    }
}