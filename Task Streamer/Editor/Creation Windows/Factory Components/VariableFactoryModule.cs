using System;
using UnityEngine;

namespace TaskStreamer.Tool
{
    public class VariableFactoryModule : FactoryModule<Variable>
    {
        public VariableFactoryModule(Type targetType, string title, int layer = 1) : base(targetType, title, true, true, layer) { }

        protected override Variable Create(Type type, Vector2 position)
        {
            Variable variable = Utility.Utilities.CreateVariable(type);
            Debug.Assert(variable is not null, "Variable is null");
            return variable;
        }
    }
}